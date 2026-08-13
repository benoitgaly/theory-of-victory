(function () {
    "use strict";

    var games = window.tovGames || [];
    var board = window.tovBoard || [];
    var geo = window.tovGeo;
    if (!games.length) {
        return;
    }

    // The page opens on the quarter we are living in, not on February 2022 — and on the
    // last played turn when the war ended before it.
    // Turns up to the quarter we are actually living in are history; everything beyond is
    // the model projecting. On a site about a war still being fought, the two must never be
    // read as the same thing.
    var HISTORY_TURNS = Math.max(1, window.tovCurrentTurn || 1);

    function openingTurnIndex(g) {
        var wanted = (window.tovCurrentTurn || 1) - 1;
        return Math.max(0, Math.min(wanted, g.turns.length - 1));
    }

    var state = { gameIndex: 0, turnIndex: 0, phase: 0 };

    var SEASONS = { Winter: "hiver", Spring: "printemps", Summer: "été", Autumn: "automne" };

    // The three flows the front consumes, and only those: a stave is a resource with a need
    // and therefore a coverage. Men have neither — they are the size of the barrel, since it
    // is the force held in line that dimensions the front and so manufactures the need.
    var FLOWS = [
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
        // French grouping uses a narrow no-break space, which several faces render with no
        // advance at all in tabular figures — « 1141000 ». A plain no-break space always
        // shows, and a figure of a million men has to be readable at a glance.
        return v.toLocaleString("fr-FR", { minimumFractionDigits: d || 0, maximumFractionDigits: d || 0 })
            .replace(/ /g, " ");
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
                // Switching runs keeps the quarter when it exists, and lands on the end
                // of the war when that run stopped earlier.
                state.turnIndex = Math.min(state.turnIndex, g.turns.length - 1);
                render();
            });
            host.appendChild(b);
        });
    }

    var HAND_SIZE = 6;

    // Every card is played by someone — nothing falls from the sky. While the deck still
    // holds a few unattributed cards, they join the hand of each side they land on rather
    // than sit in a "nobody chose this" limbo.
    function cardsOf(t, sideCode) {
        return (t.cardsPlayed || []).filter(function (c) {
            if (c.ownerSideCode) {
                return c.ownerSideCode === sideCode;
            }

            return (c.affectedSideCodes || []).indexOf(sideCode) !== -1;
        });
    }

    // The hand a side would have been choosing from: what it played this quarter, filled
    // up from its own deck. Deterministic — the run must stay reproducible — and rotated
    // by turn so the hand is not the same six cards every time.
    //
    // A played card is a decision the run actually took: it is never dropped to respect the
    // hand size. When a quarter plays more cards than the hand holds, the played cards take
    // the whole hand and no held card is drawn — the calendar is moving towards two or three
    // cards a quarter, and the rail must keep showing every one of them.
    function handFor(t, sideCode) {
        var played = cardsOf(t, sideCode).filter(function (c) { return c; });
        var playedCodes = played.map(function (c) { return c.code; });

        var pool = (window.tovDeck || []).filter(function (c) {
            return c && c.ownerSideCode === sideCode && playedCodes.indexOf(c.code) === -1;
        });

        var hand = played.map(function (c) {
            return { card: c, played: true };
        });

        // Modulo on the pool length would draw the same card twice on a short pool; walking
        // a rotated offset forward keeps the six held cards distinct whatever its size.
        var offset = pool.length ? (t.turn * 3) % pool.length : 0;
        for (var i = 0; hand.length < HAND_SIZE && i < pool.length; i++) {
            hand.push({ card: pool[(offset + i) % pool.length], played: false });
        }

        return hand;
    }

    // February 2022 is turn 1, so turn N is a known quarter of a known year.
    function quarterOf(turn) {
        var index = turn - 1;
        var seasons = ["hiver", "print", "été", "autom"];
        return {
            season: seasons[index % 4],
            year: 2022 + Math.floor(index / 4)
        };
    }

    function renderTimeline() {
        var host = document.getElementById("turnTicks");
        host.innerHTML = "";

        var g = game();

        // A control that cannot do anything says so, otherwise it reads as broken.
        var atStart = state.turnIndex <= 0;
        var atEnd = state.turnIndex >= g.turns.length - 1;
        document.getElementById("firstTurn").disabled = atStart;
        document.getElementById("prevTurn").disabled = atStart;

        var next = document.getElementById("nextTurn");
        next.disabled = atEnd;
        next.title = atEnd
            ? (g.endedEarly
                ? "La guerre s'est terminée au tour " + g.turns.length + " : il n'y a pas de trimestre suivant."
                : "Dernier trimestre de la partie.")
            : "Trimestre suivant";

        var planned = g.plannedTurns || g.turns.length;

        for (var i = 0; i < planned; i++) {
            var played = i < g.turns.length;
            var t = played ? g.turns[i] : null;
            var q = played
                ? { season: (SEASONS[t.season] || "").slice(0, 5), year: t.year }
                : quarterOf(i + 1);

            var hasCard = played && t.cardsPlayed && t.cardsPlayed.length > 0;
            var cls = "tick";
            if (played && i === state.turnIndex) { cls += " active"; }
            if (hasCard) { cls += " has-card"; }
            // The site is about a war that is still being fought: what has happened and what
            // the model projects must never be read as the same thing.
            if (i + 1 > HISTORY_TURNS) { cls += " projected"; }
            // Beyond the last played turn the war is over: the quarter still shows, greyed,
            // so the timeline never looks like a broken button.
            if (!played) { cls += " unplayed"; }

            var b = el("button", cls);
            b.type = "button";
            b.disabled = !played;
            b.appendChild(el("span", null, "T" + (i + 1)));
            b.appendChild(el("span", "t-season", q.season + " " + String(q.year).slice(2)));

            if (played) {
                b.title = "Tour " + (i + 1);
                (function (index) {
                    b.addEventListener("click", function () { state.turnIndex = index; render(); });
                })(i);
            } else {
                b.title = "La guerre s'est terminée au tour " + g.turns.length + " : ce trimestre n'a pas été joué.";
            }

            host.appendChild(b);
        }
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
        // "1 · Économie" → médaillon numéroté + libellé, pour que la chaîne se compte.
        var parts = String(step).split(" · ");
        var head = el("div", "l-step");
        if (parts.length > 1) {
            var medal = el("b", null, parts[0]);
            head.appendChild(medal);
            head.appendChild(document.createTextNode(parts.slice(1).join(" · ")));
        } else {
            head.textContent = step;
        }
        box.appendChild(head);
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

    // Teinte un hex vers le noir (t < 0) ou vers le blanc (t > 0).
    function tint(hex, t) {
        var n = parseInt(hex.slice(1), 16);
        var r = (n >> 16) & 255, g = (n >> 8) & 255, b = n & 255;
        var mix = function (v) {
            var out = t >= 0 ? v + (255 - v) * t : v * (1 + t);
            return Math.max(0, Math.min(255, Math.round(out)));
        };
        return "rgb(" + mix(r) + "," + mix(g) + "," + mix(b) + ")";
    }

    var gradSeq = 0;

    // Le tonneau de Liebig : l'eau ne monte jamais au-dessus de la douve la plus courte.
    function renderBarrel(side) {
        var W = 430, H = 300;
        var svg = svgEl("svg", { viewBox: "0 0 " + W + " " + H, width: W, height: H });
        var defs = svgEl("defs", {});
        svg.appendChild(defs);

        var staveW = 62, gapW = 2;
        var baseY = 238, maxH = 180;
        var innerW = FLOWS.length * (staveW + gapW) - gapW;
        var startX = Math.round((W - innerW) / 2);
        var rightX = startX + innerW;

        var scarcest = 2;
        FLOWS.forEach(function (f) {
            var c = side.coverage[f.key];
            if (c !== undefined && c < scarcest) { scarcest = c; }
        });
        scarcest = Math.max(0, Math.min(scarcest, 1.15));

        // Un seul goulot désigné, même quand plusieurs ressources sont à égalité.
        // Le moteur a déjà tranché : le dessin montre la ressource qu'il nomme.
        var shortIndex = -1;
        FLOWS.forEach(function (f, i) {
            if (f.label === side.bottleneckName) { shortIndex = i; }
        });
        if (shortIndex < 0) {
            for (var si = 0; si < FLOWS.length; si++) {
                var sc = side.coverage[FLOWS[si].key];
                if (sc !== undefined && Math.max(0, Math.min(sc, 1.15)) <= scarcest + 0.0001) { shortIndex = si; break; }
            }
        }
        if (shortIndex < 0) { shortIndex = 0; }

        var waterH = maxH * Math.min(scarcest, 1);
        var waterY = baseY - waterH;

        // Repère du besoin intégralement couvert : le haut théorique du tonneau.
        var fullY = baseY - maxH;
        svg.appendChild(svgEl("line", {
            x1: startX - 12, y1: fullY, x2: rightX + 12, y2: fullY,
            stroke: "#8b8578", "stroke-width": "1", "stroke-dasharray": "2 4", opacity: "0.85"
        }));
        var fullLabel = svgEl("text", {
            x: rightX + 14, y: fullY + 3.5, "font-size": "9",
            "letter-spacing": "0.08em", fill: "#8b8578", "font-weight": "700"
        });
        fullLabel.textContent = "100 %";
        svg.appendChild(fullLabel);

        // Ombre portée au sol
        svg.appendChild(svgEl("ellipse", {
            cx: startX + innerW / 2, cy: baseY + 13, rx: innerW / 2 + 12, ry: 8,
            fill: "#1a1815", opacity: "0.11"
        }));

        // Cerclages métalliques. Le cercle est continu : là où une douve manque,
        // on voit le fer de l'autre côté du tonneau, en retrait.
        function hoop(ratio) {
            var y = baseY - maxH * ratio;
            var hg = "hoop" + (gradSeq++);
            var lg = svgEl("linearGradient", { id: hg, x1: "0", y1: "0", x2: "0", y2: "1" });
            [["0%", "#8d8272"], ["35%", "#5c5346"], ["70%", "#3d372e"], ["100%", "#6a6153"]].forEach(function (s) {
                lg.appendChild(svgEl("stop", { offset: s[0], "stop-color": s[1] }));
            });
            defs.appendChild(lg);

            svg.appendChild(svgEl("rect", {
                x: startX - 7, y: y, width: innerW + 14, height: 9, rx: "2",
                fill: "url(#" + hg + ")", opacity: "0.3"
            }));

            FLOWS.forEach(function (f, i) {
                var raw = side.coverage[f.key];
                var c = Math.max(0, Math.min(raw === undefined ? 1 : raw, 1.15));
                if (baseY - maxH * c > y) { return; }
                var x = startX + i * (staveW + gapW);
                svg.appendChild(svgEl("rect", {
                    x: i === 0 ? x - 7 : x - gapW, y: y,
                    width: staveW + (i === 0 || i === FLOWS.length - 1 ? 7 : 0) + gapW, height: 9, rx: "2",
                    fill: "url(#" + hg + ")"
                }));
            });
        }
        // Douves
        FLOWS.forEach(function (f, i) {
            var raw = side.coverage[f.key];
            var c = Math.max(0, Math.min(raw === undefined ? 1 : raw, 1.15));
            var h = maxH * c;
            var x = startX + i * (staveW + gapW);
            var y = baseY - h;
            var isShortest = i === shortIndex;

            // Bois : clair au centre, sombre sur les chants — la douve est bombée.
            // Les douves de bord sont assombries en plus : l'ensemble se lit comme un cylindre.
            var cyl = Math.abs((i + 0.5) / FLOWS.length - 0.5) * 0.52;
            var gid = "stave" + (gradSeq++);
            var grad = svgEl("linearGradient", { id: gid, x1: "0", y1: "0", x2: "1", y2: "0" });
            [["0%", tint(f.colour, -0.34 - cyl)], ["16%", tint(f.colour, 0.06 - cyl)], ["50%", tint(f.colour, 0.2 - cyl)],
             ["84%", tint(f.colour, 0.02 - cyl)], ["100%", tint(f.colour, -0.38 - cyl)]].forEach(function (s) {
                grad.appendChild(svgEl("stop", { offset: s[0], "stop-color": s[1] }));
            });
            defs.appendChild(grad);

            svg.appendChild(svgEl("rect", {
                x: x, y: y, width: staveW, height: h + 4, rx: "3",
                fill: "url(#" + gid + ")",
                stroke: isShortest ? "#1a1815" : "rgba(26,24,21,0.32)",
                "stroke-width": isShortest ? "1.8" : "0.7"
            }));

            // Chant supérieur : biseau clair, la douve a une épaisseur.
            svg.appendChild(svgEl("rect", {
                x: x + 1.4, y: y + 1.2, width: staveW - 2.8, height: 3.4, rx: "1.6",
                fill: "#fff", opacity: "0.34"
            }));

            // Veines du bois
            [0.3, 0.52, 0.72].forEach(function (k) {
                svg.appendChild(svgEl("line", {
                    x1: x + staveW * k, y1: y + 7, x2: x + staveW * k, y2: baseY - 3,
                    stroke: "#1a1815", opacity: "0.08", "stroke-width": "1"
                }));
            });

            var pct = svgEl("text", {
                x: x + staveW / 2, y: y - 9,
                "text-anchor": "middle", "font-size": isShortest ? "15" : "12",
                "font-weight": isShortest ? "800" : "600",
                fill: isShortest ? "#a8322a" : "#8b8578"
            });
            pct.textContent = Math.round(c * 100) + " %";
            svg.appendChild(pct);

            var label = svgEl("text", {
                x: x + staveW / 2, y: baseY + 30,
                "text-anchor": "middle", "font-size": "11",
                fill: isShortest ? "#1a1815" : "#8b8578",
                "font-weight": isShortest ? "750" : "600"
            });
            label.textContent = f.label;
            svg.appendChild(label);

            // Sceau du goulot : la douve courte est nommée sous le tonneau, pas ailleurs.
            if (isShortest) {
                svg.appendChild(svgEl("rect", {
                    x: x + staveW / 2 - 30, y: baseY + 36, width: 60, height: 15, rx: "7.5",
                    fill: "#a8322a"
                }));
                var tag = svgEl("text", {
                    x: x + staveW / 2, y: baseY + 46.5, "text-anchor": "middle",
                    "font-size": "8.5", "font-weight": "800", "letter-spacing": "0.09em", fill: "#fff"
                });
                tag.textContent = "GOULOT";
                svg.appendChild(tag);
            }
        });

        // Eau : nappe translucide posée par-dessus le bois, comme une coupe du tonneau.
        var wg = "water" + (gradSeq++);
        var wgrad = svgEl("linearGradient", { id: wg, x1: "0", y1: "0", x2: "0", y2: "1" });
        [["0%", "#6ea8cd"], ["100%", "#2f6d99"]].forEach(function (s) {
            wgrad.appendChild(svgEl("stop", { offset: s[0], "stop-color": s[1] }));
        });
        defs.appendChild(wgrad);

        // Voile léger : la teinte de chaque ressource doit rester lisible sous l'eau.
        svg.appendChild(svgEl("rect", {
            x: startX, y: waterY, width: innerW, height: waterH,
            fill: "url(#" + wg + ")", opacity: "0.19"
        }));
        svg.appendChild(svgEl("ellipse", {
            cx: startX + innerW / 2, cy: waterY, rx: innerW / 2, ry: 8,
            fill: "#2f6d99", opacity: "0.7"
        }));
        svg.appendChild(svgEl("ellipse", {
            cx: startX + innerW / 2, cy: waterY - 1.5, rx: innerW / 2 - 9, ry: 5,
            fill: "#cbe7f4", opacity: "0.85"
        }));

        // Cerclages posés par-dessus : le métal est à l'extérieur du tonneau.
        hoop(0.15);
        hoop(0.55);
        hoop(0.92);

        // Débordement à la douve courte : c'est par là que tout le reste se perd.
        if (scarcest < 0.995) {
            var sx = startX + shortIndex * (staveW + gapW);
            var isEdge = shortIndex === 0;
            var spillX = isEdge ? sx - 2 : sx + staveW + 2;
            var dir = isEdge ? -1 : 1;
            svg.appendChild(svgEl("path", {
                d: "M" + (sx + staveW / 2) + " " + (waterY + 2) +
                   " Q" + spillX + " " + (waterY - 1) + " " + (spillX + dir * 9) + " " + (waterY + 16),
                fill: "none", stroke: "#2f6d99", "stroke-width": "2.4",
                "stroke-linecap": "round", opacity: "0.55"
            }));
            [0, 1, 2].forEach(function (k) {
                svg.appendChild(svgEl("circle", {
                    cx: spillX + dir * (10 + k * 2.5), cy: waterY + 24 + k * 13,
                    r: 2.6 - k * 0.5, fill: "#2f6d99", opacity: String(0.5 - k * 0.13)
                }));
            });
        }

        // Fond du tonneau
        svg.appendChild(svgEl("rect", {
            x: startX - 8, y: baseY + 2, width: innerW + 16, height: 8, rx: "3",
            fill: "#4a4238"
        }));

        // Ligne d'eau prolongée dans la marge gauche : la limite est un fait, pas une décoration.
        svg.appendChild(svgEl("line", {
            x1: 4, y1: waterY, x2: rightX + 10, y2: waterY,
            stroke: "#1e5fa8", "stroke-width": "1.6", "stroke-dasharray": "5 3", opacity: "0.95"
        }));
        svg.appendChild(svgEl("rect", {
            x: 2, y: waterY - 22, width: 68, height: 19, rx: "3", fill: "#1e5fa8"
        }));
        var wlab = svgEl("text", {
            x: 36, y: waterY - 13.5, "text-anchor": "middle", "font-size": "8.5",
            "font-weight": "800", "letter-spacing": "0.09em", fill: "#fff"
        });
        wlab.textContent = "NIVEAU RÉEL";
        svg.appendChild(wlab);
        var wpct = svgEl("text", {
            x: 36, y: waterY + 16, "text-anchor": "middle", "font-size": "14",
            "font-weight": "800", fill: "#1e5fa8"
        });
        wpct.textContent = Math.round(scarcest * 100) + " %";
        svg.appendChild(wpct);

        return svg;
    }

    // The hand of the quarter: what this side played, and what it was holding instead.
    // V1.0 follows a calendar, so the held cards are shown for what they are — a choice
    // that was available and not taken. In V2 this is the row the player picks from.
    function renderHand(t, sideCode) {
        var hand = handFor(t, sideCode);
        var played = hand.filter(function (h) { return h.played; });
        var held = hand.length - played.length;

        var panel = el("section", "panel hand-panel");
        var head = el("div", "hand-head");
        head.appendChild(el("div", "panel-title", "La main de ce trimestre"));

        var note = el("div", "hand-note");
        note.innerHTML = played.length
            ? "<strong>" + played.length + "</strong> carte" + (played.length > 1 ? "s jouées" : " jouée") +
              (held > 0
                  ? ", " + held + " gardée" + (held > 1 ? "s" : "") + " en main."
                  : " — la main entière est partie ce trimestre.")
            : "Aucune carte jouée ce trimestre — <strong>" + held + "</strong> gardée" +
              (held > 1 ? "s" : "") + " en main.";
        head.appendChild(note);
        panel.appendChild(head);

        var rail = el("div", "card-rail hand");
        hand.forEach(function (h) {
            rail.appendChild(h.played ? renderPlayedCard(h.card) : renderHeldCard(h.card));
        });
        panel.appendChild(rail);
        return panel;
    }

    // The card the quarter actually played: same anatomy as any other, plus the banner that
    // says so. The distinction is carried by the treatment, never by an amputated card.
    function renderPlayedCard(card) {
        var node = renderCard(card);
        node.classList.add("is-played");
        node.insertBefore(el("div", "mtg-played-tag", "Jouée ce trimestre"), node.firstChild);
        node.title = safeText(card && card.title, "Carte sans titre") + " — jouée ce trimestre";
        return node;
    }

    // A held card is a whole card: it was readable in the hand, so it is readable here.
    // Only the treatment sets it back — nothing is hidden, nothing is left blank.
    function renderHeldCard(card) {
        var node = renderCard(card);
        node.classList.add("back");
        node.querySelector(".mtg-inner").appendChild(el("div", "mtg-held", "Gardée en main"));
        node.title = safeText(card && card.title, "Carte sans titre") + " — non jouée ce trimestre";
        return node;
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

        stage.appendChild(renderHand(t, side.sideCode));
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
        readout.appendChild(el("div", "b-eyebrow", "Puissance soutenable ce trimestre"));
        readout.appendChild(el("div", "b-power", fmt(side.combatPower)));
        readout.appendChild(el("div", "b-caption",
            "sur un effectif théorique de " + fmt(side.menEstablishment) + " hommes"));

        // Jauge atteint / cible : l'écart se voit avant de se lire.
        var reach = side.menEstablishment > 0 ? Math.min(1, side.combatPower / side.menEstablishment) : 0;
        var gauge = el("div", "b-gauge");
        var gfill = el("span");
        gfill.style.width = (reach * 100).toFixed(1) + "%";
        gfill.style.background = coverColour(reach);
        gauge.appendChild(gfill);
        readout.appendChild(gauge);

        var scarcest = 2;
        FLOWS.forEach(function (f) {
            var c = side.coverage[f.key];
            if (c !== undefined && c < scarcest) { scarcest = c; }
        });

        var bn = el("div", "b-bottleneck");
        bn.appendChild(el("div", "bb-label", "Goulot d'étranglement"));
        bn.appendChild(el("div", "bb-value", side.bottleneckName || "—"));
        if (scarcest < 1.5) {
            var bbNote = el("div", "bb-note");
            bbNote.innerHTML = "Plafonné à <b>" + fmt(scarcest * 100) +
                " %</b> du besoin. Tout ce qui est produit au-delà, dans les autres ressources, ne se transforme en rien.";
            bn.appendChild(bbNote);
        }
        readout.appendChild(bn);

        var ratio = el("div", "b-ratio");
        ratio.innerHTML = "<span>Ratio de génération</span><b style=\"color:" + coverColour(side.forceGenerationRatio) +
            "\">" + fmt(side.forceGenerationRatio, 2) + "</b>";
        readout.appendChild(ratio);

        wrap.appendChild(readout);
        barrelPanel.appendChild(wrap);
        row.appendChild(barrelPanel);

        // Front flows as tokens
        var stockPanel = el("section", "panel");
        stockPanel.style.padding = "20px 22px";
        stockPanel.appendChild(el("div", "panel-title", "Ce qui atteint le front ce trimestre"));
        var grid = el("div", "stock-grid");
        grid.appendChild(manpowerCard(side));
        FLOWS.forEach(function (f) {
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

    // Men have no coverage — they are the size of the barrel, not a stave. What they have
    // is three readings that narrow, and the narrowing is the whole point: an army of a
    // million men can lack infantry, because only the last tier holds the line.
    function manpowerCard(side) {
        var card = el("div", "stock manpower");
        var head = el("div", "s-head");
        head.appendChild(el("span", "s-name", "Hommes en ligne de contact"));
        head.appendChild(el("span", "s-value", fmt(side.menInContact)));
        card.appendChild(head);

        var tiers = [
            { label: "Sous les drapeaux", value: side.menUnderArms },
            { label: "Au théâtre", value: side.menInTheatre },
            { label: "En ligne de contact", value: side.menInContact }
        ];
        var widest = tiers[0].value > 0 ? tiers[0].value : 1;

        var wrap = el("div", "mp-tiers");
        tiers.forEach(function (tier, i) {
            var row = el("div", "mp-row");
            row.appendChild(el("span", "mp-label", tier.label));
            row.appendChild(el("span", "mp-value", fmt(tier.value)));
            wrap.appendChild(row);

            var track = el("div", "mp-track" + (i === tiers.length - 1 ? " is-contact" : ""));
            var fill = el("span");
            fill.style.width = Math.max(2, Math.min(100, (tier.value / widest) * 100)).toFixed(1) + "%";
            track.appendChild(fill);
            wrap.appendChild(track);
        });
        card.appendChild(wrap);

        var note = el("div", "sc-outcome");
        note.style.marginTop = "8px";
        note.textContent = "L'effectif dimensionne le front — c'est lui qui fabrique le besoin en obus.";
        card.appendChild(note);

        return card;
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

    /* ---------------- Illustrations de cartes ----------------
       Six scènes, une grammaire commune : ciel dégradé, une source de lumière,
       un plan intermédiaire, une silhouette au premier plan. La profondeur fait
       l'illustration ; le pictogramme fait le panneau de signalisation. */

    function artDefs(g) {
        var d = svgEl("defs", {});
        g.appendChild(d);
        return d;
    }

    function linGrad(defs, stops, horizontal) {
        var id = "art" + (gradSeq++);
        var lg = svgEl("linearGradient", {
            id: id, x1: "0", y1: "0", x2: horizontal ? "1" : "0", y2: horizontal ? "0" : "1"
        });
        stops.forEach(function (s) { lg.appendChild(svgEl("stop", { offset: s[0], "stop-color": s[1], "stop-opacity": s[2] === undefined ? 1 : s[2] })); });
        defs.appendChild(lg);
        return "url(#" + id + ")";
    }

    function radGrad(defs, stops, cx, cy, r) {
        var id = "art" + (gradSeq++);
        var rg = svgEl("radialGradient", { id: id, cx: cx, cy: cy, r: r });
        stops.forEach(function (s) { rg.appendChild(svgEl("stop", { offset: s[0], "stop-color": s[1], "stop-opacity": s[2] === undefined ? 1 : s[2] })); });
        defs.appendChild(rg);
        return "url(#" + id + ")";
    }

    function sky(g, defs, stops) {
        g.appendChild(svgEl("rect", { x: 0, y: 0, width: 100, height: 60, fill: linGrad(defs, stops) }));
    }

    // Voile sombre en bas de vignette : le texte de type qui suit garde sa place.
    function vignette(g, defs) {
        g.appendChild(svgEl("rect", {
            x: 0, y: 38, width: 100, height: 22,
            fill: linGrad(defs, [["0%", "#000", 0], ["100%", "#000", 0.42]])
        }));
    }

    var ART = {
        "Économique": function (g) {
            var d = artDefs(g);
            sky(g, d, [["0%", "#241a17"], ["52%", "#6d4630"], ["100%", "#d59a5c"]]);
            // Soleil bas, posé sur la ligne d'horizon des halles.
            g.appendChild(svgEl("circle", { cx: 72, cy: 44, r: 10, fill: "#f6d097", opacity: "0.85" }));
            // Brume industrielle en bandes horizontales, pas en taches.
            [[18, 0.1], [26, 0.08], [33, 0.06]].forEach(function (b) {
                g.appendChild(svgEl("rect", { x: 0, y: b[0], width: 100, height: 3.5, fill: "#f0dcc0", opacity: String(b[1]) }));
            });
            // Halles et cheminées à contre-jour
            g.appendChild(svgEl("path", {
                d: "M0 60 L0 44 L10 44 L10 34 L16 34 L16 44 L26 44 L26 26 L31 26 L31 44 L42 44 L42 38 L54 38 L54 30 L60 30 L60 44 L74 44 L74 47 L100 47 L100 60 Z",
                fill: "#171410"
            }));
            // Courbe : ce que la comptabilité nationale raconte
            g.appendChild(svgEl("path", {
                d: "M6 40 L24 31 L40 35 L58 20 L78 24 L96 9",
                fill: "none", stroke: "#e8746a", "stroke-width": "2.2", "stroke-linecap": "round", "stroke-linejoin": "round"
            }));
            [[24, 31], [58, 20], [96, 9]].forEach(function (p) {
                g.appendChild(svgEl("circle", { cx: p[0], cy: p[1], r: 1.8, fill: "#f4a49c" }));
            });
            vignette(g, d);
        },

        "Politique occidentale": function (g) {
            var d = artDefs(g);
            sky(g, d, [["0%", "#0c1c33"], ["100%", "#1d3f6b"]]);
            // Halo de tribune
            g.appendChild(svgEl("rect", {
                x: 0, y: 0, width: 100, height: 60,
                fill: radGrad(d, [["0%", "#9dc4ec", 0.5], ["100%", "#9dc4ec", 0]], "0.5", "0.3", "0.55")
            }));
            // Cercle d'étoiles en fond de salle
            for (var i = 0; i < 12; i++) {
                var a = (i / 12) * Math.PI * 2 - Math.PI / 2;
                g.appendChild(svgEl("circle", {
                    cx: 50 + Math.cos(a) * 13, cy: 20 + Math.sin(a) * 9,
                    r: 1.3, fill: "#f0d060", opacity: "0.9"
                }));
            }
            // Hémicycle : tables courbes vues de dos
            [[36, 6.5], [46, 5]].forEach(function (row, k) {
                g.appendChild(svgEl("path", {
                    d: "M-6 " + (row[0] + 10) + " Q50 " + (row[0] - row[1] * 2) + " 106 " + (row[0] + 10),
                    fill: "none", stroke: k ? "#0a1626" : "#0f2039", "stroke-width": String(9 - k * 1.5)
                }));
            });
            // Délégués
            [[14, 34], [30, 32], [50, 31], [70, 32], [86, 34]].forEach(function (p) {
                g.appendChild(svgEl("circle", { cx: p[0], cy: p[1], r: 3, fill: "#0a1626" }));
                g.appendChild(svgEl("path", {
                    d: "M" + (p[0] - 5) + " " + (p[1] + 9) + " Q" + p[0] + " " + (p[1] + 2) + " " + (p[0] + 5) + " " + (p[1] + 9) + " Z",
                    fill: "#0a1626"
                }));
            });
            vignette(g, d);
        },

        "Politique interne": function (g) {
            var d = artDefs(g);
            sky(g, d, [["0%", "#241b16"], ["100%", "#54382a"]]);
            // Halo de la tribune : toute l'attention converge là.
            g.appendChild(svgEl("rect", {
                x: 0, y: 0, width: 100, height: 60,
                fill: radGrad(d, [["0%", "#ffd9a0", 0.5], ["100%", "#ffd9a0", 0]], "0.5", "0.32", "0.42")
            }));
            // Bannières verticales de part et d'autre
            [22, 74].forEach(function (x) {
                g.appendChild(svgEl("rect", { x: x, y: 4, width: 5, height: 30, fill: "#7a2b22", opacity: "0.75" }));
                g.appendChild(svgEl("rect", { x: x, y: 4, width: 1.4, height: 30, fill: "#fff", opacity: "0.12" }));
            });
            // Estrade, pupitre, orateur
            g.appendChild(svgEl("rect", { x: 30, y: 34, width: 40, height: 4, fill: "#17110d" }));
            g.appendChild(svgEl("rect", { x: 45, y: 26, width: 10, height: 8, fill: "#17110d" }));
            g.appendChild(svgEl("circle", { cx: 50, cy: 17, r: 3.2, fill: "#17110d" }));
            g.appendChild(svgEl("path", { d: "M45.4 26 Q50 15.5 54.6 26 Z", fill: "#17110d" }));
            g.appendChild(svgEl("path", {
                d: "M53 22 L58 14", stroke: "#17110d", "stroke-width": "1.8", "stroke-linecap": "round"
            }));
            // Foule de dos, chaque nuque détourée par la lumière de scène.
            [4, 13, 22, 31, 40, 49, 58, 67, 76, 85, 94].forEach(function (x, i) {
                var y = 47 + (i % 3) * 2.5;
                g.appendChild(svgEl("path", {
                    d: "M" + (x - 7.5) + " 60 Q" + x + " " + (y + 2) + " " + (x + 7.5) + " 60 Z", fill: "#0f0b08"
                }));
                g.appendChild(svgEl("circle", { cx: x, cy: y, r: 4.2, fill: "#0f0b08" }));
                g.appendChild(svgEl("path", {
                    d: "M" + (x - 3.6) + " " + (y - 2.2) + " A4.2 4.2 0 0 1 " + (x + 2.4) + " " + (y - 3.5),
                    fill: "none", stroke: "#c99a63", "stroke-width": "0.9", opacity: "0.55"
                }));
            });
            vignette(g, d);
        },

        "Énergie": function (g) {
            var d = artDefs(g);
            sky(g, d, [["0%", "#101c28"], ["66%", "#25384a"], ["100%", "#5a4229"]]);
            // Lueur de la torchère
            g.appendChild(svgEl("rect", {
                x: 0, y: 0, width: 100, height: 60,
                fill: radGrad(d, [["0%", "#ffb347", 0.6], ["100%", "#ffb347", 0]], "0.72", "0.25", "0.58")
            }));
            // Colonnes de distillation et tuyauterie
            g.appendChild(svgEl("path", {
                d: "M0 60 L0 50 L8 50 L8 30 L14 30 L14 50 L24 50 L24 22 L30 22 L30 50 L44 50 L44 36 L52 36 L52 50 L100 50 L100 60 Z",
                fill: "#0c1218"
            }));
            // Liseré chaud sur les arêtes tournées vers la flamme : la nuit a une source.
            [[14, 30], [30, 22], [52, 36]].forEach(function (e) {
                g.appendChild(svgEl("line", {
                    x1: e[0], y1: e[1], x2: e[0], y2: 50,
                    stroke: "#ffb347", "stroke-width": "0.9", opacity: "0.6"
                }));
            });
            [10, 18, 27, 36, 47].forEach(function (x) {
                g.appendChild(svgEl("line", { x1: x, y1: 50, x2: x, y2: 44, stroke: "#33475a", "stroke-width": "1" }));
            });
            // Torchère
            g.appendChild(svgEl("path", { d: "M70 50 L70 24 L73 24 L73 50 Z", fill: "#0c1218" }));
            g.appendChild(svgEl("path", {
                d: "M71.5 22 Q66 14 71 6 Q73 13 77 10 Q79 18 71.5 22 Z",
                fill: linGrad(d, [["0%", "#fff0c0"], ["100%", "#e8721f"]])
            }));
            g.appendChild(svgEl("path", { d: "M71.5 20 Q69 14 71.5 9 Q73.5 14 71.5 20 Z", fill: "#fff6dc", opacity: "0.9" }));
            vignette(g, d);
        },

        "Militaire et technologique": function (g) {
            var d = artDefs(g);
            sky(g, d, [["0%", "#0d1826"], ["62%", "#274056"], ["100%", "#7d99ad"]]);
            // Balayage radar depuis le sol
            [16, 27, 38].forEach(function (r, i) {
                g.appendChild(svgEl("path", {
                    d: "M" + (14 - r) + " 52 A" + r + " " + r + " 0 0 1 " + (14 + r) + " 52",
                    fill: "none", stroke: "#8fd0e8", "stroke-width": "0.9", opacity: String(0.4 - i * 0.1)
                }));
            });
            // Horizon
            g.appendChild(svgEl("path", {
                d: "M0 60 L0 51 Q18 47 34 50 Q54 54 72 49 Q88 45 100 49 L100 60 Z",
                fill: "#101a24"
            }));
            // Drone en silhouette, aile delta
            var dr = svgEl("g", {});
            dr.appendChild(svgEl("path", { d: "M40 22 L74 26 L52 30 L46 34 L44 28 L34 27 Z", fill: "#0a1219" }));
            dr.appendChild(svgEl("path", { d: "M52 26 L60 14 L62 15 L56 27 Z", fill: "#0a1219" }));
            dr.appendChild(svgEl("circle", { cx: 71, cy: 26, r: 1.4, fill: "#ff6b5e" }));
            g.appendChild(dr);
            // Traits de vitesse
            [20, 24, 31].forEach(function (y, i) {
                g.appendChild(svgEl("line", {
                    x1: 4, y1: y, x2: 24 + i * 4, y2: y, stroke: "#cfe6f2",
                    "stroke-width": "0.8", opacity: "0.35"
                }));
            });
            vignette(g, d);
        },

        "Externe": function (g) {
            var d = artDefs(g);
            sky(g, d, [["0%", "#1b2a26"], ["56%", "#4b5f4c"], ["100%", "#b6bf94"]]);
            g.appendChild(svgEl("circle", { cx: 78, cy: 40, r: 8, fill: "#f4e6b4", opacity: "0.6" }));
            // Rade au fond
            g.appendChild(svgEl("rect", { x: 0, y: 42, width: 100, height: 8, fill: "#2f4340", opacity: "0.9" }));
            [44, 47].forEach(function (y) {
                g.appendChild(svgEl("line", { x1: 0, y1: y, x2: 100, y2: y, stroke: "#c9d6ae", "stroke-width": "0.5", opacity: "0.22" }));
            });
            // Grue portuaire : la forme la plus reconnaissable d'un quai.
            g.appendChild(svgEl("path", {
                d: "M16 42 L16 14 L14 14 L14 42 M16 16 L44 16 L44 19 L16 19 M16 16 L4 22 L4 25 L16 21 M40 19 L40 30",
                fill: "none", stroke: "#131a15", "stroke-width": "2"
            }));
            g.appendChild(svgEl("rect", { x: 36, y: 30, width: 8, height: 5, fill: "#131a15" }));
            // Conteneurs empilés : le flux étranger, quantifié en boîtes.
            [[50, 30, "#c8a86a"], [62, 30, "#8d9a6a"], [74, 30, "#b07a4a"],
             [56, 24, "#9a8a5a"], [68, 24, "#7d8f6a"], [62, 18, "#c8a86a"]].forEach(function (c) {
                g.appendChild(svgEl("rect", { x: c[0], y: c[1] + 6, width: 11, height: 5.4, rx: 0.5, fill: c[2] }));
                g.appendChild(svgEl("rect", { x: c[0], y: c[1] + 6, width: 11, height: 1.4, fill: "#fff", opacity: "0.18" }));
                for (var k = 1; k < 4; k++) {
                    g.appendChild(svgEl("line", {
                        x1: c[0] + k * 2.7, y1: c[1] + 6.6, x2: c[0] + k * 2.7, y2: c[1] + 10.8,
                        stroke: "#000", opacity: "0.2", "stroke-width": "0.6"
                    }));
                }
            });
            // Quai
            g.appendChild(svgEl("rect", { x: 0, y: 42, width: 100, height: 18, fill: "#1b2419" }));
            g.appendChild(svgEl("rect", { x: 0, y: 42, width: 100, height: 1.6, fill: "#5d6a4a", opacity: "0.6" }));
            // Dockers, pour l'échelle
            [[26, 42], [33, 42], [88, 42]].forEach(function (p) {
                g.appendChild(svgEl("circle", { cx: p[0], cy: p[1] - 5.4, r: 1.3, fill: "#0d1310" }));
                g.appendChild(svgEl("rect", { x: p[0] - 1.5, y: p[1] - 4.2, width: 3, height: 4.2, rx: 1, fill: "#0d1310" }));
            });
            vignette(g, d);
        },

        // Filet de sécurité : une famille que le deck introduirait sans scène dédiée. Elle
        // reçoit un paysage neutre de la même grammaire plutôt que la scène d'une autre
        // famille, qui mentirait sur le domaine de la carte.
        "": function (g) {
            var d = artDefs(g);
            sky(g, d, [["0%", "#1d2129"], ["60%", "#454b52"], ["100%", "#9a9384"]]);
            g.appendChild(svgEl("circle", { cx: 50, cy: 30, r: 9, fill: "#e8e0cc", opacity: "0.35" }));
            g.appendChild(svgEl("path", {
                d: "M0 60 L0 48 Q26 43 50 47 Q76 51 100 45 L100 60 Z", fill: "#15181d"
            }));
            [24, 50, 76].forEach(function (x, i) {
                g.appendChild(svgEl("rect", { x: x, y: 30 + i % 2 * 4, width: 1.6, height: 18, fill: "#15181d" }));
            });
            vignette(g, d);
        }
    };

    // Couleur de coque par famille : la carte annonce son domaine avant d'être lue.
    var FAMILY_ACCENT = {
        "Économique": "#b8860b",
        "Politique occidentale": "#1e5fa8",
        "Politique interne": "#8a4b2a",
        "Énergie": "#c2621a",
        "Militaire et technologique": "#4a6070",
        "Externe": "#4a6d3a",
        "": "#6b7280"
    };

    // Un champ vide ne doit jamais produire une zone vide : il produit une mention.
    function safeText(value, fallback) {
        var s = value === null || value === undefined ? "" : String(value).trim();
        return s.length ? s : fallback;
    }

    // La ligne de type est recomposée quand le moteur ne la fournit pas, pour que le
    // bandeau garde sa hauteur et son sens plutôt que de tomber sur une bande vide.
    function typeLineOf(card) {
        var family = safeText(card.family, "Famille inconnue");
        return safeText(card.typeLine, family);
    }

    // Une carte d'événement ne se paie pas : elle n'a pas de médaillon du tout. Un médaillon
    // sans chiffre se lit comme un défaut d'affichage, pas comme une gratuité — le titre
    // occupe alors tout le cartouche, ce qui est la bonne façon de dire « rien à payer ».
    function costPips(card) {
        var pol = Number(card.politicalCost) || 0;
        var money = Number(card.moneyCost) || 0;
        if (pol <= 0 && money <= 0) { return null; }

        var cost = el("div", "mtg-cost");
        if (pol > 0) {
            var polPip = el("span", "pip pol", String(Math.round(pol)));
            polPip.title = "Coût politique : " + Math.round(pol);
            cost.appendChild(polPip);
        }
        if (money > 0) {
            var moneyPip = el("span", "pip money", String(Math.round(money)));
            moneyPip.title = "Coût financier : " + Math.round(money) + " Md";
            cost.appendChild(moneyPip);
        }

        return cost;
    }

    function renderCard(card) {
        card = card || {};
        var wrap = el("div", "mtg " + (card.ownerSideCode === "invader"
            ? "ru"
            : (card.ownerSideCode === "defender" ? "ua" : "neutral")));

        var family = safeText(card.family, "");
        var accent = FAMILY_ACCENT[family] || FAMILY_ACCENT[""];
        wrap.style.setProperty("--fam", accent);
        wrap.style.setProperty("--fam-1", tint(accent, -0.62));
        wrap.style.setProperty("--fam-2", tint(accent, -0.86));
        var inner = el("div", "mtg-inner");

        var title = el("div", "mtg-title");
        var name = el("div", "mtg-name", safeText(card.title, "Carte sans titre"));
        // Un titre long se compose plus petit plutôt que de pousser l'illustration vers
        // le bas : toutes les cartes de la main gardent le même gabarit.
        if (name.textContent.length > 32) { name.classList.add("is-longer"); }
        else if (name.textContent.length > 24) { name.classList.add("is-long"); }
        title.appendChild(name);
        var cost = costPips(card);
        if (cost) { title.appendChild(cost); }
        inner.appendChild(title);

        var art = el("div", "mtg-art");
        var svg = svgEl("svg", { viewBox: "0 0 100 60", preserveAspectRatio: "xMidYMid slice" });
        (ART[family] || ART[""])(svg);
        art.appendChild(svg);
        inner.appendChild(art);

        inner.appendChild(el("div", "mtg-type", typeLineOf(card)));

        var text = el("div", "mtg-text");
        var rulesText = (card.rulesText || []).filter(function (r) { return safeText(r, ""); });
        if (rulesText.length) {
            var rules = el("ul", "mtg-rules");
            rulesText.forEach(function (r) { rules.appendChild(el("li", null, r)); });
            text.appendChild(rules);
        } else {
            text.appendChild(el("p", "mtg-norule", "Aucun effet chiffré : la carte agit par la situation qu'elle installe."));
        }
        text.appendChild(el("p", "mtg-flavour", safeText(card.description, "Pas de texte d'ambiance pour cette carte.")));
        inner.appendChild(text);

        wrap.appendChild(inner);

        var foot = el("div", "mtg-foot");
        foot.appendChild(el("span", "fam", safeText(card.family, "Famille inconnue")));
        foot.appendChild(el("span", null, "TOV · V1"));
        wrap.appendChild(foot);

        return wrap;
    }

    function renderBattlefield() {
        var stage = document.getElementById("stage");
        var t = turn();

        var head = el("div", "stage-head");
        head.appendChild(el("h2", null, "Résolution — champ de bataille"));
        // Only the movement of the eight simulated sectors is counted, never the initial
        // rush of 2022 nor the ground given back that autumn. Saying « depuis février 2022 »
        // invited the reader to compare it with the seventy thousand square kilometres of
        // the real war, which is not what this number measures.
        head.appendChild(el("div", "turn-tag",
            "Tour " + t.turn + " · " + (SEASONS[t.season] || t.season) + " " + t.year +
            " · " + fmt(t.squareKilometresGained) + " km² pris sur les secteurs simulés"));
        stage.appendChild(head);

        // On the last turn of a run that stopped early, say so: the timeline stops here
        // because the war did, not because the button failed.
        var g = game();
        if (g.endedEarly && state.turnIndex === g.turns.length - 1) {
            var stop = el("div", "run-ended");
            stop.innerHTML = "<strong>La guerre s'arrête ici.</strong> Ce déroulé se termine au tour " +
                g.turns.length + ", " + (SEASONS[t.season] || t.season) + " " + t.year +
                " — les " + (g.plannedTurns - g.turns.length) +
                " trimestres suivants du calendrier n'ont pas été joués.";
            stage.appendChild(stop);
        }

        // No cards here at all: they are played on each side's own screen, where the
        // decision is taken. The resolution shows only what the front made of it.

        var field = el("div", "field");

        var mapPanel = el("section", "panel map-panel");
        // The hex map owns its own file so it can be reworked without touching the rest.
        var hexMap = window.tovHexMap && window.tovHexMap.render;
        var mapSvg = hexMap
            ? window.tovHexMap.render(t, board, geo, { frontLine: frontLine })
            : renderMap(t);
        mapPanel.appendChild(mapSvg);

        // La carte hexagonale porte sa propre légende : n'en afficher une seconde
        // que pour le tracé de repli, sinon les deux se contredisent.
        if (!hexMap) {
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
        }
        var leftCol = el("div");
        leftCol.appendChild(mapPanel);
        field.appendChild(leftCol);

        // Le journal appartient à la colonne de la carte : les deux racontent le même trimestre.
        if (t.narrative && t.narrative.length) {
            var narr = el("section", "panel narrative");
            narr.style.marginTop = "16px";
            narr.appendChild(el("div", "panel-title", "Journal du trimestre"));
            var ul = el("ul");
            t.narrative.forEach(function (n) {
                var li = el("li", null, n);
                // Chaque ligne s'ouvre sur un camp : on le rend repérable au liseré.
                if (/^Russie\b/.test(n)) { li.className = "ru"; }
                else if (/^Ukraine\b/.test(n)) { li.className = "ua"; }
                ul.appendChild(li);
            });
            narr.appendChild(ul);
            leftCol.appendChild(narr);
        }

        var right = el("div");
        var sectorPanel = el("section", "panel");
        sectorPanel.style.padding = "16px 18px";
        sectorPanel.appendChild(el("div", "panel-title", "Rapport de force par secteur"));

        var sectors = t.sectors || [];
        var movers = sectors.filter(function (s) { return Math.abs(s.hexesMoved) > 0.01; });
        var frozen = sectors.filter(function (s) { return Math.abs(s.hexesMoved) <= 0.01; });
        var netKm = movers.reduce(function (a, s) { return a + s.hexesMoved * 10; }, 0);

        // Une phrase avant la liste : ce que le trimestre a produit, en un coup d'œil.
        var summary = el("div", "sector-summary");
        summary.innerHTML = movers.length === 0
            ? "<b>Aucun secteur</b> n'a bougé ce trimestre sur " + sectors.length + "."
            : "<b>" + movers.length + " secteur" + (movers.length > 1 ? "s" : "") + " sur " + sectors.length +
              "</b> " + (movers.length > 1 ? "ont bougé" : "a bougé") + ", pour un solde de <b class=\"" +
              (netKm >= 0 ? "ru" : "ua") + "\">" +
              (netKm >= 0 ? "+" : "−") + fmt(Math.abs(netKm), 1) + " km</b>.";
        sectorPanel.appendChild(summary);

        // Face à une défense effondrée le rapport tend vers l'infini : le chiffre exact
        // n'apprend plus rien, seul compte le fait que plus rien ne tient en face.
        function ratioLabel(ratio) {
            return ratio > 12 ? "défense rompue" : "rapport " + fmt(ratio, 2);
        }

        // Un secteur ne bouge qu'au-delà d'un rapport de 1,1 : l'échelle s'arrête à 3.
        function ratioGauge(s, withScale) {
            var bar = el("div", "ratiobar");
            var fill = el("span");
            fill.style.width = Math.min(100, (s.ratio / 3) * 100).toFixed(0) + "%";
            fill.style.background = s.ratio >= 1.1 ? "#a8322a" : "#9b9484";
            bar.appendChild(fill);
            var th = el("div", "threshold");
            th.style.left = (1.1 / 3 * 100).toFixed(0) + "%";
            th.title = "Seuil de mouvement : 1,1";
            if (withScale) { th.appendChild(el("i", null, "1,1")); }
            bar.appendChild(th);
            return bar;
        }

        var list = el("div", "sector-list");
        movers.forEach(function (s) {
            var row = el("div", "sector " + (s.hexesMoved > 0 ? "gain" : "loss"));
            row.appendChild(el("div", "sc-name", s.sectorName));
            row.appendChild(el("div", "sc-move",
                (s.hexesMoved > 0 ? "+" : "−") + fmt(Math.abs(s.hexesMoved * 10), 1) + " km"));
            row.appendChild(el("div", "sc-outcome", s.outcome));
            row.appendChild(el("div", "sc-ratio", ratioLabel(s.ratio)));
            row.appendChild(ratioGauge(s, true));
            list.appendChild(row);
        });
        sectorPanel.appendChild(list);

        // Les secteurs figés disent tous la même chose : ils passent en second plan,
        // ramenés à leur seule information distinctive, le rapport de force.
        if (frozen.length) {
            var frozenHead = el("div", "frozen-head");
            frozenHead.innerHTML = "<span>" + frozen.length + " secteurs figés</span><em>" +
                (frozen[0].outcome || "usure réciproque") + "</em>";
            sectorPanel.appendChild(frozenHead);

            var frozenList = el("div", "sector-list frozen");
            frozen.forEach(function (s) {
                var row = el("div", "sector static");
                row.title = s.outcome;
                row.appendChild(el("div", "sc-name", s.sectorName));
                row.appendChild(el("div", "sc-ratio", fmt(s.ratio, 2)));
                row.appendChild(ratioGauge(s, false));
                frozenList.appendChild(row);
            });
            sectorPanel.appendChild(frozenList);
        }

        right.appendChild(sectorPanel);
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

    document.getElementById("firstTurn").addEventListener("click", function () {
        state.turnIndex = 0;
        render();
    });

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

    // Surface d'inspection : le deck complet se dessine hors partie, carte par carte, pour
    // vérifier qu'aucune ne sort vide, tronquée ou débordante. La partie n'en joue qu'une
    // poignée — le reste ne se contrôle qu'ici.
    window.tovCards = { render: renderCard, hand: handFor, size: HAND_SIZE };

    bindPhases();
    state.turnIndex = openingTurnIndex(game());
    render();
})();
