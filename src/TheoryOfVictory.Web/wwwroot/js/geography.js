// Approximate outlines, good enough to read as a map and honest about being approximate.
window.tovGeo = (function () {
    "use strict";

    var UKRAINE = [
        [22.15, 48.42], [22.55, 48.85], [22.75, 49.05], [23.15, 49.40], [23.70, 50.10],
        [23.60, 50.55], [24.10, 50.85], [23.65, 51.30], [23.70, 51.60], [24.35, 51.90],
        [25.30, 51.92], [26.40, 51.85], [27.25, 51.60], [28.30, 51.55], [29.20, 51.60],
        [30.00, 51.50], [30.55, 51.35], [31.20, 52.05], [32.30, 52.25], [33.20, 52.35],
        [34.10, 51.75], [34.35, 51.25], [34.05, 50.95], [34.55, 50.60], [35.40, 50.45],
        [36.10, 50.42], [36.85, 50.30], [37.45, 50.40], [38.20, 49.95], [39.20, 49.85],
        [40.15, 49.60], [39.75, 48.95], [39.95, 48.30], [39.75, 47.85], [38.85, 47.85],
        [38.30, 47.60], [37.55, 47.10], [36.90, 46.85], [36.20, 46.65], [35.20, 46.60],
        [34.85, 46.10], [35.10, 45.75], [36.10, 45.45], [36.65, 45.35], [35.85, 45.15],
        [35.00, 45.00], [34.00, 44.55], [33.55, 44.40], [32.55, 45.35], [33.20, 46.05],
        [33.60, 46.20], [32.60, 46.35], [31.90, 46.55], [31.55, 46.60], [30.90, 46.15],
        [30.35, 45.95], [29.85, 45.55], [29.65, 45.35], [28.90, 45.30], [28.50, 45.50],
        [28.35, 46.10], [28.15, 46.55], [27.55, 47.10], [26.95, 47.65], [26.60, 48.25],
        [25.85, 47.95], [25.20, 47.90], [24.60, 47.75], [23.60, 47.98], [22.90, 47.95]
    ];

    // Closes the occupied polygon: from the southern end of the line, east and back up
    // the border. Order matters — reversed, the polygon crosses itself.
    var EASTERN_EDGE = [
        [33.60, 46.20], [33.20, 46.05], [32.55, 45.35], [33.55, 44.40], [34.00, 44.55],
        [35.00, 45.00], [35.85, 45.15], [36.65, 45.35], [36.10, 45.45], [35.10, 45.75],
        [34.85, 46.10], [35.20, 46.60], [36.20, 46.65], [36.90, 46.85], [37.55, 47.10],
        [38.30, 47.60], [38.85, 47.85], [39.75, 47.85], [39.95, 48.30], [39.75, 48.95],
        [40.15, 49.60], [39.20, 49.85], [38.20, 49.95], [37.45, 50.40]
    ];

    // Fixed anchors so the contact line reaches the border and the sea.
    var LINE_NORTH_ANCHOR = [37.45, 50.40];
    var LINE_SOUTH_ANCHOR = [33.60, 46.20];

    var DNIEPR = [
        [30.50, 51.20], [30.52, 50.45], [31.20, 49.90], [32.50, 49.40], [33.50, 49.00],
        [34.10, 48.55], [35.05, 48.45], [35.10, 47.85], [34.60, 47.30], [33.30, 46.90],
        [32.62, 46.64]
    ];

    var CITIES = [
        { name: "Kyiv", lon: 30.52, lat: 50.45, rank: 1 },
        { name: "Kharkiv", lon: 36.23, lat: 49.99, rank: 1 },
        { name: "Odessa", lon: 30.73, lat: 46.48, rank: 1 },
        { name: "Lviv", lon: 24.03, lat: 49.84, rank: 2 },
        { name: "Dnipro", lon: 35.05, lat: 48.47, rank: 2 },
        { name: "Zaporijjia", lon: 35.14, lat: 47.84, rank: 2 },
        { name: "Donetsk", lon: 37.80, lat: 48.00, rank: 2 },
        { name: "Louhansk", lon: 39.31, lat: 48.57, rank: 3 },
        { name: "Marioupol", lon: 37.55, lat: 47.10, rank: 3 },
        { name: "Kherson", lon: 32.62, lat: 46.64, rank: 3 },
        { name: "Sébastopol", lon: 33.53, lat: 44.60, rank: 3 }
    ];

    var BOUNDS = { minLon: 21.8, maxLon: 40.6, minLat: 44.2, maxLat: 52.6 };

    // Equirectangular with a latitude correction: good enough at this scale.
    function projector(width, height) {
        var midLat = (BOUNDS.minLat + BOUNDS.maxLat) / 2;
        var kx = Math.cos(midLat * Math.PI / 180);

        var spanX = (BOUNDS.maxLon - BOUNDS.minLon) * kx;
        var spanY = BOUNDS.maxLat - BOUNDS.minLat;
        var scale = Math.min(width / spanX, height / spanY);

        var offsetX = (width - spanX * scale) / 2;
        var offsetY = (height - spanY * scale) / 2;

        return function (lon, lat) {
            return [
                offsetX + (lon - BOUNDS.minLon) * kx * scale,
                offsetY + (BOUNDS.maxLat - lat) * scale
            ];
        };
    }

    function path(points, project, close) {
        var d = points.map(function (p, i) {
            var xy = project(p[0], p[1]);
            return (i === 0 ? "M" : "L") + xy[0].toFixed(1) + " " + xy[1].toFixed(1);
        }).join(" ");
        return close ? d + " Z" : d;
    }

    return {
        ukraine: UKRAINE,
        easternEdge: EASTERN_EDGE,
        northAnchor: LINE_NORTH_ANCHOR,
        southAnchor: LINE_SOUTH_ANCHOR,
        dniepr: DNIEPR,
        cities: CITIES,
        projector: projector,
        path: path
    };
})();
