// Front map. Owns its own file: board.js calls window.tovHexMap.render() and nothing else.
// render(turn, board, geo, opts) -> SVGElement
// - turn.sectors[] carries hexesCumulative and hexesMoved per sector
// - board[] carries lon/lat anchors and push vectors per sector
// - geo exposes the outline, the rivers, the cities and a projector
// - opts.frontLine(turn) returns the current contact line as [[lon,lat], ...]
//
// The country is paved with hexagons. Each one is held, occupied, taken since February 2022,
// retaken, or crossed by the line. A hexagon here is a reading unit — about 50 km across,
// five of the ten-kilometre hexes the engine actually moves.
window.tovHexMap = (function () {
    "use strict";

    var HEX_KM = 40;        // across the flats
    var W = 900, H = 520, PAD = 10;

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
        geo.ukraine.forEach(function (q) {
            var xy = p(q[0], q[1]);
            minX = Math.min(minX, xy[0]); maxX = Math.max(maxX, xy[0]);
            minY = Math.min(minY, xy[1]); maxY = Math.max(maxY, xy[1]);
        });

        var cells = [];
        var rows = Math.ceil((maxY - minY) / rowH) + 2;
        var cols = Math.ceil((maxX - minX) / w) + 2;

        for (var r = -1; r < rows; r++) {
            var cy = minY + r * rowH;
            var shift = (r & 1) ? w / 2 : 0;
            for (var c = -1; c < cols; c++) {
                var cx = minX + c * w + shift;

                var d = "", corners = [], vertsIn = 0;
                for (var k = 0; k < 6; k++) {
                    var a = k * Math.PI / 3;
                    var vx = cx + R * Math.sin(a), vy = cy - R * Math.cos(a);
                    d += (k === 0 ? "M" : "L") + vx.toFixed(1) + " " + vy.toFixed(1);
                    var g = p.invert(vx, vy);
                    corners.push(g);
                    if (geo.contains(geo.ukraine, g[0], g[1])) { vertsIn++; }
                }

                var centre = p.invert(cx, cy);
                // Kept if the centre is on Ukrainian soil, or if the hex still covers a
                // decent piece of it — the clip mask trims whatever spills over the border.
                if (!geo.contains(geo.ukraine, centre[0], centre[1]) && vertsIn < 3) { continue; }

                cells.push({ centre: centre, corners: corners, d: d + "Z" });
            }
        }

        gridCache = { geo: geo, R: R, cells: cells };
        return cells;
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
            return { kind: kind, d: cell.d };
        });
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
        var hexes = classify(buildGrid(geo, p, R), classifier(line), classifier(initial));

        var byKind = { held: "", occupied: "", gained: "", retaken: "", front: "" };
        hexes.forEach(function (h) { byKind[h.kind] += h.d; });

        var grid = svgEl("g", { "clip-path": "url(#" + clipId + ")" });
        [
            { kind: "held", fill: "none", stroke: COLOUR.grid },
            { kind: "occupied", fill: "rgba(168,50,42,0.26)", stroke: "rgba(125,32,25,0.30)" },
            { kind: "gained", fill: "rgba(168,50,42,0.10)", stroke: "rgba(125,32,25,0.26)" },
            { kind: "retaken", fill: "rgba(30,95,168,0.10)", stroke: "rgba(23,71,126,0.26)" },
            { kind: "front", fill: "rgba(168,50,42,0.13)", stroke: COLOUR.grid }
        ].forEach(function (layer) {
            if (!byKind[layer.kind]) { return; }
            grid.appendChild(svgEl("path", {
                d: byKind[layer.kind], fill: layer.fill,
                stroke: layer.stroke, "stroke-width": "0.6"
            }));
        });
        ["gained", "retaken"].forEach(function (kind) {
            if (!byKind[kind]) { return; }
            grid.appendChild(svgEl("path", {
                d: byKind[kind],
                fill: "url(#tov-hatch-" + (kind === "gained" ? "ru" : "ua") + "-" + seq + ")",
                stroke: "none"
            }));
        });
        if (byKind.front) {
            grid.appendChild(svgEl("path", {
                d: byKind.front, fill: "none",
                stroke: COLOUR.ru, "stroke-width": "1.1", opacity: "0.7"
            }));
        }
        svg.appendChild(grid);

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

        // February 2022, for the eye to measure against.
        svg.appendChild(svgEl("path", {
            d: geo.path(initial, p, false),
            fill: "none", stroke: "#5c6470", "stroke-width": "1.5",
            "stroke-dasharray": "5 4", opacity: "0.85"
        }));

        // Current contact line, on a paper halo so it never drowns in the grid.
        svg.appendChild(svgEl("path", {
            d: geo.path(line, p, false),
            fill: "none", stroke: "#fffdf8", "stroke-width": "5.4",
            "stroke-linecap": "round", "stroke-linejoin": "round", opacity: "0.7"
        }));
        svg.appendChild(svgEl("path", {
            d: geo.path(line, p, false),
            fill: "none", stroke: COLOUR.ru, "stroke-width": "3",
            "stroke-linecap": "round", "stroke-linejoin": "round"
        }));

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
            window.tovCounters.draw(svg, turn, board, p, { cities: geo.cities });
        } else {
            callouts(svg, turn, board, geo, p);
        }

        return svg;
    }

    return { render: render };
})();
