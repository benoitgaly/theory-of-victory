// Sector counters. Owns its own file, like hexmap.js: the map calls
// window.tovCounters.draw() and nothing else.
//
//   draw(svg, turn, board, project, opts) -> void
//   - svg      the map's <svg>, already carrying ground, grid, rivers, cities
//   - turn     one TurnSnapshot: turn.sectors[], turn.invader, turn.defender
//   - board[]  lon/lat anchors and push vectors per sector
//   - project  lon,lat -> [x,y], the map's own padded projector
//   - opts     optional. opts.cities defaults to window.tovGeo.cities
//
// It draws, per sector, two counters astride the contact line and one resolution
// glyph on the line itself. It replaces callouts() rather than sitting next to it:
// the sector name and the distance moved are carried here, on the glyph.
//
// ---------------------------------------------------------------------------
// WHAT IS DEGRADED TODAY, AND WHY
//
// Step 0 of the implementation path — publishing per-side sector detail on
// SectorResolution — is not done yet. SectorResolution currently exposes the
// pair (attackerPower, defenderPower) oriented on whoever pushed hardest, and
// defenderPower is a RESISTANCE: hold multiplied by terrain, urbanisation,
// fortification, drone friction and season. None of those multipliers is
// published, so the holder's raw power cannot be recovered from it.
//
// The rule of the project is that JavaScript never recomputes a quantity of the
// model. So nothing is inferred, and the drawing degrades where the data is
// missing:
//
//   - Full steps        drawn for the attacking side only, from attackerPower.
//                       The holder's ladder is drawn as a DASHED empty track,
//                       which reads as "not published" and never as zero.
//                       On a still sector nobody is named attacker, so both
//                       ladders are dashed. Unlocked by invaderPower /
//                       defenderPowerRaw.
//   - Dry steps         absent everywhere. Establishment = power / coverage
//                       would be a recomputation, and a wrong one: coverage is
//                       clamped at 1.2 and training quality and cohesion also
//                       multiply. Unlocked by invaderEstablishmentPower /
//                       defenderEstablishmentPower.
//   - Crenellation      absent: fortification is per side and per sector inside
//                       FrontSector, never serialised. Unlocked by
//                       invaderFortification / defenderFortification.
//   - Attacker identity derived from the sign of hexesMoved, which says nothing
//                       when the sector did not move — the most frequent case.
//                       Unlocked by attackerSideCode.
//
// Everything else is live today: the ratio, the movement, the losses, the
// bottleneck and the collapse flag. Each field above is picked up on its own
// the day it appears, with no change here.
//
// Two conversions are done in this file, and they are unit changes, not model
// arithmetic. The engine counts men in THOUSANDS everywhere except in
// SideSnapshot, which converts; SectorResolution does not. So sector powers and
// sector losses are multiplied by a thousand before being shown, because
// "48" means nothing to a reader and "48 000 hommes" is read at once.
window.tovCounters = (function () {
    "use strict";

    /* ---------------- Display constants ----------------
       None of these is a rule. Changing any of them must change no engine
       output — they only decide how a published number is drawn. */

    var STEP_THOUSANDS = 10;   // one notch = 10 000 men-equivalents
    var STEPS = 8;             // notches on the ladder
    var ARMY_THOUSANDS = 60;   // at or above, the counter is drawn as an army

    var CW = 46, CH = 34;      // counter box
    var GAP = 38;              // minimum vertical distance between two counters
    var OFFSET = 34;           // from the contact line to the counter's inner edge
    var MAP_W = 900, MAP_H = 520;

    var COLOUR = {
        card: "#fbf9f4",
        rule: "#d9d1be",
        ink: "#1a1815",
        ink2: "#4e4a42",
        ink3: "#8b8578",
        paper: "#fffdf8",
        invader: "#a8322a",
        defender: "#1e5fa8"
    };

    // The three staves, in the colours the barrel already uses on the other screens:
    // one authority per resource, whatever the surface.
    var BOTTLENECK = {
        weapons: { code: "weapons", colour: "#b8860b" },
        fuel: { code: "fuel", colour: "#8a5a2b" },
        food: { code: "food", colour: "#3d7a51" }
    };

    var seq = 0;

    /* ---------------- Small helpers, same facture as hexmap.js ---------------- */

    // Null-valued attributes are dropped rather than stringified: several pieces
    // below are drawn with or without a dash pattern depending on what is published.
    function svgEl(tag, attrs) {
        var n = document.createElementNS("http://www.w3.org/2000/svg", tag);
        Object.keys(attrs || {}).forEach(function (k) {
            if (attrs[k] === null || attrs[k] === undefined) { return; }
            n.setAttribute(k, attrs[k]);
        });
        return n;
    }

    function text(x, y, content, attrs) {
        var n = svgEl("text", attrs || {});
        n.setAttribute("x", x);
        n.setAttribute("y", y);
        n.textContent = content;
        return n;
    }

    // Paper painted behind the glyphs, so a figure stays readable over the grid.
    function halo(node, colour) {
        node.setAttribute("stroke", colour || COLOUR.paper);
        node.setAttribute("stroke-width", "3");
        node.setAttribute("stroke-linejoin", "round");
        node.setAttribute("paint-order", "stroke");
        return node;
    }

    // French grouping: a narrow no-break space, never a comma.
    function men(thousands) {
        var v = Math.round(thousands * 1000 / 100) * 100;
        var s = String(Math.round(v));
        var out = "";
        for (var i = 0; i < s.length; i++) {
            if (i > 0 && (s.length - i) % 3 === 0) { out += " "; }
            out += s[i];
        }
        return out;
    }

    function km(hexes) {
        var v = Math.abs(hexes * 10);
        return v.toFixed(v < 10 ? 1 : 0).replace(".", ",");
    }

    /* ---------------- Reading the turn ----------------
       One place where the shape of the published data is interpreted, so the
       drawing below never has to ask whether a field exists. */

    function sideOf(turn, code) {
        return code === "invader" ? turn.invader : turn.defender;
    }

    function bottleneckOf(snapshot) {
        if (!snapshot || !snapshot.bottleneckCode) { return null; }
        // Only when it actually bites: naming a stave that covers the need would
        // put a warning on a counter that has none.
        if (snapshot.materialCoverage === undefined || snapshot.materialCoverage >= 0.95) { return null; }
        return BOTTLENECK[snapshot.bottleneckCode] || null;
    }

    function readSector(res, turn) {
        var moved = res.hexesMoved || 0;

        // Step 0 names the attacker outright. Without it, only a sector that moved
        // says who was pushing — and most sectors do not move.
        var attacker = res.attackerSideCode || null;
        if (!attacker && Math.abs(moved) > 0.001) {
            attacker = moved > 0 ? "invader" : "defender";
        }

        function power(sideCode, publishedRaw) {
            if (publishedRaw !== undefined && publishedRaw !== null) { return publishedRaw; }
            // The published pair is oriented on the attacker: only his half is a
            // raw power. The holder's is a resistance, and undoing it is impossible.
            if (attacker === sideCode && res.attackerPower !== undefined) { return res.attackerPower; }
            return null;
        }

        function losses(sideCode) {
            if (!attacker) { return null; }
            return attacker === sideCode ? res.attackerLosses : res.defenderLosses;
        }

        function side(sideCode, rawKey, estKey, fortKey) {
            var snapshot = sideOf(turn, sideCode);
            return {
                code: sideCode,
                colour: sideCode === "invader" ? COLOUR.invader : COLOUR.defender,
                power: power(sideCode, res[rawKey]),
                establishment: res[estKey] === undefined ? null : res[estKey],
                fortification: res[fortKey] === undefined ? null : res[fortKey],
                losses: losses(sideCode),
                bottleneck: bottleneckOf(snapshot),
                collapsed: !!(snapshot && snapshot.hasCollapsed)
            };
        }

        return {
            code: res.sectorCode,
            name: (res.sectorName || res.sectorCode || "").split(" — ")[0],
            ratio: res.ratio || 0,
            moved: moved,
            attacker: attacker,
            // No attribution is possible on a still sector, but the pair always
            // sums to what the quarter cost — and on a stalled sector that total
            // is the only thing worth saying.
            cost: (res.attackerLosses || 0) + (res.defenderLosses || 0),
            invader: side("invader", "invaderPower", "invaderEstablishmentPower", "invaderFortification"),
            defender: side("defender", "defenderPowerRaw", "defenderEstablishmentPower", "defenderFortification")
        };
    }

    /* ---------------- Placement ----------------
       Lifted from callouts() in hexmap.js: sort north to south, push each label
       down until it clears the previous one, then pull the column back inside the
       frame. Copied rather than shared, so the map keeps owning its own file. */

    function spread(wanted, gap, top, bottom) {
        var out = wanted.slice();
        for (var i = 0; i < out.length; i++) {
            var floor = i === 0 ? top : out[i - 1] + gap;
            if (out[i] < floor) { out[i] = floor; }
        }
        for (var j = out.length - 1; j >= 0; j--) {
            var ceiling = j === out.length - 1 ? bottom : out[j + 1] - gap;
            if (out[j] > ceiling) { out[j] = ceiling; }
        }
        return out;
    }

    // Cities own their names. A counter that would land on one is pushed further
    // away from the line, never on top of it.
    function avoidCities(x, y, towns, direction) {
        for (var pass = 0; pass < 2; pass++) {
            towns.forEach(function (t) {
                if (Math.abs(t.y - y) > CH / 2 + 7) { return; }
                var overlaps = x < t.right && x + CW > t.left;
                if (!overlaps) { return; }
                x = direction > 0 ? Math.max(x, t.right + 5) : Math.min(x, t.left - 5 - CW);
            });
        }
        return Math.max(6, Math.min(MAP_W - 6 - CW, x));
    }

    /* ---------------- The counter ----------------
       Mirrored so the two counters of a sector face each other: each keeps its
       ladder on the outer flank and its trench line on the flank turned towards
       the enemy. Everything is laid out in the unmirrored frame and passed
       through mx(), which is the only thing that knows about the flip. */

    function counter(svg, x, y, side, facing) {
        var flip = facing < 0;                       // enemy on the left
        // Mirrors a box, not a point: pass the width so a rectangle lands on the
        // other flank instead of hanging off the edge.
        function mirror(left, width) { return flip ? CW - left - (width || 0) : left; }

        var g = svgEl("g", { transform: "translate(" + x.toFixed(1) + " " + y.toFixed(1) + ")" });

        g.appendChild(svgEl("rect", {
            x: 0, y: 0, width: CW, height: CH, rx: 1.5,
            fill: COLOUR.card, stroke: side.colour, "stroke-width": "1.2"
        }));

        /* The ladder. Filled from the bottom, like a thermometer, because that is
           how a reader expects a level to grow. */
        var trackW = 6, trackX = mirror(3.5, trackW);
        var trackTop = 4, trackH = 26, notch = trackH / STEPS;
        var known = side.power !== null && side.power !== undefined;

        g.appendChild(svgEl("rect", {
            x: trackX, y: trackTop, width: trackW, height: trackH,
            fill: "none", stroke: known ? COLOUR.rule : COLOUR.ink3, "stroke-width": "0.6",
            "stroke-dasharray": known ? null : "1 1.5"
        }));

        if (known) {
            var steps = Math.min(STEPS, side.power / STEP_THOUSANDS);

            // Dry notches: the men who are there and are not supplied. They arrive
            // with step 0; until then the top of the track is simply empty.
            if (side.establishment !== null && side.establishment > side.power) {
                var dry = Math.min(STEPS, side.establishment / STEP_THOUSANDS);
                g.appendChild(svgEl("rect", {
                    x: trackX, y: trackTop + trackH - dry * notch,
                    width: trackW, height: (dry - steps) * notch,
                    fill: "url(#tov-dry-" + seq + ")", stroke: "none"
                }));
            }

            g.appendChild(svgEl("rect", {
                x: trackX, y: trackTop + trackH - steps * notch,
                width: trackW, height: steps * notch,
                fill: side.colour, opacity: "0.78"
            }));

            // What the quarter cost, carved off the top of the stack. Below one
            // notch it is a sliver, which is honest: a quarter of grinding rarely
            // costs a full step.
            if (side.losses) {
                var lost = Math.min(steps, side.losses / STEP_THOUSANDS);
                if (lost > 0.04) {
                    g.appendChild(svgEl("rect", {
                        x: trackX, y: trackTop + trackH - steps * notch,
                        width: trackW, height: lost * notch,
                        fill: COLOUR.paper, opacity: "0.55"
                    }));
                    g.appendChild(svgEl("line", {
                        x1: trackX - 1, y1: trackTop + trackH - steps * notch,
                        x2: trackX + trackW + 1, y2: trackTop + trackH - (steps - lost) * notch,
                        stroke: COLOUR.ink, "stroke-width": "0.8"
                    }));
                }
            }

            // Notch rules, drawn over the fill: the eye counts crans, not a bar.
            for (var k = 1; k < STEPS; k++) {
                g.appendChild(svgEl("line", {
                    x1: trackX, y1: trackTop + k * notch, x2: trackX + trackW, y2: trackTop + k * notch,
                    stroke: COLOUR.card, "stroke-width": "0.5", opacity: "0.85"
                }));
            }
        }

        g.appendChild(svgEl("line", {
            x1: mirror(11.5), y1: 4, x2: mirror(11.5), y2: 30,
            stroke: COLOUR.rule, "stroke-width": "0.5"
        }));

        /* The NATO frame. Infantry — the two diagonals — because the engine's own
           unit of power is the fully supplied infantryman, and the counter should
           say the unit it is drawn in. */
        var fw = 28, fx = mirror(15, fw), fy = 10, fh = 18;
        g.appendChild(svgEl("rect", {
            x: fx, y: fy, width: fw, height: fh,
            fill: "none", stroke: COLOUR.ink2, "stroke-width": "0.9"
        }));
        g.appendChild(svgEl("path", {
            d: "M" + fx + " " + fy + "L" + (fx + fw) + " " + (fy + fh) +
               "M" + (fx + fw) + " " + fy + "L" + fx + " " + (fy + fh),
            stroke: COLOUR.ink2, "stroke-width": "0.8", fill: "none", opacity: "0.75"
        }));

        // Echelon, above the frame as symbology requires. A display convention,
        // not a rule: it reads the same published power as the ladder.
        if (known) {
            g.appendChild(text(fx + fw / 2, 8,
                side.power >= ARMY_THOUSANDS ? "XXXX" : "XXX", {
                    "text-anchor": "middle", "font-size": "6", "font-weight": "700",
                    "letter-spacing": "0.5", fill: COLOUR.ink2
                }));
        }

        // The check figure. The ladder is the glance; this is the verification,
        // and never the other way round.
        if (known) {
            g.appendChild(text(fx + fw / 2, 32.5, men(side.power), {
                "text-anchor": "middle", "font-size": "7.5",
                "font-family": "Georgia, 'Palatino Linotype', serif",
                "font-variant-numeric": "tabular-nums", fill: COLOUR.ink
            }));
        }

        // The bottleneck, named only when it bites. One authority: the glyph
        // designates the stave the engine designated.
        if (side.bottleneck) {
            bottleneckGlyph(g, mirror(4, 7), 25, side.bottleneck);
        }

        /* Prepared positions, on the flank turned towards the enemy. Awaiting the
           fortification fields; the shape is here so the day they land, nothing
           else moves. */
        if (side.fortification) {
            var crenels = Math.max(1, Math.min(3, Math.round(side.fortification / 0.4)));
            crenellation(g, flip ? 0 : CW, facing, crenels);
        }

        // An army that has broken is struck out. It is the only turn that must
        // look like no other.
        if (side.collapsed) {
            g.appendChild(svgEl("path", {
                d: "M2 2L" + (CW - 2) + " " + (CH - 2) + "M" + (CW - 2) + " 2L2 " + (CH - 2),
                stroke: side.colour, "stroke-width": "1.6", opacity: "0.75", fill: "none"
            }));
        }

        svg.appendChild(g);
    }

    // Three silhouettes rather than three letters: a pictogram is read without
    // being decoded, and there are only ever three of them.
    function bottleneckGlyph(g, x, y, flow) {
        var d;
        if (flow.code === "weapons") {
            d = "M" + x + " " + (y + 6) + "v-4l3-3 3 3v4z";              // a shell
        } else if (flow.code === "fuel") {
            d = "M" + (x + 0.5) + " " + (y + 6) + "v-5h5v5zM" + (x + 5.5) + " " + (y + 2) + "h1.2v1.6h-1.2z";
        } else {
            d = "M" + x + " " + (y + 6) + "a3 3 0 0 1 6 0z";              // a loaf
        }
        g.appendChild(svgEl("path", { d: d, fill: flow.colour, opacity: "0.9" }));
    }

    // The trench symbol every player already knows: a square wave along the edge.
    function crenellation(g, edgeX, facing, crenels) {
        var step = 6, amp = 2.6, y0 = CH / 2 - (crenels * step) / 2;
        var d = "M" + edgeX + " " + y0;
        for (var i = 0; i < crenels; i++) {
            var y = y0 + i * step;
            d += "h" + (facing * amp) + "v" + (step / 2) + "h" + (-facing * amp) + "v" + (step / 2);
        }
        g.appendChild(svgEl("path", {
            d: d, fill: "none", stroke: COLOUR.ink2, "stroke-width": "1", "stroke-linejoin": "miter"
        }));
    }

    /* ---------------- The resolution glyph ----------------
       On the line itself, between the two counters. Below 1,1 it is not a short
       arrow: it is a butée, two chevrons that meet. The frozen front is the normal
       result of this model, not an absence, and it must be drawn as an event. */

    function butee(svg, ax, ay, ux, uy, sector) {
        var g = svgEl("g", {});
        var nx = -uy, ny = ux;                  // along the line, for the stop bar

        function chevron(dir) {
            var tipX = ax + ux * dir * 3.5, tipY = ay + uy * dir * 3.5;
            var backX = ax + ux * dir * 12, backY = ay + uy * dir * 12;
            return "M" + (backX + nx * 6).toFixed(1) + " " + (backY + ny * 6).toFixed(1) +
                   "L" + tipX.toFixed(1) + " " + tipY.toFixed(1) +
                   "L" + (backX - nx * 6).toFixed(1) + " " + (backY - ny * 6).toFixed(1);
        }

        [1, -1].forEach(function (dir) {
            g.appendChild(svgEl("path", {
                d: chevron(dir), fill: "none", stroke: COLOUR.ink,
                "stroke-width": "2", "stroke-linecap": "round", "stroke-linejoin": "round",
                opacity: "0.85"
            }));
        });

        // Where they meet, the line holds.
        g.appendChild(svgEl("line", {
            x1: (ax + nx * 7).toFixed(1), y1: (ay + ny * 7).toFixed(1),
            x2: (ax - nx * 7).toFixed(1), y2: (ay - ny * 7).toFixed(1),
            stroke: COLOUR.ink, "stroke-width": "1.1", opacity: "0.55"
        }));

        svg.appendChild(g);
        return sector.cost > 0 ? "− " + men(sector.cost) + " hommes" : null;
    }

    function arrow(svg, ax, ay, ux, uy, sector) {
        var dir = sector.moved > 0 ? 1 : -1;
        var colour = dir > 0 ? COLOUR.invader : COLOUR.defender;
        var broken = sector.invader.collapsed || sector.defender.collapsed;

        // Symbolic, never to scale: ten kilometres is five pixels on this map, and
        // the distance is carried by the figure beside it.
        var reach = broken ? 30 : 15;
        var thick = sector.ratio >= 3 ? 3.4 : (sector.ratio >= 2 ? 2.6 : 1.8);

        var x1 = ax - ux * dir * reach, y1 = ay - uy * dir * reach;
        var x2 = ax + ux * dir * reach, y2 = ay + uy * dir * reach;
        var nx = -uy * dir, ny = ux * dir;

        svg.appendChild(svgEl("line", {
            x1: x1.toFixed(1), y1: y1.toFixed(1), x2: x2.toFixed(1), y2: y2.toFixed(1),
            stroke: COLOUR.paper, "stroke-width": (thick + 2.4).toFixed(1),
            "stroke-linecap": "round", opacity: "0.75"
        }));
        svg.appendChild(svgEl("line", {
            x1: x1.toFixed(1), y1: y1.toFixed(1), x2: x2.toFixed(1), y2: y2.toFixed(1),
            stroke: colour, "stroke-width": thick.toFixed(1), "stroke-linecap": "round"
        }));

        var headX = ax + ux * dir * (reach - 1), headY = ay + uy * dir * (reach - 1);
        svg.appendChild(svgEl("path", {
            d: "M" + x2.toFixed(1) + " " + y2.toFixed(1) +
               "L" + (headX - ux * dir * 6 + nx * 4.6).toFixed(1) + " " + (headY - uy * dir * 6 + ny * 4.6).toFixed(1) +
               "L" + (headX - ux * dir * 6 - nx * 4.6).toFixed(1) + " " + (headY - uy * dir * 6 - ny * 4.6).toFixed(1) + "Z",
            fill: colour
        }));

        return (dir > 0 ? "+" : "−") + km(sector.moved) + " km";
    }

    /* ---------------- Draw ---------------- */

    function draw(svg, turn, board, project, opts) {
        if (!svg || !turn || !board || !project) { return; }
        var resolutions = turn.sectors || [];
        if (!resolutions.length) { return; }
        seq++;

        var options = opts || {};
        var cities = options.cities || (window.tovGeo && window.tovGeo.cities) || [];

        // The hatch that will carry the dry notches once step 0 lands. Declared
        // now so the piece is complete the day the field appears.
        var defs = svgEl("defs", {});
        var pattern = svgEl("pattern", {
            id: "tov-dry-" + seq, width: "3", height: "3",
            patternUnits: "userSpaceOnUse", patternTransform: "rotate(45)"
        });
        pattern.appendChild(svgEl("line", {
            x1: "0", y1: "0", x2: "0", y2: "3",
            stroke: COLOUR.ink3, "stroke-width": "0.8", opacity: "0.5"
        }));
        defs.appendChild(pattern);
        svg.appendChild(defs);

        var towns = cities.map(function (c) {
            var xy = project(c.lon, c.lat);
            return { left: xy[0] - 6, right: xy[0] + 10 + c.name.length * 6.4, y: xy[1] };
        });

        // Anchors: each sector sits where its own line has been pushed to.
        var marks = [];
        board.forEach(function (s) {
            var res = resolutions.find(function (r) { return r.sectorCode === s.code; });
            if (!res) { return; }

            var anchor = project(s.lon + s.pushLon * res.hexesCumulative,
                                 s.lat + s.pushLat * res.hexesCumulative);

            // The axis of advance, in screen space. Everything else — which camp
            // stands on which side, where the arrow points — follows from it,
            // rather than from an assumption about east and west.
            var ahead = project(s.lon + s.pushLon, s.lat + s.pushLat);
            var dx = ahead[0] - anchor[0], dy = ahead[1] - anchor[1];
            var len = Math.sqrt(dx * dx + dy * dy) || 1;

            marks.push({
                sector: readSector(res, turn),
                ax: anchor[0], ay: anchor[1],
                ux: dx / len, uy: dy / len
            });
        });

        if (!marks.length) { return; }
        marks.sort(function (a, b) { return a.ay - b.ay; });

        // One column per camp, each de-collided on its own: a counter never lands
        // on its neighbour, whatever the line does.
        var top = CH / 2 + 8, bottom = MAP_H - CH / 2 - 8;
        var ys = spread(marks.map(function (m) { return m.ay; }), GAP, top, bottom);

        marks.forEach(function (m, i) {
            m.cy = ys[i];
            // The invader stands behind his own axis of advance, the defender ahead
            // of it — which is what "astride the line" means once it is oriented.
            m.invaderX = avoidCities(m.ax - m.ux * OFFSET - (m.ux < 0 ? 0 : CW), m.cy, towns, m.ux < 0 ? 1 : -1);
            m.defenderX = avoidCities(m.ax + m.ux * OFFSET - (m.ux < 0 ? CW : 0), m.cy, towns, m.ux < 0 ? -1 : 1);
        });

        // Leaders first, so nothing crosses over a counter.
        marks.forEach(function (m) {
            [[m.invaderX, m.sector.invader.colour], [m.defenderX, m.sector.defender.colour]].forEach(function (c) {
                var edge = c[0] + (c[0] < m.ax ? CW : 0);
                svg.appendChild(svgEl("line", {
                    x1: edge.toFixed(1), y1: m.cy.toFixed(1),
                    x2: m.ax.toFixed(1), y2: m.ay.toFixed(1),
                    stroke: c[1], "stroke-width": "0.8", opacity: "0.35"
                }));
            });
        });

        marks.forEach(function (m) {
            var still = Math.abs(m.sector.moved) <= 0.01;
            var figure = still
                ? butee(svg, m.ax, m.ay, m.ux, m.uy, m.sector)
                : arrow(svg, m.ax, m.ay, m.ux, m.uy, m.sector);

            // Facing: each counter turns its trench line towards the other.
            counter(svg, m.invaderX, m.cy - CH / 2, m.sector.invader, m.invaderX < m.ax ? 1 : -1);
            counter(svg, m.defenderX, m.cy - CH / 2, m.sector.defender, m.defenderX < m.ax ? 1 : -1);

            // The sector names itself once, on the line, never twice on the counters.
            var lx = m.ax, ly = m.ay - 16;
            svg.appendChild(halo(text(lx, ly, m.sector.name, {
                "text-anchor": "middle", "font-size": "9.5", "font-weight": "700", fill: COLOUR.ink
            })));
            if (figure) {
                svg.appendChild(halo(text(lx, m.ay + 22, figure, {
                    "text-anchor": "middle", "font-size": "9",
                    "font-family": "Georgia, 'Palatino Linotype', serif",
                    "font-variant-numeric": "tabular-nums",
                    fill: still ? COLOUR.ink3 : (m.sector.moved > 0 ? COLOUR.invader : COLOUR.defender)
                })));
            }
        });
    }

    return { draw: draw };
})();
