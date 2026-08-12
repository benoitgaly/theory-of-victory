(function () {
    "use strict";

    var games = window.tovGames || [];
    var board = window.tovBoard || [];
    var geo = window.tovGeo;
    if (!games.length) {
        return;
    }

    var state = { gameIndex: 0, turnIndex: 0, phase: 0 };

    var SEASONS = { Winter: "hiver", Spring: "printemps", Summer: "été", Autumn: "automne" };

    var FLOWS = [
        { key: "infantry", label: "Soldats", colour: "#7a6a55", scale: 40 },
        { key: "weapons", label: "Armes", colour: "#b8860b", scale: 120 },
        { key: "fuel", label: "Carburant", colour: "#8a5a2b", scale: 60 },
        { key: "food", label: "Nourriture", colour: "#3d7a51", scale: 60 }
    ];

    var DEEP = [
        { key: "strike_drones", label: "Drones d'attaque", colour: "#8e5878", scale: 400 },
        { key: "missiles", label: "Missiles", colour: "#a8322a", scale: 120 },
        { key: "cheap_interceptors", label: "Défense bas coût", colour: "#3f7f93", scale: 900 },
        { key: "heavy_interceptors", label: "Intercepteurs lourds", colour: "#1e5fa8", scale: 120 }
    ];

    var ALLOC = [
        { key: "recruitment", label: "Recrutement", colour: "#7a6a55" },
        { key: "weapons", label: "Munitions", colour: "#b8860b" },
        { key: "strike", label: "Frappe profonde", colour: "#8e5878" },
        { key: "defence", label: "Défense AA", colour: "#3f7f93" },
        { key: "expansion", label: "Usines", colour: "#4a6d3a" },
        { key: "innovation", label: "Innovation", colour: "#2f8f8f" },
        { key: "fortification", label: "Fortifications", colour: "#6b7280" },
        { key: "foreign", label: "Achats étrangers", colour: "#a8322a" },
        { key: "civilian", label: "Civil", colour: "#c2b8a3" },
        { key: "audit", label: "Anticorruption", colour: "#9b7fb0" }
    ];

    function fmt(v, d) {
        if (v === null || v === undefined || isNaN(v)) { return "—"; }
        return v.toLocaleString("fr-FR", { minimumFractionDigits: d || 0, maximumFractionDigits: d || 0 });
    }

    function el(tag, cls, text) {
        var n = document.createElement(tag);
        if (cls) { n.className = cls; }
        if (text !== undefined) { n.textContent = text; }
        return n;
    }

    function svgEl(tag, attrs) {
        var n = document.createElementNS("http://www.w3.org/2000/svg", tag);
        Object.keys(attrs || {}).forEach(function (k) { n.setAttribute(k, attrs[k]); });
        return n;
    }

    function game() { return games[state.gameIndex]; }
    function turn() { return game().turns[Math.min(state.turnIndex, game().turns.length - 1)]; }

    function coverColour(c) {
        if (c >= 0.95) { return "#3d7a51"; }
        if (c >= 0.7) { return "#c07a1e"; }
        return "#a8322a";
    }

    /* ---------------- Chrome ---------------- */

    function renderVariants() {
        var host = document.getElementById("variantPicker");
        host.innerHTML = "";
        games.forEach(function (g, i) {
            var b = el("button", "variant" + (i === state.gameIndex ? " active" : ""));
            b.type = "button";
            b.appendChild(el("span", "v-title", g.title));
            b.appendChild(el("span", "v-sub", g.subtitle));
            b.addEventListener("click", function () {
                state.gameIndex = i;
                state.turnIndex = Math.min(state.turnIndex, g.turns.length - 1);
                render();
            });
            host.appendChild(b);
        });
    }

    function renderTimeline() {
        var host = document.getElementById("turnTicks");
        host.innerHTML = "";
        game().turns.forEach(function (t, i) {
            var hasCard = t.cardsPlayed && t.cardsPlayed.length > 0;
            var b = el("button", "tick" + (i === state.turnIndex ? " active" : "") + (hasCard ? " has-card" : ""));
            b.type = "button";
            b.appendChild(el("span", null, "T" + t.turn));
            b.appendChild(el("span", "t-season", (SEASONS[t.season] || "").slice(0, 4) + " " + String(t.year).slice(2)));
            b.addEventListener("click", function () { state.turnIndex = i; render(); });
            host.appendChild(b);
        });
    }

    function bindPhases() {
        document.querySelectorAll(".phase").forEach(function (b) {
            b.addEventListener("click", function () {
                state.phase = parseInt(b.getAttribute("data-phase"), 10);
                render();
            });
        });
    }

    /* ---------------- Force generation ---------------- */

    function link(step, value, unit, note) {
        var box = el("div", "link");
        box.appendChild(el("div", "l-step", step));
        var v = el("div", "l-value", value);
        if (unit) {
            var u = el("span", "l-unit", unit);
            v.appendChild(u);
        }
        box.appendChild(v);
        if (note) {
            var n = el("div", "l-note");
            n.innerHTML = note;
            box.appendChild(n);
        }
        return box;
    }

    function stackbar(parts, total) {
        var bar = el("div", "stackbar");
        parts.forEach(function (p) {
            if (p.value <= 0) { return; }
            var s = el("span");
            s.style.width = (p.value / total * 100).toFixed(2) + "%";
            s.style.background = p.colour;
            s.title = p.label + " : " + fmt(p.value, 1);
            bar.appendChild(s);
        });
        return bar;
    }

    function chips(parts) {
        var host = el("div", "chips");
        parts.forEach(function (p) {
            if (p.value <= 0.05) { return; }
            var c = el("div", "chip");
            var i = el("i");
            i.style.background = p.colour;
            c.appendChild(i);
            c.appendChild(el("span", null, p.label + " " + fmt(p.value, 1)));
            host.appendChild(c);
        });
        return host;
    }

    function renderChain(side, isInvader) {
        var chain = el("div", "chain");

        // 1. Economy: the headline rises while capacity is eaten.
        var gap = side.headlineGdp - side.productiveCapacity;
        chain.appendChild(link(
            "1 · Économie",
            fmt(side.headlineGdp),
            " Md",
            "Capacité productive <b>" + fmt(side.productiveCapacity) + " Md</b>" +
            (gap > 0 ? " · <span class=\"l-loss\">écart +" + fmt(gap) + "</span>" : "")));

        // 2. Revenue: where the money actually comes from.
        var rev = [
            { label: "Fiscalité", value: side.fiscalRevenue, colour: "#6b7280" },
            { label: "Pétrole", value: side.oilRevenue, colour: "#8a5a2b" },
            { label: isInvader ? "Achats étrangers" : "Aide reçue", value: isInvader ? 0 : side.foreignSupport, colour: "#1e5fa8" }
        ];
        var revTotal = rev.reduce(function (s, r) { return s + Math.max(0, r.value); }, 0) || 1;
        var revBox = link("2 · Recettes du tour", fmt(revTotal, 1), " Md", null);
        revBox.appendChild(stackbar(rev, revTotal));
        revBox.appendChild(chips(rev));
        chain.appendChild(revBox);

        // 3. War budget split by line.
        var alloc = ALLOC.map(function (a) {
            return { label: a.label, value: side.allocation[a.key] || 0, colour: a.colour };
        });
        var allocTotal = alloc.reduce(function (s, a) { return s + a.value; }, 0) || 1;
        var allocBox = link("3 · Budget de guerre", fmt(allocTotal, 1), " Md", null);
        allocBox.appendChild(stackbar(alloc, allocTotal));
        allocBox.appendChild(chips(alloc.slice(0, 5)));
        chain.appendChild(allocBox);

        // 4. Factories: money buys nothing beyond installed capacity.
        var produced = side.produced.weapons || 0;
        var capacity = side.capacity.weapons || 0;
        var used = capacity > 0 ? Math.min(1, produced / capacity) : 0;
        var factoryBox = link(
            "4 · Usines",
            fmt(produced),
            " k",
            "Capacité <b>" + fmt(capacity) + " k/tour</b> · utilisée à " + fmt(used * 100) + " %" +
            (side.productionCeiling < 0.99 ? " · <span class=\"l-loss\">plafond sanctions " + fmt(side.productionCeiling * 100) + " %</span>" : ""));
        var capBar = el("div", "stackbar");
        var usedSpan = el("span");
        usedSpan.style.width = (used * 100).toFixed(1) + "%";
        usedSpan.style.background = "#b8860b";
        capBar.appendChild(usedSpan);
        factoryBox.appendChild(capBar);
        chain.appendChild(factoryBox);

        // 5. Transmission: what actually reaches the line.
        var lost = (1 - side.transmissionRate) * 100;
        chain.appendChild(link(
            "5 · Transmission",
            fmt(side.transmissionRate * 100),
            " %",
            "<span class=\"l-loss\">−" + fmt(lost) + " %</span> perdus · corruption " + fmt(side.corruption) +
            " · logistique " + fmt(side.logisticsIntegrity * 100) + " %"));

        return chain;
    }

    /* ---------------- Liebig barrel ---------------- */

    // Liebig's barrel: the water never rises above the shortest stave.
    function renderBarrel(side) {
        var W = 268, H = 224;
        var svg = svgEl("svg", { viewBox: "0 0 " + W + " " + H, width: W, height: H });

        var staveW = 50, gapW = 5;
        var baseY = 182, maxH = 138;
        var startX = 22;
        var innerW = FLOWS.length * (staveW + gapW) - gapW;

        var scarcest = 2;
        FLOWS.forEach(function (f) {
            var c = side.coverage[f.key];
            if (c !== undefined && c < scarcest) { scarcest = c; }
        });
        scarcest = Math.max(0, Math.min(scarcest, 1.15));

        var waterH = maxH * Math.min(scarcest, 1);
        var waterY = baseY - waterH;

        // Ground shadow
        svg.appendChild(svgEl("ellipse", {
            cx: startX + innerW / 2, cy: baseY + 8, rx: innerW / 2 + 6, ry: 6,
            fill: "#17191e", opacity: "0.09"
        }));

        // Water inside the barrel
        svg.appendChild(svgEl("rect", {
            x: startX, y: waterY, width: innerW, height: waterH,
            fill: "#8fc0dd", opacity: "0.55"
        }));
        svg.appendChild(svgEl("ellipse", {
            cx: startX + innerW / 2, cy: waterY, rx: innerW / 2, ry: 7,
            fill: "#6ba9cd", opacity: "0.75"
        }));

        // Staves
        FLOWS.forEach(function (f, i) {
            var c = Math.max(0, Math.min(side.coverage[f.key] === undefined ? 1 : side.coverage[f.key], 1.15));
            var h = maxH * c;
            var x = startX + i * (staveW + gapW);
            var y = baseY - h;
            var isShortest = Math.abs(c - scarcest) < 0.0001;

            svg.appendChild(svgEl("rect", {
                x: x, y: y, width: staveW, height: h,
                fill: f.colour, opacity: isShortest ? "1" : "0.6",
                rx: "2",
                stroke: isShortest ? "#17191e" : "rgba(0,0,0,0.22)",
                "stroke-width": isShortest ? "2" : "0.8"
            }));

            // Wood grain
            svg.appendChild(svgEl("line", {
                x1: x + staveW / 2, y1: y + 4, x2: x + staveW / 2, y2: baseY - 4,
                stroke: "#000", opacity: "0.09", "stroke-width": "1"
            }));

            var pct = svgEl("text", {
                x: x + staveW / 2, y: y - 7,
                "text-anchor": "middle", "font-size": isShortest ? "13" : "11",
                "font-weight": isShortest ? "800" : "500",
                fill: isShortest ? "#a8322a" : "#8b93a1"
            });
            pct.textContent = Math.round(c * 100) + "%";
            svg.appendChild(pct);

            var label = svgEl("text", {
                x: x + staveW / 2, y: baseY + 22,
                "text-anchor": "middle", "font-size": "10",
                fill: isShortest ? "#17191e" : "#8b93a1",
                "font-weight": isShortest ? "750" : "500"
            });
            label.textContent = f.label;
            svg.appendChild(label);
        });

        // A single low hoop: enough to read as a barrel, never over the staves that matter.
        svg.appendChild(svgEl("rect", {
            x: startX - 4, y: baseY - 22, width: innerW + 8, height: 4,
            fill: "#4a4238", opacity: "0.35", rx: "2"
        }));

        // Base
        svg.appendChild(svgEl("rect", {
            x: startX - 5, y: baseY, width: innerW + 10, height: 5,
            fill: "#4a4238", rx: "2"
        }));

        // Water line, extended out to make the cap explicit
        svg.appendChild(svgEl("line", {
            x1: 6, y1: waterY, x2: startX + innerW + 10, y2: waterY,
            stroke: "#1e5fa8", "stroke-width": "1.6", "stroke-dasharray": "5 3", opacity: "0.9"
        }));

        return svg;
    }

    function renderGeneration(side, isInvader) {
        var stage = document.getElementById("stage");
        var t = turn();

        var head = el("div", "stage-head");
        var h = el("h2");
        var flag = el("span", "side-flag");
        flag.style.background = isInvader ? "var(--ru)" : "var(--ua)";
        h.appendChild(flag);
        h.appendChild(document.createTextNode("Génération de force — " + side.name));
        head.appendChild(h);
        head.appendChild(el("div", "turn-tag",
            "Tour " + t.turn + " · " + (SEASONS[t.season] || t.season) + " " + t.year + " · Brent " + fmt(t.oilPrice) + " $"));
        stage.appendChild(head);

        stage.appendChild(renderChain(side, isInvader));

        // The barrel
        var row = el("div", "barrel-row");

        var barrelPanel = el("section", "panel barrel-panel");
        barrelPanel.appendChild(el("div", "panel-title", "Puissance de combat soutenable"));
        var lead = el("p", "barrel-lead");
        lead.innerHTML = "Le niveau ne monte jamais au-dessus de la douve la plus courte. <b>Ta puissance est celle de ta ressource la plus rare</b>, jamais la somme.";
        barrelPanel.appendChild(lead);

        var wrap = el("div", "barrel-wrap");
        wrap.appendChild(renderBarrel(side));

        var readout = el("div", "barrel-readout");
        readout.appendChild(el("div", "b-power", fmt(side.combatPower)));
        readout.appendChild(el("div", "b-caption", "sur une cible de " + fmt(side.targetForceSize) + " k hommes"));

        var bn = el("div", "b-bottleneck");
        bn.appendChild(el("div", "bb-label", "Goulot d'étranglement"));
        bn.appendChild(el("div", "bb-value", side.bottleneckName || "—"));
        readout.appendChild(bn);

        var ratio = el("div", "b-caption");
        ratio.style.marginTop = "12px";
        ratio.innerHTML = "Ratio de génération <b style=\"color:" + coverColour(side.forceGenerationRatio) +
            ";font-size:15px\">" + fmt(side.forceGenerationRatio, 2) + "</b>";
        readout.appendChild(ratio);

        wrap.appendChild(readout);
        barrelPanel.appendChild(wrap);
        row.appendChild(barrelPanel);

        // Front flows as tokens
        var stockPanel = el("section", "panel");
        stockPanel.style.padding = "20px 22px";
        stockPanel.appendChild(el("div", "panel-title", "Ce qui atteint le front ce trimestre"));
        var grid = el("div", "stock-grid");
        FLOWS.forEach(function (f) {
            if (f.key === "infantry") {
                grid.appendChild(stockCard("Soldats au front", side.soldiersAtFront, side.targetForceSize,
                    f.colour, 40, side.coverage.infantry));
                return;
            }
            grid.appendChild(stockCard(f.label, side.delivered[f.key] || 0, side.need[f.key] || 0,
                f.colour, f.scale, side.coverage[f.key]));
        });
        stockPanel.appendChild(grid);
        row.appendChild(stockPanel);
        stage.appendChild(row);

        // Deep strike: what was launched, and what the exchange cost.
        var strike = isInvader ? t.invaderStrike : t.defenderStrike;
        var deepPanel = el("section", "panel");
        deepPanel.style.padding = "20px 22px";
        deepPanel.appendChild(el("div", "panel-title", "Frappe en profondeur et défense"));

        var deepGrid = el("div", "stock-grid");
        if (strike) {
            deepGrid.appendChild(stockCard("Drones lancés", strike.dronesSent, 0, "#8e5878", 400, null));
            deepGrid.appendChild(stockCard("Missiles lancés", strike.missilesSent, 0, "#a8322a", 120, null));
        }
        deepGrid.appendChild(stockCard("Défense bas coût en stock", side.stocks.cheap_interceptors || 0, 0, "#3f7f93", 900, null));
        deepGrid.appendChild(stockCard("Intercepteurs lourds en stock", side.stocks.heavy_interceptors || 0, 0, "#1e5fa8", 120, null));
        deepPanel.appendChild(deepGrid);

        if (strike) {
            var note = el("p", "barrel-lead");
            note.style.marginTop = "12px";
            note.innerHTML = "Interception adverse : <b>" + fmt(strike.interceptionRate * 100) + " %</b>" +
                (strike.saturated ? " — mais la vague a <b>saturé</b> les magasins." : ".") +
                (strike.exchangeRatio > 0
                    ? " Rapport d'échange : <b>" + fmt(strike.exchangeRatio, 1) + " €</b> dépensés en interception par euro détruit" +
                      (strike.exchangeRatio > 1 ? " — le défenseur perd la guerre des coûts." : ".")
                    : "");
            deepPanel.appendChild(note);
        }

        stage.appendChild(deepPanel);

        var foot = el("p", "footnote");
        foot.innerHTML = isInvader
            ? "La Russie <b>achète</b> son soutien étranger : le flux coûte cher et ne s'arrête jamais tant qu'elle peut payer. Dépendance actuelle : " + fmt(side.dependency * 100) + " %."
            : "L'Ukraine <b>reçoit</b> son soutien : le flux est gratuit et peut s'arrêter du jour au lendemain. Volonté des soutiens : " + fmt(side.externalWill) + "/100.";
        stage.appendChild(foot);
    }

    function stockCard(name, value, reference, colour, scale, coverage) {
        var card = el("div", "stock");
        var head = el("div", "s-head");
        head.appendChild(el("span", "s-name", name));
        head.appendChild(el("span", "s-value", fmt(value)));
        card.appendChild(head);

        var tokens = el("div", "tokens");
        var full = Math.min(16, Math.round(value / scale));
        var ghosts = reference > 0 ? Math.min(16, Math.round(reference / scale)) - full : 0;
        for (var i = 0; i < full; i++) {
            var t = el("span", "token");
            t.style.background = colour;
            tokens.appendChild(t);
        }
        for (var j = 0; j < Math.max(0, ghosts); j++) {
            tokens.appendChild(el("span", "token ghost"));
        }
        card.appendChild(tokens);

        if (coverage !== null && coverage !== undefined) {
            var meter = el("div", "covermeter");
            var fill = el("span");
            fill.style.width = Math.min(100, coverage * 100).toFixed(0) + "%";
            fill.style.background = coverColour(coverage);
            meter.appendChild(fill);
            card.appendChild(meter);

            var note = el("div", "sc-outcome");
            note.style.marginTop = "5px";
            note.textContent = "Couverture " + fmt(coverage * 100) + " % du besoin";
            card.appendChild(note);
        }

        return card;
    }

    /* ---------------- Battlefield ---------------- */

    function frontLine(t) {
        var pts = board.map(function (s) {
            var res = (t.sectors || []).find(function (r) { return r.sectorCode === s.code; });
            var hexes = res ? res.hexesCumulative : 0;
            return [s.lon + s.pushLon * hexes, s.lat + s.pushLat * hexes];
        });
        return [geo.northAnchor].concat(pts).concat([geo.southAnchor]);
    }

    function renderMap(t) {
        var W = 900, H = 520;
        var svg = svgEl("svg", { viewBox: "0 0 " + W + " " + H });
        var project = geo.projector(W - 20, H - 20);
        var p = function (lon, lat) { return project(lon, lat); };

        var defs = svgEl("defs", {});
        var clip = svgEl("clipPath", { id: "ua-clip" });
        clip.appendChild(svgEl("path", { d: geo.path(geo.ukraine, p, true) }));
        defs.appendChild(clip);
        svg.appendChild(defs);

        svg.appendChild(svgEl("rect", { x: 0, y: 0, width: W, height: H, fill: "#e8eef2" }));

        // Country
        svg.appendChild(svgEl("path", {
            d: geo.path(geo.ukraine, p, true),
            fill: "#f7f4ec", stroke: "#9aa3ad", "stroke-width": "1.4"
        }));

        var line = frontLine(t);
        var initial = [geo.northAnchor]
            .concat(board.map(function (s) { return [s.lon, s.lat]; }))
            .concat([geo.southAnchor]);

        // Occupied ground: down the line, then back up the border. Clipped to the outline.
        var occupied = line.concat(geo.easternEdge);
        svg.appendChild(svgEl("path", {
            d: geo.path(occupied, p, true),
            fill: "#a8322a", opacity: "0.22",
            "clip-path": "url(#ua-clip)"
        }));

        // Dniepr
        svg.appendChild(svgEl("path", {
            d: geo.path(geo.dniepr, p, false),
            fill: "none", stroke: "#8fb4cc", "stroke-width": "2", "stroke-linecap": "round"
        }));

        // February 2022 line, for the eye to measure against.
        svg.appendChild(svgEl("path", {
            d: geo.path(initial, p, false),
            fill: "none", stroke: "#6b7280", "stroke-width": "1.4",
            "stroke-dasharray": "5 4", opacity: "0.8"
        }));

        // Current contact line
        svg.appendChild(svgEl("path", {
            d: geo.path(line, p, false),
            fill: "none", stroke: "#a8322a", "stroke-width": "3.4",
            "stroke-linecap": "round", "stroke-linejoin": "round"
        }));

        // Cities
        geo.cities.forEach(function (c) {
            var xy = p(c.lon, c.lat);
            var r = c.rank === 1 ? 4 : (c.rank === 2 ? 3 : 2.4);
            svg.appendChild(svgEl("circle", {
                cx: xy[0], cy: xy[1], r: r,
                fill: "#fff", stroke: "#17191e", "stroke-width": "1.3"
            }));
            var label = svgEl("text", {
                x: xy[0] + r + 4, y: xy[1] + 3.5,
                "font-size": c.rank === 1 ? "11.5" : "10",
                "font-weight": c.rank === 1 ? "650" : "500",
                fill: "#3b424c"
            });
            label.textContent = c.name;
            svg.appendChild(label);
        });

        // Sector markers on the line, sized by this turn's movement
        board.forEach(function (s) {
            var res = (t.sectors || []).find(function (r) { return r.sectorCode === s.code; });
            if (!res) { return; }
            var hexes = res.hexesCumulative;
            var xy = p(s.lon + s.pushLon * hexes, s.lat + s.pushLat * hexes);
            var moved = Math.abs(res.hexesMoved) > 0.01;

            svg.appendChild(svgEl("circle", {
                cx: xy[0], cy: xy[1], r: moved ? 7 : 5,
                fill: moved ? (res.hexesMoved > 0 ? "#a8322a" : "#1e5fa8") : "#fff",
                stroke: "#17191e", "stroke-width": "1.5"
            }));

            if (moved) {
                var t2 = svgEl("text", {
                    x: xy[0], y: xy[1] + 3.4, "text-anchor": "middle",
                    "font-size": "8.5", "font-weight": "700", fill: "#fff"
                });
                t2.textContent = Math.abs(res.hexesMoved * 10).toFixed(0);
                svg.appendChild(t2);
            }

            // Only label sectors that moved: the map stays readable, the eye goes where it matters.
            if (moved) {
                var name = svgEl("text", {
                    x: xy[0] - 10, y: xy[1] - 11, "text-anchor": "end",
                    "font-size": "10", "font-weight": "700", fill: "#7d2019"
                });
                name.textContent = s.name.split(" — ")[0];
                svg.appendChild(name);
            }
        });

        return svg;
    }

    var ART = {
        "Économique": function (g) {
            g.appendChild(svgEl("rect", { x: 0, y: 0, width: 100, height: 60, fill: "#2c3e50" }));
            [20, 40, 60, 80].forEach(function (x, i) {
                g.appendChild(svgEl("rect", { x: x - 7, y: 44 - i * 6, width: 12, height: 16 + i * 6, fill: "#e0c07a", opacity: 0.85 }));
            });
            g.appendChild(svgEl("path", { d: "M4 46 L28 34 L52 38 L76 20 L96 12", fill: "none", stroke: "#e8746a", "stroke-width": "2.5" }));
        },
        "Politique occidentale": function (g) {
            g.appendChild(svgEl("rect", { x: 0, y: 0, width: 100, height: 60, fill: "#1b3a5c" }));
            g.appendChild(svgEl("circle", { cx: 50, cy: 30, r: 17, fill: "none", stroke: "#f0d060", "stroke-width": "2" }));
            for (var i = 0; i < 12; i++) {
                var a = (i / 12) * Math.PI * 2;
                g.appendChild(svgEl("circle", { cx: 50 + Math.cos(a) * 17, cy: 30 + Math.sin(a) * 17, r: 1.8, fill: "#f0d060" }));
            }
        },
        "Politique interne": function (g) {
            g.appendChild(svgEl("rect", { x: 0, y: 0, width: 100, height: 60, fill: "#3a2f2a" }));
            [18, 32, 46, 60, 74].forEach(function (x, i) {
                g.appendChild(svgEl("rect", { x: x, y: 22 + (i % 2) * 3, width: 7, height: 26, fill: "#c9b79a", opacity: 0.9 }));
                g.appendChild(svgEl("circle", { cx: x + 3.5, cy: 19 + (i % 2) * 3, r: 3.6, fill: "#c9b79a" }));
            });
        },
        "Énergie": function (g) {
            g.appendChild(svgEl("rect", { x: 0, y: 0, width: 100, height: 60, fill: "#20242c" }));
            g.appendChild(svgEl("path", { d: "M30 52 L38 8 L46 52 M22 52 L54 52 M32 34 L44 34", fill: "none", stroke: "#8fa3b8", "stroke-width": "2" }));
            g.appendChild(svgEl("path", { d: "M66 10 L58 32 L68 32 L60 52", fill: "none", stroke: "#f0c040", "stroke-width": "3" }));
        },
        "Militaire et technologique": function (g) {
            g.appendChild(svgEl("rect", { x: 0, y: 0, width: 100, height: 60, fill: "#26303a" }));
            g.appendChild(svgEl("circle", { cx: 50, cy: 30, r: 5, fill: "#d8d2c4" }));
            [[28, 16], [72, 16], [28, 44], [72, 44]].forEach(function (c) {
                g.appendChild(svgEl("line", { x1: 50, y1: 30, x2: c[0], y2: c[1], stroke: "#d8d2c4", "stroke-width": "2" }));
                g.appendChild(svgEl("circle", { cx: c[0], cy: c[1], r: 6.5, fill: "none", stroke: "#8fa3b8", "stroke-width": "1.6" }));
            });
        },
        "Externe": function (g) {
            g.appendChild(svgEl("rect", { x: 0, y: 0, width: 100, height: 60, fill: "#2a3630" }));
            g.appendChild(svgEl("path", { d: "M6 44 L34 44 L40 30 L64 30 L70 44 L94 44", fill: "none", stroke: "#9ab89a", "stroke-width": "2.4" }));
            [20, 52, 82].forEach(function (x) {
                g.appendChild(svgEl("rect", { x: x - 8, y: 12, width: 16, height: 11, fill: "#c8a86a" }));
            });
        }
    };

    function renderCard(card) {
        var wrap = el("div", "mtg " + (card.ownerSideCode === "invader" ? "ru" : (card.ownerSideCode === "defender" ? "ua" : "")));
        var inner = el("div", "mtg-inner");

        var title = el("div", "mtg-title");
        title.appendChild(el("div", "mtg-name", card.title));
        var cost = el("div", "mtg-cost");
        if (card.politicalCost > 0) {
            cost.appendChild(el("span", "pip pol", String(Math.round(card.politicalCost))));
        }
        if (card.moneyCost > 0) {
            cost.appendChild(el("span", "pip money", String(Math.round(card.moneyCost))));
        }
        title.appendChild(cost);
        inner.appendChild(title);

        var art = el("div", "mtg-art");
        var svg = svgEl("svg", { viewBox: "0 0 100 60", preserveAspectRatio: "xMidYMid slice" });
        (ART[card.family] || ART["Économique"])(svg);
        art.appendChild(svg);
        inner.appendChild(art);

        inner.appendChild(el("div", "mtg-type", card.typeLine));

        var text = el("div", "mtg-text");
        var rules = el("ul", "mtg-rules");
        (card.rulesText || []).forEach(function (r) { rules.appendChild(el("li", null, r)); });
        text.appendChild(rules);
        if (card.description) {
            text.appendChild(el("p", "mtg-flavour", card.description));
        }
        inner.appendChild(text);

        wrap.appendChild(inner);

        var foot = el("div", "mtg-foot");
        foot.appendChild(el("span", null, card.family));
        foot.appendChild(el("span", null, "TOV · V1"));
        wrap.appendChild(foot);

        return wrap;
    }

    function renderBattlefield() {
        var stage = document.getElementById("stage");
        var t = turn();

        var head = el("div", "stage-head");
        head.appendChild(el("h2", null, "Résolution — champ de bataille"));
        head.appendChild(el("div", "turn-tag",
            "Tour " + t.turn + " · " + (SEASONS[t.season] || t.season) + " " + t.year +
            " · " + fmt(t.squareKilometresGained) + " km² pris depuis février 2022"));
        stage.appendChild(head);

        if (t.cardsPlayed && t.cardsPlayed.length) {
            var rail = el("div", "card-rail");
            t.cardsPlayed.forEach(function (c) { rail.appendChild(renderCard(c)); });
            stage.appendChild(rail);
        }

        var field = el("div", "field");

        var mapPanel = el("section", "panel map-panel");
        mapPanel.appendChild(renderMap(t));
        var legend = el("div", "map-legend");
        [
            { label: "Territoire occupé", colour: "rgba(168,50,42,0.25)" },
            { label: "Ligne de contact", colour: "#a8322a" },
            { label: "Ligne de février 2022", colour: "#6b7280" }
        ].forEach(function (l) {
            var c = el("div", "chip");
            var i = el("i");
            i.style.background = l.colour;
            c.appendChild(i);
            c.appendChild(el("span", null, l.label));
            legend.appendChild(c);
        });
        mapPanel.appendChild(legend);
        field.appendChild(mapPanel);

        var right = el("div");
        var sectorPanel = el("section", "panel");
        sectorPanel.style.padding = "16px 18px";
        sectorPanel.appendChild(el("div", "panel-title", "Rapport de force par secteur"));

        var list = el("div", "sector-list");
        (t.sectors || []).forEach(function (s) {
            var moved = Math.abs(s.hexesMoved) > 0.01;
            var cls = moved ? (s.hexesMoved > 0 ? "gain" : "loss") : "static";
            var row = el("div", "sector " + cls);
            row.appendChild(el("div", "sc-name", s.sectorName));
            row.appendChild(el("div", "sc-move", moved ? (s.hexesMoved > 0 ? "+" : "−") + fmt(Math.abs(s.hexesMoved * 10), 1) + " km" : "—"));
            row.appendChild(el("div", "sc-outcome", s.outcome));
            row.appendChild(el("div", "sc-ratio", "rapport " + fmt(s.ratio, 2)));

            var bar = el("div", "ratiobar");
            var fill = el("span");
            fill.style.width = Math.min(100, (s.ratio / 3) * 100).toFixed(0) + "%";
            fill.style.background = s.ratio >= 1.1 ? "#a8322a" : "#8b93a1";
            bar.appendChild(fill);
            var th = el("div", "threshold");
            th.style.left = (1.1 / 3 * 100).toFixed(0) + "%";
            th.title = "Seuil de mouvement";
            bar.appendChild(th);
            row.appendChild(bar);

            list.appendChild(row);
        });
        sectorPanel.appendChild(list);
        right.appendChild(sectorPanel);

        if (t.narrative && t.narrative.length) {
            var narr = el("section", "panel narrative");
            narr.style.marginTop = "14px";
            narr.appendChild(el("div", "panel-title", "Journal du trimestre"));
            var ul = el("ul");
            t.narrative.forEach(function (n) { ul.appendChild(el("li", null, n)); });
            narr.appendChild(ul);
            right.appendChild(narr);
        }

        field.appendChild(right);
        stage.appendChild(field);

        var outcome = t.outcome || (state.turnIndex === game().turns.length - 1 ? game().outcome : null);
        if (outcome) {
            var box = el("section", "outcome");
            box.appendChild(el("h4", null, outcome.title));
            box.appendChild(el("p", null, outcome.explanation));
            stage.appendChild(box);
        }

        var foot = el("p", "footnote");
        foot.innerHTML = "Un secteur ne bouge qu'au-delà d'un rapport de <b>1,1</b>, et attaquer coûte trois à cinq fois ce que coûte tenir. Le tracé du front et les contours sont approximatifs, posés pour la lecture.";
        stage.appendChild(foot);
    }

    /* ---------------- Render ---------------- */

    function render() {
        renderVariants();
        renderTimeline();

        document.querySelectorAll(".phase").forEach(function (b) {
            b.classList.toggle("active", parseInt(b.getAttribute("data-phase"), 10) === state.phase);
        });

        var stage = document.getElementById("stage");
        stage.innerHTML = "";

        var t = turn();
        if (state.phase === 0) {
            renderGeneration(t.invader, true);
        } else if (state.phase === 1) {
            renderGeneration(t.defender, false);
        } else {
            renderBattlefield();
        }
    }

    document.getElementById("prevTurn").addEventListener("click", function () {
        state.turnIndex = Math.max(0, state.turnIndex - 1);
        render();
    });

    document.getElementById("nextTurn").addEventListener("click", function () {
        state.turnIndex = Math.min(game().turns.length - 1, state.turnIndex + 1);
        render();
    });

    document.addEventListener("keydown", function (e) {
        if (e.key === "ArrowLeft") {
            state.turnIndex = Math.max(0, state.turnIndex - 1);
            render();
        } else if (e.key === "ArrowRight") {
            state.turnIndex = Math.min(game().turns.length - 1, state.turnIndex + 1);
            render();
        } else if (e.key >= "1" && e.key <= "3") {
            state.phase = parseInt(e.key, 10) - 1;
            render();
        }
    });

    bindPhases();
    render();
})();
