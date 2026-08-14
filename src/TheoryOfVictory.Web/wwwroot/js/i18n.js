/* ============================================================
   La langue, côté page.

   Une seule convention pour tout le site : la clé EST le texte français source, en C# comme
   ici. `tov.t("Réserves")` et `Localizer.Loc("Réserves")` interrogent le même catalogue, si
   bien qu'une étiquette traduite une fois l'est des deux côtés — et qu'une traduction absente
   se lit en français au lieu de laisser un trou ou un identifiant.

   Le catalogue est posé par le serveur dans window.tovModel. En français il est VIDE : la
   langue source ne coûte rien à servir, chaque appel retombe sur sa propre clé.

   Les nombres suivent la langue. « 2 064 » et « 2,064 » sont la même quantité écrite pour deux
   lecteurs, et une page qui traduit ses mots en gardant les séparateurs de l'autre sent la
   traduction — c'est le détail par lequel on la reconnaît.
   ============================================================ */
(function () {
    "use strict";

    var model = window.tovModel || {};
    var catalogue = model.translations || {};
    var locale = model.numberLocale || "fr-FR";

    function translate(french) {
        var found = catalogue[french];
        return (found === undefined || found === null || found === "") ? french : found;
    }

    // %1, %2 … plutôt que des trous positionnels collés à l'ordre des mots français : une
    // traduction doit pouvoir déplacer son argument là où sa propre phrase le demande.
    function t(french) {
        var text = translate(french);
        if (arguments.length < 2) { return text; }

        var args = Array.prototype.slice.call(arguments, 1);
        return text.replace(/%(\d)/g, function (whole, rank) {
            var value = args[parseInt(rank, 10) - 1];
            return (value === undefined || value === null) ? whole : String(value);
        });
    }

    // Le groupement français est une espace fine insécable, que plusieurs fontes rendent sans
    // aucune chasse en chiffres tabulaires — « 1141000 ». L'espace insécable ordinaire, elle,
    // s'affiche toujours, et un effectif d'un million d'hommes doit se lire d'un coup d'œil.
    function num(value, decimals) {
        if (value === null || value === undefined || isNaN(value)) { return "—"; }
        return value.toLocaleString(locale, {
            minimumFractionDigits: decimals || 0,
            maximumFractionDigits: decimals || 0
        }).replace(/ /g, " ");
    }

    window.tov = {
        lang: model.language || "fr",
        locale: locale,
        t: t,
        num: num
    };

    // Le choix de langue se retient : revenir sur le site ne doit pas rebasculer le lecteur
    // dans la langue de son navigateur alors qu'il en a désigné une autre.
    document.addEventListener("DOMContentLoaded", function () {
        var links = document.querySelectorAll("[data-lang]");
        for (var i = 0; i < links.length; i++) {
            links[i].addEventListener("click", function (event) {
                try {
                    localStorage.setItem("tov-lang", event.currentTarget.getAttribute("data-lang"));
                } catch (ignored) {
                    // Navigation privée, stockage refusé : le lien fonctionne quand même.
                }
            });
        }
    });
})();
