// The real front, and where the model takes over from it.
//
//   window.tovFront.prepare(turns, board)      once per rendered run
//   window.tovFront.regimeOf(turnIndex)        "documented" | "counterfactual" | "projection"
//   window.tovFront.stateAt(lon, lat, index)   "invader" | "contested" | "defender" | "free"
//   window.tovFront.kindAt(lon, lat, index)    the five map states, plus the Kursk ones
//
// ---------------------------------------------------------------------------
// WHY THIS FILE EXISTS
//
// The engine models eight sectors of the Donbass and the south. The war did not happen in eight
// sectors: the column of Kyiv, the sieges of Chernihiv and Sumy, the right bank of Kherson, the
// Kharkiv breakthrough and the Kursk salient are all outside the board it plays on, and they are
// most of what 2022 and 2024 look like. front-history.json carries them — twenty quarters of the
// real position, sourced, with the zones named. This file reads it and hands the map a control
// state for any point on the ground.
//
// THE RULE IT ENFORCES, and it comes straight from §14 of the calendar: what is reconstructed and
// what is computed are not the same thing, and the site is public. So there are three regimes,
// they are never blended, and the map says which one it is drawing.
//
//   documented      the quarter is in the chronicle and this run still matches the real war.
//                   History governs the ground outright.
//   counterfactual  the quarter is in the chronicle, but an army has broken in THIS run — an
//                   event with no historical counterpart. From there the run is a hypothesis, so
//                   the model governs, and the map stops claiming to show what happened.
//   projection      past the last documented quarter. The model governs, for every run.
//
// HOW THE MODEL TAKES OVER WITHOUT CONTRADICTING WHAT CAME BEFORE
//
// Not by switching to a different map — that would jump. The last historical position is taken as
// the board, and the simulated sectors push it: the control field is sampled at the point moved
// BACK along the push vector by the ground gained since the handover, which slides the line and
// leaves everything the sectors do not touch exactly where the chronicle left it.
//
// One correction that matters, and it is why this is not a plain translation. An advance takes the
// UNION of the field and its shifted copy, a withdrawal the INTERSECTION. Translating outright
// would drag the 2014 enclave west along with the front and open a strip of unoccupied ground
// against the Russian border, which never happens in either direction. Dilating and eroding along
// the axis of advance moves the line and only the line.
window.tovFront = (function () {
    "use strict";

    var HISTORY = window.tovFrontHistory || null;

    // Control, ordered so that a union is a maximum and a withdrawal a minimum.
    var LEVEL = { defender: -1, free: 0, contested: 1, invader: 2 };
    var NAME = ["free", "contested", "invader"];

    // How far past the last sector anchor the front's displacement still reaches before it is
    // nothing at all. The northern axes and Crimea are outside the modelled theatre and must not
    // drift when the Donbass moves.
    var FADE_DEGREES = 0.6;

    var ready = false;
    var quarters = [];       // one entry per turn index, or null past the chronicle
    var regimes = [];        // one regime per turn index
    var referenceIndex = -1; // the last turn history governed: the board the model pushes
    var deltas = [];         // per turn index, the per-sector displacement since the reference
    var baseline = null;     // autumn 2021, the line of 2014
    // Zones the invader had held or disputed BY each turn, and not by the end of the file. A
    // single set over the whole chronicle would paint Kyiv, Kherson and Kharkiv as reconquered
    // ground in the autumn of 2021 — three months before an invasion that has not happened yet.
    var everInvader = [];

    function zoneState(quarter, zone) {
        if (!quarter || !zone) { return "free"; }
        if (quarter.heldByInvader && quarter.heldByInvader.indexOf(zone) >= 0) { return "invader"; }
        if (quarter.contested && quarter.contested.indexOf(zone) >= 0) { return "contested"; }
        if (quarter.heldByDefender && quarter.heldByDefender.indexOf(zone) >= 0) { return "defender"; }
        return "free";
    }

    function quarterFor(turn) {
        if (!HISTORY || !HISTORY.quarters) { return null; }
        for (var i = 0; i < HISTORY.quarters.length; i++) {
            var q = HISTORY.quarters[i];
            if (q.year === turn.year && q.season === turn.season) { return q; }
        }
        return null;
    }

    function broken(turn) {
        return !!((turn.invader && turn.invader.hasCollapsed) ||
                  (turn.defender && turn.defender.hasCollapsed));
    }

    /* ---------------- Preparation, once per run ---------------- */

    function prepare(turns, board) {
        ready = false;
        quarters = [];
        regimes = [];
        deltas = [];
        referenceIndex = -1;
        if (!HISTORY || !HISTORY.quarters || !HISTORY.quarters.length || !turns || !turns.length) {
            return false;
        }

        baseline = HISTORY.quarters[0];
        everInvader = [];

        var diverged = false;
        var seen = {};
        turns.forEach(function (turn, index) {
            var q = quarterFor(turn);
            // Once an army has broken, this run has left the war that happened — and it never
            // comes back, whatever the calendar says afterwards.
            if (broken(turn)) { diverged = true; }

            if (q && !diverged) {
                quarters[index] = q;
                regimes[index] = "documented";
                referenceIndex = index;
                (q.heldByInvader || []).concat(q.contested || []).forEach(function (zone) {
                    seen[zone] = true;
                });
            } else {
                quarters[index] = null;
                regimes[index] = q ? "counterfactual" : "projection";
            }

            var snapshot = {};
            Object.keys(seen).forEach(function (zone) { snapshot[zone] = true; });
            everInvader[index] = snapshot;
        });

        // Nothing documented at all — a run that opens on a collapse. The map falls back to the
        // model everywhere rather than pretending to a chronicle it never entered.
        if (referenceIndex < 0) { return false; }

        var reference = cumulativeOf(turns[referenceIndex]);
        turns.forEach(function (turn, index) {
            deltas[index] = displacement(cumulativeOf(turn), reference, board);
        });

        ready = true;
        return true;
    }

    function cumulativeOf(turn) {
        var out = {};
        (turn.sectors || []).forEach(function (res) {
            out[res.sectorCode] = res.hexesCumulative || 0;
        });
        return out;
    }

    // One displacement per sector anchor, in degrees, sorted north to south so a latitude can be
    // interpolated between two of them.
    function displacement(now, reference, board) {
        var points = (board || []).map(function (s) {
            var moved = (now[s.code] || 0) - (reference[s.code] || 0);
            return { lat: s.lat, lon: s.pushLon * moved, dlat: s.pushLat * moved, moved: moved };
        });
        points.sort(function (a, b) { return b.lat - a.lat; });
        return points;
    }

    // The displacement at one latitude: the sector's own where it sits, interpolated between
    // neighbours, and faded to nothing beyond the ends of the modelled theatre.
    function shiftAt(points, lat) {
        if (!points || !points.length) { return null; }

        var north = points[0], south = points[points.length - 1];
        if (lat >= north.lat) {
            var upFade = Math.max(0, 1 - (lat - north.lat) / FADE_DEGREES);
            return { lon: north.lon * upFade, lat: north.dlat * upFade };
        }
        if (lat <= south.lat) {
            var downFade = Math.max(0, 1 - (south.lat - lat) / FADE_DEGREES);
            return { lon: south.lon * downFade, lat: south.dlat * downFade };
        }

        for (var i = 0; i < points.length - 1; i++) {
            var a = points[i], b = points[i + 1];
            if (lat <= a.lat && lat >= b.lat) {
                var span = a.lat - b.lat;
                var k = span < 1e-9 ? 0 : (a.lat - lat) / span;
                return { lon: a.lon + (b.lon - a.lon) * k, lat: a.dlat + (b.dlat - a.dlat) * k };
            }
        }
        return { lon: 0, lat: 0 };
    }

    /* ---------------- Reading a point ---------------- */

    var geo = window.tovGeo;

    function stateOfQuarter(quarter, lon, lat) {
        return zoneState(quarter, geo.zoneAt(lon, lat));
    }

    // The control state at a point for one turn. Documented quarters read the chronicle
    // directly; the others read the last documented position, pushed by the simulated sectors.
    function stateAt(lon, lat, index) {
        if (!ready) { return "free"; }
        if (quarters[index]) { return stateOfQuarter(quarters[index], lon, lat); }

        var board = quarters[referenceIndex];
        var here = stateOfQuarter(board, lon, lat);

        var shift = shiftAt(deltas[index], lat);
        if (!shift || (Math.abs(shift.lon) < 1e-6 && Math.abs(shift.lat) < 1e-6)) { return here; }

        var there = stateOfQuarter(board, lon - shift.lon, lat - shift.lat);
        // Kursk sits outside the theatre and never shifts; nothing here can turn it over.
        if (here === "defender" || there === "defender") { return here; }

        // Which way the front went decides whether the two readings unite or intersect: an
        // advance may only add ground, a withdrawal may only take it away.
        var advancing = advanceSign(deltas[index], lat) >= 0;
        var a = LEVEL[here], b = LEVEL[there];
        return NAME[Math.max(0, advancing ? Math.max(a, b) : Math.min(a, b))];
    }

    // Positive when the invader gained ground at this latitude since the handover.
    function advanceSign(points, lat) {
        if (!points || !points.length) { return 0; }
        var nearest = points[0], best = Infinity;
        points.forEach(function (p) {
            var d = Math.abs(p.lat - lat);
            if (d < best) { best = d; nearest = p; }
        });
        return nearest.moved;
    }

    /* ---------------- What changed THIS quarter ----------------

       Five persistent states say who holds what. None of them says what just happened, and that
       is the thing a reader opening one quarter is actually looking for: ground the invader took
       and lost reads the same in the spring of 2022, when the column of Kyiv has just pulled out,
       as in 2026, when it is four years old. The retreat stops being an event.

       So each hexagon is also compared with itself one quarter earlier, and the region that
       changed hands is ringed in the colour of the side that gained it. It works in every regime,
       because it is a difference of two readings of the same authority — the chronicle against
       itself while it governs, the projection against itself afterwards. */

    function changeAt(lon, lat, index) {
        if (!ready || index < 1) { return null; }
        var now = stateAt(lon, lat, index);
        var before = stateAt(lon, lat, index - 1);
        if (now === before) { return null; }

        // Kursk swings between "at home", "disputed" and "held by the defender", and the plain
        // level order does not describe it: the invader losing ground there is the defender
        // gaining it, exactly as on Ukrainian soil.
        return LEVEL[now] > LEVEL[before] ? "toInvader" : "toDefender";
    }

    /* ---------------- The five states the map paints ---------------- */

    function kindAt(lon, lat, index) {
        if (!ready) { return null; }
        var now = stateAt(lon, lat, index);
        var zone = geo.zoneAt(lon, lat);

        // Russian soil. "Held by the invader" means "at home" here, and painting it as occupied
        // territory would be the one lie this whole file exists to avoid.
        if (zone === "kursk_incursion") {
            if (now === "defender") { return "incursion"; }
            if (now === "contested") { return "incursionFront"; }
            return "foreign";
        }

        if (now === "contested") { return "front"; }
        if (now === "invader") {
            return zoneState(baseline, zone) === "invader" ? "occupied" : "gained";
        }
        return (everInvader[index] || {})[zone] ? "retaken" : "held";
    }

    /* ---------------- What the map has to say out loud ---------------- */

    function regimeOf(index) {
        return ready ? (regimes[index] || "projection") : "model";
    }

    function quarterOf(index) {
        return ready ? (quarters[index] || null) : null;
    }

    // The quarter the model starts pushing from, named for the caption.
    function handover() {
        return ready ? quarters[referenceIndex] : null;
    }

    /* ---------------- Which zones a modelled sector answers for ----------------

       The engine plays eight sectors; the chronicle names twenty zones. This is the join, and it
       is deliberately incomplete: Kyiv, Chernihiv, Sumy, Mariupol and Kursk answer to no sector,
       because the model never had one there. Their ground changes hands on the map and no gauge
       claims to have resolved it — which is the honest reading, not a gap. */

    var SECTOR_ZONES = {
        kharkiv: ["kharkiv_north"],
        kupiansk: ["koupiansk"],
        lyman: ["lyman", "severodonetsk"],
        bakhmut: ["bakhmout"],
        pokrovsk: ["pokrovsk", "avdiivka"],
        vuhledar: ["vouhledar"],
        zaporizhzhia: ["zaporijjia_south"],
        kherson: ["kherson_right", "kherson_left", "melitopol"]
    };

    // What the real front did in this sector this quarter: +1 the invader took ground, −1 the
    // defender took it back, 0 nothing changed hands. Never a distance — the chronicle gives a
    // state of control per zone and no kilometres, and inventing one here would be a fabrication
    // dressed as a measurement.
    function sectorMoveAt(sectorCode, index) {
        if (!ready || !quarters[index] || index < 1 || !quarters[index - 1]) { return 0; }
        var zones = SECTOR_ZONES[sectorCode];
        if (!zones) { return 0; }

        var delta = 0;
        zones.forEach(function (zone) {
            delta += LEVEL[zoneState(quarters[index], zone)] - LEVEL[zoneState(quarters[index - 1], zone)];
        });
        return delta === 0 ? 0 : (delta > 0 ? 1 : -1);
    }

    // The line of 2014, which the map keeps dashed as the thing every later quarter is measured
    // against. Kursk is left out: it is Russian soil, not a 2014 conquest.
    function baselineInvader(lon, lat) {
        if (!ready) { return false; }
        var zone = geo.zoneAt(lon, lat);
        return zone !== "kursk_incursion" && zoneState(baseline, zone) === "invader";
    }

    return {
        get available() { return ready; },
        prepare: prepare,
        baselineInvader: baselineInvader,
        sectorMoveAt: sectorMoveAt,
        changeAt: changeAt,
        regimeOf: regimeOf,
        quarterOf: quarterOf,
        handover: handover,
        stateAt: stateAt,
        kindAt: kindAt
    };
})();
