// Geographic base for the front map. Everything is baked in: the page never touches the network.
//
// Sources (public domain, coordinates simplified and rounded to 0.01°, ~1 km):
// - Outline: Natural Earth 1:50m Admin 0 countries. Natural Earth files Crimea under Russia;
//   it is grafted back onto the Ukrainian ring at the Perekop isthmus, because the game
//   treats it as occupied Ukrainian territory.
// - Rivers: Natural Earth 1:50m rivers and lake centerlines (Dnipro, Dniester).
// Douglas-Peucker simplified: 284 points for the outline, which is plenty at this scale.
// This is a reading map, not a survey: it is accurate to a few kilometres, no more.
window.tovGeo = (function () {
    "use strict";

    var UKRAINE = [
        [38.21, 47.09], [37.54, 47.07], [37.34, 46.92], [37.05, 46.88], [36.79, 46.71],
        [36.69, 46.76], [36.56, 46.76], [36.28, 46.66], [36.02, 46.67], [35.83, 46.62],
        [35.40, 46.38], [35.20, 46.17], [35.01, 46.11], [35.28, 46.28], [35.29, 46.37],
        [35.23, 46.44], [35.06, 46.27], [34.85, 46.19], [34.86, 45.99], [35.02, 45.70],
        [35.26, 45.45], [35.46, 45.32], [35.56, 45.31], [35.83, 45.40], [36.01, 45.37],
        [36.17, 45.45], [36.58, 45.39], [36.39, 45.07], [35.87, 45.01], [35.68, 45.10],
        [35.47, 45.10], [35.36, 44.98], [35.15, 44.90], [35.09, 44.80], [34.72, 44.81],
        [34.47, 44.72], [34.28, 44.54], [34.07, 44.42], [33.91, 44.39], [33.76, 44.40],
        [33.45, 44.55], [33.61, 44.91], [33.56, 45.10], [33.39, 45.19], [33.19, 45.19],
        [32.92, 45.35], [32.61, 45.33], [32.51, 45.40], [33.14, 45.75], [33.28, 45.77],
        [33.66, 45.95], [33.59, 46.10], [33.43, 46.06], [33.20, 46.18], [32.48, 46.08],
        [32.04, 46.26], [31.83, 46.28], [31.78, 46.32], [31.99, 46.36], [32.01, 46.43],
        [31.71, 46.47], [31.55, 46.55], [32.36, 46.47], [32.58, 46.62], [32.35, 46.56],
        [32.13, 46.60], [32.04, 46.64], [31.94, 46.78], [31.94, 46.98], [31.76, 47.21],
        [31.91, 46.93], [31.87, 46.65], [31.78, 46.63], [31.53, 46.66], [31.56, 46.78],
        [31.40, 46.63], [31.14, 46.62], [30.80, 46.55], [30.66, 46.27], [30.22, 45.87],
        [29.82, 45.73], [29.69, 45.75], [29.63, 45.72], [29.60, 45.60], [29.67, 45.54],
        [29.71, 45.26], [29.57, 45.37], [29.40, 45.42], [29.22, 45.40], [28.89, 45.29],
        [28.78, 45.31], [28.76, 45.23], [28.45, 45.29], [28.32, 45.35], [28.21, 45.45],
        [28.31, 45.50], [28.50, 45.52], [28.49, 45.67], [28.73, 45.85], [28.74, 45.94],
        [28.95, 46.05], [29.01, 46.18], [28.93, 46.36], [28.96, 46.46], [29.19, 46.52],
        [29.20, 46.38], [29.30, 46.47], [29.61, 46.40], [29.71, 46.45], [29.84, 46.35],
        [30.08, 46.38], [30.13, 46.42], [29.92, 46.54], [29.94, 46.72], [29.88, 46.83],
        [29.57, 46.96], [29.51, 47.13], [29.54, 47.27], [29.13, 47.49], [29.21, 47.77],
        [29.13, 47.96], [28.92, 47.95], [28.77, 48.12], [28.53, 48.15], [28.46, 48.09],
        [28.42, 48.15], [28.34, 48.14], [28.35, 48.21], [28.29, 48.24], [28.09, 48.26],
        [28.04, 48.32], [27.82, 48.42], [27.55, 48.48], [27.40, 48.42], [27.34, 48.43],
        [27.23, 48.37], [26.85, 48.39], [26.62, 48.26], [26.31, 48.20], [26.16, 47.99],
        [25.46, 47.91], [25.17, 47.82], [25.07, 47.75], [24.89, 47.72], [24.58, 47.93],
        [24.48, 47.95], [24.18, 47.91], [23.67, 47.99], [23.41, 47.99], [23.14, 48.09],
        [23.05, 48.01], [22.88, 47.95], [22.77, 48.11], [22.58, 48.13], [22.52, 48.21],
        [22.35, 48.26], [22.25, 48.41], [22.13, 48.41], [22.14, 48.57], [22.30, 48.69],
        [22.54, 49.07], [22.84, 49.04], [22.85, 49.08], [22.71, 49.17], [22.73, 49.30],
        [22.65, 49.54], [22.71, 49.61], [23.04, 49.90], [23.71, 50.38], [23.97, 50.41],
        [24.09, 50.53], [24.09, 50.62], [23.98, 50.79], [24.11, 50.84], [24.10, 50.87],
        [23.99, 50.94], [23.86, 51.13], [23.66, 51.31], [23.61, 51.61], [23.79, 51.64],
        [23.98, 51.59], [24.28, 51.77], [24.36, 51.87], [25.27, 51.94], [25.93, 51.91],
        [26.77, 51.77], [27.14, 51.75], [27.30, 51.60], [27.60, 51.60], [27.69, 51.57],
        [27.70, 51.48], [27.86, 51.59], [28.01, 51.56], [28.18, 51.61], [28.60, 51.54],
        [28.65, 51.46], [28.73, 51.43], [28.85, 51.54], [29.10, 51.63], [29.35, 51.38],
        [29.55, 51.43], [30.16, 51.48], [30.31, 51.40], [30.33, 51.33], [30.54, 51.27],
        [30.63, 51.36], [30.53, 51.60], [30.58, 51.69], [30.76, 51.90], [30.98, 52.05],
        [31.57, 52.11], [32.12, 52.05], [32.28, 52.11], [32.36, 52.27], [32.44, 52.31],
        [32.81, 52.25], [33.15, 52.34], [33.74, 52.34], [33.92, 52.25], [34.11, 51.98],
        [34.40, 51.78], [34.38, 51.72], [34.12, 51.68], [34.20, 51.55], [34.23, 51.36],
        [34.27, 51.34], [34.21, 51.26], [34.49, 51.24], [34.71, 51.17], [35.06, 51.20],
        [35.16, 51.06], [35.31, 51.04], [35.31, 50.95], [35.44, 50.73], [35.41, 50.54],
        [35.59, 50.37], [35.67, 50.35], [35.89, 50.44], [36.12, 50.41], [36.31, 50.28],
        [36.50, 50.28], [36.62, 50.21], [36.76, 50.29], [37.42, 50.41], [37.58, 50.29],
        [37.70, 50.11], [38.05, 49.92], [38.15, 49.94], [38.18, 50.03], [38.26, 50.05],
        [38.45, 49.96], [38.65, 49.95], [38.92, 49.82], [39.17, 49.86], [39.30, 49.74],
        [39.46, 49.73], [39.78, 49.57], [40.08, 49.58], [40.06, 49.43], [40.13, 49.37],
        [40.11, 49.25], [39.89, 49.06], [39.69, 49.01], [39.75, 48.91], [40.00, 48.82],
        [39.79, 48.81], [39.70, 48.74], [39.64, 48.59], [39.84, 48.54], [39.89, 48.36],
        [39.85, 48.30], [39.96, 48.27], [39.78, 47.96], [39.78, 47.89], [39.74, 47.84],
        [39.66, 47.84], [38.90, 47.86], [38.82, 47.84], [38.64, 47.67], [38.37, 47.61],
        [38.29, 47.56], [38.20, 47.32], [38.28, 47.28], [38.20, 47.18]
    ];

    var DNIEPR = [
        [30.51, 51.22], [30.48, 50.75], [30.57, 50.42], [30.71, 50.22], [30.86, 50.13],
        [31.24, 49.98], [31.44, 50.00], [31.49, 49.87], [31.46, 49.76], [31.69, 49.64],
        [32.06, 49.44], [32.58, 49.22], [33.19, 49.09], [33.72, 48.94], [34.71, 48.53],
        [34.96, 48.48], [35.11, 48.39], [35.17, 48.28], [35.18, 48.15], [35.08, 48.08],
        [35.13, 47.97], [35.11, 47.84], [35.15, 47.66], [35.09, 47.53], [34.45, 47.54],
        [34.15, 47.49], [34.00, 47.41], [33.98, 47.31], [33.86, 47.15], [33.54, 46.83],
        [33.07, 46.76], [32.58, 46.62]
    ];

    var DNIESTER = [
        [22.85, 49.27], [23.08, 49.46], [23.39, 49.55], [23.84, 49.52], [24.14, 49.44],
        [24.36, 49.25], [24.59, 49.18], [25.15, 48.88], [25.52, 48.81], [25.67, 48.69],
        [25.89, 48.61], [26.04, 48.62], [26.09, 48.56], [26.50, 48.52], [26.59, 48.47],
        [26.80, 48.58], [27.43, 48.58], [27.48, 48.51], [27.82, 48.42], [28.09, 48.26],
        [28.29, 48.24], [28.34, 48.14], [28.54, 48.10], [28.60, 48.01], [28.93, 47.90],
        [29.10, 47.45], [29.14, 47.14], [29.31, 47.12], [29.36, 47.03], [29.31, 46.97],
        [29.42, 46.95], [29.54, 46.80], [29.64, 46.81], [29.77, 46.60], [30.25, 46.40],
        [30.26, 46.29], [30.49, 46.09]
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
        { name: "Sébastopol", lon: 33.53, lat: 44.60, rank: 3 },
        // The northern axes of 2022 and the Kursk salient of 2024 need somewhere to be read
        // against: without these three names, the ground that changes hands up there is a
        // coloured patch with no address.
        { name: "Tchernihiv", lon: 31.29, lat: 51.49, rank: 3 },
        { name: "Soumy", lon: 34.80, lat: 50.91, rank: 3 },
        { name: "Soudja", lon: 35.28, lat: 51.19, rank: 3 }
    ];

    // Placed in the neighbours' territory, far enough from the border to stay off the grid.
    var NEIGHBOURS = [
        { name: "Pologne", lon: 22.35, lat: 50.75 },
        { name: "Biélorussie", lon: 27.60, lat: 52.20 },
        { name: "Russie", lon: 39.60, lat: 51.30 },
        { name: "Moldavie", lon: 28.55, lat: 47.15 },
        { name: "Roumanie", lon: 25.60, lat: 46.70 },
        { name: "Hongrie", lon: 21.40, lat: 48.05 }
    ];

    var SEAS = [
        { name: "Mer Noire", lon: 31.60, lat: 44.65 },
        { name: "Mer d'Azov", lon: 37.10, lat: 46.25 }
    ];

    // Coast of the neighbours, from the Ukrainian border on the Azov round to the Danube.
    // Closes the water polygon without painting Russian or Romanian land as sea.
    var FOREIGN_COAST = [
        [38.35, 47.10], [38.92, 47.27], [39.32, 47.10], [38.55, 46.72], [38.30, 46.10],
        [37.72, 45.62], [37.40, 45.34], [36.95, 45.28], [37.32, 44.90], [38.30, 44.35],
        [39.40, 43.70], [42.20, 43.30], [42.20, 42.60], [26.60, 42.60], [27.90, 43.19],
        [28.03, 43.37], [28.58, 43.74], [28.66, 44.05], [28.79, 44.68], [29.66, 45.15]
    ];

    var BOUNDS = { minLon: 22.00, maxLon: 40.28, minLat: 44.24, maxLat: 52.46 };

    /* ---------------- The twenty zones of the front ----------------

       front-history.json says which zones each side held at the close of every documented
       quarter. It names them and does not draw them: these outlines are where that vocabulary
       lands on the ground.

       Two things to know before reading a coordinate here.

       FIRST, THEY OVERLAP ON PURPOSE. A hexagon is given to the FIRST zone in this list whose
       outline contains its centre, so the order below is the resolution rule: the small, precise
       places come before the wide ones that surround them. Gapless tiling by hand would take a
       hundred coincident vertices and one typo would open a hole in the occupied ground —
       overlap plus priority cannot.

       SECOND, THE RESOLUTION IS THE READING HEX, ABOUT 1 400 km². Nothing finer survives the
       drawing, and the file that feeds these zones says the same of itself. So a town on a zone
       edge is a town this map cannot place, not a claim: Sloviansk, Kramatorsk and Druzhkivka
       are the pocket left deliberately unassigned between `pokrovsk`, `bakhmout` and `lyman`,
       because they never fell and a zone reaching over them would say they did.

       Neither Kyiv, Chernihiv nor Sumy fell. `kyiv_axis` is cut to stay north of the capital for
       that reason; the two others carry their city inside the corridor that was occupied around
       them, which is the limit the chronicle already accepts for itself. */

    var ZONES = [
        // Crimea, annexed in 2014, north edge on the Perekop isthmus.
        { code: "crimea", ring: [
            [32.30, 45.85], [33.30, 46.10], [33.85, 46.22], [35.00, 46.15], [35.60, 46.05],
            [36.10, 45.50], [36.90, 45.55], [36.90, 44.20], [32.30, 44.20]] },

        // The two 2014 enclaves, which are one continuous area against the border: from the
        // Azov coast east of Shyrokyne, up the contact line of 2015 through Debaltseve and
        // Pervomaisk to Stanytsia Luhanska, then back down the state border.
        { code: "donbas_2014", ring: [
            [39.78, 48.85], [39.55, 48.72], [39.15, 48.72], [38.60, 48.70], [38.42, 48.62],
            [38.45, 48.42], [38.10, 48.32], [37.90, 48.30], [37.82, 48.12], [37.58, 47.95],
            [37.62, 47.68], [37.66, 47.35], [37.72, 47.02],
            [38.21, 47.05], [38.29, 47.56], [38.64, 47.67], [38.90, 47.86], [39.66, 47.84],
            [39.78, 47.96], [39.96, 48.27], [39.89, 48.36], [39.84, 48.54], [39.64, 48.59],
            [39.70, 48.74]] },

        // Coastal southern Donetsk, west of the 2014 line: Mariupol and Volnovakha.
        { code: "mariupol", ring: [
            [37.72, 47.02], [37.66, 47.35], [37.62, 47.68], [37.28, 47.62], [36.98, 47.35],
            [37.00, 46.85]] },

        { code: "avdiivka", ring: [
            [37.58, 47.95], [37.82, 48.12], [37.90, 48.30], [37.72, 48.42], [37.42, 48.32],
            [37.35, 48.05]] },

        // Bakhmut, Soledar, Chasiv Yar, Toretsk — the belt of fortress towns.
        { code: "bakhmout", ring: [
            [37.72, 48.42], [38.10, 48.32], [38.42, 48.42], [38.42, 48.62], [38.30, 48.85],
            [37.85, 48.85], [37.62, 48.62]] },

        // Lyman and Siversk, north of the Sloviansk pocket that never fell.
        { code: "lyman", ring: [
            [37.62, 48.95], [37.90, 48.88], [38.30, 48.88], [38.22, 49.18], [37.78, 49.20],
            [37.55, 49.10]] },

        { code: "pokrovsk", ring: [
            [37.35, 48.05], [37.42, 48.32], [37.72, 48.42], [37.62, 48.62], [37.20, 48.62],
            [36.85, 48.45], [36.88, 48.10], [37.10, 47.98]] },

        { code: "vouhledar", ring: [
            [37.10, 47.98], [37.35, 48.05], [37.58, 47.95], [37.62, 47.68], [37.28, 47.62],
            [36.80, 47.68], [36.45, 47.80], [36.50, 48.05], [36.88, 48.10]] },

        // Izium and Balakliia, on the bend of the Siverskyi Donets.
        { code: "izioum", ring: [
            [36.55, 49.55], [37.30, 49.52], [37.55, 49.10], [37.40, 48.88], [36.85, 48.85],
            [36.50, 49.10]] },

        // Eastern Kharkiv oblast, from the Oskil to the oblast edge.
        { code: "koupiansk", ring: [
            [37.00, 49.95], [37.55, 49.90], [38.05, 49.92], [38.15, 49.60], [38.05, 49.35],
            [37.60, 49.32], [37.25, 49.50], [36.98, 49.70]] },

        // The border belt north and east of Kharkiv — Lyptsi, Vovchansk. Its northern edge is
        // the state border itself, point for point off the outline: drawn a few kilometres
        // short of it, the belt left Vovchansk — the town this whole zone is fought over —
        // outside its own zone, and the map showed the 2022 and 2024 offensives stopping at
        // nothing. The southern edge stays clear of Kharkiv, shelled from this ground and
        // never entered.
        { code: "kharkiv_north", ring: [
            [35.62, 50.40], [35.89, 50.44], [36.12, 50.41], [36.31, 50.28], [36.50, 50.28],
            [36.62, 50.21], [36.76, 50.29], [37.42, 50.41], [37.58, 50.29], [37.70, 50.11],
            [37.42, 49.94], [37.00, 49.95], [36.80, 50.05], [36.35, 50.10], [35.95, 50.06],
            [35.70, 50.15]] },

        // Luhansk oblast outside the 2014 enclave: Sievierodonetsk, Kreminna, Svatove, Starobilsk.
        { code: "severodonetsk", ring: [
            [38.10, 48.35], [38.45, 48.42], [38.42, 48.62], [38.60, 48.70], [39.15, 48.72],
            [39.55, 48.72], [39.78, 48.85], [39.75, 48.91], [39.89, 49.06], [40.11, 49.25],
            [40.13, 49.37], [40.06, 49.43], [40.08, 49.58], [39.78, 49.57], [39.46, 49.73],
            [39.17, 49.86], [38.92, 49.82], [38.60, 49.75], [38.20, 49.60], [38.10, 49.20],
            [38.00, 48.80]] },

        // Orikhiv, Robotyne, Polohy — where the 2023 counteroffensive spent itself.
        { code: "zaporijjia_south", ring: [
            [34.50, 47.30], [34.62, 47.62], [35.20, 47.65], [36.00, 47.68], [36.45, 47.70],
            [36.40, 47.42], [36.20, 47.42], [35.30, 47.48]] },

        // The land corridor: Melitopol, Berdiansk, Enerhodar and the Azov shore.
        { code: "melitopol", ring: [
            [34.50, 47.30], [35.30, 47.48], [36.20, 47.42], [36.95, 47.30], [37.00, 46.85],
            [36.55, 46.62], [35.80, 46.52], [35.05, 46.15], [34.55, 46.30], [34.40, 46.90]] },

        // Left bank of the Dnipro, from the mouth to the Kakhovka reservoir.
        { code: "kherson_left", ring: [
            [32.50, 46.58], [33.08, 46.76], [33.58, 46.85], [34.00, 47.12], [34.32, 47.38],
            [34.58, 47.48], [34.60, 46.40], [34.20, 46.08], [33.20, 46.02], [32.35, 46.15]] },

        // Right bank: Kherson city and the pocket up to Vysokopillia, clear of Mykolaiv.
        { code: "kherson_right", ring: [
            [32.35, 46.42], [32.30, 46.78], [32.35, 47.05], [32.75, 47.28], [33.20, 47.50],
            [33.75, 47.45], [34.10, 47.30], [34.00, 47.12], [33.58, 46.85], [33.08, 46.76],
            [32.50, 46.58]] },

        // The corridor of the northern column, north of Kyiv on both banks.
        { code: "kyiv_axis", ring: [
            [29.10, 51.05], [30.05, 51.25], [30.60, 51.30], [31.20, 51.20], [31.35, 50.72],
            [30.95, 50.45], [30.62, 50.60], [30.35, 50.50], [30.00, 50.52], [29.55, 50.58],
            [29.20, 50.75]] },

        { code: "chernihiv_axis", ring: [
            [30.60, 51.30], [31.20, 51.10], [31.70, 51.00], [32.30, 51.15], [32.70, 51.60],
            [32.45, 52.28], [31.60, 52.10], [30.98, 52.05], [30.76, 51.90]] },

        // Sumy oblast: the 2022 sweep, and the border strip Russia reopened in 2025.
        { code: "sumy_axis", ring: [
            [33.60, 51.30], [34.20, 51.55], [34.60, 51.75], [35.10, 51.20], [35.20, 50.90],
            [34.90, 50.55], [34.40, 50.50], [33.90, 50.75], [33.65, 51.00]] }
    ];

    // Russian soil, north of the border around Sudzha. It is the only place the defender ever
    // held ground in the invader's country, so it has to be on the map — and the grid, which
    // otherwise stops at the Ukrainian outline, is extended over exactly this shape and no other.
    //
    // The southern edge follows the state border point for point, so the pocket rests ON the
    // frontier instead of floating beside it. Roughly two thousand square kilometres against the
    // thousand of August 2024: below one reading hexagon nothing is drawable at all, and a
    // salient that cannot be seen would be a worse account of it than one drawn slightly large.
    var KURSK_SALIENT = [
        [34.45, 51.26], [34.71, 51.17], [35.06, 51.20], [35.16, 51.06], [35.35, 51.03],
        [35.55, 51.22], [35.40, 51.48], [34.80, 51.48], [34.45, 51.42]
    ];

    function nearestIndex(ring, lon, lat) {
        var best = 0, bestD = Infinity;
        for (var i = 0; i < ring.length; i++) {
            var dx = (ring[i][0] - lon) * 0.665, dy = ring[i][1] - lat;
            var d = dx * dx + dy * dy;
            if (d < bestD) { bestD = d; best = i; }
        }
        return best;
    }

    // Walks the ring from i to j, whichever way round passes through `via`.
    function arc(ring, i, j, via) {
        var n = ring.length;
        var forwardHasVia = ((j - i + n) % n) >= ((via - i + n) % n);
        var step = forwardHasVia ? 1 : -1;
        var out = [];
        for (var k = i; ; k = (k + step + n) % n) {
            out.push(ring[k]);
            if (k === j || out.length > n) { break; }
        }
        return out;
    }

    var I_NORTH = nearestIndex(UKRAINE, 37.45, 50.40);   // Kharkiv oblast, on the Russian border
    var I_SOUTH = nearestIndex(UKRAINE, 33.62, 46.15);   // Perekop, the neck into Crimea
    var I_CRIMEA = nearestIndex(UKRAINE, 33.45, 44.55);  // Cape Chersonesus, the far south
    var I_DANUBE = nearestIndex(UKRAINE, 29.71, 45.26);  // Danube delta, the far south-west
    var I_AZOV = nearestIndex(UKRAINE, 38.21, 47.09);    // Azov coast, on the Russian border

    // Fixed anchors so the contact line lands on the border and on the coast, not near them.
    var LINE_NORTH_ANCHOR = UKRAINE[I_NORTH];
    var LINE_SOUTH_ANCHOR = UKRAINE[I_SOUTH];

    // Closes the occupied polygon: from the neck of Crimea, round the peninsula and the Azov,
    // then up the eastern border. Order matters — reversed, the polygon crosses itself.
    var EASTERN_EDGE = arc(UKRAINE, I_SOUTH, I_NORTH, I_CRIMEA);

    // Black Sea and Azov as one water body: the Ukrainian coast, then the neighbours'.
    var BLACK_SEA = arc(UKRAINE, I_DANUBE, I_AZOV, I_CRIMEA).concat(FOREIGN_COAST);

    // Ray casting. Points on the edge fall either way — at this scale nobody notices.
    function contains(polygon, lon, lat) {
        var inside = false;
        for (var i = 0, j = polygon.length - 1; i < polygon.length; j = i++) {
            var xi = polygon[i][0], yi = polygon[i][1];
            var xj = polygon[j][0], yj = polygon[j][1];
            if ((yi > lat) !== (yj > lat) &&
                lon < (xj - xi) * (lat - yi) / (yj - yi) + xi) {
                inside = !inside;
            }
        }
        return inside;
    }

    // Equirectangular with a latitude correction: good enough at this scale.
    function projector(width, height) {
        var midLat = (BOUNDS.minLat + BOUNDS.maxLat) / 2;
        var kx = Math.cos(midLat * Math.PI / 180);

        var spanX = (BOUNDS.maxLon - BOUNDS.minLon) * kx;
        var spanY = BOUNDS.maxLat - BOUNDS.minLat;
        var scale = Math.min(width / spanX, height / spanY);

        var offsetX = (width - spanX * scale) / 2;
        var offsetY = (height - spanY * scale) / 2;

        var project = function (lon, lat) {
            return [
                offsetX + (lon - BOUNDS.minLon) * kx * scale,
                offsetY + (BOUNDS.maxLat - lat) * scale
            ];
        };

        // Degrees of latitude per pixel, so callers can size things in kilometres.
        project.kmPerPixel = 111.32 / scale;
        project.invert = function (x, y) {
            return [
                BOUNDS.minLon + (x - offsetX) / (kx * scale),
                BOUNDS.maxLat - (y - offsetY) / scale
            ];
        };
        return project;
    }

    function path(points, project, close) {
        var d = points.map(function (p, i) {
            var xy = project(p[0], p[1]);
            return (i === 0 ? "M" : "L") + xy[0].toFixed(1) + " " + xy[1].toFixed(1);
        }).join(" ");
        return close ? d + " Z" : d;
    }

    // Which zone of the chronicle a point belongs to, or null where none does — the west of the
    // country, and the pocket around Sloviansk. Ukrainian soil is tested against the twenty zones
    // in priority order; anything outside the outline can only be the Kursk salient.
    function zoneAt(lon, lat) {
        if (!contains(UKRAINE, lon, lat)) {
            return contains(KURSK_SALIENT, lon, lat) ? "kursk_incursion" : null;
        }
        for (var i = 0; i < ZONES.length; i++) {
            if (contains(ZONES[i].ring, lon, lat)) { return ZONES[i].code; }
        }
        return null;
    }

    return {
        ukraine: UKRAINE,
        easternEdge: EASTERN_EDGE,
        blackSea: BLACK_SEA,
        northAnchor: LINE_NORTH_ANCHOR,
        southAnchor: LINE_SOUTH_ANCHOR,
        dniepr: DNIEPR,
        dniester: DNIESTER,
        cities: CITIES,
        neighbours: NEIGHBOURS,
        seas: SEAS,
        bounds: BOUNDS,
        zones: ZONES,
        kurskSalient: KURSK_SALIENT,
        zoneAt: zoneAt,
        contains: contains,
        projector: projector,
        path: path
    };
})();
