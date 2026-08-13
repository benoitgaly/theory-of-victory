/* ============================================================
   Le bandeau du capital de guerre, et le ciseau.

   Le plateau montre ce que le front consomme et ce qu'il produit. Il ne montrait
   nulle part le STOCK — ce qu'il reste à brûler. C'est pourtant le seul chiffre qui
   annonce la fin depuis plusieurs trimestres.

   Deux pièces, et elles ne disent pas la même chose :
     · le bandeau dit le trimestre — huit postes, leur variation, sa cause ;
     · le ciseau dit la guerre — la puissance au front contre le capital qui la nourrit.

   Mélanger les deux produirait huit petites courbes illisibles et une pièce maîtresse
   diluée, alors on les sépare.

   Aucun rang de tour n'apparaît ici : ce qui situe un trimestre, c'est sa date. Les
   libellés viennent de window.tovDates, partagés avec le reste du plateau.
   ============================================================ */
(function () {
    "use strict";

    var NS = "http://www.w3.org/2000/svg";

    // La teinte porte le poste, la position porte le camp : huit postes doivent se
    // distinguer dans une rangée, les deux camps le sont déjà par le haut et le bas.
    var POSTS = [
        { code: "reserves", colour: "#8a6f2b", yield: "comble ce que la recette du trimestre ne finance plus" },
        { code: "grid", colour: "#c2621a", yield: "ne va jamais au front : ouvre ou ferme les usines, le raffinage, le chauffage" },
        { code: "oil", colour: "#8a5a2b", yield: "recette pour qui l'exporte, charge nette pour qui l'importe" },
        { code: "civilian", colour: "#6f8060", yield: "le niveau de vie, donc le consentement à la guerre" },
        { code: "arms", colour: "#4a6070", yield: "les obus, les drones, les missiles, les intercepteurs" },
        { code: "regime", colour: "#8a4b2a", yield: "le capital politique du trimestre, celui qui paie les cartes" },
        { code: "foreign", colour: "#2f8f8f", yield: "du matériel obtenu hors de la capacité nationale" },
        { code: "international", colour: "#8a6a9c", yield: "qui vote quoi, qui sanctionne, qui achète encore le pétrole" }
    ];

    var NAMES = {
        reserves: "Réserves", grid: "Centrales", oil: "Pétrole", civilian: "Usines civiles",
        arms: "Armement", regime: "Tenue du pouvoir", foreign: "Soutien étranger",
        international: "Soutien international"
    };

    // Le poste que l'alerte la plus vive du trimestre désigne. Une seule autorité par
    // information : le dessin ne met en avant que ce que le moteur a déjà nommé.
    var ALERT_POST = {
        reserves: "reserves",
        funding_gap: "reserves",
        winter_shedding: "grid",
        regime_stress: "regime",
        external_will: "foreign"
    };

    var W = 1240;
    // Les huit colonnes commencent après le cartouche de date, qui occupe la gauche de la
    // bande de partage : un nom de poste ne doit jamais buter contre la saison.
    var COL_W = 118, COL_PITCH = 126, COL_X0 = 228;
    var RU_BASE = 134, UA_BASE = 170;
    var ROW_TOP = 20, ROW_BOTTOM = 284, H = 296;
    var PX_PER_100 = 0.52, MAX_H = 68;
    var RUINED_INDEX = 25;

    // Les chiffres ont leur bande à eux, hors d'atteinte des masses. Les poser au bord de
    // la masse les rendait illisibles dès qu'une colonne montait — et ce sont justement les
    // colonnes hautes qu'on veut lire.
    var RU_DELTA_Y = 32, RU_VALUE_Y = 50;
    var UA_VALUE_Y = 258, UA_DELTA_Y = 274;

    var seq = 0;

    function svg(tag, attrs) {
        var n = document.createElementNS(NS, tag);
        Object.keys(attrs || {}).forEach(function (k) { n.setAttribute(k, attrs[k]); });
        return n;
    }

    function text(host, x, y, value, attrs) {
        var n = svg("text", attrs || {});
        n.setAttribute("x", x);
        n.setAttribute("y", y);
        n.textContent = value;
        host.appendChild(n);
        return n;
    }

    function el(tag, cls, value) {
        var n = document.createElement(tag);
        if (cls) { n.className = cls; }
        if (value !== undefined) { n.textContent = value; }
        return n;
    }

    function num(v, d) {
        if (v === null || v === undefined || isNaN(v)) { return "—"; }
        return v.toLocaleString("fr-FR", { minimumFractionDigits: d || 0, maximumFractionDigits: d || 0 })
            .replace(/ /g, " ");
    }

    function signed(v, d) {
        return (v > 0 ? "+" : v < 0 ? "−" : "±") + num(Math.abs(v), d);
    }

    function dateOf(t) {
        return window.tovDates ? window.tovDates.of(t) : String(t.year);
    }

    function yieldOf(code) {
        for (var i = 0; i < POSTS.length; i++) {
            if (POSTS[i].code === code) { return POSTS[i].yield; }
        }
        return "";
    }

    // Les huit postes viennent du moteur, dans l'ordre du bandeau. Une seule autorité par
    // information : le lecteur de capital les mesure, les indexe et les attribue — la vue ne
    // recalcule rien, sinon deux chiffres finiraient par répondre à la même question sans
    // qu'on sache lequel croire.
    function postsOf(game, turnIndex, invader) {
        var side = invader ? game.turns[turnIndex].invader : game.turns[turnIndex].defender;
        var out = [];

        POSTS.forEach(function (p) {
            (side.capital || []).forEach(function (post) {
                if (post.code === p.code) { out.push(post); }
            });
        });

        return out;
    }

    /* ---------------- Le bandeau ---------------- */

    function hatch(defs, id) {
        var p = svg("pattern", {
            id: id, width: "6", height: "6", patternUnits: "userSpaceOnUse",
            patternTransform: "rotate(45)"
        });
        p.appendChild(svg("rect", { width: "6", height: "6", fill: "#a8322a", opacity: "0.12" }));
        p.appendChild(svg("line", { x1: "0", y1: "0", x2: "0", y2: "6", stroke: "#a8322a", "stroke-width": "2.6", opacity: "0.45" }));
        defs.appendChild(p);
    }

    // L'arête de coupe n'est jamais droite : on doit voir le morceau arraché, pas un
    // rectangle propre. Même convention que les cinq parcs.
    function tornEdge(x, y, w, amplitude, down) {
        var steps = 6, d = "M" + x + " " + y;
        for (var i = 1; i <= steps; i++) {
            var px = x + (w * i / steps);
            var wobble = (i % 2 === 0 ? amplitude : -amplitude) * (down ? -1 : 1);
            d += " L" + px.toFixed(1) + " " + (y + (i === steps ? 0 : wobble)).toFixed(1);
        }
        return d;
    }

    function toIndex(post, value) {
        if (!post.reference) { return 100; }
        if (post.inverted) { return value <= 0 ? 100 : post.reference / value * 100; }
        return value / post.reference * 100;
    }

    function cartouche(host, post, colour, invader, hatchId, alerted, column) {
        var x = COL_X0 + column * COL_PITCH - COL_W / 2;
        var base = invader ? RU_BASE : UA_BASE;
        var dir = invader ? -1 : 1;
        var index = post.index;
        var h = Math.min(Math.max(index, 0) * PX_PER_100, MAX_H);
        var ruined = index < RUINED_INDEX;

        var g = svg("g", { class: "cap-post" + (alerted ? " alerted" : "") });

        var tip = svg("title", {});
        tip.textContent = post.name + " — " + num(post.value, post.decimals) + " " + post.unit +
            " (indice " + num(index, 0) + ", base 100 au premier trimestre)\n" + yieldOf(post.code) +
            "\nVariation du trimestre : " + signed(post.displayDelta, Math.max(post.decimals, 1)) + " " + post.unit +
            (post.destructionCause ? "\nDétruit par : " + post.destructionCause : "") +
            (post.secondaryLabel ? "\n" + post.secondaryLabel + " : " + num(post.secondary, 2) : "");
        g.appendChild(tip);

        // Le repère du premier trimestre : le pointillé qui rend toute masse lisible sans
        // échelle. Aucune étiquette — c'est le 100 % du tonneau, et il se lit pareil.
        var refY = base + dir * (100 * PX_PER_100);
        g.appendChild(svg("line", {
            x1: x - 4, y1: refY, x2: x + COL_W + 4, y2: refY,
            stroke: "#8b8578", "stroke-width": "1", "stroke-dasharray": "2 4", opacity: "0.9"
        }));

        var top = invader ? base - h : base;
        var bottom = invader ? base : base + h;

        if (ruined) {
            // Sous le quart de son indice de départ, un poste cesse d'être une masse : il
            // devient un trou dans le bandeau, et un trou se repère avant d'être lu.
            var ghostH = Math.abs(refY - base);
            g.appendChild(svg("rect", {
                x: x, y: invader ? base - ghostH : base, width: COL_W, height: ghostH,
                fill: "url(#" + hatchId + ")"
            }));
            g.appendChild(svg("rect", {
                x: x, y: invader ? base - ghostH : base, width: COL_W, height: ghostH,
                fill: "none", stroke: colour, "stroke-width": "1.2", "stroke-dasharray": "3 3", opacity: "0.8"
            }));
            if (h > 1) {
                g.appendChild(svg("rect", { x: x, y: top, width: COL_W, height: h, fill: colour, opacity: "0.95" }));
            }
        } else {
            g.appendChild(svg("rect", {
                x: x, y: top, width: COL_W, height: Math.max(h, 1.5),
                fill: colour, stroke: "rgba(26,24,21,0.3)", "stroke-width": "0.7"
            }));
            // Chant extérieur biseauté : la matière a une épaisseur, comme les douves.
            g.appendChild(svg("rect", {
                x: x + 1.4, y: invader ? top + 1.2 : bottom - 4.6, width: COL_W - 2.8, height: 3.4,
                fill: "#fff", opacity: "0.34"
            }));
        }

        // L'encoche de destruction : la part détruite est découpée dans le haut de la masse
        // par une arête irrégulière. On voit le morceau manquant.
        var destroyedIndex = post.reference ? Math.abs(post.destruction) / post.reference * 100 : 0;
        var hd = Math.min(destroyedIndex * PX_PER_100, h);
        if (post.destruction > 0 && hd > 0.6 && !ruined) {
            var cutY = invader ? top + hd : bottom - hd;
            g.appendChild(svg("rect", {
                x: x, y: invader ? top : bottom - hd, width: COL_W, height: hd,
                fill: "url(#" + hatchId + ")"
            }));
            g.appendChild(svg("path", {
                d: tornEdge(x, cutY, COL_W, 2.4, !invader),
                fill: "none", stroke: "#a8322a", "stroke-width": "1.1", opacity: "0.85"
            }));
        }

        // Le filet de variation ordinaire : gravé, jamais plein. Le gravé est ordinaire,
        // le plein est une destruction — aucune légende à consulter.
        var ordinary = post.regeneration - post.consumption;
        if (Math.abs(ordinary) > 0.0001 && post.reference) {
            var ho = Math.min(Math.abs(ordinary) / post.reference * 100 * PX_PER_100, MAX_H);
            if (ho > 0.8) {
                var fy = ordinary > 0
                    ? (invader ? top - 3 : bottom + 3)
                    : (invader ? top + ho : bottom - ho);
                g.appendChild(svg("line", {
                    x1: x + 10, y1: fy, x2: x + COL_W - 10, y2: fy,
                    stroke: "#8b8578", "stroke-width": "2", "stroke-dasharray": "1 2"
                }));
            }
        }

        // Le seuil, sur les postes qui en ont un, et seulement ceux-là.
        if (post.threshold !== null && post.threshold !== undefined) {
            var ht = Math.min(toIndex(post, post.threshold) * PX_PER_100, MAX_H);
            var ty = base + dir * ht;
            g.appendChild(svg("line", {
                x1: x - 4, y1: ty, x2: x + COL_W + 4, y2: ty,
                stroke: "#a8322a", "stroke-width": "1", "stroke-dasharray": "3 2", opacity: "0.85"
            }));
        }

        // La valeur et son delta occupent une bande qui leur est réservée : quelle que soit
        // la hauteur de la colonne, le chiffre reste lisible.
        text(g, x + COL_W / 2, invader ? RU_VALUE_Y : UA_VALUE_Y, num(post.value, post.decimals), {
            "text-anchor": "middle", class: "cap-value"
        });

        // Un poste qui n'a pas bougé le dit : « −0,0 » n'est pas une variation, c'est un
        // arrondi, et l'écrire comme une variation ferait chercher une cause qui n'existe pas.
        var precision = Math.max(post.decimals, 1);
        var moved = Math.abs(post.displayDelta) >= 0.5 / Math.pow(10, precision);

        text(g, x + COL_W / 2, invader ? RU_DELTA_Y : UA_DELTA_Y,
            moved ? signed(post.displayDelta, precision) + " " + post.unit : "inchangé", {
                "text-anchor": "middle",
                class: "cap-delta" + (moved ? (post.destruction > 0 ? " destroyed" : "") : " flat")
            });

        // La masse bute sur son plafond : le chevron dit que la colonne est hors d'échelle,
        // et le chiffre imprimé dans sa bande dit de combien. Il se grave DANS le chant de la
        // masse, jamais au-dessus : posé à l'extérieur, il venait buter contre le chiffre sur
        // les colonnes les plus hautes — c'est-à-dire précisément celles qu'il annote.
        if (index * PX_PER_100 > MAX_H) {
            var cy = invader ? top + 10 : bottom - 10;
            g.appendChild(svg("path", {
                d: "M" + (x + COL_W / 2 - 7) + " " + cy + " l7 " + (invader ? -5 : 5) + " l7 " + (invader ? 5 : -5),
                fill: "none", stroke: "#fff", "stroke-width": "2", "stroke-linecap": "round", opacity: "0.8"
            }));
        }

        // Le cadenas : la perte est définitive à l'échelle de la partie. Il dit
        // « irréparable » mieux qu'un chiffre.
        if (post.permanentLoss) {
            var lx = x + COL_W - 15, ly = invader ? base - 20 : base + 6;
            g.appendChild(svg("rect", { x: lx, y: ly + 6, width: 11, height: 8, rx: "1.5", fill: "#1a1815" }));
            g.appendChild(svg("path", {
                d: "M" + (lx + 2.2) + " " + (ly + 6) + " v-2.6 a3.3 3.3 0 0 1 6.6 0 V" + (ly + 6),
                fill: "none", stroke: "#1a1815", "stroke-width": "1.5"
            }));
        }

        host.appendChild(g);
    }

    function quarterCartouche(host, t) {
        var g = svg("g", {});
        g.appendChild(svg("rect", {
            x: 22, y: 132, width: 136, height: 40, rx: "2",
            fill: "#fbf9f4", stroke: "#d9d1be", "stroke-width": "1"
        }));
        text(g, 30, 146, dateOf(t), { class: "cap-quarter" });
        text(g, 30, 164, num(t.oilPrice, 0) + " $", { class: "cap-brent" });

        // La saison décide : elle mérite un signe et pas un mot.
        if (t.season === "Winter") {
            var cx = 140, cy = 158;
            [0, 60, 120].forEach(function (a) {
                var r = a * Math.PI / 180, dx = Math.cos(r) * 6, dy = Math.sin(r) * 6;
                g.appendChild(svg("line", {
                    x1: cx - dx, y1: cy - dy, x2: cx + dx, y2: cy + dy,
                    stroke: "#4e4a42", "stroke-width": "1.1"
                }));
            });
        }

        host.appendChild(g);
    }

    // Deux lignes aussi égales que possible : « Tenue du / pouvoir » se lit, « Tenue / du
    // pouvoir » trébuche.
    function splitName(name) {
        var words = name.split(" ");
        if (words.length < 2) { return [name]; }

        var best = null;
        for (var cut = 1; cut < words.length; cut++) {
            var head = words.slice(0, cut).join(" ");
            var tail = words.slice(cut).join(" ");
            var widest = Math.max(head.length, tail.length);
            if (best === null || widest < best.widest) {
                best = { lines: [head, tail], widest: widest };
            }
        }

        return best.lines;
    }

    // Le nom du poste est écrit une seule fois, dans la bande de partage, et les deux masses
    // le tirent de part et d'autre comme une corde. Un nom de deux mots passe sur deux lignes
    // plutôt que de déborder sur la colonne voisine : « Soutien étranger » et « Soutien
    // international » sont deux postes distincts, et deux libellés qui se chevauchent
    // annuleraient précisément la distinction qu'on vient d'établir.
    function sharedNames(host, alertedCode) {
        POSTS.forEach(function (p, i) {
            var cx = COL_X0 + i * COL_PITCH;
            var lines = splitName(NAMES[p.code]);

            var widest = Math.max.apply(null, lines.map(function (line) { return line.length; }));
            var top = lines.length > 1 ? 150 : 156;

            if (p.code === alertedCode) {
                host.appendChild(svg("rect", {
                    x: cx - widest * 3.1 - 8, y: top - 10,
                    width: widest * 6.2 + 16, height: lines.length > 1 ? 26 : 15,
                    rx: "7.5", fill: "#1a1815"
                }));
            }

            lines.forEach(function (line, row) {
                text(host, cx, top + row * 11, line, {
                    "text-anchor": "middle",
                    class: "cap-name" + (p.code === alertedCode ? " alerted" : "")
                });
            });

            host.appendChild(svg("line", {
                x1: cx, y1: 134, x2: cx, y2: 138, stroke: "#d9d1be", "stroke-width": "1"
            }));
            host.appendChild(svg("line", {
                x1: cx, y1: 166, x2: cx, y2: 170, stroke: "#d9d1be", "stroke-width": "1"
            }));
        });
    }

    function alertedPost(t) {
        var alerts = t.alerts || [];
        for (var i = 0; i < alerts.length; i++) {
            var code = ALERT_POST[alerts[i].code];
            if (code) { return code; }
        }
        return null;
    }

    // Le ruban : trois deltas côte à côte ne font pas une chaîne. Il n'apparaît que s'il y
    // a eu destruction, et un trimestre calme est un bandeau mince — cette minceur est
    // elle-même une information.
    function ribbon(side, cls) {
        if (!side.chain) { return null; }

        var row = el("div", "cap-ribbon " + cls);
        row.appendChild(el("span", "cr-origin", "« " + side.chain.origin + " »"));

        (side.chain.links || []).forEach(function (link) {
            row.appendChild(el("span", "cr-arrow", "→"));
            var box = el("span", "cr-link");
            box.appendChild(el("b", null, link.label));
            box.appendChild(el("i", null, signed(link.percentDelta, 1) + " %"));
            row.appendChild(box);
        });

        return row;
    }

    function band(game, turnIndex) {
        var t = game.turns[turnIndex];
        var host = el("section", "panel capital-band");

        var head = el("div", "cap-head");
        head.appendChild(el("h3", null, "Le capital de guerre"));
        head.appendChild(el("p", null,
            "Ce que chaque camp possède encore pour faire la guerre, et ce que le trimestre lui a pris. " +
            "Chaque masse est un indice base 100 au premier trimestre de son propre camp : on compare " +
            "des trajectoires, jamais des masses."));
        host.appendChild(head);

        var s = svg("svg", { viewBox: "0 0 " + W + " " + H, class: "cap-svg", role: "img" });
        var defs = svg("defs", {});
        s.appendChild(defs);
        var hatchId = "capHatch" + (seq++);
        hatch(defs, hatchId);

        // Le camp est rappelé par un filet sur le bord extérieur de sa rangée, et par son
        // nom une seule fois à l'extrémité gauche.
        s.appendChild(svg("rect", { x: 22, y: ROW_TOP - 3, width: W - 44, height: 3, fill: "#a8322a" }));
        s.appendChild(svg("rect", { x: 22, y: ROW_BOTTOM, width: W - 44, height: 3, fill: "#1e5fa8" }));
        text(s, 22, ROW_TOP + 12, t.invader.name.toUpperCase(), { class: "cap-side ru" });
        text(s, 22, ROW_BOTTOM - 4, t.defender.name.toUpperCase(), { class: "cap-side ua" });

        s.appendChild(svg("line", { x1: 22, y1: RU_BASE, x2: W - 22, y2: RU_BASE, stroke: "#d9d1be", "stroke-width": "1" }));
        s.appendChild(svg("line", { x1: 22, y1: UA_BASE, x2: W - 22, y2: UA_BASE, stroke: "#d9d1be", "stroke-width": "1" }));

        var alerted = alertedPost(t);
        quarterCartouche(s, t);
        sharedNames(s, alerted);

        var ru = postsOf(game, turnIndex, true);
        var ua = postsOf(game, turnIndex, false);

        POSTS.forEach(function (p, i) {
            ru.forEach(function (post) {
                if (post.code === p.code) { cartouche(s, post, p.colour, true, hatchId, p.code === alerted, i); }
            });
            ua.forEach(function (post) {
                if (post.code === p.code) { cartouche(s, post, p.colour, false, hatchId, p.code === alerted, i); }
            });
        });

        host.appendChild(s);

        var ruRibbon = ribbon(t.invader, "ru");
        var uaRibbon = ribbon(t.defender, "ua");
        if (ruRibbon) { host.appendChild(ruRibbon); }
        if (uaRibbon) { host.appendChild(uaRibbon); }

        return host;
    }

    /* ---------------- Le ciseau ---------------- */

    var SW = 560, SH = 230, SX0 = 62, SX1 = 530, SY0 = 210, SYTOP = 20;

    function series(game, invader, upTo) {
        var out = [];
        var key = invader ? "invader" : "defender";
        var opening = game.turns[0][key].combatPower || 1;

        for (var i = 0; i <= upTo && i < game.turns.length; i++) {
            out.push({
                t: game.turns[i],
                front: game.turns[i][key].combatPower / opening * 100,
                capital: game.turns[i][key].capitalIndex,
                gained: invader
                    ? (game.turns[i].totalHexesGained || 0) > 0.01
                    : (game.turns[i].totalHexesGained || 0) < -0.01
            });
        }

        return out;
    }

    function scissor(game, invader, upTo, colour, title, top) {
        var pts = series(game, invader, upTo);
        var host = el("div", "scissor");
        var s = svg("svg", { viewBox: "0 0 " + SW + " " + SH, class: "sc-svg", role: "img" });
        var defs = svg("defs", {});
        s.appendChild(defs);
        var burnId = "scBurn" + (seq++);
        hatch(defs, burnId);

        var step = pts.length > 1 ? (SX1 - SX0) / (pts.length - 1) : 0;
        var x = function (i) { return SX0 + i * step; };
        var y = function (v) { return SY0 - Math.max(0, Math.min(v, top)) / top * (SY0 - SYTOP); };

        // La ligne de départ : cent points, le niveau du premier trimestre pour les deux courbes.
        s.appendChild(svg("line", {
            x1: SX0 - 8, y1: y(100), x2: SX1 + 8, y2: y(100),
            stroke: "#8b8578", "stroke-width": "1", "stroke-dasharray": "2 4"
        }));
        s.appendChild(svg("line", { x1: SX0 - 8, y1: SY0, x2: SX1 + 8, y2: SY0, stroke: "#d9d1be", "stroke-width": "1" }));
        text(s, SX0 - 12, y(100) + 3.5, "100", { "text-anchor": "end", class: "cap-label" });

        // La surface entre les deux courbes EST le sujet : hachurée quand le front vit sur
        // le capital, crème quand le capital se reconstitue.
        var burning = "", rebuilding = "";
        for (var i = 0; i < pts.length - 1; i++) {
            var seg = "M" + x(i) + " " + y(pts[i].front) + " L" + x(i + 1) + " " + y(pts[i + 1].front) +
                " L" + x(i + 1) + " " + y(pts[i + 1].capital) + " L" + x(i) + " " + y(pts[i].capital) + " Z";
            if (pts[i].front > pts[i].capital) { burning += seg; } else { rebuilding += seg; }
        }
        if (rebuilding) { s.appendChild(svg("path", { d: rebuilding, fill: "#f5f1e6" })); }
        if (burning) { s.appendChild(svg("path", { d: burning, fill: "url(#" + burnId + ")", opacity: "0.6" })); }

        var line = function (key, stroke) {
            var d = "";
            pts.forEach(function (p, i) { d += (i ? " L" : "M") + x(i) + " " + y(p[key]); });
            s.appendChild(svg("path", { d: d, fill: "none", stroke: stroke, "stroke-width": "2.2", "stroke-linejoin": "round" }));
        };
        line("capital", colour);
        line("front", "#1a1815");

        // Les trimestres où ce camp a pris du terrain : on avance, et la courbe monte.
        pts.forEach(function (p, i) {
            if (p.gained) {
                s.appendChild(svg("circle", { cx: x(i), cy: y(p.front), r: "2.4", fill: "#1a1815" }));
            }
        });

        // Le croisement qui compte n'est pas le premier : c'est celui après lequel le front
        // ne repasse plus jamais sous le capital. Nommer une oscillation d'un trimestre
        // ferait annoncer une bascule qui n'a pas eu lieu.
        var crossing = -1;
        for (var c = pts.length - 1; c > 0; c--) {
            if (pts[c].front - pts[c].capital < 8) { break; }
            if (pts[c - 1].front <= pts[c - 1].capital) { crossing = c; break; }
            crossing = c;
        }

        if (crossing > 0) {
            s.appendChild(svg("line", {
                x1: x(crossing), y1: SYTOP, x2: x(crossing), y2: SY0,
                stroke: "#1a1815", "stroke-width": "1", opacity: "0.5"
            }));
            text(s, Math.min(x(crossing) + 6, SX1 - 250), SYTOP - 6,
                "le front vit sur le capital — " + dateOf(pts[crossing].t), { class: "cap-label" });
        }

        // Chaque valeur de fin s'écarte du côté où sa courbe est déjà : celle du dessus monte,
        // celle du dessous descend. Le trimestre où les deux se rejoignent est justement celui
        // où il faut pouvoir les lire toutes les deux.
        var last = pts[pts.length - 1];
        var frontOnTop = last.front >= last.capital;
        text(s, SX1 - 4, y(last.front) + (frontOnTop ? -9 : 19), num(last.front, 0),
            { "text-anchor": "end", class: "sc-end front" });
        text(s, SX1 - 4, y(last.capital) + (frontOnTop ? 19 : -9), num(last.capital, 0),
            { "text-anchor": "end", class: "sc-end", fill: colour });

        text(s, SX0 - 40, SY0 + 18, dateOf(pts[0].t), { class: "cap-label" });
        text(s, SX1, SY0 + 18, dateOf(last.t), { "text-anchor": "end", class: "cap-label" });

        host.appendChild(el("div", "sc-title", title));
        host.appendChild(s);
        return host;
    }

    function divergence(game, turnIndex) {
        var host = el("section", "panel capital-scissor");

        var head = el("div", "cap-head");
        head.appendChild(el("h3", null, "Le front contre le capital"));
        head.appendChild(el("p", null,
            "Trait noir : la puissance au front, ce qu'on prend pour son succès. Trait de couleur : " +
            "le capital qui la produit. Quand le noir passe au-dessus, le camp avance en brûlant ce qui " +
            "lui reste — la hachure s'installe plusieurs trimestres avant que le front ne cède."));
        host.appendChild(head);

        // Une seule ordonnée pour les deux panneaux. Deux échelles côte à côte laisseraient
        // croire que les deux camps brûlent au même rythme parce que les courbes se
        // ressemblent, ce qui est exactement l'erreur que la pièce existe pour éviter.
        var top = 130;
        [true, false].forEach(function (invader) {
            series(game, invader, turnIndex).forEach(function (p) {
                top = Math.max(top, p.front, p.capital);
            });
        });
        top = Math.ceil(top / 25) * 25;

        var pair = el("div", "sc-pair");
        pair.appendChild(scissor(game, true, turnIndex, "#a8322a", game.turns[0].invader.name, top));
        pair.appendChild(scissor(game, false, turnIndex, "#1e5fa8", game.turns[0].defender.name, top));
        host.appendChild(pair);

        return host;
    }

    window.tovCapital = { band: band, divergence: divergence };
})();
