// Front map. Owns its own file: board.js calls window.tovHexMap.render() and nothing else.
// render(turn, board, geo, opts) -> SVGElement
// - turn.sectors[] carries hexesCumulative and hexesMoved per sector
// - board[] carries lon/lat anchors and push vectors per sector
// - geo exposes the outline, the rivers, the cities, the twenty zones and a projector
// - opts.frontLine(turn) returns the simulated contact line as [[lon,lat], ...]
// - opts.turnIndex says which turn of the run is being drawn
//
// The country is paved with hexagons. Each one is held, occupied, taken since February 2022,
// retaken, or crossed by the line. A hexagon here is a reading unit — about 40 km across,
// four of the ten-kilometre hexes the engine actually moves, and roughly 1 400 km².
//
// ---------------------------------------------------------------------------
// WHO DECIDES WHERE THE LINE IS
//
// Two authorities, never blended, and the map names the one in force at the bottom left.
//
// While frontline.js reports a DOCUMENTED quarter, the ground is read from the chronicle: twenty
// sourced quarters, twenty named zones, the whole country and not just the eight modelled
// sectors. That is the only way the column of Kyiv arrives and leaves, Mariupol falls, the right
// bank of Kherson comes back, Kharkiv breaks through and Kursk appears — none of which happens on
// a board of eight Donbass sectors.
//
// Past the chronicle, or as soon as an army breaks in a run that history never broke, the model
// governs and the contact line is DASHED. A dashed front is the wargamer's own mark for a
// position nobody has confirmed, and it costs no legend.
//
// The line itself is no longer the eight-point polyline. It is the boundary of the hexagons the
// invader controls, minus the edges that are the state border — so it follows the ground the map
// actually paints instead of running beside it.
window.tovHexMap = (function () {
    "use strict";

    var HEX_KM = 40;        // across the flats
    var W = 900, H = 520, PAD = 10;
    var SEASON_FR = { Winter: "l'hiver", Spring: "le printemps", Summer: "l'été", Autumn: "l'automne" };

    var COLOUR = {
        foreign: "#eae5d9",
        water: "#e8eef2",
        land: "#f7f4ec",
        grid: "#cec5b2",
        outline: "#8b93a1",
        ink: "#17191e",
        ru: "#a8322a",
        ua: "#1e5fa8",
        river: "#8fb4cc"
    };

    var seq = 0;

    function svgEl(tag, attrs) {
        var n = document.createElementNS("http://www.w3.org/2000/svg", tag);
        Object.keys(attrs || {}).forEach(function (k) { n.setAttribute(k, attrs[k]); });
        return n;
    }

    function text(x, y, content, attrs) {
        var n = svgEl("text", attrs || {});
        n.setAttribute("x", x);
        n.setAttribute("y", y);
        n.textContent = content;
        return n;
    }

    // A label that stays readable over the grid: the paper colour is painted behind the glyphs.
    function halo(node, colour) {
        node.setAttribute("stroke", colour || COLOUR.land);
        node.setAttribute("stroke-width", "3");
        node.setAttribute("stroke-linejoin", "round");
        node.setAttribute("paint-order", "stroke");
        return node;
    }

    /* ---------------- Which side of the line ---------------- */

    // The contact line runs north to south, so it is a function of latitude. Extended past
    // both ends — north into Russia, south round the west of Crimea — every point on the
    // map falls cleanly on one side or the other.
    function classifier(linePoints) {
        var pts = linePoints.slice().sort(function (a, b) { return b[1] - a[1]; });
        var south = pts[pts.length - 1];
        var ext = [[pts[0][0], 60]]
            .concat(pts)
            .concat([
                [south[0] - 0.70, south[1] - 0.35],
                [south[0] - 1.50, south[1] - 0.75],
                [south[0] - 2.20, south[1] - 1.90]
            ]);

        function lonAt(lat) {
            if (lat >= ext[0][1]) { return ext[0][0]; }
            if (lat <= ext[ext.length - 1][1]) { return ext[ext.length - 1][0]; }
            for (var i = 0; i < ext.length - 1; i++) {
                var a = ext[i], b = ext[i + 1];
                if (lat <= a[1] && lat >= b[1]) {
                    var dy = a[1] - b[1];
                    if (dy < 1e-9) { return Math.max(a[0], b[0]); }
                    return a[0] + (b[0] - a[0]) * (a[1] - lat) / dy;
                }
            }
            return ext[ext.length - 1][0];
        }

        // Positive east of the line — the occupied side.
        return function (lon, lat) { return lon - lonAt(lat); };
    }

    /* ---------------- The grid ---------------- */

    // The tiling never changes, only who holds it. Built once, reused every turn.
    var gridCache = null;

    function buildGrid(geo, p, R) {
        if (gridCache && gridCache.geo === geo && Math.abs(gridCache.R - R) < 0.01) {
            return gridCache.cells;
        }

        var w = R * Math.sqrt(3);       // column spacing, pointy-top
        var rowH = R * 1.5;

        var minX = Infinity, maxX = -Infinity, minY = Infinity, maxY = -Infinity;
        // The Kursk salient is the one piece of the board outside Ukraine, so the sweep has to
        // cover it: the grid stops at the outline everywhere else.
        geo.ukraine.concat(geo.kurskSalient || []).forEach(function (q) {
            var xy = p(q[0], q[1]);
            minX = Math.min(minX, xy[0]); maxX = Math.max(maxX, xy[0]);
            minY = Math.min(minY, xy[1]); maxY = Math.max(maxY, xy[1]);
        });

        var salient = geo.kurskSalient || null;
        var cells = [];
        var rows = Math.ceil((maxY - minY) / rowH) + 2;
        var cols = Math.ceil((maxX - minX) / w) + 2;

        for (var r = -1; r < rows; r++) {
            var cy = minY + r * rowH;
            var shift = (r & 1) ? w / 2 : 0;
            for (var c = -1; c < cols; c++) {
                var cx = minX + c * w + shift;

                var d = "", corners = [], pts = [], inside = [], vertsIn = 0;
                for (var k = 0; k < 6; k++) {
                    var a = k * Math.PI / 3;
                    var vx = cx + R * Math.sin(a), vy = cy - R * Math.cos(a);
                    d += (k === 0 ? "M" : "L") + vx.toFixed(1) + " " + vy.toFixed(1);
                    pts.push([vx, vy]);
                    var g = p.invert(vx, vy);
                    corners.push(g);
                    if (geo.contains(geo.ukraine, g[0], g[1])) { inside.push(g); vertsIn++; }
                }

                var centre = p.invert(cx, cy);
                var centreIn = geo.contains(geo.ukraine, centre[0], centre[1]);
                var inSalient = !centreIn && salient && geo.contains(salient, centre[0], centre[1]);

                // Kept if the centre is on Ukrainian soil, or if the hex still covers a
                // decent piece of it — the clip mask trims whatever spills over the border —
                // or if it sits in the salient, where the grid is deliberately extended.
                if (!centreIn && !inSalient && vertsIn < 3) { continue; }

                // The point that speaks for the hexagon. On a border hexagon the centre can be
                // in a neighbouring country, where no zone answers: the Ukrainian vertices do.
                var sample = centre;
                if (!centreIn && !inSalient && inside.length) {
                    var sx = 0, sy = 0;
                    inside.forEach(function (g2) { sx += g2[0]; sy += g2[1]; });
                    sample = [sx / inside.length, sy / inside.length];
                }

                // Seven samples for the share of the hexagon that is actually land inside the
                // country: enough to keep a coastal or border hexagon from being counted whole.
                var land = inSalient ? 1 : (vertsIn + (centreIn ? 1 : 0)) / 7;

                cells.push({
                    centre: centre, sample: sample, corners: corners, pts: pts,
                    foreign: !centreIn, land: land, d: d + "Z"
                });
            }
        }

        gridCache = { geo: geo, R: R, cells: cells };
        return cells;
    }

    /* ---------------- The line, drawn from the hexagons themselves ----------------

       Every edge is keyed by its two endpoints, so the two hexagons that share it meet on the
       same key. An edge with a single owner is the outline of the board — the state border, the
       coast — and is never a front, whatever is on the other side of it. An edge with two owners
       is a front exactly when they disagree. */

    function edgeKey(a, b) {
        var one = a[0].toFixed(1) + "," + a[1].toFixed(1);
        var two = b[0].toFixed(1) + "," + b[1].toFixed(1);
        return one < two ? one + "|" + two : two + "|" + one;
    }

    function boundary(cells, held, includeOuter) {
        var edges = {};
        cells.forEach(function (cell, index) {
            for (var k = 0; k < 6; k++) {
                var a = cell.pts[k], b = cell.pts[(k + 1) % 6];
                var key = edgeKey(a, b);
                if (!edges[key]) { edges[key] = { a: a, b: b, own: [] }; }
                edges[key].own.push(index);
            }
        });

        var d = "";
        Object.keys(edges).forEach(function (key) {
            var e = edges[key];
            var draw;
            if (e.own.length === 1) {
                draw = includeOuter && held(e.own[0]);
            } else {
                draw = held(e.own[0]) !== held(e.own[1]);
            }
            if (!draw) { return; }
            d += "M" + e.a[0].toFixed(1) + " " + e.a[1].toFixed(1) +
                 "L" + e.b[0].toFixed(1) + " " + e.b[1].toFixed(1);
        });
        return d;
    }

    // Changing hands outranks being crossed: a hexagon that flipped since February 2022 is
    // the story, and the line will have moved on next turn anyway.
    function classify(cells, eastNow, eastStart) {
        return cells.map(function (cell) {
            var east = 0;
            for (var v = 0; v < 6; v++) {
                if (eastNow(cell.corners[v][0], cell.corners[v][1]) > 0) { east++; }
            }
            var now = eastNow(cell.centre[0], cell.centre[1]) > 0;
            var start = eastStart(cell.centre[0], cell.centre[1]) > 0;

            var kind;
            if (now !== start) {
                kind = now ? "gained" : "retaken";
            } else if (east > 0 && east < 6) {
                kind = "front";
            } else {
                kind = now ? "occupied" : "held";
            }
            return { kind: kind, d: cell.d, land: cell.land };
        });
    }

    // The same five states, read from the chronicle instead of from a line: each hexagon asks
    // frontline.js who controls the point that speaks for it, and whether that changed this
    // quarter.
    function classifyHistory(cells, index) {
        return cells.map(function (cell) {
            var kind = window.tovFront.kindAt(cell.sample[0], cell.sample[1], index);
            return {
                kind: kind || "held",
                change: window.tovFront.changeAt(cell.sample[0], cell.sample[1], index),
                d: cell.d,
                land: cell.land
            };
        });
    }

    // What the quarter's map is worth in square kilometres, measured on the drawing itself
    // rather than taken from a counter that measures something else. Coastal and border
    // hexagons count for the share of them that is land.
    function measure(hexes, hexKm2) {
        var out = { occupied: 0, contested: 0, incursion: 0 };
        hexes.forEach(function (h) {
            var area = hexKm2 * h.land;
            if (h.kind === "occupied" || h.kind === "gained") { out.occupied += area; }
            else if (h.kind === "front") { out.contested += area; }
            else if (h.kind === "incursion") { out.incursion += area; }
        });
        return out;
    }

    /* ---------------- Sector callouts ---------------- */

    function callouts(svg, turn, board, geo, p) {
        var marks = [];
        board.forEach(function (s) {
            var res = (turn.sectors || []).find(function (r) { return r.sectorCode === s.code; });
            if (!res) { return; }
            var lon = s.lon + s.pushLon * res.hexesCumulative;
            var lat = s.lat + s.pushLat * res.hexesCumulative;
            var xy = p(lon, lat);
            marks.push({
                x: xy[0], y: xy[1],
                moved: Math.abs(res.hexesMoved) > 0.01,
                gain: res.hexesMoved > 0,
                km: Math.abs(res.hexesMoved * 10),
                name: s.name.split(" — ")[0]
            });
        });

        // Quiet sectors: a small open dot, nothing more.
        marks.filter(function (m) { return !m.moved; }).forEach(function (m) {
            svg.appendChild(svgEl("circle", {
                cx: m.x, cy: m.y, r: 3.2, fill: "#fff",
                stroke: "#5c6470", "stroke-width": "1.3", opacity: "0.9"
            }));
        });

        var active = marks.filter(function (m) { return m.moved; })
            .sort(function (a, b) { return a.y - b.y; });

        if (!active.length) { return; }

        // Cities own their names: a callout that would land on one is pushed west of it.
        var townMarks = geo.cities.map(function (c) {
            var xy = p(c.lon, c.lat);
            return { x: xy[0], y: xy[1], right: xy[0] + 10 + c.name.length * 6.4 };
        });

        var lastY = -1e9;
        active.forEach(function (m) {
            m.labelY = Math.max(m.y, lastY + 16);
            lastY = m.labelY;
            m.width = m.name.length * 6.2 + 60;

            // West of the line: that is where the paper is empty.
            m.lx = m.x - 26 - m.width;
            for (var pass = 0; pass < 2; pass++) {
                townMarks.forEach(function (t) {
                    var clash = Math.abs(t.y - m.labelY) < 15 && m.lx + m.width > t.x - 7 && m.lx < t.right;
                    if (clash) { m.lx = Math.min(m.lx, t.x - 9 - m.width); }
                });
            }
            m.lx = Math.max(m.lx, 8);
        });

        active.forEach(function (m) {
            var colour = m.gain ? COLOUR.ru : COLOUR.ua;
            var edge = m.lx + m.width;

            svg.appendChild(svgEl("line", {
                x1: m.x, y1: m.y, x2: edge, y2: m.labelY,
                stroke: colour, "stroke-width": "0.9", opacity: "0.45"
            }));

            svg.appendChild(svgEl("rect", {
                x: m.lx, y: m.labelY - 8, width: m.width, height: 16, rx: 8,
                fill: "#fffdf8", stroke: colour, "stroke-width": "0.9", opacity: "0.96"
            }));

            svg.appendChild(text(m.lx + 9, m.labelY + 3.5, m.name, {
                "font-size": "9.5", "font-weight": "700", fill: COLOUR.ink
            }));
            svg.appendChild(text(edge - 9, m.labelY + 3.5,
                (m.gain ? "+" : "−") + m.km.toFixed(m.km < 10 ? 1 : 0).replace(".", ",") + " km", {
                    "text-anchor": "end", "font-size": "9.5", "font-weight": "700", fill: colour
                }));

            svg.appendChild(svgEl("circle", {
                cx: m.x, cy: m.y, r: 4.6, fill: colour, stroke: "#fffdf8", "stroke-width": "1.6"
            }));
        });
    }

    /* ---------------- Render ---------------- */

    function render(turn, board, geo, opts) {
        seq++;
        var svg = svgEl("svg", { viewBox: "0 0 " + W + " " + H });
        var project = geo.projector(W - 2 * PAD, H - 2 * PAD);
        var p = function (lon, lat) {
            var xy = project(lon, lat);
            return [xy[0] + PAD, xy[1] + PAD];
        };
        p.invert = function (x, y) { return project.invert(x - PAD, y - PAD); };

        var defs = svgEl("defs", {});
        var clipId = "tov-ua-clip-" + seq;
        var clip = svgEl("clipPath", { id: clipId });
        clip.appendChild(svgEl("path", { d: geo.path(geo.ukraine, p, true) }));
        defs.appendChild(clip);

        // A mask of its own for the ground beyond the border. Two masks and not one union: with
        // a union, a hexagon straddling the frontier would show on both sides of it and the
        // pocket would read as a smear rather than as a salient resting on the border.
        var salientId = "tov-kursk-clip-" + seq;
        if (geo.kurskSalient) {
            var salientClip = svgEl("clipPath", { id: salientId });
            salientClip.appendChild(svgEl("path", { d: geo.path(geo.kurskSalient, p, true) }));
            defs.appendChild(salientClip);
        }

        [["ru", COLOUR.ru, 45], ["ua", COLOUR.ua, -45]].forEach(function (h) {
            var pat = svgEl("pattern", {
                id: "tov-hatch-" + h[0] + "-" + seq,
                width: "5", height: "5",
                patternUnits: "userSpaceOnUse",
                patternTransform: "rotate(" + h[2] + ")"
            });
            pat.appendChild(svgEl("line", {
                x1: "0", y1: "0", x2: "0", y2: "5",
                stroke: h[1], "stroke-width": "1.5", opacity: "0.34"
            }));
            defs.appendChild(pat);
        });
        svg.appendChild(defs);

        // Ground: neighbours, then water, then Ukrainian soil.
        svg.appendChild(svgEl("rect", { x: 0, y: 0, width: W, height: H, fill: COLOUR.foreign }));
        svg.appendChild(svgEl("path", {
            d: geo.path(geo.blackSea, p, true),
            fill: COLOUR.water, stroke: "#d3dee6", "stroke-width": "1"
        }));

        (geo.neighbours || []).forEach(function (n) {
            var xy = p(n.lon, n.lat);
            svg.appendChild(text(xy[0], xy[1], n.name.toUpperCase(), {
                "text-anchor": "middle", "font-size": "9", "letter-spacing": "1.6",
                fill: "#9c9484", "font-weight": "600"
            }));
        });
        (geo.seas || []).forEach(function (s) {
            var xy = p(s.lon, s.lat);
            svg.appendChild(text(xy[0], xy[1], s.name, {
                "text-anchor": "middle", "font-size": "10", "font-style": "italic",
                fill: "#93a8b6", "letter-spacing": "0.6"
            }));
        });

        svg.appendChild(svgEl("path", { d: geo.path(geo.ukraine, p, true), fill: COLOUR.land }));

        // The grid.
        var line = opts.frontLine(turn);
        var initial = [geo.northAnchor]
            .concat(board.map(function (s) { return [s.lon, s.lat]; }))
            .concat([geo.southAnchor]);

        var R = (HEX_KM / project.kmPerPixel) / Math.sqrt(3);
        var hexKm2 = 1.5 * Math.sqrt(3) * Math.pow(R * project.kmPerPixel, 2);
        var cells = buildGrid(geo, p, R);

        var index = opts.turnIndex === undefined ? -1 : opts.turnIndex;
        var history = window.tovFront && window.tovFront.available && index >= 0;
        var regime = history ? window.tovFront.regimeOf(index) : "model";
        var hexes = history
            ? classifyHistory(cells, index)
            : classify(cells, classifier(line), classifier(initial));

        var byKind = {
            held: "", occupied: "", gained: "", retaken: "", front: "",
            foreign: "", incursion: "", incursionFront: ""
        };
        hexes.forEach(function (h) { byKind[h.kind] += h.d; });

        var grid = svgEl("g", { "clip-path": "url(#" + clipId + ")" });
        var beyond = svgEl("g", { "clip-path": "url(#" + salientId + ")" });
        [
            // Russian soil, drawn faintly and never filled: the salient has to be on the board
            // before it is taken, or its capture would read as new land appearing.
            { kind: "foreign", fill: "none", stroke: "rgba(140,132,116,0.40)", host: beyond },
            { kind: "held", fill: "none", stroke: COLOUR.grid, host: grid },
            { kind: "occupied", fill: "rgba(168,50,42,0.26)", stroke: "rgba(125,32,25,0.30)", host: grid },
            { kind: "gained", fill: "rgba(168,50,42,0.10)", stroke: "rgba(125,32,25,0.26)", host: grid },
            { kind: "retaken", fill: "rgba(30,95,168,0.10)", stroke: "rgba(23,71,126,0.26)", host: grid },
            { kind: "front", fill: "rgba(168,50,42,0.13)", stroke: COLOUR.grid, host: grid },
            // The mirror image of "occupied", and the only one of its kind on the map: ground
            // the defender holds in the invader's own country.
            { kind: "incursion", fill: "rgba(30,95,168,0.30)", stroke: "rgba(23,71,126,0.34)", host: beyond },
            { kind: "incursionFront", fill: "rgba(30,95,168,0.14)", stroke: COLOUR.grid, host: beyond }
        ].forEach(function (layer) {
            if (!byKind[layer.kind]) { return; }
            layer.host.appendChild(svgEl("path", {
                d: byKind[layer.kind], fill: layer.fill,
                stroke: layer.stroke, "stroke-width": "0.6"
            }));
        });
        [["gained", "ru", grid], ["retaken", "ua", grid], ["incursion", "ua", beyond]]
            .forEach(function (hatch) {
                if (!byKind[hatch[0]]) { return; }
                hatch[2].appendChild(svgEl("path", {
                    d: byKind[hatch[0]],
                    fill: "url(#tov-hatch-" + hatch[1] + "-" + seq + ")",
                    stroke: "none"
                }));
            });
        // The disputed ground is ringed, not hatched hexagon by hexagon. A per-hexagon outline
        // was right when "contested" meant the one row the line ran through; read from the
        // chronicle it can mean a whole zone, and eight outlined hexagons in a block read as
        // ground taken rather than ground disputed.
        if (byKind.front) {
            grid.appendChild(svgEl("path", {
                d: history
                    ? boundary(cells, function (i) { return hexes[i].kind === "front"; }, false)
                    : byKind.front,
                fill: "none", stroke: COLOUR.ru, "stroke-width": "1.1", opacity: "0.7"
            }));
        }
        svg.appendChild(grid);
        svg.appendChild(beyond);

        // Rivers, under the line but over the grid.
        svg.appendChild(svgEl("path", {
            d: geo.path(geo.dniepr, p, false),
            fill: "none", stroke: COLOUR.river, "stroke-width": "2.2",
            "stroke-linecap": "round", "stroke-linejoin": "round", opacity: "0.85"
        }));
        if (geo.dniester) {
            svg.appendChild(svgEl("path", {
                d: geo.path(geo.dniester, p, false),
                fill: "none", stroke: COLOUR.river, "stroke-width": "1.4",
                "stroke-linecap": "round", "stroke-linejoin": "round", opacity: "0.6"
            }));
        }

        // Border on top of the grid, so the country reads as one shape.
        svg.appendChild(svgEl("path", {
            d: geo.path(geo.ukraine, p, true),
            fill: "none", stroke: COLOUR.outline, "stroke-width": "1.5", "stroke-linejoin": "round"
        }));

        // The line of 2014, for the eye to measure against. Traced round the hexagons the
        // invader already held in the autumn of 2021, so it sits on the same ground as
        // everything else rather than beside it.
        var referenceD = history
            ? boundary(cells, function (i) {
                return window.tovFront.baselineInvader(cells[i].sample[0], cells[i].sample[1]);
            }, false)
            : geo.path(initial, p, false);
        if (referenceD) {
            svg.appendChild(svgEl("path", {
                d: referenceD, fill: "none", stroke: "#5c6470", "stroke-width": "1.5",
                "stroke-dasharray": "5 4", opacity: "0.85",
                "clip-path": history ? "url(#" + clipId + ")" : null
            }));
        }

        // Current contact line, on a paper halo so it never drowns in the grid. Dashed as soon
        // as the model, and not the chronicle, is the one placing it.
        var contactD = history
            ? boundary(cells, function (i) {
                var kind = hexes[i].kind;
                return kind === "occupied" || kind === "gained";
            }, false)
            : geo.path(line, p, false);
        var projected = history && regime !== "documented";

        if (contactD) {
            svg.appendChild(svgEl("path", {
                d: contactD, fill: "none", stroke: "#fffdf8", "stroke-width": "5.4",
                "stroke-linecap": "round", "stroke-linejoin": "round", opacity: "0.7",
                "clip-path": history ? "url(#" + clipId + ")" : null
            }));
            svg.appendChild(svgEl("path", {
                d: contactD, fill: "none", stroke: COLOUR.ru, "stroke-width": "3",
                "stroke-linecap": "round", "stroke-linejoin": "round",
                "stroke-dasharray": projected ? "7 5" : null,
                "clip-path": history ? "url(#" + clipId + ")" : null
            }));
        }

        // The pocket in Kursk, outlined on all sides because none of its neighbours is on the
        // board: it is the only ground the defender ever held in the invader's own country, and
        // an unoutlined blue patch beyond the border would read as a drawing error.
        if (history && byKind.incursion) {
            svg.appendChild(svgEl("path", {
                d: boundary(cells, function (i) { return hexes[i].kind === "incursion"; }, true),
                fill: "none", stroke: COLOUR.ua, "stroke-width": "2.4",
                "stroke-linecap": "round", "stroke-linejoin": "round",
                "stroke-dasharray": projected ? "7 5" : null,
                "clip-path": "url(#" + salientId + ")"
            }));
        }

        // What changed hands THIS quarter, ringed in the colour of whoever gained it. Without
        // this, a quarter is a state of the world and never an event: the spring of 2022 would
        // show the northern axes as ordinary reconquered ground rather than as the retreat that
        // had just happened, and the reader would have to compare two screens to see it.
        //
        // Above the contact line on purpose. It is the one mark that answers « what happened »,
        // and nothing on the map may cover it.
        if (history) {
            [["toInvader", COLOUR.ru], ["toDefender", COLOUR.ua]].forEach(function (side) {
                var d = boundary(cells, function (i) { return hexes[i].change === side[0]; }, true);
                if (!d) { return; }
                svg.appendChild(svgEl("path", {
                    d: d, fill: "none", stroke: "#fffdf8", "stroke-width": "4.6",
                    "stroke-linejoin": "round", opacity: "0.55"
                }));
                svg.appendChild(svgEl("path", {
                    d: d, fill: "none", stroke: side[1], "stroke-width": "2.2",
                    "stroke-linejoin": "round",
                    "stroke-dasharray": projected ? "6 4" : null
                }));
            });
        }

        // Cities.
        geo.cities.forEach(function (c) {
            var xy = p(c.lon, c.lat);
            var r = c.rank === 1 ? 3.8 : (c.rank === 2 ? 3 : 2.4);
            svg.appendChild(svgEl("circle", {
                cx: xy[0], cy: xy[1], r: r, fill: "#fff",
                stroke: COLOUR.ink, "stroke-width": "1.3"
            }));
            svg.appendChild(halo(text(xy[0] + r + 4, xy[1] + 3.5, c.name, {
                "font-size": c.rank === 1 ? "11.5" : "10",
                "font-weight": c.rank === 1 ? "700" : "600",
                fill: "#2f353d"
            })));
        });

        // Sector counters replace the callouts: they carry the name and the distance moved
        // themselves, on the resolution glyph. The callouts stay as the fallback for a page
        // served without counters.js.
        if (window.tovCounters && typeof window.tovCounters.draw === "function") {
            window.tovCounters.draw(svg, turn, board, p, {
                cities: geo.cities,
                // The gauges belong on the line the map is drawing, not on the one the model
                // would have drawn: a piece sitting two hundred kilometres from its own front
                // is the contradiction this whole change exists to remove.
                lineAt: history
                    ? function (lat, anchorLon) { return contactLon(cells, hexes, lat, anchorLon); }
                    : null,
                historical: history && regime === "documented",
                turnIndex: index
            });
        } else {
            callouts(svg, turn, board, geo, p);
        }

        if (history) { caption(svg, regime); }

        // What this drawing is worth, for whoever prints a figure beside it. Attached to the
        // element rather than returned, so render() keeps its one-value signature.
        svg.tovReading = {
            regime: regime,
            quarter: window.tovFront.quarterOf(index),
            handover: window.tovFront.handover(),
            area: measure(hexes, hexKm2)
        };
        return svg;
    }

    /* ---------------- Saying which authority drew the line ----------------
       One line, bottom left, where the legend used to be. It is not a legend: it names the
       authority in force and nothing else, because a map of a war still being fought that does
       not say whether it is reporting or projecting is worse than no map. */

    var NOTE = {
        documented: "Front réel — position reconstituée, sources dans le dépôt",
        counterfactual: "Déroulé hypothétique — après {q}, le front est celui du modèle",
        projection: "Après {q}, le front est projeté par le modèle"
    };

    function caption(svg, regime) {
        var quarter = window.tovFront.handover();
        var when = quarter
            ? (SEASON_FR[quarter.season] || "") + " " + quarter.year
            : "la période documentée";
        var label = (NOTE[regime] || NOTE.projection).replace("{q}", when);

        var y = H - 13;
        if (regime !== "documented") {
            svg.appendChild(svgEl("line", {
                x1: 14, y1: y - 3.5, x2: 34, y2: y - 3.5,
                stroke: COLOUR.ru, "stroke-width": "2.4", "stroke-dasharray": "7 5"
            }));
        } else {
            svg.appendChild(svgEl("line", {
                x1: 14, y1: y - 3.5, x2: 34, y2: y - 3.5,
                stroke: COLOUR.ru, "stroke-width": "2.4"
            }));
        }

        svg.appendChild(halo(text(40, y, label, {
            "font-size": "9", "letter-spacing": "0.05em", fill: "#6f6a5e",
            "font-family": "Georgia, 'Palatino Linotype', serif"
        })));
    }

    // Where the front stands at one latitude: the westernmost longitude the invader holds or
    // disputes on that row, so a gauge can be hung on the line the reader is looking at rather
    // than on the one the model would have drawn.
    //
    // The search is fenced to the east of the sector's own February 2022 anchor, and for a plain
    // reason: at the latitude of Kharkiv the map also carries the column of Kyiv, four hundred
    // kilometres west, and an unfenced minimum would hang the Kharkiv gauge on it.
    function contactLon(cells, hexes, lat, anchorLon) {
        var floor = anchorLon - 2.5;
        var best = null;
        for (var i = 0; i < cells.length; i++) {
            var kind = hexes[i].kind;
            if (kind !== "occupied" && kind !== "gained" && kind !== "front") { continue; }
            // Mostly-water hexagons are skipped: the western tip of the Kherson left bank is a
            // sand spit, and hanging a gauge on it puts the piece out at sea.
            if (cells[i].land < 0.5) { continue; }
            var sample = cells[i].sample;
            if (Math.abs(sample[1] - lat) > 0.32 || sample[0] < floor) { continue; }
            if (best === null || sample[0] < best) { best = sample[0]; }
        }
        return best;
    }

    return { render: render };
})();
