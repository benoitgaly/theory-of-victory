// Sector gauges. Owns its own file, like hexmap.js: the map calls
// window.tovCounters.draw() and nothing else.
//
//   draw(svg, turn, board, project, opts) -> void
//   - svg      the map's <svg>, already carrying ground, grid, rivers, cities
//   - turn     one TurnSnapshot: turn.sectors[], turn.invader, turn.defender
//   - board[]  lon/lat anchors and push vectors per sector
//   - project  lon,lat -> [x,y], the map's own padded projector
//   - opts     optional. opts.cities defaults to window.tovGeo.cities
//
// It also calls window.tovDepth.draw() first, before any of its own pieces, so the two
// deep-strike overflights pass UNDER the front rather than over it. That is not a detail of
// z-order, it is the argument the whole board is built on.
//
// ---------------------------------------------------------------------------
// ONE PIECE PER SECTOR, AND WHY THE COUNTERS WENT AWAY
//
// The first draft drew two counters of 46 by 34 per sector, each with its own vertical ladder,
// bottleneck glyph and figure, plus a resolution glyph and a label. Thirty-two objects on a
// two-hundred-pixel band of Donbas. It was unreadable, and worse, the one thing it existed to
// show — the ladder — was the first thing that disappeared at that size, while the infantry
// cross filled the counter and read as a strike-through.
//
// What replaced it is one horizontal gauge per sector, eight objects instead of thirty-two:
//
//     POKROVSK   ▭ ■■■□□  ◄|►  ■■□□
//
// Notches, not a bar: a wargamer counts steps, he does not measure a length, and a bar chart
// laid on a map would be the histogram this board exists to avoid. Each notch is a fixed
// quantity of men, filled where the flows supply them and hollow where they do not — Liebig
// read on the front, the barrel that would reach that far against the level it actually holds.
// The two sides grow outward from the line, so the longer run of filled notches is the
// stronger army and no reader has to compare two distant ladders.
//
// The centre carries the resolution: a butée, two chevrons that meet, when nothing moved —
// which is the model's normal result and must look deliberate — or an arrow pointing at
// whoever gave ground.
//
// Three things left the map on purpose:
//
//   - The per-sector figure. Repeating "2 000 hommes" eight times spends eight labels on a
//     number nobody compares. It lives in the tooltip now, with everything else.
//   - The bottleneck glyph. It was drawn sixteen times for two distinct values, because the
//     scarcest stave is a property of an ARMY, not of a sector. It moved to the rear card in
//     depth.js, where the side is named once.
//   - Fortification crenels. Second-order, and already inside the ratio the glyph shows.
//
// The NATO frame stays, small, and only on the side that is pushing — so it says who the
// attacker is, which nothing else on the piece does, and costs eight marks instead of sixteen.
//
// ---------------------------------------------------------------------------
// WHERE THE FIGURES COME FROM
//
// Step 0 is done: SectorResolution names the attacker outright and publishes, per side, the
// raw power committed to the sector and what that power would be with every stave full.
// Everything below reads those fields and recomputes none of them — the rule of the project is
// that JavaScript never recomputes a quantity of the model, and if a figure is missing the
// drawing degrades rather than inventing one.
//
// Two readings that are easy to confuse, and the class comment on SectorResolution says the
// same: AttackerPush against HolderResistance is what produced the ratio, and the resistance
// already carries terrain, urbanisation, fortification, drone friction and season.
// InvaderCommitted and DefenderCommitted are raw powers, comparable to each other and to
// nothing else. The notches draw the second pair, never the first.
//
// Still degraded, on purpose:
//
//   - Older payload   a page served before step 0 has no per-side power. The attacking side
//                     then falls back to AttackerPush and the holder's run is drawn as DASHED
//                     empty notches, which reads as "not published" and never as zero.
//   - Dry notches     shown only where the shortfall exceeds the thousand-man grain the engine
//                     rounds to. On a small sector a five per cent gap is fewer than a thousand
//                     men: real, not printable, and inventing it would be worse.
//
// UNIT — every man count arriving here is already in MEN, rounded to the thousand by the
// engine. Nothing in this file converts; men() groups the digits and stops there.
window.tovCounters = (function () {
    "use strict";

    /* ---------------- Display constants ----------------
       None of these is a rule. Changing any of them must change no engine output — they only
       decide how a published number is drawn. */

    // One notch, measured rather than guessed: the highest power any sector carries across the
    // three reference runs is 102 000 men-equivalents, and the rule is that the maximum fills
    // all but one of the notches, so nothing ever saturates.
    var STEP_MEN = 15000;
    var NOTCHES = 7;
    var ARMY_MEN = 60000;      // at or above, the pushing side is drawn as an army

    // The notch has to be readable without a lens: the gap between a filled step and a hollow
    // one IS the thesis, and at six pixels by nine it was not decipherable. Removing half the
    // objects on the map is what bought the room to make it eight by twelve.
    var NW = 8, NH = 12, NGAP = 2;           // notch box and spacing
    var RUN = NOTCHES * (NW + NGAP);         // one side's full run
    var CENTRE = 10;                         // half-width of the resolution glyph
    var FRAME_W = 15, FRAME_H = 11;          // the NATO mark on the pushing side
    var NOTE_CHAR = 4.9;                     // width of one glyph of the note, at 8,5 px serif
    var PITCH = 26;                          // minimum vertical distance between two gauges
    var MARGIN = 8;                          // nothing this file draws comes closer to the edge
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

    /* ---------------- Small helpers, same facture as hexmap.js ---------------- */

    // Null-valued attributes are dropped rather than stringified: several pieces below are
    // drawn with or without a dash pattern depending on what is published.
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

    // Paper painted behind the glyphs, so a label stays readable over the grid.
    function halo(node, colour) {
        node.setAttribute("stroke", colour || COLOUR.paper);
        node.setAttribute("stroke-width", "2.6");
        node.setAttribute("stroke-linejoin", "round");
        node.setAttribute("paint-order", "stroke");
        return node;
    }

    function tip(node, content) {
        var t = svgEl("title", {});
        t.textContent = content;
        node.appendChild(t);
        return node;
    }

    // Grouping only. Sector figures leave the engine in men, already rounded to the thousand,
    // so converting here a second time would multiply an army by a thousand.
    function men(value) {
        var s = String(Math.round(value));
        var out = "";
        for (var i = 0; i < s.length; i++) {
            if (i > 0 && (s.length - i) % 3 === 0) { out += " "; }
            out += s[i];
        }
        return out;
    }

    function km(hexes) {
        var v = Math.abs(hexes * 10);
        return v.toFixed(v < 10 ? 1 : 0).replace(".", ",");
    }

    /* ---------------- Reading the turn ---------------- */

    function sideOf(turn, code) {
        return code === "invader" ? turn.invader : turn.defender;
    }

    function readSector(res, turn) {
        var moved = res.hexesMoved || 0;

        var attacker = res.attackerSideCode || null;
        if (!attacker && Math.abs(moved) > 0.001) {
            attacker = moved > 0 ? "invader" : "defender";
        }

        function power(sideCode, published) {
            if (published !== undefined && published !== null) { return published; }
            // Fallback for a payload without step 0. The resolution pair is oriented on the
            // attacker and only his half is a raw power: the other is HolderResistance, with
            // terrain and fortification and season already baked in, and undoing it is
            // impossible.
            if (attacker === sideCode && res.attackerPush !== undefined) { return res.attackerPush; }
            return null;
        }

        function side(sideCode, committedKey, establishmentKey) {
            var snapshot = sideOf(turn, sideCode);
            return {
                code: sideCode,
                colour: sideCode === "invader" ? COLOUR.invader : COLOUR.defender,
                power: power(sideCode, res[committedKey]),
                establishment: res[establishmentKey] === undefined ? null : res[establishmentKey],
                losses: attacker === null
                    ? null
                    : (attacker === sideCode ? res.attackerLosses : res.defenderLosses),
                collapsed: !!(snapshot && snapshot.hasCollapsed)
            };
        }

        return {
            code: res.sectorCode,
            name: (res.sectorName || res.sectorCode || "").split(" — ")[0],
            ratio: res.ratio || 0,
            moved: moved,
            attacker: attacker,
            outcome: res.outcome || "",
            // No attribution is possible on a still sector, but the pair always sums to what
            // the quarter cost — and on a stalled sector that total is the only thing worth
            // saying out loud.
            cost: (res.attackerLosses || 0) + (res.defenderLosses || 0),
            invader: side("invader", "invaderCommitted", "invaderEstablishment"),
            defender: side("defender", "defenderCommitted", "defenderEstablishment")
        };
    }

    // True when the map has already named this place near this gauge. Accent- and
    // case-insensitive, because "Zaporijjia" the city and "Zaporijjia" the sector are the same
    // word and only one of them needs printing.
    function named(name, cx, cy, towns) {
        var wanted = fold(name);
        for (var i = 0; i < towns.length; i++) {
            if (fold(towns[i].name) !== wanted) { continue; }
            if (Math.abs(towns[i].x - cx) < 120 && Math.abs(towns[i].y - cy) < 40) { return true; }
        }
        return false;
    }

    function fold(value) {
        return value.normalize
            ? value.normalize("NFD").replace(/[̀-ͯ]/g, "").toLowerCase()
            : value.toLowerCase();
    }

    // Above the gauge by preference, below it when a city label is already sitting there.
    // Cities were on this map first and they are what gives it scale, so they never lose.
    function nameY(name, cx, cy, towns) {
        var half = name.length * 2.7 + 3;

        function free(y) {
            for (var i = 0; i < towns.length; i++) {
                var t = towns[i];
                if (Math.abs(t.y - y) > 9) { continue; }
                if (cx - half < t.right && cx + half > t.x - 6) { return false; }
            }
            return true;
        }

        if (free(cy - 12)) { return cy - 12; }
        if (free(cy + 18)) { return cy + 18; }
        return null;
    }

    function tooltip(sector) {
        var lines = [sector.name.toUpperCase(), sector.outcome];

        [sector.invader, sector.defender].forEach(function (side) {
            var label = side.code === "invader" ? "Russie" : "Ukraine";
            if (side.power === null) {
                lines.push(label + " : puissance non publiée");
                return;
            }
            var line = label + " : " + men(side.power) + " hommes-équivalents engagés";
            if (side.establishment !== null && side.establishment > side.power) {
                line += ", " + men(side.establishment) + " si les flux suivaient";
            }
            lines.push(line);
        });

        lines.push("Rapport de force : " + sector.ratio.toFixed(2).replace(".", ","));
        if (sector.cost > 0) {
            lines.push("Coût du trimestre : " + men(sector.cost) + " hommes, les deux camps réunis.");
        }
        return lines.join("\n");
    }

    /* ---------------- Placement ----------------
       The gauge is fourteen pixels tall against the counters' thirty-four, so eight of them fit
       on the front without stretching: each one sits on its own anchor and the column no longer
       drags its southern half eighty pixels away. The de-collision below almost never binds,
       and that is the point of the smaller piece. */

    function spread(wanted, gap, top, bottom) {
        var out = wanted.slice(), i, j;
        for (i = 0; i < out.length; i++) {
            var floor = i === 0 ? top : out[i - 1] + gap;
            if (out[i] < floor) { out[i] = floor; }
        }
        for (j = out.length - 1; j >= 0; j--) {
            var ceiling = j === out.length - 1 ? bottom : out[j + 1] - gap;
            if (out[j] > ceiling) { out[j] = ceiling; }
        }

        var drift = 0;
        for (i = 0; i < out.length; i++) { drift += out[i] - wanted[i]; }
        drift /= out.length || 1;
        if (Math.abs(drift) > 0.5 && out[0] - drift >= top && out[out.length - 1] - drift <= bottom) {
            for (i = 0; i < out.length; i++) { out[i] -= drift; }
        }
        return out;
    }

    /* ---------------- The notch run ----------------
       Filled outward from the line: the reader counts steps away from the contact, which is the
       direction the army stands in. Hollow notches continue the run where the establishment
       stands above what the flows actually supplied. */

    // How many notch slots this side actually occupies. A weak sector draws a short run and a
    // strong one a long one, so the piece is as long as its data rather than padded out to the
    // maximum — which is also what keeps a label from floating half a gauge away from its own
    // notches, and what keeps the whole piece off the city names it does not need to cover.
    function notchCount(side) {
        if (side.power === null || side.power === undefined) { return NOTCHES; }
        var filled = Math.min(NOTCHES, side.power / STEP_MEN);
        var total = side.establishment !== null && side.establishment !== undefined
            ? Math.min(NOTCHES, Math.max(filled, side.establishment / STEP_MEN))
            : filled;
        return Math.max(1, Math.ceil(total));
    }

    function run(g, cx, cy, direction, side) {
        var known = side.power !== null && side.power !== undefined;
        var filled = known ? Math.min(NOTCHES, side.power / STEP_MEN) : 0;
        var total = known && side.establishment !== null && side.establishment !== undefined
            ? Math.min(NOTCHES, Math.max(filled, side.establishment / STEP_MEN))
            : filled;

        for (var i = 0; i < NOTCHES; i++) {
            var x = cx + direction * (CENTRE + i * (NW + NGAP)) - (direction < 0 ? NW : 0);
            var full = Math.min(1, Math.max(0, filled - i));
            var dry = Math.min(1, Math.max(0, total - i));

            if (!known) {
                // Not published, and never to be read as zero.
                g.appendChild(svgEl("rect", {
                    x: x, y: cy - NH / 2, width: NW, height: NH,
                    fill: "none", stroke: COLOUR.ink3, "stroke-width": "0.5",
                    "stroke-dasharray": "1 1.4"
                }));
                continue;
            }

            if (dry <= 0.02) { break; }

            // The hollow part: men present, and not supplied. It carries a faint wash as well
            // as an outline, so an empty step reads as a step that exists and is empty — and
            // never as nothing at all, which is what the eye does with a bare rule.
            g.appendChild(svgEl("rect", {
                x: x, y: cy - NH / 2, width: NW * dry, height: NH,
                fill: side.colour, "fill-opacity": "0.11",
                stroke: side.colour, "stroke-width": "0.9", opacity: "0.7"
            }));

            if (full > 0.02) {
                g.appendChild(svgEl("rect", {
                    x: x, y: cy - NH / 2, width: NW * full, height: NH,
                    fill: side.colour, opacity: "0.88"
                }));
            }
        }

        // A broken army is struck through, and it must not look like any other quarter: this is
        // the turn the whole game exists to reach.
        if (side.collapsed) {
            var end = cx + direction * (CENTRE + notchCount(side) * (NW + NGAP));
            g.appendChild(svgEl("line", {
                x1: cx + direction * CENTRE, y1: cy - NH / 2 - 3,
                x2: end, y2: cy + NH / 2 + 3,
                stroke: side.colour, "stroke-width": "1.4", opacity: "0.85"
            }));
        }
    }

    /* ---------------- The resolution glyph ---------------- */

    /* A front at rest is not a front that holds, and the two must not be drawn alike.
       A butée is two armies pushing and bleeding for nothing; a quiet line is a sector where
       nothing is being spent at all — the autumn of 2021, when the line is the one from 2014,
       nobody attacks, and Russia is doing nothing but massing behind it.

       The discriminator is losses, not the ratio. Measured over the three reference runs, still
       sectors carry ratios from 0,15 to 1,11 with a median at 0,55, and the opening quarter of
       the invasion sits at 0,49 to 0,97 — squarely inside that range. No threshold on the ratio
       separates the two, and one would have reclassified between a fifth and four fifths of
       ordinary frozen sectors depending on where it was put. A quarter that cost nobody a
       thousand men, on the other hand, is unambiguous. */
    function quiet(g, cx, cy) {
        g.appendChild(svgEl("line", {
            x1: cx, y1: cy - 6, x2: cx, y2: cy + 6,
            stroke: COLOUR.ink3, "stroke-width": "1.2", opacity: "0.75"
        }));
    }

    // Below 1,1 the line holds. Two chevrons that meet and a stop bar between them — the frozen
    // front is the normal result of this model, not an absence, and it is drawn as an event.
    function butee(g, cx, cy) {
        [1, -1].forEach(function (dir) {
            g.appendChild(svgEl("path", {
                d: "M" + (cx + dir * 7.5) + " " + (cy - 6.5) +
                   "L" + (cx + dir * 2.5) + " " + cy +
                   "L" + (cx + dir * 7.5) + " " + (cy + 6.5),
                fill: "none", stroke: COLOUR.ink, "stroke-width": "1.5",
                "stroke-linecap": "round", "stroke-linejoin": "round", opacity: "0.85"
            }));
        });
        g.appendChild(svgEl("line", {
            x1: cx, y1: cy - 7, x2: cx, y2: cy + 7,
            stroke: COLOUR.ink, "stroke-width": "1", opacity: "0.5"
        }));
    }

    // Above 1,1 the line gave. The arrow points at whoever gave ground; its weight is the band
    // of the resolution table, never the distance, which is five pixels at this scale.
    function arrow(g, cx, cy, sector) {
        var dir = sector.moved > 0 ? -1 : 1;   // the invader advances west, so leftward
        var colour = sector.moved > 0 ? COLOUR.invader : COLOUR.defender;
        var broken = sector.invader.collapsed || sector.defender.collapsed;
        var weight = sector.ratio >= 3 ? 3 : (sector.ratio >= 2 ? 2.3 : 1.6);
        var reach = broken ? CENTRE + 8 : CENTRE - 1;

        g.appendChild(svgEl("line", {
            x1: cx - dir * (CENTRE - 1), y1: cy, x2: cx + dir * reach, y2: cy,
            stroke: colour, "stroke-width": weight, "stroke-linecap": "round"
        }));
        g.appendChild(svgEl("path", {
            d: "M" + (cx + dir * reach) + " " + cy +
               "L" + (cx + dir * (reach - 7)) + " " + (cy - 4.8) +
               "L" + (cx + dir * (reach - 7)) + " " + (cy + 4.8) + "Z",
            fill: colour
        }));
    }

    /* ---------------- The pushing side's mark ----------------
       Infantry, because the engine's own unit of power is the fully supplied infantryman, and
       the piece should say the unit it is drawn in. Only on the side that is pushing: it names
       the attacker, which nothing else on the gauge does. */

    function mark(g, x, cy, side) {
        var y = cy - FRAME_H / 2;
        g.appendChild(svgEl("rect", {
            x: x, y: y, width: FRAME_W, height: FRAME_H,
            fill: COLOUR.card, stroke: side.colour, "stroke-width": "0.9"
        }));
        g.appendChild(svgEl("path", {
            d: "M" + x + " " + y + "L" + (x + FRAME_W) + " " + (y + FRAME_H) +
               "M" + (x + FRAME_W) + " " + y + "L" + x + " " + (y + FRAME_H),
            stroke: COLOUR.ink2, "stroke-width": "0.6", fill: "none", opacity: "0.7"
        }));
        g.appendChild(text(x + FRAME_W / 2, y - 1.5,
            side.power >= ARMY_MEN ? "XXXX" : "XXX", {
                "text-anchor": "middle", "font-size": "4.6", "font-weight": "700",
                "letter-spacing": "0.4", fill: COLOUR.ink2
            }));
    }

    /* ---------------- Draw ---------------- */

    function draw(svg, turn, board, project, opts) {
        if (!svg || !turn || !board || !project) { return; }
        var options = opts || {};

        // First, and deliberately: the rear cards and their overflights go down before the
        // front does, so a deep strike never reads as an arrow on the line of contact. It owns
        // its own file and is optional — the map still draws without it.
        if (window.tovDepth && typeof window.tovDepth.draw === "function" && options.depth !== false) {
            window.tovDepth.draw(svg, turn, project, options);
        }

        var resolutions = turn.sectors || [];
        if (!resolutions.length) { return; }

        var cities = options.cities || (window.tovGeo && window.tovGeo.cities) || [];
        // The label box, not the dot: a city's name runs to the right of its mark, and it is
        // the name that the gauge would bury. Same estimate hexmap.js uses for its own labels.
        var towns = cities.map(function (c) {
            var xy = project(c.lon, c.lat);
            return { name: c.name, x: xy[0], y: xy[1], right: xy[0] + 10 + c.name.length * 6.4 };
        });

        var marks = [];
        board.forEach(function (s) {
            var res = resolutions.find(function (r) { return r.sectorCode === s.code; });
            if (!res) { return; }

            var lon = s.lon + s.pushLon * res.hexesCumulative;
            var lat = s.lat + s.pushLat * res.hexesCumulative;
            var anchor = project(lon, lat);

            marks.push({ sector: readSector(res, turn), ax: anchor[0], ay: anchor[1] });
        });

        if (!marks.length) { return; }
        marks.sort(function (a, b) { return a.ay - b.ay; });

        var ys = spread(marks.map(function (m) { return m.ay; }), PITCH, 20, MAP_H - 20);

        // One figure on the map, not eight: the quarter's dearest sector says what it cost, the
        // way the rest of this board gives a screen a single hero number.
        var dearest = null;
        marks.forEach(function (m) {
            if (!dearest || m.sector.cost > dearest.sector.cost) { dearest = m; }
        });

        marks.forEach(function (m, index) {
            var sector = m.sector;
            var cy = ys[index];

            // Fold back inside the frame, reserving what this particular gauge actually draws
            // on each flank — its own notches plus the pushing side's mark. The note that still
            // would not fit changes flank rather than being clipped.
            var reserveEast = CENTRE + notchCount(sector.invader) * (NW + NGAP)
                + (sector.attacker === "invader" ? FRAME_W + 3 : 0);
            var reserveWest = CENTRE + notchCount(sector.defender) * (NW + NGAP)
                + (sector.attacker === "defender" ? FRAME_W + 3 : 0);
            var cx = Math.max(MARGIN + reserveWest, Math.min(MAP_W - MARGIN - reserveEast, m.ax));
            var g = svgEl("g", {});

            // A leader back to the true position whenever the piece had to be nudged.
            if (Math.abs(cy - m.ay) > 3 || Math.abs(cx - m.ax) > 3) {
                g.appendChild(svgEl("line", {
                    x1: cx, y1: cy, x2: m.ax, y2: m.ay,
                    stroke: COLOUR.ink3, "stroke-width": "0.6", opacity: "0.35"
                }));
            }

            // The invader stands east of the line, the defender west of it.
            run(g, cx, cy, 1, sector.invader);
            run(g, cx, cy, -1, sector.defender);

            if (Math.abs(sector.moved) > 0.01) {
                arrow(g, cx, cy, sector);
            } else if (sector.cost > 0) {
                butee(g, cx, cy);
            } else {
                quiet(g, cx, cy);
            }

            // Where the piece actually ends on each flank, notches drawn and mark included.
            var east = cx + CENTRE + notchCount(sector.invader) * (NW + NGAP);
            var west = cx - CENTRE - notchCount(sector.defender) * (NW + NGAP);

            var attacking = sector.attacker === "invader" ? sector.invader
                : (sector.attacker === "defender" ? sector.defender : null);
            if (attacking && attacking.power !== null && attacking.power !== undefined) {
                if (sector.attacker === "invader") {
                    mark(g, east + 3, cy, attacking);
                    east += 3 + FRAME_W;
                } else {
                    mark(g, west - 3 - FRAME_W, cy, attacking);
                    west -= 3 + FRAME_W;
                }
            }

            // The name rides above its own gauge, in small capitals so it never reads as one of
            // the map's city labels — and it is left out entirely where the map already names
            // the place. Half these sectors are named after the town they are fought over, and
            // printing "KHERSON" beside Kherson was both a duplicate and the thing that buried
            // the city underneath it.
            var label = sector.name.toUpperCase();
            var ly = named(sector.name, cx, cy, towns) ? null : nameY(label, cx, cy, towns);
            if (ly !== null) {
                g.appendChild(halo(text(cx, ly, label, {
                    "text-anchor": "middle", "font-size": "7.5", "font-weight": "700",
                    "letter-spacing": "0.09em", fill: COLOUR.ink2
                })));
            }

            // Only what a reader would actually compare: the ground that changed hands, and the
            // single dearest sector of the quarter.
            var note = null, noteColour = COLOUR.ink3;
            if (Math.abs(sector.moved) > 0.01) {
                note = (sector.moved > 0 ? "+" : "−") + km(sector.moved) + " km";
                noteColour = sector.moved > 0 ? COLOUR.invader : COLOUR.defender;
            } else if (m === dearest && sector.cost > 0) {
                note = "− " + men(sector.cost) + " hommes";
            }

            if (note) {
                // Against the piece's own edge, not against the widest a piece could ever be —
                // a figure floating half a gauge away from its notches has no owner. East by
                // default, west when this particular string would not clear the frame there,
                // and clamped outright if neither flank has the room: a figure cut by the
                // border is the one thing worse than a figure on the wrong side.
                var width = note.length * NOTE_CHAR;
                var onEast = east + 8 + width <= MAP_W - MARGIN;
                var nx = onEast
                    ? Math.min(east + 8, MAP_W - MARGIN - width)
                    : Math.max(west - 8, MARGIN + width);
                g.appendChild(halo(text(nx, cy + 3, note, {
                    "text-anchor": onEast ? "start" : "end",
                    "font-size": "8.5", "font-family": "Georgia, 'Palatino Linotype', serif",
                    "font-variant-numeric": "tabular-nums", fill: noteColour
                })));
            }

            tip(g, tooltip(sector));
            svg.appendChild(g);
        });
    }

    return { draw: draw };
})();
