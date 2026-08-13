// The depth ribbon. Owns its own file, like hexmap.js and counters.js.
//
//   draw(svg, turn, project, opts) -> void
//   - svg      the map's <svg>
//   - turn     one TurnSnapshot: turn.invaderStrike, turn.defenderStrike, turn.invader, turn.defender
//   - project  lon,lat -> [x,y], the map's own padded projector
//   - opts     optional. opts.arcs === false leaves out the two overflights
//
// counters.js calls it FIRST, before its own pieces, so the two overflights pass under the
// front rather than over it. That is not a detail of z-order, it is the argument: striking
// deep does not move the line by a metre, and it is the only order that wins the war.
//
// One card per side, standing in that side's own rear, well off the contact line. The rear is
// NOT to scale and the card says so: Russia's depth is off this map entirely, and pretending
// otherwise would be the one lie the piece cannot afford.
//
// ---------------------------------------------------------------------------
// WHAT THE CARD SAYS, AND WHY IT IS NEVER EMPTY
//
// A strike wave has two readings and the model needs both:
//
//   - what was STOPPED — the salvo bar, intercepted against leaked, and the interceptors it
//     cost to stop it. Those are the rounds that will be missing at the front, which the model
//     calls the finest trade in the game;
//   - what GOT THROUGH — the impact squares, hollow for damage that will be repaired in weeks
//     and solid for a loss measured in years. A sustained campaign wins where a spectacular
//     raid gives nothing, and that is the whole distinction.
//
// This matters right now because of a known calibration fault, written up in
// docs/design/09-audit-realisme.md §3.2: no wave saturates in any of the three runs, and
// interception sits at 100 % from turn 3 onwards. Drawn as leaked damage alone, the ribbon
// would be a row of empty boxes for seventeen consecutive turns. Drawn as a shield that held
// and what holding cost, it stays true and stays informative — and it will show the leaks by
// itself once the vector capacities are raised.
//
// ---------------------------------------------------------------------------
// WHAT IS DEGRADED, AND WHY
//
//   - No wave at all         turn.invaderStrike or turn.defenderStrike is null on a quarter
//                            with no campaign. The card stands and says so. A quiet quarter is
//                            information, not a broken piece.
//   - Industry has no gauge  the three other targets each have a published standing figure
//                            (permanent grid damage in GW, refining integrity, logistics
//                            integrity). Arms plants are damaged capacity by capacity, with no
//                            scalar on the wire, so that line is simply absent rather than
//                            approximated from the capacity map.
//   - Damage is abstract     DamageInflicted is in the engine's own damage points, deliberately
//                            unitless. Turning it into squares needs a display constant, and
//                            SQUARE_DAMAGE below is exactly that — to be re-tuned once the
//                            corrected engine produces waves that actually leak. It changes no
//                            engine output.
//
// Same discipline as the counters: the JavaScript never recomputes a quantity of the model. If
// a figure is missing the drawing degrades; it never invents one.
window.tovDepth = (function () {
    "use strict";

    /* ---------------- Display constants ---------------- */

    var CW = 132, CH = 82;          // rear card
    var INNER = CW - 20;            // what a line of text may occupy inside it

    // The hollow square against the solid one — repairable damage against a loss measured in
    // years — is what this card has that nothing else on the board has. At seven pixels the
    // distinction did not survive; ten is the size at which it reads. The count came down from
    // twelve to eight to pay for it, which costs nothing: a wave that fills eight squares is
    // already a catastrophe, and the exact number is in the tooltip.
    var SQUARE = 10, SQUARE_GAP = 3, MAX_SQUARES = 6;
    var SQUARE_DAMAGE = 0.075;      // damage points per square — display only

    // Fixed screen anchors rather than coordinates. The Ukrainian rear is real ground and sits
    // over the empty paper west of the line, around Zhytomyr; the Russian rear is not on this
    // map at all and stands beyond the border, where the map has nothing to say.
    //
    // Both stand at the western and eastern extremities, as far from the contact line as the
    // frame allows: the deep rear of Ukraine is its west, and Russia's is off this map
    // entirely. The first draft put the Ukrainian card in the middle of the country, over the
    // Kyiv region, which said the opposite of what the piece is for — it made the depth look
    // like a place on the plateau rather than a place beyond it.
    //
    // The clearances are measured, not guessed: the POLOGNE label sits at y 114, Lviv's at
    // x 163 running to 198, the legend panel starts at y 214, the RUSSIE label is around
    // x 720 y 80, and the eastern column of gauges comes down from y 137.
    var PLACE = {
        defender: { x: 20, y: 122, label: "Arrière ukrainien", bow: 40 },
        invader: { x: 752, y: 16, label: "Arrière russe", bow: 72 }
    };

    var COLOUR = {
        card: "#fbf9f4",
        rule: "#d9d1be",
        ink: "#1a1815",
        ink2: "#4e4a42",
        ink3: "#8b8578",
        paper: "#fffdf8",
        invader: "#a8322a",
        defender: "#1e5fa8",
        gold: "#b8860b"
    };

    var TARGETS = {
        PowerGrid: "Réseau électrique",
        Refining: "Raffinage et terminaux",
        Industry: "Usines d'armement",
        Logistics: "Nœuds logistiques"
    };

    /* ---------------- Helpers, same facture as hexmap.js ---------------- */

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

    // Estimated width of a run of text. SVG can measure itself once it is in a document, but
    // nothing here is in a document yet and the card has to be laid out before it is attached.
    // The coefficients are deliberately generous: a line that is judged slightly too wide and
    // set one point smaller costs nothing, a line judged too narrow runs off the card.
    function widthOf(content, attrs) {
        var size = parseFloat(attrs["font-size"] || "10");
        var serif = (attrs["font-family"] || "").indexOf("Georgia") >= 0;
        var spacing = String(attrs["letter-spacing"] || "0");
        var extra = spacing.indexOf("em") >= 0 ? size * parseFloat(spacing) : parseFloat(spacing) || 0;
        return content.length * ((size * (serif ? 0.58 : 0.56)) + extra);
    }

    // A line that never leaves its card. It shrinks a point at a time down to a floor, then
    // truncates — because the alternative, which this card shipped with, is a sentence running
    // off the panel and off the map frame behind it.
    function fitted(x, y, content, attrs, maxWidth) {
        var size = parseFloat(attrs["font-size"] || "10");
        var trimmed = content;

        while (widthOf(trimmed, attrs) > maxWidth && size > 7) {
            size -= 0.5;
            attrs["font-size"] = String(size);
        }
        while (widthOf(trimmed, attrs) > maxWidth && trimmed.length > 4) {
            trimmed = trimmed.slice(0, -2) + "…";
        }

        return text(x, y, trimmed, attrs);
    }

    function tip(node, content) {
        var t = svgEl("title", {});
        t.textContent = content;
        node.appendChild(t);
        return node;
    }

    // French grouping: a narrow no-break space, never a comma.
    function count(value) {
        var s = String(Math.round(value));
        var out = "";
        for (var i = 0; i < s.length; i++) {
            if (i > 0 && (s.length - i) % 3 === 0) { out += " "; }
            out += s[i];
        }
        return out;
    }

    function percent(value) {
        return Math.round(value * 100) + " %";
    }

    /* ---------------- Reading the turn ---------------- */

    // Each side's card shows what was done TO its rear, so it reads the OTHER side's wave.
    // The standing figures come from its own snapshot, where the damage has landed.
    function readRear(turn, sideCode) {
        var wave = sideCode === "invader" ? turn.defenderStrike : turn.invaderStrike;
        var snapshot = sideCode === "invader" ? turn.invader : turn.defender;

        return {
            code: sideCode,
            colour: sideCode === "invader" ? COLOUR.invader : COLOUR.defender,
            place: PLACE[sideCode],
            wave: wave || null,
            standing: standingFigure(wave, snapshot)
        };
    }

    // The line that makes a sustained campaign legible: what the rear still carries from every
    // quarter before this one. Only published fields, one per target, and none for the arms
    // plants — they are damaged capacity by capacity, with no single figure on the wire.
    function standingFigure(wave, snapshot) {
        if (!wave || !snapshot) { return null; }

        if (wave.target === "PowerGrid" && snapshot.permanentGridDamage !== undefined) {
            return snapshot.permanentGridDamage <= 0.01
                ? "réseau intact"
                : "réseau : " + snapshot.permanentGridDamage.toFixed(1).replace(".", ",") + " GW perdus";
        }

        if (wave.target === "Refining" && snapshot.refiningIntegrity !== undefined) {
            return "raffinage : " + percent(snapshot.refiningIntegrity) + " debout";
        }

        if (wave.target === "Logistics" && snapshot.logisticsIntegrity !== undefined) {
            return "logistique : " + percent(snapshot.logisticsIntegrity) + " debout";
        }

        return null;
    }

    /* ---------------- The rear card ---------------- */

    function card(svg, rear) {
        var x = rear.place.x, y = rear.place.y;
        var g = svgEl("g", { transform: "translate(" + x + " " + y + ")" });

        g.appendChild(svgEl("rect", {
            x: 0, y: 0, width: CW, height: CH, rx: 3,
            fill: COLOUR.card, stroke: COLOUR.rule, "stroke-width": "1"
        }));

        // The rear is not to scale and never pretends to be: a dashed edge, not a border.
        g.appendChild(svgEl("line", {
            x1: 0, y1: 0, x2: 0, y2: CH,
            stroke: rear.colour, "stroke-width": "2.4", opacity: "0.8"
        }));

        g.appendChild(fitted(10, 13, rear.place.label.toUpperCase(), {
            "font-size": "8", "font-weight": "700", "letter-spacing": "0.13em", fill: COLOUR.ink3
        }, INNER));

        if (!rear.wave) {
            g.appendChild(fitted(10, 32, "Aucune frappe ce trimestre", {
                "font-size": "10", "font-style": "italic",
                "font-family": "Georgia, 'Palatino Linotype', serif", fill: COLOUR.ink3
            }, INNER));
            g.appendChild(fitted(10, 48, "Vecteurs restés au dépôt.", {
                "font-size": "8.5", fill: COLOUR.ink3
            }, INNER));
            svg.appendChild(g);
            return;
        }

        wave(g, rear);
        svg.appendChild(g);
    }

    function wave(g, rear) {
        var w = rear.wave;
        var sent = (w.dronesSent || 0) + (w.missilesSent || 0);
        var leaked = (w.dronesLeaked || 0) + (w.missilesLeaked || 0);
        var stopped = Math.max(0, sent - leaked);

        g.appendChild(fitted(10, 29, TARGETS[w.target] || w.target, {
            "font-size": "10.5", "font-weight": "600", fill: COLOUR.ink
        }, INNER));

        /* The salvo bar. One length, two parts: what the magazines stopped and what went
           through. A rate read as a length rather than a percentage — and on this model, most
           quarters, it is almost entirely one colour, which is itself the finding.

           It used to carry a "0 passés sur 7 047" caption, right-aligned on the title's own
           line, where the two ran into each other and neither could be read. The bar already
           says it as a length and the tooltip says it in figures: the caption was the third
           telling and it is gone. */
        var barX = 10, barY = 36, barW = CW - 20, barH = 7;
        var through = sent <= 0 ? 0 : Math.min(1, leaked / sent);

        var bar = svgEl("g", {});
        bar.appendChild(svgEl("rect", {
            x: barX, y: barY, width: barW, height: barH,
            fill: COLOUR.paper, stroke: COLOUR.rule, "stroke-width": "0.7"
        }));
        if (stopped > 0) {
            bar.appendChild(svgEl("rect", {
                x: barX, y: barY, width: barW * (1 - through), height: barH,
                fill: COLOUR.ink3, opacity: "0.38"
            }));
        }
        if (leaked > 0) {
            bar.appendChild(svgEl("rect", {
                x: barX + barW * (1 - through), y: barY, width: barW * through, height: barH,
                fill: rear.colour === COLOUR.invader ? COLOUR.defender : COLOUR.invader
            }));
        }
        tip(bar, count(sent) + " vecteurs tirés · " + count(stopped) + " interceptés · "
            + count(leaked) + " passés\n"
            + count(w.cheapInterceptorsSpent || 0) + " intercepteurs bas coût et "
            + count(w.heavyInterceptorsSpent || 0) + " lourds dépensés — "
            + "ce sont ceux qui manqueront au front.");
        g.appendChild(bar);

        /* The impacts. Hollow is repaired in weeks and has to be done again; solid is a turbine
           hall, and the delay is measured in years. This is the whole reason a campaign beats
           a raid, and it is the one distinction the piece exists to carry. */
        squares(g, 10, 49, w);

        // The trade, in the model's own terms: what a round cost against what it destroyed.
        if (w.exchangeRatio > 0) {
            var ratio = svgEl("g", {});
            ratio.appendChild(svgEl("rect", {
                x: CW - 42, y: 48, width: 32, height: 14, rx: 2,
                fill: "#f5f1e6", stroke: COLOUR.rule, "stroke-width": "0.6"
            }));
            ratio.appendChild(text(CW - 26, 58, "1 : " + Math.round(w.exchangeRatio), {
                "text-anchor": "middle", "font-size": "9",
                "font-family": "Georgia, 'Palatino Linotype', serif", fill: COLOUR.ink
            }));
            tip(ratio, "Rapport d'échange : le défenseur dépense "
                + Math.round(w.exchangeRatio) + " fois ce que vaut le vecteur qu'il abat.");
            g.appendChild(ratio);
        }

        if (rear.standing) {
            g.appendChild(fitted(10, CH - 10, rear.standing, {
                "font-size": "8.5", fill: COLOUR.ink2
            }, INNER));
        }

        // Saturation is what opens the door to the missiles, and it is invisible everywhere
        // else on this board. When it happens, it is the loudest thing on the card.
        if (w.saturated) {
            g.appendChild(text(CW / 2, 44, "SATURATION", {
                "text-anchor": "middle", "font-size": "13", "font-weight": "700",
                "letter-spacing": "0.18em", fill: COLOUR.gold, opacity: "0.85",
                transform: "rotate(-8 " + (CW / 2) + " 44)"
            }));
        }
    }

    function squares(g, x, y, w) {
        var total = Math.min(MAX_SQUARES, Math.ceil((w.damageInflicted || 0) / SQUARE_DAMAGE));
        if (total <= 0) {
            g.appendChild(fitted(x, y + SQUARE - 2, "Rien n'a percé.", {
                "font-size": "8.5", "font-style": "italic",
                "font-family": "Georgia, 'Palatino Linotype', serif", fill: COLOUR.ink3
            }, INNER - 46));
            return;
        }

        var permanent = Math.round(total * (w.permanentDamageShare || 0));
        var row = svgEl("g", {});

        for (var i = 0; i < total; i++) {
            var solid = i < permanent;
            row.appendChild(svgEl("rect", {
                x: x + i * (SQUARE + SQUARE_GAP), y: y, width: SQUARE, height: SQUARE,
                fill: solid ? COLOUR.ink : COLOUR.paper,
                stroke: COLOUR.ink, "stroke-width": solid ? "0" : "1.2"
            }));
        }

        tip(row, permanent + " impact(s) définitif(s) — salles des machines, turbines, "
            + "délais en années.\n" + (total - permanent)
            + " réparable(s) — sous-stations et transformateurs, remplacés en semaines : "
            + "il faudra y revenir.");
        g.appendChild(row);
    }

    /* ---------------- The overflight ----------------
       The one mark that states the thesis: it leaves a rear, crosses the whole front without
       touching it, and lands in the other rear. Dashed, faint, drawn under everything — a
       deep strike is not a front arrow and must never be mistaken for one. */

    function overflight(svg, from, to, colour, bow) {
        var x1 = from.x + CW / 2, y1 = from.y + CH / 2;
        var x2 = to.x + CW / 2, y2 = to.y + CH / 2;

        // Bowed north, over the counters rather than through them — and by a different amount
        // each way, so two waves in the same quarter read as two flights and not as one
        // muddled line drawn twice.
        var midX = (x1 + x2) / 2, midY = (y1 + y2) / 2 - bow;

        var path = svgEl("path", {
            d: "M" + x1.toFixed(1) + " " + y1.toFixed(1) +
               "Q" + midX.toFixed(1) + " " + midY.toFixed(1) +
               " " + x2.toFixed(1) + " " + y2.toFixed(1),
            fill: "none", stroke: colour, "stroke-width": "1.1",
            "stroke-dasharray": "5 5", opacity: "0.34"
        });
        tip(path, "La frappe en profondeur ne touche pas la ligne de contact.");
        svg.appendChild(path);
    }

    /* ---------------- Draw ---------------- */

    function draw(svg, turn, project, opts) {
        if (!svg || !turn) { return; }
        var options = opts || {};

        var invaderRear = readRear(turn, "invader");
        var defenderRear = readRear(turn, "defender");

        if (options.arcs !== false) {
            // Each wave is drawn from the rear that launched it to the rear it struck.
            if (turn.invaderStrike) {
                overflight(svg, PLACE.invader, PLACE.defender, COLOUR.invader, PLACE.invader.bow);
            }
            if (turn.defenderStrike) {
                overflight(svg, PLACE.defender, PLACE.invader, COLOUR.defender, PLACE.defender.bow);
            }
        }

        card(svg, invaderRear);
        card(svg, defenderRear);
    }

    return { draw: draw };
})();
