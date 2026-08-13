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

    // The opening quarter is found by its own number, never by converting that number into
    // an array index: the scenario now starts on a 2021 prologue and will keep moving, and
    // a numbering that no longer begins at one would silently open a quarter off.
    function openingTurnIndex(g) {
        var wanted = window.tovCurrentTurn || 1;
        var fallback = 0;

        for (var i = 0; i < g.turns.length; i++) {
            if (g.turns[i].turn === wanted) { return i; }
            // The run stopped before the quarter we are living in: open on the last one it played.
            if (g.turns[i].turn < wanted) { fallback = i; }
        }

        return fallback;
    }

    var state = { gameIndex: 0, turnIndex: 0, phase: 0 };

    var SEASONS = { Winter: "hiver", Spring: "printemps", Summer: "été", Autumn: "automne" };
    var QUARTERS = { Winter: 1, Spring: 2, Summer: 3, Autumn: 4 };

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

    // The quarters a run never reached carry no season of their own: they are counted from
    // the last one it did play. Deriving them from a hard-coded origin was correct only as
    // long as turn 1 was February 2022 — the scenario now opens on the autumn 2021 prologue,
    // and that origin would have dated every unplayed quarter one season off.
    function quarterAfter(last, steps) {
        var order = ["Winter", "Spring", "Summer", "Autumn"];
        var from = order.indexOf(last.season);
        if (from < 0) { return { quarter: 0, year: last.year }; }

        var total = from + steps;
        return { quarter: (total % 4) + 1, year: last.year + Math.floor(total / 4) };
    }

    // La frise se lit au calendrier, pas au compteur de parties : « T1 22 » est le premier
    // trimestre de 2022, quel que soit le rang de ce trimestre dans le scénario. Le numéro
    // de tour reste dans l'infobulle, pour qui suit la mécanique.
    function quarterLabel(q) {
        return "T" + q.quarter + " " + String(q.year).slice(2);
    }

    // Le rang du tour n'intéresse personne : ce qui situe un trimestre, c'est sa date. Les
    // bandeaux de titre portent donc la saison et l'année, jamais le compteur.
    function dateOf(t) {
        var season = SEASONS[t.season] || t.season;
        return season.charAt(0).toUpperCase() + season.slice(1) + " " + t.year;
    }

    function dateInOf(t) {
        var articles = { Winter: "à l'hiver ", Spring: "au printemps ", Summer: "à l'été ", Autumn: "à l'automne " };
        return (articles[t.season] || "en ") + t.year;
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
                ? "La guerre s'est terminée ici : il n'y a pas de trimestre suivant."
                : "Dernier trimestre de la partie.")
            : "Trimestre suivant";

        var planned = g.plannedTurns || g.turns.length;
        var lastIndex = g.turns.length - 1;
        var last = g.turns[lastIndex];

        for (var i = 0; i < planned; i++) {
            var played = i < g.turns.length;
            var t = played ? g.turns[i] : null;
            // A played quarter carries its own number; only the quarters the run never
            // reached have to be deduced from their position.
            var number = played ? t.turn : last.turn + (i - lastIndex);
            var q = played
                ? { quarter: QUARTERS[t.season] || 0, year: t.year }
                : quarterAfter(last, i - lastIndex);

            var hasCard = played && t.cardsPlayed && t.cardsPlayed.length > 0;
            var cls = "tick";
            if (played && i === state.turnIndex) { cls += " active"; }
            if (hasCard) { cls += " has-card"; }
            // The site is about a war that is still being fought: what has happened and what
            // the model projects must never be read as the same thing.
            if (number > HISTORY_TURNS) { cls += " projected"; }
            // Beyond the last played turn the war is over: the quarter still shows, greyed,
            // so the timeline never looks like a broken button.
            if (!played) { cls += " unplayed"; }

            var b = el("button", cls);
            b.type = "button";
            b.disabled = !played;
            // Trimestre au-dessus, millésime au-dessous : « T4 21 » d'un seul tenant se lit
            // « 14 21 » dans une graisse de titre. Deux lignes, et chacune respire.
            var label = el("span", "t-quarter");
            label.appendChild(el("b", null, "T" + q.quarter));
            label.appendChild(el("i", null, String(q.year)));
            b.appendChild(label);

            if (played) {
                var season = SEASONS[t.season] || t.season;
                b.title = season.charAt(0).toUpperCase() + season.slice(1) + " " + t.year;
                (function (index) {
                    b.addEventListener("click", function () { state.turnIndex = index; render(); });
                })(i);
            } else {
                b.title = "La guerre s'est terminée avant ce trimestre : il n'a pas été joué.";
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
            dateOf(t) + " · Brent " + fmt(t.oilPrice) + " $"));
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
       Une scène par sujet, jamais par famille : sur un jeu de cartes, l'illustration
       est ce qui fait reconnaître une carte avant même d'en lire le titre. Deux cartes
       qui racontent la même chose sous deux angles peuvent partager un motif ; deux
       cartes de sens opposé, jamais.

       Grammaire commune à toutes : ciel dégradé accordé à la teinte de la famille,
       une source de lumière unique, un plan intermédiaire, une silhouette au premier
       plan, un voile sombre en bas. La profondeur fait l'illustration ; le pictogramme
       ferait un panneau de signalisation.

       Les scènes se composent à partir des primitives qui suivent : c'est ce qui tient
       la cohérence du deck sur une centaine de vignettes, et ce qui les garde lisibles
       à deux cents pixels de large. */

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

    /* --- Matière commune ------------------------------------------------- */

    // Le ciel appartient à la famille, pas à la scène : c'est ce qui fait qu'un deck de
    // cent cartes se lit comme un deck et non comme une collection d'images.
    var SKY = {
        "Économique": [["0%", "#241a17"], ["52%", "#6d4630"], ["100%", "#d59a5c"]],
        "Politique occidentale": [["0%", "#0c1c33"], ["56%", "#1d3f6b"], ["100%", "#6f93bf"]],
        "Politique interne": [["0%", "#241b16"], ["54%", "#54382a"], ["100%", "#a9784f"]],
        "Énergie": [["0%", "#101c28"], ["58%", "#3b3524"], ["100%", "#c4761f"]],
        "Militaire et technologique": [["0%", "#0d1826"], ["60%", "#274056"], ["100%", "#8ba5b8"]],
        "Externe": [["0%", "#1b2a26"], ["54%", "#4b5f4c"], ["100%", "#b6bf94"]],
        "": [["0%", "#1d2129"], ["58%", "#454b52"], ["100%", "#9a9384"]]
    };

    // Encre chaude : sur du papier, un noir froid sonne écran.
    var INK = "#12100d";
    var PAPER = "#efe6d2";
    var WARM = "#f6d097";
    var FIRE = "#e8721f";
    var ALERT = "#e8746a";
    var SIGNAL = "#8fd0e8";

    function shape(g, path, fill, op) {
        g.appendChild(svgEl("path", {
            d: path, fill: fill || INK, opacity: op === undefined ? "1" : String(op)
        }));
    }

    function box(g, x, y, w, h, fill, op) {
        g.appendChild(svgEl("rect", {
            x: x, y: y, width: w, height: h, fill: fill || INK,
            opacity: op === undefined ? "1" : String(op)
        }));
    }

    function disc(g, x, y, r, fill, op) {
        g.appendChild(svgEl("circle", {
            cx: x, cy: y, r: r, fill: fill || INK, opacity: op === undefined ? "1" : String(op)
        }));
    }

    function stroke(g, path, colour, w, op) {
        g.appendChild(svgEl("path", {
            d: path, fill: "none", stroke: colour, "stroke-width": String(w || 1),
            "stroke-linecap": "round", "stroke-linejoin": "round",
            opacity: op === undefined ? "1" : String(op)
        }));
    }

    // Source de lumière unique, posée bas : c'est elle qui donne la profondeur.
    function sun(g, x, y, r, fill, op) {
        disc(g, x, y, r, fill || WARM, op === undefined ? 0.8 : op);
    }

    // Le halo déborde de sa source, sinon la lumière reste un disque collé au fond.
    function glow(g, d, colour, cx, cy, r, op) {
        box(g, 0, 0, 100, 60, radGrad(d,
            [["0%", colour, op === undefined ? 0.5 : op], ["100%", colour, 0]], cx, cy, r || "0.55"));
    }

    // Sol plat : la scène se ferme sur une masse sombre.
    function ground(g, y, fill) {
        box(g, 0, y, 100, 60 - y, fill);
    }

    // Horizon vallonné : le plein air, sans dessiner de paysage.
    function ridge(g, y, fill) {
        shape(g, "M0 60 L0 " + y + " Q26 " + (y - 5) + " 50 " + (y - 1) +
                 " Q76 " + (y + 3) + " 100 " + (y - 4) + " L100 60 Z", fill);
    }

    // Une silhouette humaine : tête, épaules, buste qui s'évase. C'est la seule échelle qui
    // parle — et il faut les épaules, sinon une tête posée sur un dôme fait une note de
    // musique plutôt qu'un homme.
    function figure(g, x, y, h, fill) {
        var w = h * 0.52;
        disc(g, x, y - h * 0.84, h * 0.17, fill);
        shape(g, "M" + (x - w * 0.5) + " " + y +
                 " L" + (x - w * 0.44) + " " + (y - h * 0.5) +
                 " Q" + (x - w * 0.4) + " " + (y - h * 0.68) + " " + (x - h * 0.1) + " " + (y - h * 0.7) +
                 " L" + (x + h * 0.1) + " " + (y - h * 0.7) +
                 " Q" + (x + w * 0.4) + " " + (y - h * 0.68) + " " + (x + w * 0.44) + " " + (y - h * 0.5) +
                 " L" + (x + w * 0.5) + " " + y + " Z", fill);
    }

    // Le fusil se porte à l'épaule, en oblique : c'est ce qui distingue une colonne
    // d'hommes mobilisés d'une file d'attente.
    function rifle(g, x, y, h, fill) {
        stroke(g, "M" + (x + h * 0.22) + " " + (y - h * 0.34) + " L" + (x + h * 0.42) + " " + (y - h * 1.1),
               fill || INK, h * 0.09);
    }

    // Une foule de dos : des nuques, jamais des visages.
    function crowd(g, y, from, step, n, r, fill) {
        for (var i = 0; i < n; i++) {
            var x = from + i * step;
            var yy = y + (i % 3) * (r * 0.5);
            shape(g, "M" + (x - r * 1.8) + " 60 Q" + x + " " + (yy + r * 0.5) + " " +
                     (x + r * 1.8) + " 60 Z", fill);
            disc(g, x, yy, r, fill);
        }
    }

    // Caisse de matériel : le flux étranger, quantifié en boîtes.
    function crate(g, x, y, w, h, fill) {
        box(g, x, y, w, h, fill || "#c8a86a");
        box(g, x, y, w, h * 0.24, "#fff", 0.18);
        for (var k = 1; k < 3; k++) {
            g.appendChild(svgEl("line", {
                x1: x + k * (w / 3), y1: y + 0.6, x2: x + k * (w / 3), y2: y + h - 0.6,
                stroke: "#000", opacity: "0.22", "stroke-width": "0.6"
            }));
        }
    }

    // Flamme : une seule, jamais un incendie décoratif.
    function flame(g, x, y, s, d) {
        shape(g, "M" + x + " " + y + " Q" + (x - 2.6 * s) + " " + (y - 4 * s) + " " +
                 (x - 0.3 * s) + " " + (y - 8 * s) + " Q" + (x + 0.7 * s) + " " + (y - 4.6 * s) + " " +
                 (x + 2.4 * s) + " " + (y - 6 * s) + " Q" + (x + 3 * s) + " " + (y - 1.6 * s) + " " +
                 x + " " + y + " Z",
             d ? linGrad(d, [["0%", "#fff0c0"], ["100%", FIRE]]) : FIRE);
    }

    // Drone : aile delta de trois quarts, l'objet le plus fréquent du deck.
    function droneShape(g, x, y, s, flip) {
        var f = flip ? -1 : 1;
        shape(g, "M" + x + " " + y + " L" + (x + 17 * s * f) + " " + (y + 2 * s) +
                 " L" + (x + 6 * s * f) + " " + (y + 4 * s) + " L" + (x + 3 * s * f) + " " + (y + 6 * s) +
                 " L" + (x + 2 * s * f) + " " + (y + 3 * s) + " L" + (x - 3 * s * f) + " " + (y + 2.5 * s) + " Z");
        shape(g, "M" + (x + 6 * s * f) + " " + (y + 2 * s) + " L" + (x + 10 * s * f) + " " + (y - 4 * s) +
                 " L" + (x + 11 * s * f) + " " + (y - 3.4 * s) + " L" + (x + 8 * s * f) + " " + (y + 2.6 * s) + " Z");
        disc(g, x + 15 * s * f, y + 2 * s, 0.7 * s, "#ff6b5e");
    }

    // Pylône de ligne à haute tension.
    function pylon(g, x, y, h) {
        var w = h * 0.34;
        stroke(g, "M" + (x - w / 2) + " " + y + " L" + x + " " + (y - h) + " L" + (x + w / 2) + " " + y +
                  " M" + (x - w * 0.34) + " " + (y - h * 0.42) + " L" + (x + w * 0.34) + " " + (y - h * 0.42) +
                  " M" + (x - w * 0.2) + " " + (y - h * 0.72) + " L" + (x + w * 0.2) + " " + (y - h * 0.72),
               INK, h * 0.07);
        stroke(g, "M" + (x - w * 0.62) + " " + (y - h * 0.86) + " L" + (x + w * 0.62) + " " + (y - h * 0.86),
               INK, h * 0.07);
    }

    // Feuille de papier : décrets, contrats, registres, plans. Sur un fond sombre,
    // c'est la forme claire qui porte le sens.
    function sheet(g, x, y, w, h, tilt, lines) {
        var t = tilt || 0;
        var s = svgEl("g", { transform: "rotate(" + t + " " + (x + w / 2) + " " + (y + h / 2) + ")" });
        g.appendChild(s);
        box(s, x, y, w, h, PAPER, 0.94);
        box(s, x, y, w, h * 0.1, "#fff", 0.5);
        for (var i = 0; i < (lines === undefined ? 4 : lines); i++) {
            box(s, x + w * 0.14, y + h * (0.26 + i * 0.16), w * (i % 2 ? 0.5 : 0.68), h * 0.06, INK, 0.42);
        }
        return s;
    }

    // Sceau : ce qui rend un document opposable — ou ce qu'on brise.
    function seal(g, x, y, r, colour) {
        disc(g, x, y, r, colour || "#a8322a");
        disc(g, x, y, r * 0.62, "#fff", 0.22);
    }

    // Courbe : ce que la comptabilité raconte, en une ligne.
    function curve(g, pts, colour, w) {
        var d = pts.map(function (p, i) { return (i ? "L" : "M") + p[0] + " " + p[1]; }).join(" ");
        stroke(g, d, colour || ALERT, w || 2.2);
        pts.forEach(function (p, i) {
            if (i % 2 === 0) { disc(g, p[0], p[1], 1.7, colour || ALERT, 0.85); }
        });
    }

    // Navire vu de flanc : coque basse, superstructure arrière.
    function ship(g, x, y, s, fill) {
        shape(g, "M" + (x - 20 * s) + " " + y + " L" + (x + 20 * s) + " " + y +
                 " L" + (x + 16 * s) + " " + (y + 4 * s) + " L" + (x - 16 * s) + " " + (y + 4 * s) + " Z", fill);
        box(g, x + 6 * s, y - 5 * s, 8 * s, 5 * s, fill);
        box(g, x + 9 * s, y - 8 * s, 2 * s, 3 * s, fill);
    }

    // Voie ferrée en fuite : deux files qui convergent, des traverses qui se resserrent
    // vers le fond. Les traverses restent entre les files, sinon on lit un chevron.
    function rails(g, y) {
        for (var i = 0; i < 9; i++) {
            var f = Math.pow(i / 8, 0.75);
            var yy = 60 - (60 - y) * f;
            var xl = 20 + 27 * f, xr = 80 - 27 * f;
            stroke(g, "M" + (xl - 4 * (1 - f)) + " " + yy + " L" + (xr + 4 * (1 - f)) + " " + yy,
                   INK, 2.6 - f * 2);
        }
        stroke(g, "M20 60 L47 " + y + " M80 60 L53 " + y, INK, 2.6);
    }

    // Coupole de protection : le geste « on couvre ce qui est en dessous ».
    function dome(g, cx, cy, r, colour) {
        stroke(g, "M" + (cx - r) + " " + cy + " A" + r + " " + r + " 0 0 1 " + (cx + r) + " " + cy,
               colour || SIGNAL, 1.5, 0.85);
        stroke(g, "M" + (cx - r * 0.72) + " " + cy + " A" + (r * 0.72) + " " + (r * 0.72) +
                  " 0 0 1 " + (cx + r * 0.72) + " " + cy, colour || SIGNAL, 1, 0.5);
    }

    // Faisceau de projecteur : ce que le régime éclaire, et ce qu'il laisse dans l'ombre.
    function beam(g, x, y, spread, len, colour) {
        shape(g, "M" + x + " " + y + " L" + (x - spread) + " " + (y + len) + " L" + (x + spread) + " " +
                 (y + len) + " Z", colour || WARM, 0.2);
    }

    /* --- Les scènes, une par sujet ---------------------------------------
       Chaque scène reçoit le groupe et ses defs : le ciel de la famille et le voile du
       bas sont posés autour d'elle, elle ne dessine que son sujet. */

    var ART = {

        /* Économie de guerre et sanctions ---------------------------------- */

        // Ce qui interdit tient en une feuille et un sceau.
        sanctions_decree: function (g, d) {
            glow(g, d, "#f0dcc0", "0.5", "0.34", "0.6", 0.3);
            ridge(g, 52);
            sheet(g, 32, 7, 37, 40, -5, 5);
            seal(g, 62, 40, 6);
        },

        // La machine-outil sous scellés : la sanction qui mord est celle qui ne se voit pas.
        machine_embargo: function (g, d) {
            glow(g, d, "#f0dcc0", "0.34", "0.34", "0.6", 0.3);
            ground(g, 48);
            // La machine-outil d'abord, les scellés ensuite : c'est l'outil qui manque,
            // pas l'interdiction qui décore.
            box(g, 22, 18, 40, 30);
            box(g, 27, 24, 30, 14, "#3a2c1c");
            box(g, 33, 10, 8, 14);
            box(g, 29, 6, 16, 5);
            box(g, 62, 32, 20, 16);
            box(g, 62, 32, 20, 3, "#4a3a24");
            stroke(g, "M16 42 L70 20", "#a8322a", 2.4, 0.95);
            seal(g, 43, 31, 4.6);
        },

        // Deux majors désignées : on cesse de plafonner le prix pour nommer les vendeurs.
        oil_majors: function (g, d) {
            glow(g, d, WARM, "0.5", "0.42", "0.6", 0.32);
            ridge(g, 48);
            // Deux derricks en treillis : on cesse de plafonner le prix pour désigner
            // nommément les vendeurs, et le cercle rouge dit lequel.
            [28, 66].forEach(function (x) {
                stroke(g, "M" + (x - 11) + " 48 L" + x + " 10 L" + (x + 11) + " 48", INK, 2.6);
                [20, 28, 36, 44].forEach(function (y, k) {
                    var w = 2 + k * 2.6;
                    stroke(g, "M" + (x - w) + " " + y + " L" + (x + w) + " " + y, INK, 1.6);
                });
                stroke(g, "M" + (x - 6) + " 30 L" + (x + 6) + " 40 M" + (x + 6) + " 30 L" + (x - 6) + " 40", INK, 1.2, 0.7);
                box(g, x - 3, 6, 6, 5, INK);
            });
            [28, 66].forEach(function (x) {
                g.appendChild(svgEl("circle", {
                    cx: x, cy: 28, r: 20, fill: "none", stroke: "#a8322a", "stroke-width": "2.4", opacity: "0.9"
                }));
            });
        },

        // La flotte fantôme : des coques sans pavillon, feux éteints dans la brume.
        ghost_fleet: function (g, d) {
            glow(g, d, "#cbd8dd", "0.28", "0.3", "0.6", 0.3);
            box(g, 0, 44, 100, 16, "#1c211f");
            ship(g, 34, 42, 1, "#0f1412");
            ship(g, 72, 36, 0.62, "#161b19");
            [46, 50].forEach(function (y) {
                stroke(g, "M0 " + y + " L100 " + y, "#c9d6ae", 0.5, 0.16);
            });
            [20, 44, 68].forEach(function (x, i) {
                box(g, x, 24 + i * 3, 34, 2.4, "#e8e2d2", 0.14);
            });
        },

        // Contourner : la route officielle est barrée, la marchandise passe à côté.
        detour: function (g, d) {
            glow(g, d, WARM, "0.72", "0.36", "0.55", 0.28);
            ridge(g, 46);
            box(g, 40, 20, 20, 3.4, "#a8322a");
            box(g, 40, 20, 3, 20, "#a8322a");
            box(g, 57, 20, 3, 20, "#a8322a");
            stroke(g, "M8 50 Q26 50 30 34 Q34 16 52 12 Q72 8 92 18", PAPER, 2, 0.55);
            crate(g, 74, 20, 13, 7);
        },

        // Réorientation vers l'Asie : la même cargaison, un autre acheteur.
        east_flow: function (g, d) {
            glow(g, d, "#f4b46a", "0.82", "0.42", "0.6", 0.45);
            sun(g, 84, 34, 11, "#f4c98a", 0.75);
            box(g, 0, 44, 100, 16, "#241d16");
            ship(g, 30, 43, 1.05, "#100d0a");
            stroke(g, "M46 26 L82 26", PAPER, 1.8, 0.7);
            shape(g, "M82 26 L74 22 L74 30 Z", PAPER, 0.7);
        },

        // Le port céréalier : le blé n'a jamais tiré un coup de feu.
        grain_port: function (g, d) {
            glow(g, d, FIRE, "0.62", "0.44", "0.55", 0.4);
            box(g, 0, 46, 100, 14, "#1a1510");
            [16, 28, 40, 60, 72].forEach(function (x) {
                box(g, x, 18, 9, 28);
                shape(g, "M" + x + " 18 L" + (x + 4.5) + " 13 L" + (x + 9) + " 18 Z");
            });
            flame(g, 64, 13, 1.1, d);
            box(g, 0, 50, 100, 1.6, "#5d6a4a", 0.5);
        },

        // L'usine bascule : ce qu'elle fabriquait ne compte plus.
        war_factory: function (g, d) {
            glow(g, d, WARM, "0.34", "0.3", "0.6", 0.3);
            ground(g, 44);
            shape(g, "M0 44 L0 30 L10 22 L20 30 L20 44 L30 44 L30 26 L40 18 L50 26 L50 44 Z");
            box(g, 12, 8, 4, 16);
            box(g, 42, 4, 4, 16);
            [58, 68, 78, 88].forEach(function (x) {
                box(g, x, 34, 6, 10, "#2b2119");
                shape(g, "M" + x + " 34 L" + (x + 3) + " 30 L" + (x + 6) + " 34 Z", "#2b2119");
            });
        },

        // Réquisition : l'usine reste privée sur le papier, elle ne choisit plus ses commandes.
        requisition: function (g, d) {
            glow(g, d, WARM, "0.5", "0.34", "0.5", 0.26);
            ground(g, 48);
            box(g, 20, 12, 60, 36, "#211a13");
            box(g, 34, 22, 32, 26, "#100c08");
            stroke(g, "M28 34 L72 26", PAPER, 2.6, 0.9);
            seal(g, 50, 30, 6);
        },

        // L'impôt de guerre : on cesse de faire payer la réserve pour faire payer ceux qui travaillent.
        war_tax: function (g, d) {
            glow(g, d, WARM, "0.5", "0.32", "0.55", 0.28);
            ridge(g, 50);
            sheet(g, 22, 12, 30, 34, -6, 4);
            seal(g, 46, 40, 5);
            [64, 72, 80].forEach(function (x, i) {
                var h = 6 + i * 5;
                box(g, x, 46 - h, 7, h, "#c8a86a");
                box(g, x, 46 - h, 7, 1.4, "#fff", 0.25);
            });
            shape(g, "M86 20 L86 34 L92 27 Z", ALERT);
        },

        // Le fonds souverain : on y puise, puis il n'y a plus rien à puiser.
        vault: function (empty) {
            return function (g, d) {
                glow(g, d, empty ? "#8d8578" : WARM, "0.5", "0.36", "0.55", empty ? 0.22 : 0.34);
                ground(g, 48);
                box(g, 24, 20, 52, 28, "#191309");
                shape(g, "M24 20 L34 12 L86 12 L76 20 Z", "#241a10");
                box(g, 76, 12, 10, 28, "#120d07");
                box(g, 28, 24, 44, 20, empty ? "#241c12" : "#0b0805");
                if (empty) {
                    // Le fond du coffre se voit : c'est ce qui dit qu'il n'y a plus rien.
                    box(g, 28, 38, 44, 6, "#3a3128");
                    stroke(g, "M32 38 L68 38", "#5c5346", 1.4);
                    disc(g, 42, 41, 2, "#6b6152");
                    disc(g, 58, 42, 1.4, "#6b6152", 0.7);
                } else {
                    [34, 42, 50, 58].forEach(function (x, i) {
                        box(g, x, 40 - i % 2 * 3, 7, 4, "#d8b45c");
                        box(g, x, 40 - i % 2 * 3, 7, 1, "#fff", 0.3);
                    });
                    shape(g, "M64 22 L64 34 L70 28 Z", WARM, 0.9);
                }
            };
        },

        // Le baril et son cours : la même scène, trois histoires opposées.
        barrel: function (dir) {
            return function (g, d) {
                glow(g, d, WARM, "0.28", "0.36", "0.55", 0.3);
                ridge(g, 50);
                box(g, 12, 22, 22, 28, "#2a1f14");
                [24, 32, 40].forEach(function (y) {
                    box(g, 12, y, 22, 2.2, "#6b5330");
                });
                shape(g, "M12 22 Q23 18 34 22 L34 24 Q23 20 12 24 Z", "#7a5f38");
                if (dir > 0) {
                    curve(g, [[42, 44], [56, 34], [68, 38], [82, 18], [94, 10]]);
                } else if (dir < 0) {
                    curve(g, [[42, 12], [56, 22], [68, 18], [82, 36], [94, 44]]);
                } else {
                    // Le plafond : la courbe monte et vient s'écraser dessous.
                    curve(g, [[42, 44], [56, 32], [70, 26], [84, 25], [94, 25]]);
                    g.appendChild(svgEl("line", {
                        x1: 38, y1: 16, x2: 98, y2: 16, stroke: PAPER, "stroke-width": "2",
                        "stroke-dasharray": "5 3", opacity: "0.95"
                    }));
                    [[46, 22], [66, 20], [86, 20]].forEach(function (p) {
                        stroke(g, "M" + p[0] + " " + p[1] + " L" + p[0] + " 17", PAPER, 1, 0.55);
                    });
                }
            };
        },

        // L'atelier : cent ateliers plutôt qu'une usine, rien de décisif à frapper.
        drone_workshop: function (g, d) {
            glow(g, d, WARM, "0.5", "0.34", "0.65", 0.42);
            ground(g, 50);
            // Cent ateliers plutôt qu'une usine : les établis sont éclairés un à un, et
            // c'est l'appareil posé dessus qu'on doit reconnaître, pas le hangar.
            [4, 38, 72].forEach(function (x, i) {
                box(g, x, 40, 26, 3.4, "#2c2114");
                box(g, x + 2, 43.4, 3, 7, "#1d160f");
                box(g, x + 21, 43.4, 3, 7, "#1d160f");
                disc(g, x + 13, 22, 9, WARM, 0.16);
                droneShape(g, x + 2, 32, 0.72);
                box(g, x + 11, 12, 4, 4, "#1d160f");
                stroke(g, "M" + (x + 13) + " 16 L" + (x + 13) + " 20", "#1d160f", 1);
            });
        },

        // La demande mondiale recule, et le baril suit.
        world_slump: function (g, d) {
            glow(g, d, "#e8dcc0", "0.34", "0.32", "0.6", 0.24);
            ridge(g, 52);
            disc(g, 32, 26, 15, "#241c14");
            [20, 26, 32].forEach(function (y, i) {
                stroke(g, "M" + (18 + i) + " " + y + " Q32 " + (y + 4) + " " + (46 - i) + " " + y,
                       "#6b5b42", 0.8, 0.7);
            });
            stroke(g, "M32 11 L32 41", "#6b5b42", 0.8, 0.7);
            curve(g, [[54, 14], [66, 24], [78, 20], [92, 40]]);
        },

        // La monnaie décroche : le taux ne dit rien de ce qu'on produit.
        currency_fall: function (g, d) {
            glow(g, d, "#e0cba8", "0.5", "0.32", "0.55", 0.26);
            ridge(g, 52);
            var left = sheet(g, 16, 14, 34, 26, -8, 3);
            var right = sheet(g, 52, 20, 34, 26, 10, 3);
            stroke(left, "M50 14 L44 22 L50 30 L44 38", "#241f16", 1.4, 0.5);
            stroke(right, "M52 20 L58 28 L52 36 L58 44", "#241f16", 1.4, 0.5);
        },

        /* Soutien occidental ----------------------------------------------- */

        // Le convoi : l'essentiel arrive en nature, pas en argent.
        aid_convoy: function (g, d) {
            glow(g, d, "#cfe0f4", "0.24", "0.34", "0.6", 0.34);
            ridge(g, 48);
            [10, 40, 70].forEach(function (x, i) {
                var s = 1 - i * 0.16;
                box(g, x, 44 - 12 * s, 26 * s, 12 * s, "#0c1626");
                box(g, x + 19 * s, 44 - 16 * s, 8 * s, 8 * s, "#0c1626");
                disc(g, x + 6 * s, 45, 2.4 * s, "#0c1626");
                disc(g, x + 20 * s, 45, 2.4 * s, "#0c1626");
                crate(g, x + 3 * s, 44 - 11 * s, 12 * s, 6 * s, "#9dc4ec");
            });
            disc(g, 8, 40, 2, "#ffe9a8", 0.9);
        },

        // Le robinet : le flux gratuit peut se rouvrir, ou s'arrêter d'un coup.
        tap: function (open) {
            return function (g, d) {
                glow(g, d, open ? "#9dc4ec" : "#4a5a70", "0.5", "0.3", "0.55", open ? 0.4 : 0.2);
                ground(g, 50);
                box(g, 20, 14, 36, 7, "#0a1626");
                box(g, 46, 21, 8, 8, "#0a1626");
                box(g, 40, 8, 5, 8, "#0a1626");
                stroke(g, "M34 8 L52 8", "#0a1626", 3);
                if (open) {
                    [46, 49, 52].forEach(function (x, i) {
                        stroke(g, "M" + x + " 30 L" + (x - 1 + i) + " 50", "#9dc4ec", 2.2, 0.75);
                    });
                    box(g, 34, 48, 34, 4, "#9dc4ec", 0.4);
                } else {
                    disc(g, 50, 34, 2, "#9dc4ec", 0.7);
                    disc(g, 50, 42, 1.4, "#9dc4ec", 0.4);
                }
            };
        },

        // Le veto : un seul gouvernement suffit à suspendre ce que vingt-six ont voté.
        veto: function (g, d) {
            glow(g, d, "#9dc4ec", "0.5", "0.3", "0.55", 0.34);
            [34, 44].forEach(function (y, k) {
                stroke(g, "M-6 " + (y + 12) + " Q50 " + (y - 10) + " 106 " + (y + 12), k ? "#0a1626" : "#0f2039", 9 - k * 1.5);
            });
            [16, 32, 50, 68, 84].forEach(function (x) {
                disc(g, x, 32, 3, "#0a1626");
                shape(g, "M" + (x - 5) + " 41 Q" + x + " 34 " + (x + 5) + " 41 Z", "#0a1626");
            });
            stroke(g, "M14 12 L86 34", "#a8322a", 3.4);
        },

        // La fatigue : personne n'abandonne, on reporte, on rogne, on conditionne.
        budget_fatigue: function (g, d) {
            glow(g, d, "#7f92ad", "0.72", "0.24", "0.45", 0.24);
            ground(g, 50);
            // Une pile de dossiers qu'on ne rouvre pas, sous une lampe qui baisse : on ne
            // renonce pas, on reporte.
            [0, 1, 2, 3, 4].forEach(function (i) {
                box(g, 14 + i * 2.4, 44 - i * 7, 46 - i * 4, 6.4, i % 2 ? "#16243a" : "#22375a");
                box(g, 14 + i * 2.4, 44 - i * 7, 46 - i * 4, 1.4, "#9dc4ec", 0.28);
            });
            box(g, 78, 4, 2.6, 14, "#0a1626");
            shape(g, "M70 18 L90 18 L85 26 L75 26 Z", "#0a1626");
            beam(g, 80, 26, 15, 24, "#9dc4ec");
            disc(g, 80, 25, 3, "#ffe9a8", 0.55);
        },

        // La rupture : le lien est coupé, la caisse reste à quai.
        aid_cut: function (g, d) {
            glow(g, d, "#6d84a3", "0.3", "0.28", "0.5", 0.22);
            ground(g, 48);
            crate(g, 8, 34, 18, 12, "#3f5a7c");
            stroke(g, "M28 32 L46 24", "#9dc4ec", 2.4, 0.8);
            stroke(g, "M62 20 L84 14", "#9dc4ec", 2.4, 0.35);
            shape(g, "M46 24 L54 20 L50 28 Z", "#a8322a");
            shape(g, "M62 20 L56 26 L58 17 Z", "#a8322a");
        },

        // L'aide pluriannuelle : ce qui change n'est pas le montant, c'est la prévisibilité.
        multi_year: function (g, d) {
            glow(g, d, "#9dc4ec", "0.5", "0.32", "0.6", 0.32);
            ground(g, 50);
            [14, 40, 66].forEach(function (x, i) {
                box(g, x, 20 + i * 0, 22, 26, "#12253f");
                box(g, x, 20, 22, 5, "#9dc4ec", 0.75);
                [0, 1, 2].forEach(function (k) {
                    box(g, x + 3, 29 + k * 5, 16, 2.4, PAPER, 0.5);
                });
            });
            stroke(g, "M8 15 L92 15", "#9dc4ec", 1.6, 0.8);
        },

        // La garantie : une promesse écrite ne livre pas un obus, elle change ce qu'on ose planifier.
        sealed_treaty: function (g, d) {
            glow(g, d, "#cfe0f4", "0.5", "0.32", "0.55", 0.34);
            ridge(g, 52);
            sheet(g, 28, 8, 44, 38, 3, 5);
            box(g, 28, 40, 44, 4, "#1e5fa8", 0.85);
            seal(g, 64, 42, 6, "#1e5fa8");
        },

        // Les avoirs gelés : on les transfère, ou on emprunte contre eux.
        frozen_assets: function (pledge) {
            return function (g, d) {
                glow(g, d, "#cfe6f4", "0.5", "0.32", "0.6", 0.36);
                ground(g, 50);
                box(g, 22, 16, 56, 32, "#8fb4cc", 0.28);
                [30, 44, 58].forEach(function (x, i) {
                    shape(g, "M" + x + " " + (40 - i % 2 * 6) + " L" + (x + 14) + " " + (40 - i % 2 * 6) +
                             " L" + (x + 11) + " " + (34 - i % 2 * 6) + " L" + (x + 3) + " " + (34 - i % 2 * 6) + " Z",
                          "#d8b45c");
                });
                [26, 40, 54, 68].forEach(function (x) {
                    stroke(g, "M" + x + " 16 L" + (x - 3) + " 48", "#e8f2f8", 0.7, 0.3);
                });
                if (pledge) {
                    sheet(g, 58, 30, 30, 22, 8, 3);
                } else {
                    stroke(g, "M72 30 L94 30", PAPER, 2, 0.85);
                    shape(g, "M94 30 L86 26 L86 34 Z", PAPER, 0.85);
                }
            };
        },

        // L'urne : un scrutin à cinq mille kilomètres décide du sort d'un front.
        ballot: function (g, d) {
            glow(g, d, "#f0d060", "0.5", "0.3", "0.55", 0.3);
            ground(g, 50);
            box(g, 30, 24, 40, 26, "#0c1c33");
            box(g, 30, 24, 40, 3.4, "#9dc4ec", 0.5);
            box(g, 44, 26, 12, 1.6, "#050a12");
            sheet(g, 42, 8, 16, 16, 14, 2);
            for (var i = 0; i < 12; i++) {
                var a = (i / 12) * Math.PI * 2 - Math.PI / 2;
                disc(g, 50 + Math.cos(a) * 30, 34 + Math.sin(a) * 16, 1.1, "#f0d060", 0.5);
            }
        },

        // L'inflation chez les soutiens : la guerre se paie aussi dans les caddies.
        home_prices: function (g, d) {
            glow(g, d, "#cfe0f4", "0.3", "0.3", "0.55", 0.26);
            ground(g, 50);
            [12, 32, 52, 72].forEach(function (x, i) {
                var h = 8 + i * 7;
                box(g, x, 50 - h, 16, h, "#12253f");
                box(g, x, 50 - h, 16, 3, "#9dc4ec", 0.6);
            });
            curve(g, [[14, 40], [36, 30], [58, 22], [82, 10]], "#e8746a", 2);
        },

        // La coalition : plusieurs pavillons pour une seule flotte de drones.
        drone_coalition: function (g, d) {
            glow(g, d, "#9dc4ec", "0.5", "0.28", "0.6", 0.34);
            ridge(g, 52);
            droneShape(g, 12, 12, 0.72);
            droneShape(g, 34, 24, 0.9);
            droneShape(g, 62, 8, 0.6);
            [20, 30, 40].forEach(function (x, i) {
                box(g, x, 44, 8, 5, ["#0a1626", "#9dc4ec", "#f0d060"][i], 0.85);
            });
        },

        // Le renseignement allié : une image satellite vaut une division.
        satellite: function (g, d) {
            glow(g, d, "#9dc4ec", "0.5", "0.2", "0.5", 0.3);
            ridge(g, 50);
            box(g, 44, 8, 12, 7, "#0a1626");
            [36, 58].forEach(function (x) {
                box(g, x, 9, 8, 5, "#1e5fa8", 0.8);
            });
            beam(g, 50, 15, 20, 34, "#9dc4ec");
            g.appendChild(svgEl("ellipse", { cx: 50, cy: 48, rx: 18, ry: 4, fill: "#9dc4ec", opacity: "0.28" }));
        },

        // L'avertissement : on écoute poliment, on ne change rien.
        ignored_warning: function (g, d) {
            glow(g, d, "#9dc4ec", "0.5", "0.34", "0.5", 0.24);
            ground(g, 46);
            sheet(g, 22, 20, 56, 26, -2, 0);
            stroke(g, "M28 40 Q44 28 58 34 Q70 38 76 26", "#a8322a", 1.6, 0.85);
            [32, 46, 60].forEach(function (x) { disc(g, x, 34, 1.4, "#a8322a", 0.7); });
            [16, 84].forEach(function (x) {
                box(g, x - 4, 30, 8, 3, "#0a1626");
                box(g, x - 3, 33, 2, 12, "#0a1626");
                box(g, x + 1, 33, 2, 12, "#0a1626");
            });
        },

        // La formation à l'étranger : douze semaines dans une lande galloise.
        training_field: function (g, d) {
            glow(g, d, "#cfe0f4", "0.5", "0.34", "0.6", 0.3);
            ridge(g, 44, "#16283c");
            ground(g, 50, "#0d1a29");
            [10, 26, 42, 58, 74, 90].forEach(function (x, i) {
                figure(g, x, 54, 24 - i % 2 * 3, "#050a12");
            });
            stroke(g, "M0 44 L100 44", "#9dc4ec", 0.7, 0.25);
        },

        // La campagne diplomatique : on parle pour que d'autres paient.
        diplomacy: function (g, d) {
            glow(g, d, "#ffe9a8", "0.5", "0.3", "0.5", 0.3);
            ground(g, 48);
            box(g, 28, 30, 44, 6, "#0a1626");
            box(g, 44, 20, 12, 10, "#0a1626");
            disc(g, 50, 13, 3.4, "#0a1626");
            [12, 22, 78, 88].forEach(function (x, i) {
                box(g, x, 10, 2, 26, "#0a1626");
                box(g, x + 2, 10, 10, 7, ["#1e5fa8", "#f0d060", "#f0d060", "#1e5fa8"][i], 0.8);
            });
        },

        /* Militaire et technologique --------------------------------------- */

        // La frappe de précision : le dépôt qui saute vaut trois offensives.
        precision_strike: function (g, d) {
            glow(g, d, FIRE, "0.68", "0.6", "0.6", 0.5);
            ridge(g, 50);
            stroke(g, "M4 8 Q40 12 62 40", PAPER, 1.6, 0.6);
            shape(g, "M62 40 L54 34 L58 44 Z", PAPER, 0.7);
            disc(g, 68, 48, 13, FIRE, 0.55);
            flame(g, 68, 48, 1.5, d);
            [56, 80].forEach(function (x) { box(g, x, 42, 10, 8, "#0d1218"); });
        },

        // La percée : un front tenu trop mince cède là où personne ne regardait.
        breakthrough: function (g, d) {
            glow(g, d, "#cfe6f2", "0.5", "0.3", "0.6", 0.28);
            ridge(g, 50);
            stroke(g, "M10 30 L34 30 M62 30 L92 30", "#a8322a", 2.6, 0.8);
            [30, 48, 66].forEach(function (x, i) {
                var y = 46 - i % 2 * 4;
                stroke(g, "M" + x + " " + y + " L" + (x + 12) + " " + (y - 22), "#1e5fa8", 3);
                shape(g, "M" + (x + 12) + " " + (y - 22) + " L" + (x + 4) + " " + (y - 19) + " L" + (x + 12) + " " +
                         (y - 14) + " Z", "#1e5fa8");
            });
        },

        // L'offensive qui s'enlise : du terrain qui ne se prend plus par la volonté.
        bogged_offensive: function (g, d) {
            glow(g, d, "#8ea6b8", "0.3", "0.3", "0.5", 0.22);
            ridge(g, 48, "#1a232c");
            ground(g, 52, "#0f151b");
            // La flèche bute sur le réseau de barbelés : le terrain ne se prend plus par
            // la volonté. Les fils courent d'un piquet à l'autre, sinon on lit des arbres.
            stroke(g, "M4 46 L34 32", "#1e5fa8", 4, 0.85);
            shape(g, "M34 32 L25 33 L31 40 Z", "#1e5fa8", 0.85);
            [44, 58, 72, 86].forEach(function (x) {
                stroke(g, "M" + x + " 48 L" + (x - 2) + " 28", "#080d12", 1.8);
            });
            [32, 38, 44].forEach(function (y, k) {
                stroke(g, "M42 " + y + " Q58 " + (y + 3) + " 88 " + (y - 1), "#080d12", 1);
                for (var i = 0; i < 5; i++) {
                    var px = 46 + i * 10;
                    stroke(g, "M" + (px - 2) + " " + (y + k * 0.4 - 1.6) + " L" + (px + 2) + " " + (y + k * 0.4 + 2.4) +
                              " M" + (px - 2) + " " + (y + k * 0.4 + 2.4) + " L" + (px + 2) + " " + (y + k * 0.4 - 1.6),
                           "#080d12", 0.8);
                }
            });
        },

        // Le drone à fibre optique : on ne coupe plus le lien, on le déroule.
        fibre_drone: function (g, d) {
            glow(g, d, SIGNAL, "0.28", "0.26", "0.55", 0.28);
            ridge(g, 52);
            droneShape(g, 44, 14, 1.05);
            stroke(g, "M42 18 Q28 26 22 40 Q18 50 8 54", "#8fd0e8", 1.2, 0.85);
            [26, 34, 42].forEach(function (y, i) {
                disc(g, 22 - i * 2, y + 8, 1, "#8fd0e8", 0.5);
            });
        },

        // Le mur de brouillage : ce qui couvrait un état-major couvre une division.
        jamming_wall: function (g, d) {
            glow(g, d, SIGNAL, "0.5", "0.5", "0.55", 0.3);
            ground(g, 48);
            [22, 50, 78].forEach(function (x, i) {
                var h = 20 + i % 2 * 6;
                stroke(g, "M" + x + " 48 L" + x + " " + (48 - h), INK, 2);
                stroke(g, "M" + (x - 6) + " " + (48 - h) + " L" + (x + 6) + " " + (48 - h), INK, 2);
                [10, 18].forEach(function (r) {
                    stroke(g, "M" + (x - r) + " " + (48 - h) + " A" + r + " " + r + " 0 0 1 " + (x + r) + " " + (48 - h),
                           SIGNAL, 0.9, 0.45);
                });
            });
            stroke(g, "M8 14 L92 14", SIGNAL, 1.6, 0.7);
        },

        // Le brouillage du guidage : la bombe part quand même, et tombe à côté.
        guidance_jam: function (g, d) {
            glow(g, d, SIGNAL, "0.24", "0.4", "0.55", 0.26);
            ridge(g, 52);
            stroke(g, "M6 10 Q34 18 50 28", PAPER, 1.4, 0.5);
            stroke(g, "M50 28 Q62 34 60 48", "#e8746a", 1.8, 0.9);
            stroke(g, "M50 28 Q72 30 90 22", PAPER, 1.2, 0.28);
            shape(g, "M56 44 L64 44 L62 52 L58 52 Z", INK);
            [16, 24].forEach(function (r) {
                stroke(g, "M" + (24 - r) + " 46 A" + r + " " + r + " 0 0 1 " + (24 + r) + " 46", SIGNAL, 1, 0.5);
            });
        },

        // La contre-batterie : le lanceur tire trois coups et se déplace, ou ne se déplace plus.
        counter_battery: function (g, d) {
            glow(g, d, SIGNAL, "0.24", "0.34", "0.5", 0.26);
            ridge(g, 50);
            [12, 20, 28].forEach(function (r, i) {
                stroke(g, "M" + (18 - r) + " 46 A" + r + " " + r + " 0 0 1 " + (18 + r) + " 46",
                       SIGNAL, 0.9, 0.45 - i * 0.11);
            });
            box(g, 60, 38, 20, 8, "#0d1218");
            stroke(g, "M64 38 L84 26", "#0d1218", 3);
            stroke(g, "M84 26 Q64 10 30 22", "#e8746a", 1.6, 0.85);
            disc(g, 30, 22, 2.4, "#e8746a", 0.9);
        },

        // Le trou dans la couverture : ce n'est pas le taux d'interception qui décide, c'est le coût.
        open_sky: function (g, d) {
            glow(g, d, "#e8746a", "0.62", "0.24", "0.6", 0.28);
            ridge(g, 50);
            stroke(g, "M6 40 A30 30 0 0 1 40 16", SIGNAL, 1.6, 0.7);
            stroke(g, "M64 14 A30 30 0 0 1 94 40", SIGNAL, 1.6, 0.7);
            droneShape(g, 42, 16, 0.72);
            [12, 22].forEach(function (x) {
                box(g, x, 42, 6, 6, "#0d1218");
                stroke(g, "M" + (x + 3) + " 42 L" + (x + 3) + " 36", "#0d1218", 1.2);
            });
        },

        // L'essaim : le drone n'ajoute pas de puissance de feu, il permet de l'obtenir sans obus.
        drone_swarm: function (g, d) {
            glow(g, d, "#cfe6f2", "0.5", "0.26", "0.6", 0.28);
            ridge(g, 52);
            [[10, 6, 0.5], [30, 14, 0.62], [56, 8, 0.44], [22, 26, 0.4], [50, 24, 0.72], [76, 18, 0.55]]
                .forEach(function (p) { droneShape(g, p[0], p[1], p[2]); });
        },

        // La bombe planante : soixante-dix kilomètres derrière la ligne, sans entrer dans la couverture.
        glide_bomb: function (g, d) {
            glow(g, d, "#cfe6f2", "0.24", "0.24", "0.55", 0.26);
            ridge(g, 50);
            shape(g, "M30 18 L54 24 L52 28 L28 22 Z");
            shape(g, "M36 20 L34 12 L38 13 L40 21 Z");
            shape(g, "M46 22 L44 14 L48 15 L50 23 Z");
            stroke(g, "M8 12 Q22 14 30 18", PAPER, 1.2, 0.4);
            stroke(g, "M54 26 Q70 32 84 44", PAPER, 1.4, 0.55);
            disc(g, 86, 46, 6, FIRE, 0.4);
        },

        // Les leurres : la moitié de l'essaim ne porte rien, et vide les magasins d'en face.
        decoys: function (g, d) {
            glow(g, d, "#cfe6f2", "0.5", "0.3", "0.6", 0.26);
            ridge(g, 52);
            // Un seul essaim, une seule ogive : le reste ne porte rien et vide les magasins
            // d'en face. Les leurres sont dessinés en creux, la vraie tête en plein.
            [[8, 8], [34, 20], [58, 6], [80, 22]].forEach(function (p, i) {
                if (i === 1) { droneShape(g, p[0], p[1], 1); return; }
                var host = svgEl("g", { opacity: "0.42" });
                g.appendChild(host);
                droneShape(host, p[0], p[1], 0.95);
            });
            disc(g, 34 + 15, 22, 2.4, "#ff6b5e", 0.9);
        },

        // L'assaut à découvert : on paie des hommes avec une monnaie que l'État frappe lui-même.
        infantry_assault: function (g, d) {
            glow(g, d, "#8ea6b8", "0.5", "0.24", "0.55", 0.22);
            ridge(g, 44, "#18222b");
            ground(g, 48, "#0e141a");
            [26, 62].forEach(function (x) { disc(g, x, 40, 6, FIRE, 0.3); });
            [8, 24, 40, 56, 72, 88].forEach(function (x, i) {
                var h = 22 - i % 3 * 4;
                figure(g, x, 54 + i % 2 * 2, h, "#05080b");
                rifle(g, x, 54 + i % 2 * 2, h, "#05080b");
            });
        },

        // Le drone naval : un pays sans marine chasse une flotte de sa propre base.
        naval_drone: function (g, d) {
            glow(g, d, "#cfe6f2", "0.34", "0.26", "0.55", 0.26);
            box(g, 0, 40, 100, 20, "#0f1c26");
            ship(g, 74, 32, 0.72, "#060c11");
            shape(g, "M12 48 L26 46 L28 50 L12 52 Z");
            stroke(g, "M0 54 Q14 50 26 48", "#cfe6f2", 1.6, 0.6);
            [30, 44, 58].forEach(function (x, i) {
                stroke(g, "M" + x + " " + (49 - i) + " Q" + (x + 8) + " " + (46 - i) + " " + (x + 14) + " " + (44 - i),
                       "#cfe6f2", 0.9, 0.3 - i * 0.07);
            });
        },

        // L'interception à bas coût : abattre à cinquante mille ce qui coûte trois millions.
        cheap_interceptor: function (g, d) {
            glow(g, d, SIGNAL, "0.34", "0.4", "0.55", 0.3);
            ridge(g, 50);
            droneShape(g, 62, 12, 0.66, true);
            shape(g, "M18 40 L26 34 L30 36 L22 42 Z");
            stroke(g, "M14 46 Q26 36 44 24", SIGNAL, 1.4, 0.8);
            disc(g, 48, 20, 4, FIRE, 0.5);
            box(g, 8, 44, 12, 6, "#0d1218");
        },

        // La base aérienne : frapper l'avion au sol coûte moins que courir après ce qu'il largue.
        airbase_strike: function (g, d) {
            glow(g, d, FIRE, "0.6", "0.5", "0.6", 0.42);
            ground(g, 44, "#131a20");
            box(g, 0, 44, 100, 3, "#2b3640");
            [16, 44].forEach(function (x) {
                shape(g, "M" + x + " 40 L" + (x + 26) + " 40 L" + (x + 20) + " 36 L" + (x + 4) + " 36 Z", "#070b0f");
                shape(g, "M" + (x + 8) + " 36 L" + (x + 14) + " 28 L" + (x + 17) + " 36 Z", "#070b0f");
            });
            flame(g, 52, 38, 1.2, d);
            [70, 84].forEach(function (x) { box(g, x, 30, 10, 14, "#0d1218"); });
        },

        // Les manœuvres : l'exercice est annoncé ; ce qui ne repart pas, personne ne le compte.
        exercise: function (g, d) {
            glow(g, d, "#cfe6f2", "0.5", "0.32", "0.6", 0.24);
            ridge(g, 46);
            [8, 30, 52, 74].forEach(function (x, i) {
                var s = 1 - i * 0.14;
                box(g, x, 44 - 7 * s, 20 * s, 5 * s, "#0a1017");
                box(g, x + 5 * s, 44 - 11 * s, 9 * s, 4 * s, "#0a1017");
                stroke(g, "M" + (x + 12 * s) + " " + (44 - 9 * s) + " L" + (x + 22 * s) + " " + (44 - 10 * s),
                       "#0a1017", 1.2 * s);
            });
            box(g, 86, 8, 2, 20, "#0a1017");
            shape(g, "M88 8 L98 12 L88 16 Z", "#8ea6b8", 0.8);
        },

        /* Politique intérieure ---------------------------------------------- */

        // La mobilisation : des hommes retirés à l'économie, ruineux en consentement.
        mobilisation: function (g, d) {
            glow(g, d, "#ffd9a0", "0.5", "0.28", "0.5", 0.34);
            ground(g, 48);
            sheet(g, 4, 6, 24, 32, -6, 3);
            [38, 51, 64, 77, 90].forEach(function (x, i) {
                var h = 26 - i % 2 * 3;
                figure(g, x, 52, h, "#0f0b08");
                rifle(g, x, 52, h, "#0f0b08");
            });
        },

        // L'âge abaissé : un réservoir trois fois plus petit, et un prix politique trois fois plus lourd.
        conscription_age: function (g, d) {
            glow(g, d, "#ffd9a0", "0.5", "0.3", "0.55", 0.3);
            ground(g, 50);
            var s = sheet(g, 24, 8, 52, 40, -3, 0);
            box(s, 32, 18, 36, 5, INK, 0.5);
            box(s, 32, 28, 36, 9, INK, 0.7);
            stroke(g, "M28 34 L74 24", "#a8322a", 2.6);
            [18, 50, 82].forEach(function (x, i) { figure(g, x, 56, 20 - i % 2 * 4, "#0f0b08"); });
        },

        // La prime : chaque hausse achète la paix civile et retire un homme au budget des obus.
        bounty: function (g, d) {
            glow(g, d, "#ffd9a0", "0.5", "0.32", "0.55", 0.32);
            ground(g, 50);
            sheet(g, 44, 12, 40, 32, 6, 4);
            [10, 18, 26].forEach(function (y, i) {
                box(g, 12 + i * 2, 24 + i * 6, 26, 6, "#c8a86a");
                box(g, 12 + i * 2, 24 + i * 6, 26, 1.4, "#fff", 0.28);
            });
            seal(g, 74, 38, 5);
        },

        // Les prisons : six mois de front contre la liberté, et une troupe qui ne tient pas deux assauts.
        prison_gate: function (g, d) {
            glow(g, d, "#ffd9a0", "0.72", "0.34", "0.5", 0.3);
            ground(g, 50);
            box(g, 8, 6, 54, 44, "#1a120c");
            [12, 20, 28, 36, 44, 52].forEach(function (x) {
                box(g, x, 6, 3.4, 44, "#0a0705");
            });
            box(g, 56, 6, 6, 44, "#0a0705", 0.3);
            beam(g, 76, 4, 14, 46, "#ffd9a0");
            figure(g, 78, 52, 28, "#0f0b08");
        },

        // Le bureau de recrutement : une durée de service écrite, un solde tenu, un retour possible.
        recruit_office: function (g, d) {
            glow(g, d, "#ffd9a0", "0.34", "0.3", "0.55", 0.3);
            ground(g, 48);
            box(g, 4, 28, 36, 20, "#1a120c");
            sheet(g, 8, 18, 26, 14, -2, 2);
            [54, 70, 86].forEach(function (x, i) {
                figure(g, x, 52, 24 - i * 3, "#0f0b08");
            });
        },

        // Le tour de vis : le mécontentement ne disparaît pas, il cesse de se voir.
        repression: function (g, d) {
            glow(g, d, "#ffd9a0", "0.5", "0.1", "0.5", 0.26);
            ground(g, 50);
            beam(g, 50, 0, 30, 50, "#ffd9a0");
            crowd(g, 44, 8, 12, 8, 4, "#0b0705");
            [18, 34, 50, 66, 82].forEach(function (x) {
                box(g, x, 0, 5, 50, "#0b0705", 0.55);
            });
        },

        // La propagande : on ne convainc personne, on occupe tout l'espace.
        propaganda: function (g, d) {
            glow(g, d, "#ffd9a0", "0.5", "0.26", "0.55", 0.3);
            ground(g, 50);
            box(g, 46, 20, 8, 30, "#17110d");
            [38, 62].forEach(function (x, i) {
                var f = i ? 1 : -1;
                shape(g, "M" + x + " 14 L" + (x + 10 * f) + " 8 L" + (x + 10 * f) + " 26 L" + x + " 20 Z", "#17110d");
                [8, 14, 20].forEach(function (r) {
                    stroke(g, "M" + (x + (10 + r) * f) + " " + (10) + " Q" + (x + (13 + r) * f) + " 17 " +
                              (x + (10 + r) * f) + " 24", "#ffd9a0", 0.9, 0.4);
                });
            });
            crowd(g, 50, 10, 16, 6, 3.4, "#0b0705");
        },

        // La fracture : ce n'est pas la rue qui menace le régime, c'est une fraction de l'appareil.
        cracked_table: function (g, d) {
            glow(g, d, "#ffd9a0", "0.5", "0.24", "0.5", 0.28);
            ground(g, 50);
            box(g, 10, 30, 80, 7, "#17110d");
            box(g, 16, 37, 6, 13, "#17110d");
            box(g, 78, 37, 6, 13, "#17110d");
            [20, 34, 66, 80].forEach(function (x) { disc(g, x, 24, 4, "#17110d"); });
            stroke(g, "M50 18 L46 30 L54 37 L48 50", "#a8322a", 2.2);
        },

        // La rupture : un régime ne tombe pas parce que la rue crie, il tombe quand l'appareil lâche.
        empty_council: function (g, d) {
            glow(g, d, "#a8886a", "0.5", "0.2", "0.5", 0.2);
            ground(g, 50);
            box(g, 10, 30, 80, 7, "#17110d");
            box(g, 16, 37, 6, 13, "#17110d");
            box(g, 78, 37, 6, 13, "#17110d");
            [24, 40].forEach(function (x) {
                shape(g, "M" + x + " 44 L" + (x + 12) + " 40 L" + (x + 14) + " 46 L" + (x + 2) + " 50 Z", "#17110d");
            });
            box(g, 64, 20, 4, 10, "#17110d", 0.5);
        },

        // La mutinerie : une colonne remonte l'autoroute sans rencontrer personne, puis fait demi-tour.
        mutiny: function (g, d) {
            glow(g, d, "#ffd9a0", "0.28", "0.34", "0.55", 0.26);
            ridge(g, 48);
            box(g, 0, 48, 100, 12, "#100c08");
            box(g, 0, 52, 100, 1.4, "#6b5a44", 0.5);
            [14, 40].forEach(function (x) {
                box(g, x, 38, 20, 6, "#0a0705");
                box(g, x + 5, 33, 9, 5, "#0a0705");
            });
            stroke(g, "M70 42 Q88 42 88 32 Q88 22 72 22", "#a8322a", 2.6);
            shape(g, "M72 22 L80 18 L79 27 Z", "#a8322a");
        },

        // La décapitation : on retire un homme et l'on découvre que la chaîne tenait à lui.
        empty_podium: function (g, d) {
            glow(g, d, "#ffd9a0", "0.5", "0.16", "0.45", 0.3);
            ground(g, 48);
            beam(g, 50, 0, 20, 48, "#ffd9a0");
            // L'estrade est là, le pupitre est à terre : on retire un homme et l'on
            // découvre que la chaîne tenait à lui.
            box(g, 26, 40, 48, 5, "#17110d");
            shape(g, "M40 40 L58 30 L64 36 L46 46 Z", "#17110d");
            box(g, 55, 24, 3, 9, "#17110d");
            crowd(g, 54, 8, 14, 7, 4.4, "#0b0705");
        },

        // Le contre-espionnage : on ne protège pas un homme, on rend son emploi du temps illisible.
        surveillance: function (g, d) {
            glow(g, d, "#ffd9a0", "0.34", "0.3", "0.5", 0.24);
            ground(g, 50);
            sheet(g, 14, 14, 40, 32, -4, 4);
            g.appendChild(svgEl("circle", {
                cx: 66, cy: 26, r: 13, fill: "#cfd6dd", opacity: "0.16", stroke: "#17110d", "stroke-width": "2.4"
            }));
            stroke(g, "M76 36 L88 48", "#17110d", 3);
            figure(g, 66, 32, 12, "#17110d");
        },

        // Le scandale : la crise éclate au trimestre où l'on nettoie, le bénéfice arrive plus tard.
        scandal: function (g, d) {
            glow(g, d, "#ffd9a0", "0.5", "0.14", "0.5", 0.3);
            ground(g, 50);
            beam(g, 50, 0, 24, 50, "#ffd9a0");
            [20, 38, 58].forEach(function (x, i) {
                sheet(g, x, 30 + i % 2 * 6, 26, 18, -14 + i * 12, 2);
            });
            disc(g, 78, 44, 4, "#a8322a", 0.8);
        },

        // La transparence : assainir coûte avant de rapporter.
        ledger: function (g, d) {
            glow(g, d, "#ffd9a0", "0.5", "0.3", "0.55", 0.3);
            ground(g, 50);
            var s = sheet(g, 16, 12, 68, 34, 0, 0);
            box(s, 50, 12, 1.6, 34, INK, 0.35);
            [0, 1, 2, 3].forEach(function (i) {
                box(s, 22, 20 + i * 6, 22, 2.6, INK, 0.42);
                box(s, 56, 20 + i * 6, 22, 2.6, INK, 0.42);
            });
            seal(g, 74, 40, 5, "#3d7a51");
        },

        // Le mur démographique : les classes creuses arrivent à l'âge de servir.
        age_pyramid: function (g, d) {
            glow(g, d, "#a8886a", "0.5", "0.3", "0.55", 0.22);
            ground(g, 52);
            [0, 1, 2, 3, 4].forEach(function (i) {
                var w = [40, 34, 16, 26, 30][i];
                box(g, 50 - w / 2, 44 - i * 8, w, 6.4, "#17110d");
            });
            stroke(g, "M18 30 L82 30", "#a8322a", 1.6, 0.8);
        },

        // L'amassement : les hôpitaux de campagne ne se déploient pas pour un exercice.
        massing: function (g, d) {
            glow(g, d, "#cfd6dd", "0.5", "0.28", "0.6", 0.24);
            ridge(g, 44, "#1d1610");
            ground(g, 50, "#100b07");
            stroke(g, "M0 47 L100 47", "#a8322a", 1.4, 0.55);
            [6, 20, 34, 48, 62, 76, 90].forEach(function (x, i) {
                shape(g, "M" + x + " 42 L" + (x + 5) + " 34 L" + (x + 10) + " 42 Z", "#0a0705");
                if (i % 2 === 0) { box(g, x + 1, 26, 8, 3, "#0a0705"); }
            });
        },

        // L'ordre qui n'est pas donné : mobiliser, c'est donner raison à celui qui masse ses troupes.
        unsigned_order: function (g, d) {
            glow(g, d, "#ffd9a0", "0.5", "0.34", "0.5", 0.3);
            ground(g, 50);
            box(g, 6, 40, 88, 10, "#17110d");
            sheet(g, 26, 14, 44, 28, -2, 4);
            stroke(g, "M74 20 L84 38", "#17110d", 2.2);
            disc(g, 74, 19, 2, "#17110d");
        },

        /* Énergie ------------------------------------------------------------ */

        // Le réseau : une ville qu'on éteint, pas une centrale qu'on détruit.
        dark_grid: function (g, d) {
            glow(g, d, "#3f5a70", "0.5", "0.24", "0.6", 0.2);
            ridge(g, 50);
            [18, 46, 74].forEach(function (x) { pylon(g, x, 50, 30); });
            stroke(g, "M12 30 Q32 36 40 30 Q60 36 68 30 Q86 36 96 32", "#0a0f14", 1);
            [6, 16, 26, 60, 70, 80, 90].forEach(function (x, i) {
                box(g, x, 50 - (4 + i % 3 * 3), 7, 4 + i % 3 * 3, "#080c10");
            });
            disc(g, 63, 44, 1.2, "#ffd9a0", 0.5);
        },

        // Le poste de raccordement : débranchée, la centrale ne produit pour personne.
        substation: function (g, d) {
            glow(g, d, FIRE, "0.66", "0.5", "0.55", 0.42);
            ground(g, 48);
            [10, 30, 50, 70].forEach(function (x, i) {
                box(g, x, 26 + i % 2 * 4, 14, 22 - i % 2 * 4, "#0e1216");
                [3, 7, 11].forEach(function (k) {
                    box(g, x + k, 20 + i % 2 * 4, 1.6, 6, "#0e1216");
                });
            });
            flame(g, 74, 26, 1.1, d);
            stroke(g, "M4 18 L96 18", "#0e1216", 1.2);
        },

        // Le raffinage : frapper là, c'est frapper le portefeuille.
        burning_refinery: function (g, d) {
            glow(g, d, FIRE, "0.7", "0.28", "0.6", 0.5);
            ground(g, 48);
            shape(g, "M0 48 L0 38 L8 38 L8 20 L14 20 L14 38 L26 38 L26 14 L32 14 L32 38 L46 38 L46 26 L54 26 L54 38 L100 38 L100 48 Z",
                  "#0c1218");
            [10, 20, 30, 40].forEach(function (x) {
                stroke(g, "M" + x + " 38 L" + x + " 32", "#33475a", 1);
            });
            box(g, 70, 14, 3, 24, "#0c1218");
            flame(g, 71.5, 14, 1.5, d);
            [14, 32, 54].forEach(function (x) {
                stroke(g, "M" + x + " 20 L" + x + " 38", FIRE, 0.9, 0.55);
            });
        },

        // Le parapluie : on couvre ce qui compte, et l'on découvre le reste.
        shield: function (refinery) {
            return function (g, d) {
                glow(g, d, SIGNAL, "0.5", "0.5", "0.6", 0.3);
                ground(g, 48);
                if (refinery) {
                    shape(g, "M28 48 L28 22 L36 22 L36 48 M46 48 L46 16 L54 16 L54 48 M62 48 L62 26 L70 26 L70 48 Z",
                          "#0c1218");
                    box(g, 24, 44, 52, 4, "#0c1218");
                } else {
                    [32, 50, 68].forEach(function (x) { pylon(g, x, 48, 24); });
                }
                dome(g, 50, 48, 34);
                dome(g, 50, 48, 26);
                [22, 50, 78].forEach(function (x) {
                    box(g, x - 3, 42, 6, 6, "#0c1218");
                    stroke(g, "M" + x + " 42 L" + x + " 34", "#0c1218", 1.2);
                });
            };
        },

        // Le rail : un nœud coupé le lundi roule à nouveau le jeudi — ou ne roule plus.
        rail: function (cut) {
            return function (g, d) {
                glow(g, d, cut ? FIRE : WARM, "0.5", "0.36", "0.6", cut ? 0.4 : 0.3);
                ridge(g, 36, "#241a10");
                // Le ballast est clair : c'est ce qui détache la voie du sol. Des rails
                // sombres sur une terre sombre ne se voient tout simplement pas.
                ground(g, 40, "#241a10");
                shape(g, "M12 60 L42 34 L58 34 L88 60 Z", "#6b5539");
                rails(g, 34);
                if (cut) {
                    // La voie s'arrête net : le ballast est ouvert, les files se tordent.
                    shape(g, "M32 44 L68 44 L74 58 L26 58 Z", "#1d150e");
                    stroke(g, "M30 54 L44 46 L40 58", "#0a0705", 3);
                    stroke(g, "M70 54 L58 46 L62 58", "#0a0705", 3);
                    flame(g, 50, 52, 1.4, d);
                } else {
                    // Le nœud coupé le lundi roule à nouveau le jeudi : l'équipe est là.
                    [20, 78].forEach(function (x, i) {
                        figure(g, x, 58, 28 - i * 4, "#0a0705");
                        stroke(g, "M" + (x + 7) + " 46 L" + (x + 16) + " 38", "#0a0705", 2.4);
                    });
                }
            };
        },

        // Mille petites machines : plus rien de décisif à viser.
        islanded_power: function (g, d) {
            glow(g, d, WARM, "0.5", "0.4", "0.6", 0.3);
            ground(g, 48);
            [8, 26, 44, 62, 80].forEach(function (x, i) {
                var h = 10 + i % 3 * 4;
                box(g, x, 48 - h, 12, h, "#0e1216");
                box(g, x + 3, 48 - h - 4, 2.6, 4, "#0e1216");
                disc(g, x + 6, 48 - h + 4, 1.6, "#ffd9a0", 0.75);
            });
        },

        // L'hiver : la même destruction ne vaut rien en juillet et devient une crise en janvier.
        winter: function (cold) {
            return function (g, d) {
                glow(g, d, cold ? "#cfe6f4" : WARM, "0.5", "0.3", "0.6", cold ? 0.3 : 0.4);
                ridge(g, 46, cold ? "#dfe8ee" : "#3a2e1e");
                if (cold) {
                    box(g, 0, 46, 100, 14, "#e8eef2", 0.9);
                    for (var i = 0; i < 22; i++) {
                        disc(g, (i * 37) % 100, (i * 53) % 44, 0.9, "#fff", 0.6);
                    }
                } else {
                    sun(g, 74, 30, 10, "#f6d097", 0.6);
                    box(g, 0, 46, 100, 14, "#4b4230");
                    [10, 30, 50, 70, 90].forEach(function (x) {
                        stroke(g, "M" + x + " 46 Q" + (x + 3) + " 52 " + x + " 58", "#cfe6f4", 0.8, 0.4);
                    });
                }
                shape(g, "M30 46 L30 30 L44 22 L58 30 L58 46 Z", "#17110d");
                box(g, 40, 36, 6, 6, cold ? "#3a3a3a" : "#ffd9a0", cold ? 0.9 : 0.85);
                box(g, 48, 18, 3, 12, "#17110d");
            };
        },

        // La raspoutitsa : on ne choisit pas la saison, on choisit le trimestre où l'on frappe.
        mud: function (g, d) {
            glow(g, d, "#8d8578", "0.5", "0.28", "0.5", 0.2);
            ridge(g, 44, "#2b2318");
            box(g, 0, 46, 100, 14, "#3a2e1e");
            [50, 54].forEach(function (y, i) {
                stroke(g, "M0 " + y + " Q30 " + (y - 2) + " 60 " + y + " Q80 " + (y + 2) + " 100 " + (y - 1),
                       "#1d170f", 3 - i);
            });
            disc(g, 62, 48, 7, "#17110d");
            disc(g, 62, 48, 3, "#3a2e1e");
            box(g, 34, 40, 26, 9, "#17110d");
            box(g, 30, 50, 40, 4, "#241c12", 0.8);
        },

        // Le barrage : l'attribution ne sera jamais établie, l'eau emporte les villages.
        dam: function (g, d) {
            glow(g, d, "#8fb4cc", "0.5", "0.34", "0.55", 0.28);
            ground(g, 46, "#141c22");
            shape(g, "M0 46 L0 20 L38 20 L38 46 Z", "#0e1216");
            shape(g, "M62 46 L62 20 L100 20 L100 46 Z", "#0e1216");
            box(g, 0, 20, 100, 3, "#2b3640");
            shape(g, "M38 22 Q50 34 46 60 L62 60 Q60 32 62 22 Z", "#6ea8cd", 0.7);
            [50, 54, 58].forEach(function (x, i) {
                disc(g, x, 40 + i * 5, 1.6 - i * 0.3, "#cbe7f4", 0.6);
            });
        },

        // Le gazoduc : quatre tubes sous quatre-vingts mètres d'eau, et aucune enquête n'aboutira.
        pipeline: function (g, d) {
            glow(g, d, "#8fb4cc", "0.5", "0.24", "0.6", 0.24);
            box(g, 0, 8, 100, 52, "#0e1c26", 0.7);
            // Le tube passe d'un bord à l'autre, la brèche est au milieu et le gaz remonte.
            box(g, 0, 34, 42, 10, "#2b3640");
            box(g, 58, 34, 42, 10, "#2b3640");
            box(g, 0, 34, 42, 2.4, "#5c7080", 0.8);
            box(g, 58, 34, 42, 2.4, "#5c7080", 0.8);
            shape(g, "M42 34 L48 40 L42 44 Z", "#141c22");
            shape(g, "M58 34 L52 40 L58 44 Z", "#141c22");
            [[48, 26, 3], [44, 18, 2.2], [53, 12, 1.6], [47, 6, 1.1]].forEach(function (b, i) {
                disc(g, b[0], b[1], b[2], "#cbe7f4", 0.55 - i * 0.08);
            });
            ground(g, 52, "#141c22");
        },

        /* Extérieur ---------------------------------------------------------- */

        // Une autre guerre : les stocks partent ailleurs, et les capitales aussi.
        spotlight_elsewhere: function (g, d) {
            glow(g, d, "#c9d6ae", "0.78", "0.22", "0.5", 0.34);
            ridge(g, 50);
            beam(g, 82, 4, 18, 46, "#f4e6b4");
            [66, 78, 90].forEach(function (x, i) { figure(g, x, 50, 12 - i, "#131a15"); });
            [10, 22, 34].forEach(function (x, i) { figure(g, x, 50, 12 - i, "#0a0e0b"); });
            box(g, 0, 0, 46, 60, "#050806", 0.34);
        },

        // Les munitions étrangères : le flux ne demande de vote à personne.
        shell_crates: function (g, d) {
            glow(g, d, "#f4e6b4", "0.66", "0.36", "0.55", 0.3);
            box(g, 0, 44, 100, 16, "#1b2419");
            box(g, 0, 44, 100, 1.6, "#5d6a4a", 0.6);
            [[10, 32], [24, 32], [38, 32], [17, 24], [31, 24], [24, 16]].forEach(function (c) {
                crate(g, c[0], c[1], 13, 7.4);
            });
            [60, 72, 84].forEach(function (x) {
                shape(g, "M" + x + " 44 L" + x + " 30 Q" + (x + 3) + " 24 " + (x + 6) + " 30 L" + (x + 6) + " 44 Z",
                      "#131a15");
            });
        },

        // La licence : ce qu'on achetait hier, on le fabrique demain chez soi.
        blueprint: function (g, d) {
            glow(g, d, "#c9d6ae", "0.28", "0.3", "0.55", 0.3);
            ground(g, 48);
            var s = sheet(g, 6, 12, 38, 30, -3, 0);
            stroke(s, "M12 20 L38 20 L38 36 L12 36 Z", "#1e5fa8", 1, 0.6);
            stroke(s, "M12 28 L38 28 M25 20 L25 36", "#1e5fa8", 0.7, 0.4);
            stroke(g, "M48 28 L64 28", PAPER, 1.8, 0.8);
            shape(g, "M64 28 L56 24 L56 32 Z", PAPER, 0.8);
            shape(g, "M70 48 L70 30 L78 24 L86 30 L86 48 Z", "#131a15");
            box(g, 79, 16, 3, 10, "#131a15");
        },

        // Le fournisseur se retire : le quai reste, la grue ne charge plus.
        empty_quay: function (g, d) {
            glow(g, d, "#8d8578", "0.3", "0.28", "0.55", 0.2);
            box(g, 0, 42, 100, 18, "#1b2419");
            box(g, 0, 42, 100, 1.6, "#4b543a", 0.5);
            stroke(g, "M16 42 L16 12 L14 12 L14 42 M16 14 L46 14 L46 18 L16 18 M42 18 L42 32", "#131a15", 2);
            box(g, 38, 32, 8, 5, "#131a15");
            [60, 74].forEach(function (x) {
                box(g, x, 38, 12, 4, "#131a15", 0.35);
            });
            box(g, 0, 46, 100, 3, "#0f1410", 0.5);
        },

        // Le grand voisin : on ne menace pas, on rappelle les termes.
        big_neighbour: function (g, d) {
            glow(g, d, "#f4e6b4", "0.34", "0.3", "0.55", 0.3);
            ridge(g, 50);
            figure(g, 30, 52, 40, "#0a0e0b");
            figure(g, 66, 52, 20, "#131a15");
            stroke(g, "M40 26 Q54 22 62 32", "#0a0e0b", 3.4);
        },

        // Les drones achetés : un moteur de scooter, une aile de mousse, le prix d'une berline.
        drone_cargo: function (g, d) {
            glow(g, d, "#c9d6ae", "0.5", "0.34", "0.55", 0.3);
            box(g, 0, 44, 100, 16, "#1b2419");
            [[8, 32], [22, 32], [36, 32], [15, 24]].forEach(function (c) { crate(g, c[0], c[1], 13, 7.4); });
            droneShape(g, 58, 18, 0.86);
            stroke(g, "M52 34 L58 24", PAPER, 1, 0.4);
        },

        // Les missiles achetés : ils se règlent en devises et se remboursent en concessions.
        missile_flatcar: function (g, d) {
            glow(g, d, "#f4e6b4", "0.5", "0.36", "0.55", 0.3);
            ground(g, 46, "#131a15");
            box(g, 6, 38, 88, 6, "#0a0e0b");
            [12, 40, 68].forEach(function (x) {
                shape(g, "M" + x + " 38 L" + x + " 30 Q" + (x + 6) + " 22 " + (x + 12) + " 30 L" + (x + 24) + " 30 L" +
                         (x + 24) + " 38 Z", "#1f2a1c");
                box(g, x + 4, 26, 3, 6, "#1f2a1c");
            });
            [14, 34, 54, 74, 90].forEach(function (x) { disc(g, x, 45, 2.2, "#0a0e0b"); });
        },

        // La chaîne de montage : acheter n'était que la première étape.
        assembly_line: function (g, d) {
            glow(g, d, "#c9d6ae", "0.5", "0.3", "0.6", 0.3);
            shape(g, "M0 48 L0 20 L50 8 L100 20 L100 48 Z", "#141c14");
            box(g, 0, 44, 100, 4, "#0a0e0b");
            [12, 40, 68].forEach(function (x) { droneShape(g, x, 26, 0.6); });
            [8, 30, 52, 74].forEach(function (x) { box(g, x, 38, 14, 3, "#0a0e0b"); });
        },

        // La complaisance : un prêt, un terminal, une abstention.
        handshake_case: function (g, d) {
            glow(g, d, "#f4e6b4", "0.5", "0.3", "0.5", 0.32);
            ground(g, 50);
            stroke(g, "M22 26 L48 32 M78 26 L52 32", "#131a15", 4);
            box(g, 44, 30, 14, 6, "#0a0e0b");
            box(g, 34, 40, 22, 12, "#0a0e0b");
            box(g, 42, 36, 6, 4, "#0a0e0b");
            box(g, 34, 44, 22, 1.6, "#c8a86a", 0.7);
        },

        // Les pourparlers : on négocie une frontière que chacun compte reprendre.
        talks: function (g, d) {
            glow(g, d, "#f4e6b4", "0.5", "0.28", "0.5", 0.28);
            ground(g, 50);
            box(g, 18, 32, 64, 5, "#131a15");
            box(g, 24, 37, 5, 13, "#131a15");
            box(g, 71, 37, 5, 13, "#131a15");
            [12, 88].forEach(function (x, i) {
                box(g, x - 4, 24, 8, 4, "#0a0e0b");
                box(g, x - 3, 28, 2, 12, "#0a0e0b");
                box(g, x + 1, 28, 2, 12, "#0a0e0b");
            });
            box(g, 48, 26, 4, 6, "#0a0e0b");
        },

        // L'ultimatum : on exige par écrit ce qu'on sait ne pas pouvoir obtenir.
        ultimatum: function (g, d) {
            glow(g, d, "#f4e6b4", "0.5", "0.32", "0.5", 0.3);
            ridge(g, 52);
            var s = sheet(g, 26, 14, 46, 30, -4, 0);
            shape(s, "M26 14 L49 32 L72 14 Z", "#d8cdb4", 0.9);
            stroke(s, "M26 14 L49 32 L72 14", "#241f16", 0.9, 0.5);
            stroke(g, "M14 46 L28 40", "#131a15", 3.4);
            stroke(g, "M86 46 L72 40", "#131a15", 3.4);
        },

        // L'armée qui se dissout : rien n'est vaincu, tout se disperse.
        dissolving_army: function (g, d) {
            glow(g, d, "#cfd6dd", "0.5", "0.3", "0.6", 0.26);
            ridge(g, 46);
            ground(g, 50, "#100b07");
            // La colonne s'éclaircit de proche en loin : rien n'est vaincu, tout se disperse,
            // et le matériel reste sur place.
            [8, 24, 40, 58, 78].forEach(function (x, i) {
                var host = svgEl("g", { opacity: String(0.95 - i * 0.17) });
                g.appendChild(host);
                figure(host, x, 54, 28 - i * 3, INK);
            });
            box(g, 62, 46, 18, 5, "#0a0705", 0.7);
            shape(g, "M84 52 L98 46 L100 51 L86 57 Z", "#0a0705", 0.6);
        },

        // Filet de sécurité : un sujet que le deck introduirait sans scène dédiée.
        neutral: function (g, d) {
            glow(g, d, "#e8e0cc", "0.5", "0.3", "0.55", 0.24);
            sun(g, 50, 30, 9, "#e8e0cc", 0.35);
            ridge(g, 48);
            [24, 50, 76].forEach(function (x, i) {
                box(g, x, 30 + i % 2 * 4, 1.6, 18);
            });
        }
    };

    // Scènes paramétrées : une même construction, des histoires opposées. Le baril qui
    // flambe et le baril qui s'effondre ne peuvent pas porter la même image ; le rail
    // qu'on coupe et le rail qu'on repose non plus.
    [["oil_spike", "barrel", 1], ["oil_crash", "barrel", -1], ["oil_cap", "barrel", 0],
     ["vault_draw", "vault", 0], ["vault_empty", "vault", 1],
     ["tap_open", "tap", 1], ["tap_shut", "tap", 0],
     ["assets_move", "frozen_assets", 0], ["assets_pledge", "frozen_assets", 1],
     ["shield_grid", "shield", 0], ["shield_refinery", "shield", 1],
     ["rail_cut", "rail", 1], ["rail_repair", "rail", 0],
     ["hard_winter", "winter", 1], ["mild_winter", "winter", 0]
    ].forEach(function (v) { ART[v[0]] = ART[v[1]](v[2]); });
    ["barrel", "vault", "tap", "frozen_assets", "shield", "rail", "winter"].forEach(function (k) {
        delete ART[k];
    });

    // Une carte, un motif. Deux cartes ne partagent une scène que lorsqu'elles racontent
    // le même fait sous deux angles — le train de sanctions et l'embargo étendu, la
    // raffinerie frappée une fois et la campagne qui y revient chaque trimestre.
    var CARD_ART = {
        // Économie de guerre et sanctions
        sanctions_package_1: "sanctions_decree",
        component_embargo: "machine_embargo",
        component_embargo_total: "machine_embargo",
        major_oil_sanctions: "oil_majors",
        oil_price_cap: "oil_cap",
        oil_price_crash: "oil_crash",
        oil_price_spike: "oil_spike",
        shadow_fleet: "ghost_fleet",
        shadow_fleet_sanctions: "ghost_fleet",
        evasion_network: "detour",
        component_smuggling: "detour",
        oil_export_rerouting: "east_flow",
        grain_port_strikes: "grain_port",
        war_economy_conversion: "war_factory",
        industrial_requisition: "requisition",
        war_tax_rise: "war_tax",
        sovereign_fund_draw: "vault_draw",
        sovereign_fund_empty: "vault_empty",
        domestic_drone_industry: "drone_workshop",
        global_recession: "world_slump",
        currency_collapse: "currency_fall",

        // Soutien occidental
        western_aid_opens: "aid_convoy",
        first_defensive_deliveries: "aid_convoy",
        aid_unblocked: "tap_open",
        aid_blocked: "tap_shut",
        parliament_veto: "veto",
        budget_fatigue: "budget_fatigue",
        aid_collapse: "aid_cut",
        aid_predictable: "multi_year",
        security_guarantees: "sealed_treaty",
        frozen_assets_released: "assets_move",
        frozen_assets_windfall: "assets_pledge",
        us_election_swing: "ballot",
        european_election_swing: "ballot",
        inflation_surge: "home_prices",
        drone_coalition: "drone_coalition",
        allied_intelligence: "satellite",
        intelligence_warning: "ignored_warning",
        nato_training_pipeline: "training_field",
        diplomatic_campaign: "diplomacy",

        // Militaire et technologique
        himars_deep_strike: "precision_strike",
        depot_strikes: "precision_strike",
        counter_offensive_2022: "breakthrough",
        failed_offensive: "bogged_offensive",
        fibre_optic_drones: "fibre_drone",
        electronic_warfare: "jamming_wall",
        electronic_warfare_scaling: "jamming_wall",
        electronic_warfare_ukraine: "guidance_jam",
        counter_battery: "counter_battery",
        air_defence_gap: "open_sky",
        drone_swarm_scaling: "drone_swarm",
        glide_bombs: "glide_bomb",
        decoy_saturation: "decoys",
        meat_assault: "infantry_assault",
        naval_drones_black_sea: "naval_drone",
        cheap_interception: "cheap_interceptor",
        air_base_strikes: "airbase_strike",
        zapad_exercises: "exercise",

        // Politique intérieure
        partial_mobilisation: "mobilisation",
        mobilisation_wave_two: "mobilisation",
        conscription_law: "conscription_age",
        contract_recruitment_drive: "bounty",
        prison_recruitment: "prison_gate",
        recruitment_reform: "recruit_office",
        domestic_repression: "repression",
        state_propaganda_surge: "propaganda",
        elite_fracture: "cracked_table",
        elite_break: "empty_council",
        armed_mutiny: "mutiny",
        decapitation_strike: "empty_podium",
        counter_intelligence: "surveillance",
        anticorruption_crisis: "scandal",
        transparency_reform: "ledger",
        demographic_wall: "age_pyramid",
        force_concentration: "massing",
        no_mobilisation_yet: "unsigned_order",

        // Énergie
        grid_campaign: "dark_grid",
        substation_strikes: "substation",
        refinery_strikes: "burning_refinery",
        refinery_campaign_sustained: "burning_refinery",
        air_defence_surge: "shield_grid",
        refinery_air_defence: "shield_refinery",
        rail_interdiction: "rail_cut",
        rail_repair_brigades: "rail_repair",
        decentralised_generation: "islanded_power",
        harsh_winter: "hard_winter",
        mild_winter: "mild_winter",
        rasputitsa: "mud",
        dam_breach: "dam",
        pipeline_sabotage: "pipeline",

        // Extérieur
        attention_elsewhere: "spotlight_elsewhere",
        foreign_shells: "shell_crates",
        licence_transfer: "blueprint",
        supplier_withdraws: "empty_quay",
        chinese_pressure: "big_neighbour",
        foreign_drones: "drone_cargo",
        foreign_ballistic_missiles: "missile_flatcar",
        shahed_plant: "assembly_line",
        diplomatic_complaisance: "handshake_case",
        ceasefire_talks: "talks",
        ultimatum_to_nato: "ultimatum"
    };

    // Le deck s'écrit encore : prologue de l'automne 2021, sortie de guerre de 2027. Une
    // carte qui arrive sans motif attribué est reconnue à son titre plutôt que renvoyée
    // au fond de sa famille — les scènes correspondantes existent déjà.
    var TITLE_ART = [
        [/démobilis|dissou|se disperse|retour au pays|rentrent chez/i, "dissolving_army"],
        [/amass|concentration de force|masse ses troupes/i, "massing"],
        [/armistice|cessez-le-feu|ligne de démarcation|pourparlers|négociation/i, "talks"],
        [/ultimatum|exigence écrite/i, "ultimatum"],
        [/exercice|manœuvres/i, "exercise"],
        [/mobilisation/i, "mobilisation"],
        [/raffin/i, "burning_refinery"],
        [/réseau électrique|centrale|poste de raccordement/i, "dark_grid"],
        [/sanction|embargo/i, "sanctions_decree"],
        [/drone/i, "drone_swarm"],
        [/élection|scrutin/i, "ballot"],
        [/hiver/i, "hard_winter"]
    ];

    // Repli de dernier ressort : la scène la plus représentative du domaine, jamais un
    // fond vide.
    var FAMILY_ART = {
        "Économique": "sanctions_decree",
        "Politique occidentale": "aid_convoy",
        "Politique interne": "mobilisation",
        "Énergie": "dark_grid",
        "Militaire et technologique": "drone_swarm",
        "Externe": "shell_crates"
    };

    function motifOf(card) {
        if (CARD_ART[card.code]) { return CARD_ART[card.code]; }

        var title = safeText(card.title, "");
        for (var i = 0; i < TITLE_ART.length; i++) {
            if (TITLE_ART[i][0].test(title)) { return TITLE_ART[i][1]; }
        }

        return FAMILY_ART[safeText(card.family, "")] || "neutral";
    }

    // Le ciel appartient à la famille, la scène au sujet : c'est cet accord qui fait lire
    // cent une cartes comme un deck et non comme une collection d'images.
    function paintArt(card, svg) {
        var d = artDefs(svg);
        sky(svg, d, SKY[safeText(card.family, "")] || SKY[""]);
        (ART[motifOf(card)] || ART.neutral)(svg, d);
        vignette(svg, d);
    }

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
        paintArt(card, svg);
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
            dateOf(t) + " · " + fmt(t.squareKilometresGained) + " km² pris sur les secteurs simulés"));
        stage.appendChild(head);

        // On the last turn of a run that stopped early, say so: the timeline stops here
        // because the war did, not because the button failed.
        var g = game();
        if (g.endedEarly && state.turnIndex === g.turns.length - 1) {
            var stop = el("div", "run-ended");
            stop.innerHTML = "<strong>La guerre s'arrête ici.</strong> Ce déroulé se termine " +
                dateInOf(t) + " — les " + (g.plannedTurns - g.turns.length) +
                " trimestres suivants du calendrier n'ont pas été joués.";
            stage.appendChild(stop);
        }

        // Le capital de guerre passe AVANT la carte, et le ciseau juste après lui. La carte
        // montre le thermomètre, le bandeau montre le moteur, et les deux se lisent d'un seul
        // regard. Les deux pièces vivent dans leur propre fichier : ce qu'elles dessinent est
        // un modèle à part entière, pas une décoration de la résolution.
        var capital = window.tovCapital;
        if (capital) {
            stage.appendChild(capital.band(g, state.turnIndex));
            stage.appendChild(capital.divergence(g, state.turnIndex));
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

        var leftCol = el("div");
        leftCol.appendChild(mapPanel);
        field.appendChild(leftCol);

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

        // Les deux écrans de génération portent la même chaîne : sans teinte de camp, on ne
        // sait plus lequel on lit. La couleur du camp habille l'écran entier, pas seulement
        // le fanion du titre.
        stage.className = state.phase === 0 ? "side-ru" : (state.phase === 1 ? "side-ua" : "");

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

    // Une seule formulation des dates pour tout le plateau. Le bandeau de capital vit dans son
    // propre fichier et doit dater ses trimestres exactement comme les écrans de génération —
    // recopier la table des saisons ailleurs, c'est se garantir deux libellés qui divergent.
    window.tovDates = { of: dateOf, in: dateInOf };

    bindPhases();
    state.turnIndex = openingTurnIndex(game());
    render();
})();
