/* ============================================================
   Le bandeau du capital de guerre, et le ciseau.

   Le plateau montre ce que le front consomme et ce qu'il produit. Il ne montrait
   nulle part le STOCK — ce qu'il reste à brûler. C'est pourtant le seul chiffre qui
   annonce la fin depuis plusieurs trimestres.

   Deux pièces, et elles ne disent pas la même chose :
     · le bandeau dit le trimestre — sept postes, leur variation, sa cause ;
     · le ciseau dit la guerre — la puissance au front contre le capital qui la nourrit.

   Le bandeau compte TOUT en milliards de dollars, les deux camps sur la même règle. L'idée du
   jeu est capitaliste — le capital produit les éléments du front — et un capital se compte en
   argent : un indice base 100 pour les réserves, des gigawatts pour les centrales et des points
   de marge pour le régime faisaient une liste, pas un bilan. La valorisation tient en une
   phrase, écrite une seule fois dans le moteur : un actif vaut cinq années de ce qu'il produit.
   Le prix de tout cela est que la masse ukrainienne devient courte ; c'est le pourcentage,
   contre chaque masse, qui porte désormais la trajectoire de chaque camp.

   Mélanger les deux produirait sept petites courbes illisibles et une pièce maîtresse
   diluée, alors on les sépare.

   Aucun rang de tour n'apparaît ici : ce qui situe un trimestre, c'est sa date. Les
   libellés viennent de window.tovDates, partagés avec le reste du plateau.
   ============================================================ */
(function () {
    "use strict";

    var NS = "http://www.w3.org/2000/svg";

    // La clé EST le texte français source, comme dans le C# : une étiquette traduite une fois
    // l'est des deux côtés, et une traduction absente se lit en français.
    var T = window.tov.t;

    // La teinte porte le poste, la position porte le camp : sept postes doivent se
    // distinguer dans une rangée, les deux camps le sont déjà par la gauche et la droite.
    var POSTS = [
        { code: "reserves", colour: "#8a6f2b", yield: T("comble ce que la recette du trimestre ne finance plus") },
        { code: "grid", colour: "#c2621a", yield: T("ne va jamais au front : ouvre ou ferme les usines, le raffinage, le chauffage") },
        { code: "oil", colour: "#8a5a2b", yield: T("recette pour qui l'exporte, charge nette pour qui l'importe") },
        { code: "civilian", colour: "#6f8060", yield: T("le niveau de vie, donc le consentement à la guerre") },
        { code: "arms", colour: "#4a6070", yield: T("les obus, les drones, les missiles, les intercepteurs") },
        // Un flux donné à l'un, acheté par l'autre — et dans les deux cas c'est la position
        // diplomatique qui décide s'il se resserre ou se relâche. Cette position est mesurée
        // par le moteur et se lit sur ce poste, dans l'infobulle : elle n'a plus de colonne.
        { code: "foreign", colour: "#2f8f8f", yield: T("du matériel obtenu hors de la capacité nationale : donné à l'un, acheté par l'autre, et c'est la position diplomatique qui décide s'il continue") },

        // La tenue du pouvoir ferme le bandeau, séparée des six autres, et ne se compte pas en
        // dollars. Le moteur lui donne bien un montant — ce que le régime peut encore dépenser
        // pour se tenir en place — mais un consentement n'est pas un avoir : le poser sur la
        // même règle que deux mille milliards d'appareil industriel en faisait un trait, et
        // laissait croire qu'on pouvait comparer les deux. Il se lit sur son indice, cent au
        // départ, et se dessine en jauge et non en masse. Le montant reste dans l'infobulle,
        // parce que c'est lui que le total « flux » compte.
        { code: "regime", colour: "#8a4b2a", gauge: true, yield: T("le capital politique du trimestre, celui qui paie les cartes") }
    ];

    var NAMES = {
        reserves: T("Réserves"), grid: T("Centrales"), oil: T("Pétrole"), civilian: T("Usines civiles"),
        arms: T("Usines d'armement"), regime: T("Tenue du pouvoir"), foreign: T("Soutien extérieur")
    };

    // Le poste que le moteur mesure encore, et que le bandeau ne dessine plus : le soutien
    // étranger et le soutien international sont un seul capital, vu à deux étages. La colonne
    // disparaît, la mesure reste — c'est elle qui explique le resserrement du flux, et elle
    // descend donc dans l'infobulle du poste fusionné plutôt que de se perdre.
    var DIPLOMATIC = "international";

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

    // Deux colonnes qui se font face. La Russie tient la moitié gauche et s'aligne à gauche,
    // l'Ukraine tient la moitié droite et s'aligne à droite ; les sept postes se lisent l'un
    // sous l'autre, chacun en vis-à-vis de son homologue. L'œil compare alors poste par poste
    // d'un balayage horizontal, ce que l'empilement en miroir haut/bas ne permettait pas.
    //
    // La bande de partage devient une gouttière centrale : le poste y est nommé une seule
    // fois, avec son icône, et les deux masses le tirent de part et d'autre comme une corde.
    // Deux présentations, et le mode courant décide de la géométrie. Face à face sur un écran
    // large ; un camp à la fois sur un téléphone, où un bandeau de 1 240 unités ramené à la
    // largeur de l'écran réduirait ses libellés à deux pixels. Dans ce second mode la gouttière
    // passe à gauche, le camp affiché tire ses masses vers la droite, et l'autre camp attend
    // derrière son onglet.
    var solo = false;
    var GUT_L = 528, GUT_R = 712;
    var HEAD_H = 54, ROW_H = 36, H = HEAD_H + POSTS.length * ROW_H + 12;

    // Les chiffres ont leur bande à eux, au bord extérieur du bandeau : quelle que soit la
    // longueur de la masse, le chiffre reste lisible et jamais recouvert.
    var NUM_IN = 186, NUM_OUT = 22;
    // TRACK : la longueur de la piste, et la butée de toute masse. Ce qui la dépasse est taillé
    // à cette longueur et le dit par une déchirure en éclair. SEAM place cette déchirure le long
    // de la barre, SEAM_GAP en donne la largeur et SEAM_AMP l'amplitude du zigzag.
    var BAR_H = 16, TRACK = 306;
    var SEAM = 0.55, SEAM_GAP = 9, SEAM_AMP = 4;

    var ICON_S = 22;
    var ICON_X = GUT_L + 7, NAME_X = GUT_L + 37, NAME_MAX = GUT_R - 6;

    // Bascule de géométrie. Tout le dessin lit ces variables, si bien qu'un seul appel change
    // la présentation entière sans qu'aucune fonction de tracé ait à savoir dans quel mode
    // elle travaille.
    function layout(mobile) {
        solo = mobile;
        // 420 unités et pas une de plus : c'est la largeur d'un téléphone courant, donc la
        // plaque s'y affiche à l'échelle 1 et ses libellés de 9,5 px rendent à 9,5 px. À 690,
        // la même plaque était réduite d'un tiers et retombait sous six pixels — soit
        // exactement le défaut qu'on prétendait corriger, à un camp près.
        W = mobile ? 420 : 1240;
        GUT_L = mobile ? 4 : 528;
        GUT_R = mobile ? 148 : 712;
        NUM_IN = mobile ? 104 : 186;
        NUM_OUT = mobile ? 6 : 22;
        TRACK = mobile ? 164 : 306;
        H = HEAD_H + POSTS.length * ROW_H + 12;
        ICON_X = GUT_L + 7;
        NAME_X = GUT_L + 37;
        NAME_MAX = GUT_R - 6;
    }

    var seq = 0;

    // Un attribut absent se passe en null, et il ne doit PAS être écrit : setAttribute écrirait
    // la chaîne « null », que le navigateur ignore poliment mais qui traîne dans le DOM et
    // trompe quiconque l'inspecte. Même règle que dans depth.js.
    function svg(tag, attrs) {
        var n = document.createElementNS(NS, tag);
        Object.keys(attrs || {}).forEach(function (k) {
            if (attrs[k] === null || attrs[k] === undefined) { return; }
            n.setAttribute(k, attrs[k]);
        });
        return n;
    }

    // Une masse taillée se rompt EN SON MILIEU, et non au bout : deux tronçons pleins, aux
    // quatre coins francs, séparés par une déchirure en éclair. C'est le signe de l'axe brisé,
    // et il ne se lit que si la barre garde un morceau normal de chaque côté — rognée à son
    // extrémité, elle ressemblait à un fanion et disait surtout qu'elle finissait mal.
    //
    // Les deux bords de la déchirure portent le même zigzag, décalé : la fente garde une
    // largeur constante, et rien ne la remplit — c'est le fond de la carte qui passe au travers.
    function brokenBar(anchor, edge, top, dir) {
        var teeth = 6;
        var mid = anchor + (edge - anchor) * SEAM;
        var near = mid - dir * SEAM_GAP / 2;
        var far = mid + dir * SEAM_GAP / 2;
        var bottom = top + BAR_H;

        var zig = function (base) {
            var pts = [];
            for (var i = 0; i <= teeth; i++) {
                pts.push("L" + (base - (i % 2 ? dir * SEAM_AMP : 0)) + " " + (top + BAR_H * i / teeth));
            }
            return pts;
        };

        return [
            ["M" + anchor + " " + top].concat(zig(near), ["L" + anchor + " " + bottom, "Z"]).join(" "),
            ["M" + far + " " + top].concat(zig(far).slice(1),
                ["L" + edge + " " + bottom, "L" + edge + " " + top, "Z"]).join(" ")
        ];
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

    // Le formatage suit la langue : « 2 064 » et « 2,064 » sont la même quantité écrite pour
    // deux lecteurs. La règle est dans i18n.js, partagée avec le reste du plateau.
    var num = window.tov.num;

    function signed(v, d) {
        return (v > 0 ? "+" : v < 0 ? "−" : "±") + num(Math.abs(v), d);
    }

    // Tout le capital se compte en milliards de dollars : un bilan ne se lit pas dans cinq
    // langues à la fois. Sous dix milliards la décimale porte l'information — les usines
    // d'armement ukrainiennes valent 1,3 Md$ par an, et « 1 » n'aurait rien dit ; au-dessus,
    // elle n'est plus que du bruit devant un parc électrique à 368.
    function money(v) {
        if (v === null || v === undefined || isNaN(v)) { return "—"; }
        return num(v, Math.abs(v) < 10 ? 1 : 0);
    }

    // Sous ce seuil, l'arrondi imprimerait « +0,0 % » : ce n'est pas une variation, c'est un
    // arrondi, et l'écrire comme une variation ferait chercher une cause qui n'existe pas.
    var FLAT_PERCENT = 0.05;

    // Une variation, c'est un tiret quand rien ne bouge et sinon un pourcentage signé. Une
    // seule forme, partout : le cartouche, l'infobulle et le ruban de conséquence. Des points
    // d'indice ici, des milliards là et des « pts » ailleurs obligeaient à convertir de tête
    // avant de comparer deux lignes du même bandeau.
    function pct(v) {
        if (v === null || v === undefined || isNaN(v) || Math.abs(v) < FLAT_PERCENT) { return "—"; }
        return signed(v, 1) + " %";
    }

    function dateOf(t) {
        return window.tovDates ? window.tovDates.of(t) : String(t.year);
    }

    // Le déroulé et le trimestre en cours, posés par band() juste avant de dessiner les sept
    // rangées. Le cartouche en a besoin pour ancrer son lien, et le lui passer en onzième
    // paramètre aurait allongé une signature déjà trop longue pour ce qu'elle porte.
    var context = { scenario: "", year: 0, season: "" };

    // Le lien de provenance d'un poste. Une page par chiffre, ancrée sur le trimestre lu — et
    // un nom de fichier plat plutôt qu'un chemin : le site est publié en statique à la racine
    // d'un sous-répertoire, et une page à plat garde les mêmes chemins d'actifs que le plateau.
    function provenanceHref(post, invader) {
        return "provenance-" + post.code + "-" + (invader ? "ru" : "ua") + ".html"
            + "#" + context.scenario + "-" + context.year + "-" + context.season;
    }

    function yieldOf(code) {
        for (var i = 0; i < POSTS.length; i++) {
            if (POSTS[i].code === code) { return POSTS[i].yield; }
        }
        return "";
    }

    // Le nom du poste fusionné. Le moteur mesure encore « soutien étranger » et « soutien
    // international » séparément, et on ne touche pas à la mesure pour renommer un cartouche :
    // la vue les réunit sous le nom que le bandeau porte, lu dans sa table par CODE — un nom
    // reconnu à son libellé ne se reconnaîtrait plus dans une autre langue.
    function nameOf(post) {
        return NAMES[post.code] || post.name;
    }

    function postIn(side, code) {
        var found = null;
        (side.capital || []).forEach(function (post) {
            if (post.code === code) { found = post; }
        });
        return found;
    }

    /* ---------------- Les sept icônes ----------------

       Dessinées à la main, dans le trait gravé du plateau — ni police d'icônes, ni emoji, ni
       bibliothèque : le site est servi en statique et rien d'extérieur ne peut être chargé.
       Même main que les cent une scènes de cartes, à ceci près qu'on est ici sur du papier et
       non dans une nuit : le trait porte tout, la matière ne sert qu'à donner du poids.

       Chacune tient dans une boîte de 24 et se dessine à l'encre de son poste, de sorte que
       l'icône, le nom et les deux masses se lisent comme un seul objet. */

    function ink(g, d, colour, w, op) {
        g.appendChild(svg("path", {
            d: d, fill: "none", stroke: colour, "stroke-width": String(w || 1.3),
            "stroke-linecap": "round", "stroke-linejoin": "round",
            opacity: op === undefined ? "1" : String(op)
        }));
    }

    // Un lavis très pâle sous le trait : la forme a une masse, sans jamais devenir un aplat
    // qui rivaliserait avec celui de la colonne.
    function wash(g, d, colour) {
        g.appendChild(svg("path", { d: d, fill: colour, opacity: "0.14" }));
    }

    function solid(g, d, colour, w) {
        wash(g, d, colour);
        ink(g, d, colour, w);
    }

    var ICONS = {
        // Trois lingots empilés : une réserve, c'est du métal qu'on met de côté.
        reserves: function (g, c) {
            var ingot = function (x, y, w, h) {
                return "M" + x + " " + (y + h) + " L" + (x + w) + " " + (y + h) +
                    " L" + (x + w - 2.2) + " " + y + " L" + (x + 2.2) + " " + y + " Z";
            };
            [ingot(1.6, 14, 9.6, 6.6), ingot(12.8, 14, 9.6, 6.6), ingot(7.2, 6.6, 9.6, 6.6)]
                .forEach(function (d) { solid(g, d, c, 1.3); });
        },

        // Le pylône à haute tension : le réseau ne va jamais au front, il ouvre ou ferme tout
        // le reste. Les câbles sortent du cadre — la ligne continue au-delà.
        grid: function (g, c) {
            ink(g, "M4 21.6 L12 2.6 L20 21.6", c, 1.3);
            ink(g, "M7 14.6 L17 14.6 M9.2 9.6 L14.8 9.6", c, 1);
            ink(g, "M5.4 6.8 L18.6 6.8", c, 1.3);
            ink(g, "M0.4 9.6 Q3 6.9 5.4 6.8 M23.6 9.6 Q21 6.9 18.6 6.8", c, 1, 0.7);
            ink(g, "M6.2 18.2 L17.8 18.2", c, 1, 0.55);
        },

        // Le baril et la goutte : recette pour qui l'exporte, facture pour qui l'importe.
        oil: function (g, c) {
            var drop = "M12 0.4 L14.2 3.4 A2.7 2.7 0 1 1 9.8 3.4 Z";
            solid(g, drop, c, 1.1);
            var body = "M6 9.4 Q12 7.4 18 9.4 L18 21 Q12 23 6 21 Z";
            solid(g, body, c, 1.3);
            ink(g, "M6 9.4 Q12 11.4 18 9.4 M6 13.4 Q12 15.4 18 13.4 M6 17.2 Q12 19.2 18 17.2", c, 1);
        },

        // Le toit en shed : l'atelier civil se reconnaît à ses sheds, l'usine d'armement à
        // ce qu'elle produit. Deux usines, deux silhouettes — sinon les deux postes se
        // confondent, et c'est justement leur écart qui est la leçon.
        civilian: function (g, c) {
            var body = "M3 12.8 L21 12.8 L21 21.4 L3 21.4 Z";
            solid(g, body, c, 1.3);
            ink(g, "M3 12.8 L3 7.4 L7.5 12.8 L7.5 7.4 L12 12.8 L12 7.4 L16.5 12.8 L16.5 7.4 L21 12.8", c, 1.3);
            ink(g, "M5.8 15.6 L8.8 15.6 L8.8 18.2 L5.8 18.2 Z M11 15.6 L14 15.6 L14 18.2 L11 18.2 Z", c, 0.9);
            ink(g, "M16.6 21.4 L16.6 16.2 L19.4 16.2 L19.4 21.4", c, 0.9);
        },

        // L'obus sur la chaîne : le seul poste qui produit du matériel, et il en sort à la
        // pièce, sur un convoyeur.
        arms: function (g, c) {
            var shell = "M12 2 Q15.4 5.8 15.4 9.6 L15.4 17.2 L8.6 17.2 L8.6 9.6 Q8.6 5.8 12 2 Z";
            solid(g, shell, c, 1.3);
            ink(g, "M8.6 12.4 L15.4 12.4", c, 1);
            ink(g, "M7.8 17.2 L16.2 17.2 L16.2 19.2 L7.8 19.2 Z", c, 1.1);
            ink(g, "M1.6 20.8 L22.4 20.8", c, 1.2);
            [5, 12, 19].forEach(function (x) {
                g.appendChild(svg("circle", {
                    cx: x, cy: "22.2", r: "1.4", fill: "none", stroke: c, "stroke-width": "1"
                }));
            });
        },

        // La colonne fendue : le régime ne tombe pas par la rue, il se fissure de l'intérieur.
        // La fissure part du haut, là où l'appareil décide.
        regime: function (g, c) {
            solid(g, "M3.4 2.4 L20.6 2.4 L20.6 5.2 L3.4 5.2 Z", c, 1.3);
            solid(g, "M2.6 18.8 L21.4 18.8 L21.4 21.8 L2.6 21.8 Z", c, 1.3);
            solid(g, "M7 5.2 L17 5.2 L17 18.8 L7 18.8 Z", c, 1.3);
            ink(g, "M9.6 6.6 L9.6 17.4 M14.4 6.6 L14.4 17.4", c, 0.8, 0.5);
            ink(g, "M17 8.2 L11.4 11.4 L14.2 12.8 L7 16.6", c, 1.6);
        },

        // La caisse et la flèche qui entre : le soutien extérieur est ce qu'on REÇOIT, et il
        // arrive de l'extérieur du cadre. Un seul dessin pour le flux donné et le flux acheté,
        // parce que c'est un seul capital — ce qui change d'un camp à l'autre est le prix, pas
        // la nature, et le prix se lit dans les chiffres.
        foreign: function (g, c) {
            var crate = "M6 9.6 L21.4 9.6 L21.4 21.4 L6 21.4 Z";
            solid(g, crate, c, 1.3);
            ink(g, "M6 9.6 L21.4 21.4 M21.4 9.6 L6 21.4", c, 0.9, 0.6);
            ink(g, "M6 12.4 L21.4 12.4", c, 1);
            ink(g, "M0.8 2.4 L5.4 7.6 M5.4 7.6 L1.4 7.2 M5.4 7.6 L5 3.6", c, 1.4);
        }
    };

    function icon(host, code, colour, x, y, size) {
        var g = svg("g", {
            class: "cap-icon",
            transform: "translate(" + x + " " + y + ") scale(" + (size / 24).toFixed(4) + ")"
        });
        if (ICONS[code]) { ICONS[code](g, colour); }
        host.appendChild(g);
        return g;
    }

    /* ---------------- Le bandeau ---------------- */

    // Les rayures du gain : ce que le trimestre a ajouté au bout de la masse. Dans l'encre du
    // poste, jamais en rouge — c'est un gain — et hachuré plutôt que plein, pour qu'on voie que
    // ce bout-là est arrivé ce trimestre et n'était pas là avant. Plus serré et plus fin que la
    // rayure de la charge, qui dit tout autre chose et ne doit pas se confondre avec lui.
    function slicePattern(defs, id, colour) {
        var p = svg("pattern", {
            id: id, width: "4", height: "4", patternUnits: "userSpaceOnUse",
            patternTransform: "rotate(45)"
        });
        p.appendChild(svg("line", {
            x1: "0", y1: "0", x2: "0", y2: "4", stroke: colour, "stroke-width": "1.6", opacity: "0.85"
        }));
        defs.appendChild(p);
    }

    // Les rayures de la charge : le pétrole ukrainien n'est pas une recette mais une facture,
    // et une facture qui grossit n'est pas un capital qui grossit. Elle se dessine donc en
    // matière rayée, jamais en aplat plein — une masse pleine dit « je possède », une masse
    // rayée dit « je paie ».
    function chargePattern(defs, id, colour) {
        var p = svg("pattern", {
            id: id, width: "5", height: "5", patternUnits: "userSpaceOnUse",
            patternTransform: "rotate(45)"
        });
        p.appendChild(svg("rect", { width: "5", height: "5", fill: colour, opacity: "0.14" }));
        p.appendChild(svg("line", {
            x1: "0", y1: "0", x2: "0", y2: "5", stroke: colour, "stroke-width": "2.2", opacity: "0.7"
        }));
        defs.appendChild(p);
    }

    // Un poste, un camp, une ligne. La masse est ancrée à la gouttière centrale et pousse vers
    // le bord du bandeau ; les chiffres l'attendent dans leur bande réservée, où rien ne vient
    // jamais les recouvrir quelle que soit la longueur atteinte.
    //
    // L'échelle est celle de la RANGÉE, partagée par les deux camps : un milliard vaut la même
    // longueur à gauche et à droite. C'est le prix à payer pour que le bilan se compare, et il
    // se paie surtout côté ukrainien, où une masse dix fois plus courte ne montre plus sa
    // propre trajectoire. C'est le pourcentage, imprimé contre chaque masse, qui la porte
    // désormais : la longueur dit ce qu'on possède, le pourcentage dit ce que le trimestre en
    // a fait. Deux questions, deux réponses, et plus une seule ligne qui prétend aux deux.
    // La jauge de la tenue du pouvoir : une piste courte, cent au départ, et ce qu'il en reste.
    // Ni la longueur des masses ni leur règle ne s'y appliquent — c'est bien le propos.
    var GAUGE = 96;

    function gaugeRow(g, post, colour, invader, top) {
        var right = solo || !invader;
        var anchor = right ? GUT_R : GUT_L;
        var dir = right ? 1 : -1;
        var share = Math.max(0, Math.min(post.index / 100, 1.4));
        var full = anchor + dir * GAUGE;
        var end = anchor + dir * Math.min(GAUGE * share, GAUGE);

        g.appendChild(svg("rect", {
            x: Math.min(anchor, full), y: top + 3, width: GAUGE, height: BAR_H - 6,
            fill: "#e6e0d0"
        }));
        g.appendChild(svg("rect", {
            x: Math.min(anchor, end), y: top + 3, width: Math.abs(end - anchor), height: BAR_H - 6,
            fill: colour, opacity: "0.9"
        }));
        // Le repère du départ : cent, la ligne au-delà de laquelle le régime tient mieux
        // qu'au premier jour. Il ne bouge jamais, donc il se lit comme une graduation.
        g.appendChild(svg("line", {
            x1: full, y1: top, x2: full, y2: top + BAR_H,
            stroke: "#8b8578", "stroke-width": "1", opacity: "0.8"
        }));
    }

    function cartouche(host, defs, post, colour, invader, pressure, row, scale, diplomatic, gauge) {
        var yc = HEAD_H + row * ROW_H + ROW_H / 2;
        var top = yc - BAR_H / 2;
        // Le camp donne l'IDENTITÉ — la couleur, le lien de provenance — mais plus le sens du
        // dessin : sur un téléphone les deux onglets tirent vers la droite, sinon on comparerait
        // deux images en miroir de mémoire.
        var right = solo || !invader;
        var anchor = right ? GUT_R : GUT_L;
        var dir = right ? 1 : -1;
        var index = post.index;
        var charge = !!post.inverted;
        var len = Math.min(Math.max(post.value, 0) * scale, TRACK);
        var was = Math.min(Math.max(post.opening, 0) * scale, TRACK);
        var cut = !gauge && post.value * scale > TRACK + 0.5;
        var edge = anchor + dir * len;
        var wasEdge = anchor + dir * was;

        var chargeId = null;
        if (charge) {
            chargeId = "capCharge" + (seq++);
            chargePattern(defs, chargeId, colour);
        }

        var span = function (from, to) {
            return { x: Math.min(from, to), width: Math.abs(to - from) };
        };

        var g = svg("g", { class: "cap-post" });

        var tip = svg("title", {});
        tip.textContent = nameOf(post) + " — " + (charge ? "−" : "") + money(post.value) + " Md$" +
            (post.nature === "AnnualFlow" ? T(" par an") : "") +
            (charge ? T(" : une facture, retranchée du bilan") : "") + "\n" + yieldOf(post.code) +
            "\n" + T("Au trimestre précédent : %1 Md$", (charge ? "−" : "") + money(post.opening)) +
            "\n" + T("Variation du trimestre : %1", pct(post.percentDelta)) +
            (post.destructionCause ? "\n" + T("Détruit par : %1", post.destructionCause) : "") +
            (post.permanentLoss ? "\n" + T("Perte définitive à l'échelle de cette guerre") : "") +
            // Ce que le dessin ne porte plus. La pression du trimestre valait au lecteur une
            // pastille noire et une bannière qu'il fallait relier lui-même ; elle se lit ici,
            // dans la phrase que le moteur a écrite, sur le poste qu'elle vise.
            (pressure ? "\n" + T("Sous pression : %1", pressure) : "") +
            (post.secondaryLabel
                ? "\n" + post.secondaryLabel + " : " + num(post.secondary, post.secondary < 10 ? 1 : 0) +
                  (post.secondaryUnit ? " " + post.secondaryUnit : "")
                : "") +
            (post.thresholdLabel ? "\n" + T("Ce poste rompt à : %1", post.thresholdLabel) : "") +
            // La position diplomatique n'a plus de colonne, mais elle reste mesurée et elle
            // reste la raison du resserrement : elle se lit ici, du point de vue du camp qui
            // la lit — un verrou que le monde referme est une perte à Moscou et un gain à Kyiv.
            // C'est le seul poste qui ne se compte pas en dollars, parce que c'est le seul qui
            // ne se possède pas : une latitude commerciale ne se met pas au bilan.
            (diplomatic
                ? "\n" + T("Position diplomatique") +
                  " : " + num(diplomatic.value, 0) + " " + diplomatic.unit +
                  " " + T("(%1 ce trimestre)", pct(diplomatic.percentDelta)) +
                  (diplomatic.secondaryLabel
                      ? "\n" + diplomatic.secondaryLabel + " : " + num(diplomatic.secondary, 0) +
                        (diplomatic.secondaryUnit ? " " + diplomatic.secondaryUnit : "")
                      : "")
                : "");
        g.appendChild(tip);

        // LA MASSE — ce que le camp possède à la clôture du trimestre, et rien d'autre.
        //
        // Une seule forme par ligne. Trois filets verticaux se superposaient ici : le repère de
        // février 2022, l'ouverture du trimestre et le seuil de rupture. Tous trois gris, tous
        // trois pointillés, tous trois débordant de la barre jusqu'à frôler la rangée voisine —
        // on lisait le repère des réserves au niveau des centrales. Trois références de temps
        // différentes, aucune nommée à l'écran : ce n'était pas dense, c'était indéchiffrable.
        // Le bandeau n'en garde qu'une, celle qu'il annonce déjà en toutes lettres au bord —
        // le trimestre précédent —, et les deux autres descendent dans l'infobulle.
        var body = span(anchor, edge);

        if (gauge) {
            gaugeRow(g, post, colour, invader, top);
        }

        // Une facture n'est pas un avoir, et un poste tombé sous le quart de son départ n'est
        // plus une masse : ces deux-là se disent par la MATIÈRE de la barre — rayée pour ce
        // qu'on paie, hachurée pour ce qui est en ruine — et non par un signe posé à côté.
        // Un poste tombé sous le quart de son départ se dessinait entièrement en hachures. Deux
        // rangées sur sept y passaient en fin de partie, et une trame pâle sur fond clair ne se
        // lit plus du tout sur un téléphone : on ne voyait ni la masse, ni ce que la trame
        // voulait dire. Elle ne disait d'ailleurs rien que la barre ne dise déjà — cent contre
        // six cent douze au départ, c'est une longueur, et le pourcentage est écrit à côté.
        var fill = charge ? "url(#" + chargeId + ")" : colour;
        if (!gauge) {
            // Une masse qui tient dans la piste est un rectangle. Une masse taillée est la même
            // barre, rompue en son milieu : deux tronçons francs et une déchirure entre eux.
            // Le chiffre au bord reste, lui, la valeur entière.
            var edging = charge ? colour : "rgba(26,24,21,0.3)";
            var thickness = charge ? "0.9" : "0.7";

            if (cut) {
                brokenBar(anchor, edge, top, dir).forEach(function (d) {
                    g.appendChild(svg("path", {
                        d: d, class: "cap-mass", fill: fill,
                        stroke: edging, "stroke-width": thickness
                    }));
                });
            } else {
                g.appendChild(svg("rect", {
                    x: body.x, y: top, width: Math.max(body.width, 1.5), height: BAR_H,
                    class: "cap-mass", fill: fill,
                    stroke: edging, "stroke-width": thickness
                }));
            }
        }

        if (!charge && !gauge) {
            // Chant supérieur biseauté : la matière a une épaisseur, comme les douves. Sur une
            // barre rompue il se rompt avec elle, sinon il enjamberait la déchirure en blanc.
            var lip = function (from, to) {
                g.appendChild(svg("rect", {
                    x: Math.min(from, to) + 1.2, y: top + 1.2,
                    width: Math.max(Math.abs(to - from) - 2.4, 0), height: 3.2,
                    fill: "#fff", opacity: "0.34"
                }));
            };

            if (cut) {
                var seam = anchor + (edge - anchor) * SEAM;
                lip(anchor, seam - dir * (SEAM_GAP / 2 + SEAM_AMP));
                lip(seam + dir * SEAM_GAP / 2, edge);
            } else {
                lip(anchor, edge);
            }
        }

        // CE QUE LE TRIMESTRE A FAIT — dans le prolongement de la masse, jamais un trait posé
        // par-dessus. DEUX états, et deux seulement, qui ne dépendent que du sens :
        //
        //   · la barre a reculé  → le manque reste dessiné EN CREUX, au bout, là où c'était ;
        //   · la barre s'est allongée → le bout gagné est HACHURÉ dans l'encre du poste.
        //
        // Le creux se remplissait auparavant d'une hachure rouge dès que le moteur savait nommer
        // un coupable, et restait vide sinon : une baisse sur deux changeait donc d'apparence
        // sans que rien à l'écran ne dise pourquoi, et on cherchait une règle qui n'existait pas.
        // Le coupable se lit dans l'infobulle ; il ne se devine pas à la texture d'un creux.
        var moved = Math.abs(post.percentDelta) >= FLAT_PERCENT;
        if (!gauge && moved && Math.abs(len - was) > 0.8) {
            var slice = span(edge, wasEdge);
            var gained = len > was;
            var gainId = null;

            if (gained) {
                gainId = "capGain" + (seq++);
                slicePattern(defs, gainId, colour);
            }

            g.appendChild(svg("rect", {
                x: slice.x, y: top, width: slice.width, height: BAR_H,
                fill: gained ? "url(#" + gainId + ")" : "none",
                stroke: colour, "stroke-width": "1",
                "stroke-dasharray": gained ? null : "3 2",
                opacity: "0.85"
            }));
        }

        // La valeur au bord du bandeau, le delta contre elle : la Russie se lit de gauche à
        // droite, l'Ukraine de droite à gauche, et chaque camp aligne ses chiffres sur son
        // propre bord.
        var out = right ? W - NUM_OUT : NUM_OUT;
        var inner = right ? W - NUM_IN : NUM_IN;

        // Une charge s'imprime en négatif, comme dans n'importe quel bilan : c'est de l'argent
        // qui sort, et le total du camp la retranche. La masse, elle, dessine son ampleur —
        // une facture qui double est un trait deux fois plus long, et rayé.
        //
        // Le chiffre est un lien : il mène à la page qui dit d'où il vient, comment il a été
        // calculé et sur quelles sources — ancrée sur CE trimestre de CE déroulé. C'est la seule
        // chose que le bandeau ne peut pas dire lui-même sans redevenir un document.
        var figure = svg("a", {
            href: provenanceHref(post, invader),
            class: "cap-link"
        });
        // Un consentement se lit sur cent, pas en milliards : la jauge imprime son indice. Le
        // montant que le moteur lui donne reste dans l'infobulle, parce que c'est lui que le
        // total « flux » compte — l'écrire ici ferait croire qu'on peut le comparer au reste.
        text(figure, out, yc + 6, gauge ? num(index, 0) + " %" : (charge ? "−" : "") + money(post.value), {
            "text-anchor": right ? "end" : "start", class: "cap-value"
        });
        g.appendChild(figure);

        // Un trimestre où rien n'a bougé porte un tiret, et le tiret est une information : il
        // dit qu'aucune cause n'est à chercher. Partout ailleurs, un pourcentage signé.
        //
        // La couleur suit le SIGNE, et rien d'autre. Elle disait auparavant « ce recul a un
        // coupable » : sur sept lignes dont quatre en baisse, une seule sortait en rouge et
        // aucun trait de l'écran ne disait pourquoi celle-là. Rouge quand le capital descend,
        // vert quand il monte — la seule convention qu'on n'a pas à apprendre. Le coupable,
        // lui, se lit sur la masse, qui garde son arête déchirée.
        text(g, inner, yc + 5, pct(post.percentDelta), {
            "text-anchor": right ? "start" : "end",
            class: "cap-delta" + (moved ? (post.percentDelta > 0 ? " up" : " down") : "")
        });

        host.appendChild(g);
    }

    // Le cartouche du trimestre coiffe la gouttière : il tient la date et le baril au-dessus
    // de la colonne des noms, à égale distance des deux camps, parce qu'il n'appartient à
    // aucun des deux.
    function quarterCartouche(host, t) {
        // Le cartouche vit dans la gouttière — sauf en mode onglet, où la gouttière porte déjà
        // le nom du camp et ses habitants. Il passe alors au bord droit, où la place est libre
        // parce qu'un seul camp occupe la plaque.
        var x0 = solo ? W - 176 : GUT_L;
        var x1 = solo ? W - 4 : GUT_R;

        var g = svg("g", {});
        g.appendChild(svg("rect", {
            x: x0, y: 8, width: x1 - x0, height: 38, rx: "2",
            fill: "#fbf9f4", stroke: "#d9d1be", "stroke-width": "1"
        }));
        text(g, x0 + 12, 25, dateOf(t), { class: "cap-quarter" });
        text(g, x0 + 12, 41, num(t.oilPrice, 0) + " $", { class: "cap-brent" });
        text(g, x1 - 12, 41, T("le baril"), { "text-anchor": "end", class: "cap-label" });

        // La saison décide : elle mérite un signe et pas un mot.
        if (t.season === "Winter") {
            var cx = x1 - 18, cy = 21;
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

    // La gouttière : le nom du poste y est écrit une seule fois, précédé de son icône, et les
    // deux masses le tirent de part et d'autre comme une corde. Les icônes s'alignent toutes
    // sur la même verticale — une colonne de signes se parcourt d'un regard, là où sept signes
    // décalés se lisent un par un. Un nom de deux mots passe sur deux lignes plutôt que de
    // déborder sur la piste voisine : un libellé qui empiète sur la masse du camp d'en face
    // fait douter de qui possède quoi, et c'est la seule question que le bandeau pose.
    function spine(host) {
        POSTS.forEach(function (p, i) {
            var yc = HEAD_H + i * ROW_H + ROW_H / 2;
            var lines = splitName(NAMES[p.code]);
            var top = lines.length > 1 ? yc - 4 : yc + 3.5;

            var g = svg("g", { class: "cap-post" });
            var tip = svg("title", {});
            tip.textContent = NAMES[p.code] + " — " + yieldOf(p.code);
            g.appendChild(tip);

            icon(g, p.code, p.colour, ICON_X, yc - ICON_S / 2, ICON_S);

            // Quatorze points d'interligne, pas onze : à onze, les deux lignes d'un même nom
            // se touchaient de deux points — mesuré sur les soixante-dix-huit trimestres.
            lines.forEach(function (line, row) {
                text(g, NAME_X, top + row * 14, line, { class: "cap-name" });
            });

            host.appendChild(g);
        });
    }

    // LA RÈGLE — une seule, pour les deux camps ET pour les sept postes.
    //
    // Chaque rangée avait la sienne, posée sur le plus gros capital que CE poste atteignait dans
    // le déroulé. La conséquence était intenable à l'œil : deux mille soixante-quatre milliards
    // d'appareil civil et trois cent dix de réserves tiraient des barres de longueur voisine.
    // Un bandeau où la longueur ne veut pas dire la même chose d'une ligne à l'autre n'est pas
    // dense, il est faux — il invite précisément à la comparaison qu'il rend fausse.
    //
    // La règle est donc unique et posée sur le plus gros capital du déroulé, tous postes
    // confondus. Trois propriétés en découlent : un milliard vaut la même longueur partout, une
    // masse ne change jamais d'échelle d'un trimestre à l'autre, et aucune ne bute sur le bord.
    //
    // Le prix est connu et il est assumé : les petits postes deviennent courts. C'est déjà le
    // choix qui avait été fait entre les deux camps, où la masse ukrainienne est courte par
    // construction — le pourcentage contre chaque masse porte la trajectoire, le chiffre au bord
    // du bandeau porte le niveau, et la longueur porte enfin ce qu'elle prétend porter.
    // La règle est unique pour les sept postes et les deux camps : c'est ce qui permet de
    // comparer deux longueurs d'un coup d'œil. Mais un poste écrase les six autres — l'appareil
    // civil russe vaut près de trois fois la plus grosse des autres masses — et une règle calée
    // sur lui réduirait tout le reste à des traits.
    //
    // Elle se cale donc sur la SECONDE masse du jeu, jamais sur la première. Ce qui dépasse est
    // dessiné à la longueur de la piste et porte une coupure : la barre dit alors elle-même
    // qu'elle a été taillée, et le chiffre au bord reste exact. Rien n'est nommé ici — si deux
    // postes venaient à sortir du lot, la seconde masse monterait avec eux et plus rien ne
    // serait coupé.
    function rules(game) {
        if (game.tovRule) { return game.tovRule; }

        // Un sommet PAR POSTE, et non par valeur : le même poste culmine à peu près au même
        // niveau à chaque tour, si bien qu'un simple « deuxième plus grand nombre » retomberait
        // sur le tour d'à côté du poste écrasant, et n'écarterait rien du tout.
        var peaks = {};
        (game.turns || []).forEach(function (t) {
            [t.invader, t.defender].forEach(function (side) {
                (side.capital || []).forEach(function (post) {
                    var v = Math.max(post.value || 0, post.reference || 0);
                    if (!(post.code in peaks) || v > peaks[post.code]) { peaks[post.code] = v; }
                });
            });
        });

        var ranked = Object.keys(peaks).map(function (code) { return peaks[code]; })
            .sort(function (a, b) { return b - a; });

        game.tovRule = ranked.length > 1 ? ranked[1] : (ranked[0] || 0);
        return game.tovRule;
    }

    // Le goulot du trimestre : le poste que l'alerte la plus vive du moteur désigne, et le camp
    // qu'elle vise. Il ne se dessine plus — ni pastille noire sur le nom, ni bannière au-dessus
    // du bandeau. La pastille était un signe qu'il fallait deviner, et la bannière était la
    // notice de ce signe : deux pièces d'interface pour une seule information, dont l'une
    // n'existait que pour expliquer l'autre. La phrase du moteur descend dans l'infobulle du
    // poste concerné, où on va la chercher quand on la veut.
    function bottleneck(t) {
        var alerts = t.alerts || [];
        for (var i = 0; i < alerts.length; i++) {
            var code = ALERT_POST[alerts[i].code];
            if (code) {
                return { code: code, side: alerts[i].sideCode, detail: alerts[i].detail || alerts[i].title };
            }
        }
        return null;
    }

    // Le ruban : trois deltas côte à côte ne font pas une chaîne. Il n'apparaît que s'il y
    // a eu destruction, et un trimestre calme est un bandeau mince — cette minceur est
    // elle-même une information.
    function ribbon(side, cls) {
        if (!side.chain) { return null; }

        var row = el("div", "cap-ribbon " + cls);
        // Les guillemets appartiennent à la langue autant que les mots qu'ils entourent.
        row.appendChild(el("span", "cr-origin", T("« %1 »", side.chain.origin)));

        (side.chain.links || []).forEach(function (link) {
            row.appendChild(el("span", "cr-arrow", "→"));
            var box = el("span", "cr-link");
            box.appendChild(el("b", null, NAMES[link.postCode] || link.label));

            var moved = Math.abs(link.percentDelta) >= FLAT_PERCENT;
            box.appendChild(el("i", moved ? (link.percentDelta > 0 ? "up" : "down") : null,
                pct(link.percentDelta)));
            row.appendChild(box);
        });

        return row;
    }

    // UNE plaque du bandeau, dessinée dans la géométrie courante. `only` vaut « invader » ou
    // « defender » pour n'en tracer qu'un camp — c'est la vue des onglets — et null pour le
    // vis-à-vis complet.
    function plate(game, turnIndex, only) {
        var t = game.turns[turnIndex];
        var bottle = bottleneck(t);

        var s = svg("svg",{ viewBox: "0 0 " + W + " " + H, class: "cap-svg", role: "img" });
        var defs = svg("defs", {});
        s.appendChild(defs);

        // Le camp est rappelé par son nom au bord qui lui revient et par le filet qui court
        // sous lui : à gauche l'envahisseur, à droite l'envahi, et plus rien à chercher.
        if (!only) {
            s.appendChild(svg("rect", { x: 22, y: 46, width: GUT_L - 36, height: 3, fill: "#a8322a" }));
            s.appendChild(svg("rect", { x: GUT_R + 14, y: 46, width: W - 36 - GUT_R, height: 3, fill: "#1e5fa8" }));
        } else {
            s.appendChild(svg("rect", {
                x: 22, y: 46, width: W - 202, height: 3,
                fill: only === "invader" ? "#a8322a" : "#1e5fa8"
            }));
        }
        // Le nombre d'habitants se pose contre le nom, du côté de la gouttière. Il n'entre dans
        // aucun calcul : il est là parce que l'écart entre les deux bilans est trois fois plus
        // de gens produisant trois fois plus chacun, et que la seconde moitié de cette phrase
        // se lit comme un réglage arbitraire tant qu'on ne voit pas la première.
        var side = function (camp, x, anchorEnd, cls) {
            var n = text(s, x, 26, "", { "text-anchor": anchorEnd ? "end" : "start", class: "cap-side " + cls });
            var name = svg("tspan", {});
            name.textContent = camp.name.toUpperCase();
            var people = svg("tspan", { class: "cap-people", dx: anchorEnd ? "-10" : "10" });
            people.textContent = T("%1 M hab.", num(camp.population, 1));
            if (anchorEnd) {
                people.setAttribute("dx", "0");
                name.setAttribute("dx", "10");
                n.appendChild(people);
                n.appendChild(name);
            } else {
                n.appendChild(name);
                n.appendChild(people);
            }
        };
        if (only) {
            side(t[only], 22, false, only === "invader" ? "ru" : "ua");
        } else {
            side(t.invader, 22, false, "ru");
            side(t.defender, W - 22, true, "ua");
        }

        // Deux totaux par camp, jamais leur somme : un fonds souverain et une année de recette
        // pétrolière ne s'additionnent pas, et le chiffre unique qui prétendrait le contraire
        // est exactement l'arithmétique des communiqués de guerre. Le patrimoine dit ce qu'on
        // possède, le flux ce qu'on gagne — et la question devient : ce camp vit-il sur ce
        // qu'il possède ou sur ce qu'il produit ?
        var totals = function (side, x, anchorEnd) {
            text(s, x, 41,
                T("patrimoine %1 Md$  ·  flux %2 Md$/an", money(side.capitalStock), money(side.capitalFlow)),
                { "text-anchor": anchorEnd ? "end" : "start", class: "cap-total" });
        };
        if (only) {
            totals(t[only], 22, false);
        } else {
            totals(t.invader, 22, false);
            totals(t.defender, W - 22, true);
        }

        // Les deux bords de la gouttière : c'est le sol commun, et c'est de là que les deux
        // masses partent en sens contraire.
        (only ? [GUT_R] : [GUT_L, GUT_R]).forEach(function (x) {
            s.appendChild(svg("line", {
                x1: x, y1: HEAD_H - 2, x2: x, y2: H - 6, stroke: "#d9d1be", "stroke-width": "1"
            }));
        });

        // Un filet entre deux postes : c'est lui qui tient le regard sur sa ligne quand il
        // traverse le bandeau d'un camp à l'autre. Celui qui précède la dernière rangée est
        // franc et non pâle : ce qui suit ne se compte pas dans la même unité que ce qui
        // précède, et la coupure doit se voir avant qu'on ait lu quoi que ce soit.
        for (var r = 1; r < POSTS.length; r++) {
            var parting = POSTS[r].gauge;
            s.appendChild(svg("line", {
                x1: 22, y1: HEAD_H + r * ROW_H, x2: W - 22, y2: HEAD_H + r * ROW_H,
                stroke: parting ? "#8b8578" : "#d9d1be",
                "stroke-width": "1", opacity: parting ? "0.9" : "0.55"
            }));
        }

        quarterCartouche(s, t);
        spine(s);

        // La position diplomatique n'accompagne que le poste qu'elle commande.
        var ruDiplomatic = postIn(t.invader, DIPLOMATIC);
        var uaDiplomatic = postIn(t.defender, DIPLOMATIC);

        context.scenario = game.scenarioCode || "";
        context.year = t.year;
        context.season = t.season;

        var ruler = rules(game);
        var scale = ruler > 0 ? TRACK / ruler : 0;

        POSTS.forEach(function (p, i) {
            var carries = p.code === "foreign";
            var ru = postIn(t.invader, p.code);
            var ua = postIn(t.defender, p.code);

            // La phrase de pression ne va qu'au camp ET au poste qu'elle vise : la porter des
            // deux côtés ferait dire au bandeau que les deux camps étouffent au même endroit.
            var aimed = bottle && bottle.code === p.code;
            var ruPressure = aimed && bottle.side === "invader" ? bottle.detail : null;
            var uaPressure = aimed && bottle.side !== "invader" ? bottle.detail : null;

            if (ru && only !== "defender") {
                cartouche(s, defs, ru, p.colour, true, ruPressure, i, scale, carries ? ruDiplomatic : null, p.gauge);
            }
            if (ua && only !== "invader") {
                cartouche(s, defs, ua, p.colour, false, uaPressure, i, scale, carries ? uaDiplomatic : null, p.gauge);
            }
        });

        return s;
    }

    // Le bandeau, dans ses deux présentations, toutes deux posées dans la page. Aucune écoute
    // de redimensionnement, aucune détection de largeur en JavaScript : c'est la feuille de
    // style qui décide laquelle s'affiche, et une rotation de téléphone ne peut donc pas
    // laisser le plateau dans un état qu'il n'a pas prévu.
    function band(game, turnIndex) {
        var t = game.turns[turnIndex];
        var host = el("section", "panel capital-band");

        // Un titre, et rien d'autre. La règle de valorisation, la lecture des masses et le sens
        // du tiret tenaient ici en un paragraphe de dix lignes : c'était de la documentation de
        // conception posée sur un plateau de jeu. Un plateau se lit, il ne se préface pas — et
        // ce qui doit rester atteignable l'est à sa place, la production de l'année dans
        // l'infobulle de chaque poste, la règle des cinq ans dans 08-capital-de-guerre.md.
        var head = el("div", "cap-head");
        head.appendChild(el("h3", null, T("Le capital de guerre")));
        host.appendChild(head);

        // ── Écran large : les deux camps en vis-à-vis ────────────────────────────────────
        layout(false);
        var wide = el("div", "cap-scroll cap-wide");
        wide.appendChild(plate(game, turnIndex, null));
        host.appendChild(wide);

        // Le bandeau large tient dans 1 240 unités : sur un écran juste un peu étroit il défile
        // à l'intérieur de son propre cadre, à une échelle où il se lit — la page, elle, ne
        // défile jamais latéralement. Et il s'ouvre sur la gouttière plutôt que sur le bord
        // gauche : arriver par la gauche, c'est arriver sur sept chiffres russes sans savoir de
        // quoi ils parlent, alors que le centre montre les noms et le vis-à-vis des deux masses.
        var opened = W, gut = (GUT_L + GUT_R) / 2;
        requestAnimationFrame(function () {
            if (wide.scrollWidth <= wide.clientWidth + 1) { return; }
            wide.scrollLeft = Math.max(0, gut / opened * wide.scrollWidth - wide.clientWidth / 2);
        });

        // ── Téléphone : un onglet par camp ───────────────────────────────────────────────
        //
        // Le vis-à-vis ne survit pas à un écran de trois cent cinquante points : ou bien on
        // réduit tout et plus rien ne se lit, ou bien on fait défiler et on compare deux camps
        // de mémoire. L'onglet tranche — un camp entier, lisible, et l'autre à un doigt.
        layout(true);
        var tabbed = el("div", "cap-tabbed");
        var tabs = el("div", "cap-tabs");
        var pages = el("div", "cap-pages");

        [["invader", "ru"], ["defender", "ua"]].forEach(function (pair, i) {
            var page = el("div", "cap-page " + pair[1]);
            page.appendChild(plate(game, turnIndex, pair[0]));

            var tab = el("button", "cap-tab " + pair[1]);
            tab.type = "button";
            tab.textContent = t[pair[0]].name;
            tab.setAttribute("aria-pressed", i === 0 ? "true" : "false");
            tab.addEventListener("click", function () {
                Array.prototype.forEach.call(tabs.children, function (other) {
                    other.setAttribute("aria-pressed", other === tab ? "true" : "false");
                });
                Array.prototype.forEach.call(pages.children, function (other) {
                    other.classList.toggle("on", other === page);
                });
            });

            if (i === 0) { page.classList.add("on"); }
            tabs.appendChild(tab);
            pages.appendChild(page);
        });

        tabbed.appendChild(tabs);
        tabbed.appendChild(pages);
        host.appendChild(tabbed);

        layout(false);

        var ruRibbon = ribbon(t.invader, "ru");
        var uaRibbon = ribbon(t.defender, "ua");
        if (ruRibbon) { host.appendChild(ruRibbon); }
        if (uaRibbon) { host.appendChild(uaRibbon); }

        return host;
    }

    /* ---------------- Le ciseau ---------------- */

    // Le tracé s'arrête à 452 et non plus à 530 : les soixante-dix-huit unités libérées à droite
    // sont la gouttière où chaque courbe écrit sa valeur de fin et son nom. Posés à l'intérieur,
    // ces deux mots tombaient sur le tracé — la puissance russe culmine précisément dans le coin
    // où il fallait les mettre. Une marge propre vaut mieux qu'un placement qui se rattrape.
    var SW = 560, SH = 230, SX0 = 62, SX1 = 452, SY0 = 210, SYTOP = 36, SGUT = SX1 + 10;

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

    // La hachure du ciseau, et elle seule. Le bandeau en avait une autre, qui teignait toute une
    // masse tombée sous le quart de son départ : un signe muet, illisible sur un téléphone, et
    // redondant avec la longueur de la barre. Ici elle dit tout autre chose — la SURFACE entre
    // deux courbes, celle où le front brûle le capital — et c'est une aire, pas un objet, donc
    // elle n'a que sa texture pour se dire.
    function hatch(defs, id) {
        var p = svg("pattern", {
            id: id, width: "6", height: "6", patternUnits: "userSpaceOnUse",
            patternTransform: "rotate(45)"
        });
        p.appendChild(svg("rect", { width: "6", height: "6", fill: "#a8322a", opacity: "0.12" }));
        p.appendChild(svg("line", { x1: "0", y1: "0", x2: "0", y2: "6", stroke: "#a8322a", "stroke-width": "2.6", opacity: "0.45" }));
        defs.appendChild(p);
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
        if (burning) {
            var burn = svg("path", { d: burning, fill: "url(#" + burnId + ")", opacity: "0.6" });
            var burnTip = svg("title", {});
            burnTip.textContent = T("Le front tient au-dessus de ce que le capital produit : ce camp avance en brûlant ce qui lui reste.");
            burn.appendChild(burnTip);
            s.appendChild(burn);
        }

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
                T("le front vit sur le capital — %1", dateOf(pts[crossing].t)), { class: "cap-label" });
        }

        // Chaque courbe porte son nom au bout d'elle-même. Le paragraphe qui coiffait la pièce
        // disait « trait noir : la puissance au front, trait de couleur : le capital » — une
        // légende, c'est-à-dire un mode d'emploi pour un dessin qui ne se suffisait pas. Le nom
        // écrit contre le trait supprime la question au lieu d'y répondre ailleurs.
        var last = pts[pts.length - 1];
        var frontOnTop = last.front >= last.capital;

        // Dans la gouttière, chaque courbe est prolongée d'un filet jusqu'à son propre chiffre :
        // c'est ce filet qui dit lequel des deux blocs appartient à laquelle, sans quoi deux
        // valeurs empilées à droite redeviendraient une légende à recomposer.
        var endOf = function (value, push, label, stroke, attrs) {
            var yv = y(value);
            var yb = Math.max(SYTOP + 10, Math.min(SY0 - 12, yv + push));

            s.appendChild(svg("line", {
                x1: SX1, y1: yv, x2: SGUT - 4, y2: yb - 5,
                stroke: stroke, "stroke-width": "1", opacity: "0.45"
            }));
            text(s, SGUT, yb, num(value, 0), attrs);
            text(s, SGUT, yb + 11, label, { class: "cap-label" });
        };

        // Les deux blocs s'écartent l'un de l'autre : sur l'Ukraine, où les deux courbes finissent
        // à trente-neuf unités l'une de l'autre, ils se toucheraient sinon.
        var apart = Math.abs(y(last.front) - y(last.capital)) < 30;
        endOf(last.front, apart ? (frontOnTop ? -6 : 12) : 4, "front", "#1a1815",
            { class: "sc-end front" });
        endOf(last.capital, apart ? (frontOnTop ? 12 : -6) : 4, "capital", colour,
            { class: "sc-end", fill: colour });

        text(s, SX0 - 40, SY0 + 18, dateOf(pts[0].t), { class: "cap-label" });
        text(s, SX1, SY0 + 18, dateOf(last.t), { "text-anchor": "end", class: "cap-label" });

        host.appendChild(el("div", "sc-title", title));
        host.appendChild(s);
        return host;
    }

    function divergence(game, turnIndex) {
        var host = el("section", "panel capital-scissor");

        var head = el("div", "cap-head");
        head.appendChild(el("h3", null, T("Le front contre le capital")));
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
        // Huit pour cent de ciel au-dessus de la plus haute courbe. Sans cette marge, le nom
        // que la courbe porte à son extrémité se pose sur le tracé lui-même : le sommet de la
        // puissance russe et son propre libellé occupaient la même bande de pixels.
        top = Math.ceil(top * 1.08 / 25) * 25;

        var pair = el("div", "sc-pair");
        pair.appendChild(scissor(game, true, turnIndex, "#a8322a", game.turns[0].invader.name, top));
        pair.appendChild(scissor(game, false, turnIndex, "#1e5fa8", game.turns[0].defender.name, top));
        host.appendChild(pair);

        return host;
    }

    window.tovCapital = { band: band, divergence: divergence };
})();
