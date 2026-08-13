# La carte suit l'histoire — et dit quand elle cesse de la suivre

> Comment les vingt trimestres sourcés de
> [`front-history.json`](11-front-historique.md) arrivent sur la carte hexagonale, ce qu'ils
> gouvernent, ce qu'ils ne gouvernent pas, et comment le modèle reprend la main sans que la page
> raconte deux histoires à la fois.
>
> Ce document ne modifie aucune règle du moteur. Aucune valeur, aucun seuil, aucune trajectoire
> n'a bougé : les trois issues sont intactes et la marge de l'asphyxie reste celle du
> [§12 du calendrier](06-calendrier-propose.md).

---

## 1. Le défaut, et pourquoi il était structurel

Le [§14 du calendrier](06-calendrier-propose.md) l'avait déjà écrit sans pouvoir le corriger :
**la géographie du modèle ne connaît que huit secteurs**, l'arc Kharkiv-Kherson. Les axes de Kyiv,
de Tchernihiv et de Soumy n'existent pas ; la rive droite de Kherson n'est pas distinguée de la
rive gauche ; Marioupol n'est pas un lieu ; le saillant de Koursk est hors du pays et donc hors de
la grille.

La conséquence était arithmétique et le §14 l'avait chiffrée : le pic de mars 2022 demanderait
vingt-cinq hexagones de profondeur sur tout le front en un seul trimestre, quand le plafond du
moteur en autorise trois. **2022, l'année la plus spectaculaire de la guerre, ne bougeait
pratiquement pas à l'écran** — 4 183 km² au lieu de 120 000 —, et ce n'était pas un défaut de
calibration : c'était la carte qui manquait, pas la physique.

Le §14 avait aussi écarté le scriptage, et pour une bonne raison qu'il faut redire ici : un
mouvement scripté raconterait que le terrain se perd parce que l'adversaire a été brillant, quand
il se perd parce qu'on n'a plus assez d'hommes pour le tenir. La règle qu'il posait pour le jour
où l'on y viendrait quand même est celle que ce document applique **sans exception** :

> Le passé rejoué et l'avenir simulé ne sont pas la même chose, et le site est public. Tout
> mouvement reconstitué doit être marqué comme tel dans les données, affiché comme tel à l'écran,
> et distingué des trimestres que le modèle calcule.

---

## 2. Trois régimes, jamais mélangés

`frontline.js` classe chaque trimestre d'un déroulé dans un régime, et un seul. La carte nomme
celui qui est en vigueur, en bas à gauche, en une ligne.

| Régime | Quand | Qui gouverne le terrain | Ce que la carte montre |
|---|---|---|---|
| **documenté** | Le trimestre est dans la chronique **et** le déroulé colle encore à la guerre réelle | La chronique | Front en trait plein, note du trimestre sous la carte |
| **contrefactuel** | Le trimestre est dans la chronique, mais **une armée a rompu dans ce déroulé** | Le modèle | Front en pointillé, bandeau « déroulé hypothétique » |
| **projection** | Au-delà du dernier trimestre documenté | Le modèle | Front en pointillé, bandeau « projection » |

Le pointillé n'est pas une décoration : c'est la convention du wargamer pour une position que
personne n'a confirmée, et elle ne coûte pas une ligne de légende.

**Le régime contrefactuel est l'arbitrage le moins évident du chantier**, et c'est celui dont le
site avait le plus besoin. Le déroulé « le soutien s'arrête » fait céder l'Ukraine au printemps
2024. Si la carte y affichait l'histoire réelle sous prétexte que le calendrier est encore dans la
période documentée, **le résultat propre de ce déroulé serait invisible** — on lirait « l'Ukraine
cède » au-dessus d'une carte où elle tient. Le basculement se déclenche donc sur un événement du
modèle qui n'a pas d'équivalent historique, `HasCollapsed`, et il est définitif : une fois qu'un
déroulé a quitté la guerre réelle, il n'y revient pas.

Ce que cela donne à l'écran, mesuré :

| Déroulé « le soutien s'arrête » | Régime | Territoire russe |
|---|---|---|
| Hiver 2024 | documenté | 87 000 km² |
| Printemps 2024 | **contrefactuel** | 105 000 km² |
| Été 2024 | contrefactuel | 142 000 km² |
| Automne 2024 | contrefactuel | 145 000 km² |

C'est pédagogiquement le meilleur moment du site : *jusqu'ici, voilà ce qui s'est passé ; à partir
d'ici, voilà ce que le modèle dit qu'il se serait passé.*

---

## 3. Les vingt zones sur le terrain

La chronique nomme vingt zones et n'en dessine aucune. Les contours vivent dans
`geography.js`, et trois décisions les gouvernent.

### 3.1 Elles se chevauchent, et c'est la solution et non le problème

Un hexagone revient à **la première zone de la liste dont le contour contient son point de
référence**. L'ordre de la liste est donc la règle de résolution : les lieux petits et précis
passent avant les grands qui les entourent — `donbas_2014` avant `severodonetsk`, `avdiivka` avant
`pokrovsk`.

Un pavage sans trou dessiné à la main demanderait une centaine de sommets rigoureusement
coïncidents, et une seule faute de frappe ouvrirait un trou dans le territoire occupé — un trou
qui ressemblerait à du terrain ukrainien et que personne ne verrait. Le chevauchement avec
priorité ne peut pas produire ce défaut.

### 3.2 La résolution est l'hexagone de lecture, et rien de plus fin ne survit

L'hexagone fait **40 km sur les plats, soit environ 1 385 km²**. Trois conséquences qu'il faut
assumer plutôt que masquer :

- **Le bord d'une zone est une donnée, pas un détail de dessin.** `kharkiv_north` a d'abord été
  tracée quelques kilomètres en deçà de la frontière d'État, ce qui laissait **Vovtchansk —
  la ville que cette zone existe pour porter — en dehors de sa propre zone** : les offensives de
  2022 et de 2024 s'y arrêtaient sur rien. Le bord nord suit désormais la frontière point pour
  point, relevé sur le contour du pays. Correction invisible à l'écran, et il faut le dire :
  aucun centre d'hexagone n'a changé de camp, les surfaces mesurées sont identiques au mètre près.
  Une zone qui exclut sa propre ville reste une faute, même quand elle ne se voit pas encore.
- **Une ville sur une frontière de zone est une ville que cette carte ne sait pas placer.**
  Sloviansk, Kramatorsk et Droujkivka sont la poche laissée **délibérément non attribuée** entre
  `pokrovsk`, `bakhmout` et `lyman` : elles ne sont jamais tombées, et une zone qui passerait
  au-dessus d'elles dirait le contraire.
- **Ni Kyiv, ni Tchernihiv, ni Soumy ne sont tombées.** `kyiv_axis` est découpée pour rester au
  nord de la capitale, ce qui fait arriver la colonne russe à une trentaine de kilomètres de Kyiv
  sans jamais l'y faire entrer. Les deux autres portent leur ville à l'intérieur du corridor qui a
  été occupé autour d'elle — c'est la limite que la chronique accepte déjà pour elle-même, et le
  texte du trimestre la nomme sous la carte.
- **Le saillant de Koursk est dessiné large.** Environ 2 800 km² à l'écran contre les 1 000 km²
  d'août 2024. En dessous d'un hexagone de lecture, rien n'est dessinable du tout, et un saillant
  qu'on ne voit pas serait un plus mauvais compte-rendu qu'un saillant légèrement grossi.

### 3.3 Koursk est sur la carte parce que la grille y déborde exprès

C'est le seul endroit où le pavage franchit la frontière ukrainienne. Le contour du saillant suit
la frontière d'État point par point sur son bord sud, de sorte que **la poche repose sur la
frontière au lieu de flotter à côté**, et il possède son propre masque de découpe : avec un masque
unique, un hexagone à cheval sur la frontière s'afficherait des deux côtés et la poche se lirait
comme une bavure.

Quand la Russie tient son propre oblast, la grille y est dessinée en gris très pâle, sans
remplissage. Ce n'est pas une coquetterie : **le saillant doit être sur le plateau avant d'être
pris**, sinon sa capture se lirait comme du territoire qui apparaît.

Et quand l'Ukraine le tient, il est bleu plein, hachuré, cerné d'un trait de 2,4 px sur tout son
pourtour. C'est le seul terrain que le défenseur ait jamais tenu chez l'envahisseur, et c'est la
seule ligne `heldByDefender` de tout le fichier.

---

## 4. Comment le modèle reprend la main sans que la carte saute

Pas en basculant sur une autre carte : le lecteur verrait un saut, et un saut est un aveu que les
deux images ne parlent pas du même objet.

**La dernière position historique sert de plateau, et les huit secteurs simulés la poussent.** Le
champ de contrôle est échantillonné au point ramené en arrière le long du vecteur de poussée du
secteur, du terrain gagné depuis la reprise en main — ce qui fait glisser la ligne et laisse
exactement où la chronique les avait laissés tous les endroits que les secteurs ne touchent pas.
Le déplacement est interpolé en latitude entre les huit ancrages et s'éteint au-delà des extrémités
du théâtre modélisé, si bien que la Crimée et le nord ne dérivent pas quand le Donbass bouge.

**Une correction, et c'est elle qui empêche que ce soit une simple translation.** Une avance prend
l'**union** du champ et de sa copie décalée, un recul leur **intersection**. Translater tel quel
tirerait l'enclave de 2014 vers l'ouest en même temps que le front et ouvrirait une bande de
terrain inoccupé contre la frontière russe, ce qui n'arrive dans aucun des deux sens. Dilater et
éroder le long de l'axe d'avance déplace la ligne, et seulement la ligne.

Ce que le raccord produit sur le déroulé de la victoire, mesuré à l'écran :

| Trimestre | Régime | Territoire russe |
|---|---|---|
| Été 2026 | documenté | 98 000 km² |
| Automne 2026 → hiver 2027 | projection | 98 000 km² |
| Printemps 2027 | projection | 97 000 km² |
| Automne 2027 — l'armistice | projection | 93 000 km² |

Et sur le déroulé « le soutien tient, sans plus » : **98 000 km² pendant les six trimestres
projetés, sans varier d'un hexagone.** Le front figé n'est plus une absence à l'écran, c'est un
chiffre qui ne bouge pas.

---

## 5. L'arbitrage : quatre pièces, quatre autorités nommées

C'est la partie que l'utilisateur a explicitement demandé de trancher, parce que brancher
l'histoire sur la carte met quatre objets en concurrence sur la même page.

### 5.1 Le compteur du bandeau — une seule mesure, lue sur le dessin

Il affichait « *X* km² pris sur les secteurs simulés » : le cumul du modèle sur ses huit secteurs,
au-dessus d'une carte qui dessine désormais le pays entier depuis la chronique. Deux mesures de
deux choses différentes, côte à côte.

Le bandeau porte maintenant **le territoire sous contrôle russe, compté sur les hexagones
eux-mêmes**, les hexagones côtiers et frontaliers ne comptant que pour la part d'eux-mêmes qui est
de la terre à l'intérieur du pays. C'est le seul chiffre de la page que le lecteur peut vérifier en
regardant l'image, et c'est le même chiffre dans les trois régimes — donc une seule courbe d'un
bout à l'autre de la frise. Il est arrondi au millier, parce qu'une grille de lecture de quarante
kilomètres ne peut pas honorer mieux.

Le cumul du modèle n'a pas disparu : il est descendu dans le panneau des secteurs, qui est le seul
endroit où il veut dire quelque chose.

### 5.2 Les pions de secteur — position historique, force modélisée

Un pion posé sur la position simulée pendant que le terrain derrière lui est la position réelle,
c'est la page qui parle deux fois. Les jauges sont donc **accrochées à la ligne que la carte
dessine** : pour chaque secteur, la longitude la plus occidentale que l'envahisseur tient ou
dispute à la latitude de son ancrage. Quand plus rien n'est tenu à cette latitude — le secteur de
Kharkiv à l'automne 2022, repoussé jusqu'à la frontière d'État —, la jauge revient sur son ancrage
de février 2022, qui est le seul point de cette rangée sur lequel la carte est encore d'accord.

**Les crans, eux, restent ceux du modèle**, et c'est délibéré. Les crans alimentés et les crans à
sec sont de la génération de force : c'est le sujet du site, ce n'est pas dans la chronique, et
c'est la seule chose que la carte apporte que la chronique ne saurait pas dire.

**En revanche, le glyphe de résolution devient historique.** Pendant un trimestre documenté, la
flèche pointe le camp qui a réellement perdu du terrain dans ce secteur — un sens, jamais une
distance, **parce que la chronique enregistre un état de contrôle et pas des kilomètres**, et
qu'imprimer un chiffre à cet endroit serait une fabrication avec l'autorité d'une mesure. Le
cartouche en kilomètres ne revient qu'en régime projeté, où le modèle a effectivement calculé une
distance.

Cinq zones — Kyiv, Tchernihiv, Soumy, Marioupol, Koursk — ne répondent à aucun secteur : le modèle
n'en a jamais eu là. **Leur terrain change de main sur la carte et aucune jauge ne prétend l'avoir
résolu.** C'est la lecture honnête, pas un manque.

### 5.3 Le panneau « rapport de force par secteur » — le modèle, dit à voix haute

Il n'a pas changé de contenu : c'est la lecture que le modèle fait de ses huit secteurs. Il porte
désormais une ligne de provenance qui le dit, et qui dit aussi que la carte au-dessus porte, elle,
la position réelle. Une fois nommées, les deux autorités peuvent se contredire sans mentir — et
elles se contredisent, de 0,6 km à l'été 2026. C'est la précision du modèle sur un trimestre, et
ce n'est pas la carte qui a tort.

### 5.4 Ce qui a changé **ce trimestre-là** — la pièce qui manquait

Les cinq états disent qui tient quoi. Aucun ne dit ce qui vient de se passer, et c'est pourtant la
seule chose que cherche un lecteur qui ouvre un trimestre. Le terrain que l'envahisseur a pris puis
perdu se lit exactement pareil au printemps 2022, quand la colonne de Kyiv vient de partir, qu'en
2026, où c'est vieux de quatre ans : **le retrait cesse d'être un événement.**

Chaque hexagone est donc aussi comparé à lui-même un trimestre plus tôt, et **la région qui a changé
de main est cerclée dans la couleur du camp qui l'a gagnée**, au-dessus de tout le reste. Le trait
mesure 2,2 px sur un halo de papier ; en régime projeté il passe en pointillé comme la ligne de
contact.

C'est la différence entre deux lectures de la même autorité — la chronique contre elle-même tant
qu'elle gouverne, la projection contre elle-même ensuite —, donc la marque fonctionne dans les trois
régimes sans rien mélanger.

Ce que ça change, sur les deux trimestres qui portent l'année 2022 :

| Trimestre | Ce que la carte dit maintenant d'un seul coup d'œil |
|---|---|
| Hiver 2022 | Tout ce que l'invasion a pris est cerclé de rouge — les trois axes du nord, Kharkiv nord, les deux rives de Kherson, Melitopol. La Crimée et le Donbass de 2014 ne le sont pas : ils n'ont pas changé |
| Printemps 2022 | Trois anneaux bleus au nord — Kyiv, Tchernihiv, Soumy libérées — et des anneaux rouges sur Marioupol, Izioum, Lyman et Sievierodonetsk |

C'est le seul objet de la carte qui répond à « qu'est-ce qui s'est passé », et rien n'a le droit de
passer par-dessus.

### 5.5 La carte — l'histoire l'emporte, et le dit

Sur la période documentée, l'histoire gouverne le terrain. C'est la consigne, et c'est aussi la
seule position défendable : le site est public, la guerre est en cours, et une carte qui
projetterait par-dessus des faits établis serait la seule chose du projet qu'on ne pourrait pas
défendre.

---

## 6. Ce que les chiffres valent

Mesurés sur la grille de lecture, comparés aux ordres de grandeur de la chronique.

| Trimestre | Carte | Réel | Écart |
|---|---|---|---|
| Automne 2021 — la ligne de 2014 | 39 800 km² | ~43 000 km² | −7 % |
| Hiver 2022 — le pic | 119 000 tenus + 7 500 disputés | ~163 000 km² | −22 % |
| Été 2026 | 98 200 km², soit 16,3 % du pays | ~117 000 km², 19 % | −16 % |
| Koursk, été 2024 | 2 800 km² | ~1 000 km² | +180 % |

**La ligne de 2014 et la position d'aujourd'hui sont bonnes à moins de dix pour cent** — ce sont
les deux extrémités de la frise, et ce sont elles que le lecteur peut recouper.

**Le pic de mars 2022 reste sous-estimé d'un cinquième**, et pour une raison identifiable : le
vocabulaire de vingt zones ne distingue pas la Kharkiv de 2022, où la Russie tenait une large part
de l'oblast, de la Kharkiv de 2024, réduite à la bande frontalière de Vovtchansk et Lyptsi. Une
seule zone pour deux emprises : elle est calée sur la seconde, qui court sur quinze trimestres, au
prix de la première, qui en occupe deux. Les parties de l'oblast de Mykolaïv et du nord de la
Kherson tenues en 2022 manquent pour la même raison.

À comparer à ce que le modèle seul produisait : **4 183 km² au pic de 2022**. L'écart passe d'un
facteur vingt-huit à un facteur un virgule deux.

`sumy_axis` porte l'inverse du même défaut : la zone est dimensionnée sur le balayage de 2022 et
elle est aussi celle qui s'allume, `contested`, pour la zone tampon de 200 km² de 2025. La tache
est trop grande pour ce qu'elle rapporte. Elle est **disputée** et non tenue, ce qui est la bonne
catégorie, et le texte du trimestre donne la mesure — mais c'est une surestimation assumée, la
seule à l'intérieur de la période documentée.

---

## 7. L'adresse porte l'état consulté

Vingt-six trimestres, trois déroulés, trois écrans — et une seule adresse pour les deux cent
trente-quatre combinaisons. On ne pouvait ni partager ce qu'on regardait, ni y revenir, ni ouvrir
deux trimestres côte à côte pour les comparer, ce qui est précisément le geste que cette carte
appelle. L'adresse dit maintenant le déroulé, le trimestre et l'écran, et remet la page dans cet
état exact à l'ouverture :

```
?deroule=victoire&trimestre=2022-printemps&ecran=front
```

`deroule` vaut `victoire`, `front-fige` ou `effondrement` — les trois issues que le README nomme.
`ecran` vaut `russie`, `ukraine` ou `front`.

**Pas de rang de tour, même ici.** « 2022-printemps » se lit, « t=6 » non : c'est la règle de la
frise, qui porte la saison et jamais le numéro, appliquée à l'adresse. Un rang est accepté **en
entrée**, parce qu'une adresse tapée à la main doit marcher, mais il n'est jamais écrit — `?
trimestre=6` est réécrit en `2023-hiver` dès le premier rendu.

**Tout dans la query string.** Le site est publié en statique sur GitHub Pages : un chemin
`/2022/printemps/` y donnerait un 404, alors qu'un paramètre ne touche pas à la résolution du
fichier. `pushState` à chaque navigation, `popstate` pour le retour arrière du navigateur, et un
`replaceState` au premier rendu pour que le bouton *précédent* ne commence pas par revenir sur la
même page.

**Une adresse illisible ne casse rien.** Chaque paramètre est lu séparément et ce qu'on ne sait pas
relire est ignoré en silence ; l'adresse est ensuite réécrite sur ce qui est réellement affiché, de
sorte que la barre ne ment jamais. En entrée, sont acceptés : le slug, le code de scénario complet
(`ukraine_2022_collapse`), son suffixe (`holds`), la saison accentuée (`2022-été`), l'ordre inversé
(`printemps-2022`) et le rang d'écran (`ecran=2`). Un trimestre qui n'existe pas dans ce déroulé-là
— l'automne 2027 dans un déroulé qui s'arrête à l'automne 2024 — retombe sur la fin de cette
guerre-là plutôt que sur rien.

---

## 8. Ce qui a été écarté, et pourquoi

**Un pavage de zones sans chevauchement.** §3.1 : un trou dans le pavage produirait du terrain
ukrainien crédible au milieu du territoire occupé, et rien ne le signalerait.

**Un diagramme de Voronoï sur tout le pays.** Économique en données, mais la cellule de
`kherson_right` avalerait Mykolaïv et celle de `crimea` remonterait dans la steppe. Les contours
explicites coûtent deux cents coordonnées et rendent chaque limite discutable une par une, ce que
des centroïdes ne permettent pas.

**Faire porter le mouvement historique par les huit secteurs du moteur.** C'était la tentation :
convertir chaque bascule de zone en hexagones et laisser `FrontPhase` les appliquer. C'est
exactement le scriptage que le §14 a écarté — le moteur affirmerait avoir calculé ce qu'on lui a
soufflé, et sa parole sur les trimestres qu'il calcule vraiment n'aurait plus de valeur. La
chronique n'entre jamais dans le moteur : elle est lue par le plateau, et par lui seul.

**Une distance en kilomètres sur les flèches des trimestres documentés.** §5.2. Le fichier donne
un contrôle par zone, pas des kilomètres.

**Remettre une légende dessinée sur la carte.** Elle a été retirée sur demande explicite. La ligne
du coin bas-gauche n'en est pas une : elle nomme l'autorité en vigueur et rien d'autre.

**Un numéro de tour à l'écran.** Nulle part, comme partout ailleurs sur le site : « Hiver 2022 »,
jamais « T2 ».

---

## 9. Où c'est écrit

| Fichier | Rôle |
|---|---|
| `src/TheoryOfVictory.Core/FrontHistory.cs` | Le modèle typé de la chronique — vingt trimestres, vingt zones |
| `src/TheoryOfVictory.Engine/Scenarios/FrontHistoryLibrary.cs` | Le chargeur, sur le patron de `CardLibrary`. Il valide le vocabulaire au chargement : une zone que la carte ne saurait pas dessiner ne passe pas |
| `.../Web/Services/PlayedGameLibrary.cs` · `Controllers/GameController.cs` | La chronique sert la page ; le moteur ne la lit jamais |
| `wwwroot/js/geography.js` | Les vingt contours, le saillant de Koursk, `zoneAt()` |
| `wwwroot/js/frontline.js` | Les trois régimes, le raccord vers la projection, les cinq états de la carte |
| `wwwroot/js/hexmap.js` | Le pavage étendu au saillant, la ligne tracée sur les arêtes des hexagones, la mesure en km², la ligne d'autorité |
| `wwwroot/js/counters.js` | Les jauges posées sur la ligne historique, la flèche sans kilomètres |
| `wwwroot/js/board.js` | Le compteur du bandeau, la note sous la carte, la provenance du panneau, et l'adresse qui porte déroulé + trimestre + écran |
| `tests/…/FrontHistoryTests.cs` | Le fichier charge, il colle au calendrier trimestre par trimestre, Koursk n'est jamais implicite, et le vocabulaire est exactement celui que la carte sait dessiner |
