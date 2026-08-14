// The depth ribbon. Owns its own file, like hexmap.js and counters.js.
//
//   draw(svg, turn, project, opts) -> void
//   - svg      the map's <svg>
//   - turn     one TurnSnapshot: turn.invaderStrike, turn.defenderStrike
//   - project  lon,lat -> [x,y], the map's own padded projector
//   - opts     optional. opts.arcs === false leaves out the two overflights
//
// counters.js calls it FIRST, before its own pieces, so the two overflights pass under the
// front rather than over it. That is not a detail of z-order, it is the argument: striking
// deep does not move the line by a metre, and it is the only order that wins the war.
//
// ---------------------------------------------------------------------------
// WHAT THIS FILE STOPPED DRAWING, AND WHY
//
// It used to stand a card in each side's rear — the wave that struck it, the salvo bar, the
// impact squares, the standing damage figure. Two panels of dense readout, laid over a map
// whose subject is the position of the front. The same campaign is already read, in full and
// at its own scale, by the deep-strike panel of the resolution screen: drones and missiles
// launched, interception rate, stocks, exchange ratio. The map carried a second telling of it,
// smaller and harder to read, in the two corners where the eye goes last.
//
// What remains is the one mark the map alone can make: the flight itself, crossing the entire
// front without touching it. That is the thesis, and it needs no panel to say it.
window.tovDepth = (function () {
    "use strict";

    /* ---------------- Display constants ---------------- */

    // Fixed screen anchors rather than coordinates. The Ukrainian rear is real ground and sits
    // over the empty paper west of the line, around Zhytomyr; the Russian rear is not on this
    // map at all and stands beyond the border, where the map has nothing to say. Both are at
    // the western and eastern extremities, as far from the contact line as the frame allows —
    // the deep rear of Ukraine is its west, and Russia's is off this map entirely.
    var PLACE = {
        defender: { x: 86, y: 163, bow: 40 },
        invader: { x: 818, y: 57, bow: 72 }
    };

    var COLOUR = {
        invader: "#a8322a",
        defender: "#1e5fa8"
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

    function tip(node, content) {
        var t = svgEl("title", {});
        t.textContent = content;
        node.appendChild(t);
        return node;
    }

    /* ---------------- The overflight ----------------
       The one mark that states the thesis: it leaves a rear, crosses the whole front without
       touching it, and lands in the other rear. Dashed, faint, drawn under everything — a
       deep strike is not a front arrow and must never be mistaken for one. */

    function overflight(svg, from, to, colour, bow) {
        // Bowed north, over the counters rather than through them — and by a different amount
        // each way, so two waves in the same quarter read as two flights and not as one
        // muddled line drawn twice.
        var midX = (from.x + to.x) / 2, midY = (from.y + to.y) / 2 - bow;

        var path = svgEl("path", {
            d: "M" + from.x.toFixed(1) + " " + from.y.toFixed(1) +
               "Q" + midX.toFixed(1) + " " + midY.toFixed(1) +
               " " + to.x.toFixed(1) + " " + to.y.toFixed(1),
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
        if (options.arcs === false) { return; }

        // Each wave is drawn from the rear that launched it to the rear it struck.
        if (turn.invaderStrike) {
            overflight(svg, PLACE.invader, PLACE.defender, COLOUR.invader, PLACE.invader.bow);
        }
        if (turn.defenderStrike) {
            overflight(svg, PLACE.defender, PLACE.invader, COLOUR.defender, PLACE.defender.bow);
        }
    }

    return { draw: draw };
})();
