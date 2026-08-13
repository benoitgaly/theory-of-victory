# Gameplay — ce qui donne envie de jouer

> Document de suivi du travail de game design sur la V1. Le modèle lui-même est dans
> [`01-modele-de-jeu.md`](01-modele-de-jeu.md), qui fait autorité sur les règles.

---

## Le problème posé

La V1 est une partie déterministe qu'on regarde se dérouler. C'est instructif, mais il n'y a
aucune raison de cliquer « tour suivant » : rien n'est en suspens, rien ne monte, rien ne menace.
Or la thèse elle-même est une thèse de **tension lente** — l'effondrement est un seuil, pas une
pente, et un seuil que personne n'a vu venir n'apprend rien.

Le travail a donc porté sur une seule question : **rendre visible la pente avant la falaise.**

---

## Ce qui a été implémenté

### 1. Le fonds souverain est réellement liquidé

C'était le défaut le plus grave, et il vidait le modèle de son sens. Les réserves étaient
comptées mais jamais dépensées : la Russie pouvait perdre toutes ses recettes pétrolières sans
que son effort de guerre en souffre. Autrement dit, **couper la caisse ne servait à rien**, ce
qui contredit exactement la thèse que le jeu prétend démontrer.

Désormais, chaque trimestre, le fonds comble ce que les recettes ordinaires ne financent pas —
et il s'épuise en le faisant. D'où deux indicateurs nouveaux :

- `ReserveQuartersLeft` — combien de trimestres le fonds tient encore au rythme actuel de
  ponction. C'est le **compte à rebours** de la partie.
- `FundingGap` — la part de l'effort de guerre que le trimestre n'a pas pu financer. Au-dessus
  de zéro, l'appareil commence à s'apercevoir que la guerre ne paie plus.

`FundingGap` ronge directement la cohésion des élites. La chaîne est enfin complète et lisible :
**baril → recettes → fonds souverain → capacité à financer → cohésion des élites → survie du
régime.**

### 2. Une couche de tension, dérivée et jamais scriptée

`PressureAlert` et `PressureReading` exposent, à chaque tour et pour chaque camp, ce qu'il sait
de son propre avenir : trimestres de dépôt restants par flux, trimestres de réserve, tours déjà
passés sous le seuil de rupture, tours restants avant que le front cède, et la **pente** du ratio
de génération — pas seulement son niveau.

Trois niveaux : *à surveiller*, *alerte*, *critique*. Rien de tout cela ne modifie la simulation :
c'est une lecture. Un indice de menace synthétique en découle, affiché tour par tour.

### 3. Les contre-cartes

Une carte peut désormais en annuler une autre jouée le même tour (`CountersCardCode`). La carte
contrée est **jouée quand même et reste visible** — elle ne produit simplement rien. Voir une
carte dépensée pour rien est tout le plaisir de la mécanique, et c'est ce qui créera le bluff en
V2, quand l'attaquant devra décider s'il s'engage face à deux cartes inconnues en main.

### 4. Le capital politique est débité

Chaque carte coûte à son camp. La V1.0 joue son calendrier quelle que soit la trésorerie
politique — les trois déroulés doivent rester comparables — mais **le découvert est enregistré**
(`PoliticalCapitalOverdraft`, `AffordedInFull`). C'est la monnaie de la V2 testée en conditions
réelles avant qu'elle ne bloque quoi que ce soit.

### 5. Le deck

Passé à 39 cartes, dont 4 contre-cartes. Chaque carte porte son coût, son type, son délai, ses
effets typés et son texte d'ambiance — le format d'impression est déjà celui de la V2.

---

## Corrections de calibration apportées après coup

Deux aberrations trouvées en relisant les parties à l'écran, toutes deux dues à des boucles non
bornées :

**La capacité industrielle croissait indéfiniment.** Une ligne de production pouvait passer de
1 400 à 73 000 unités par trimestre : le budget d'expansion étant constant, la capacité composait
sans limite. Elle est désormais plafonnée à **3,5 fois le niveau d'avant-guerre**. La Russie a
approximativement triplé sa production d'obus ; elle ne l'a pas multipliée par cinquante. Les
machines-outils, la main-d'œuvre qualifiée et l'espace au sol s'épuisent.

**Les dépôts se remplissaient sans fin.** On accumulait plus de 300 000 intercepteurs faute d'une
règle simple : une armée commande jusqu'à quelques trimestres de stock de guerre, puis dépense
son argent ailleurs. Plafond posé à **six trimestres de production**.

C'est le même enseignement que celui déjà noté pour le recrutement : *on produit pour couvrir un
besoin, jamais pour dépenser un budget*. Trois fois le même piège, à trois endroits différents.

**Correction de fond : l'effectif n'est pas une ressource à couvrir.** Celle-ci n'est pas une boucle
mal bornée mais une erreur de catégorie. Le modèle traitait les soldats comme un flux de plus, avec
un besoin et un taux de couverture ; or il n'existe aucun besoin exogène en hommes auquel comparer
un effectif — c'est l'effectif qui dimensionne le front et fabrique donc le besoin en obus, en
carburant et en vivres. Les hommes tenus en ligne sont désormais la **taille du tonneau**, les trois
flux consommés en sont les douves, et un déficit d'effectif se paie deux fois : le tonneau rétrécit
et il fuit, par perte de cohésion. La règle du minimum ne bouge pas — elle reste entière sur les
matériels, et c'est toujours le cœur pédagogique du jeu. Calibration, trajectoires et sources dans
[`04-calibration-effectifs.md`](04-calibration-effectifs.md).

---

## Ce qui reste à faire

### Mise en scène de la bascule

Le tour où l'aide s'arrête ne produit encore rien de visible — c'est juste, mais le joueur devrait
**sentir** la catastrophe arriver. Les alertes existent dans le moteur ; leur mise en scène à
l'écran (compte à rebours qui s'affole, dépôts qui se vident visuellement) reste à faire côté
interface.

### Le tonneau dessine encore une douve « Soldats »

Le moteur a été corrigé, le dessin non : `FLOWS`, dans `board.js`, compte toujours quatre douves
dont une pour les soldats, et le tonneau est construit en les parcourant. Tant qu'elle est là, la
page affirme exactement le contraire du modèle — que l'effectif est une ressource dont on couvre un
besoin. L'effectif doit devenir la **taille** du tonneau, et non l'une de ses douves.

À traiter dans la même passe : l'unité d'affichage. Le moteur compte en milliers, la page doit
montrer l'homme réel — « 560 000 hommes », jamais « 560 ». La conversion se fait côté C#, pour que
l'affichage n'ait aucun calcul à faire.

### Le coût de l'inaction

Piste non implémentée, et sans doute la plus prometteuse pour l'aspect pédagogique : à chaque
tour, montrer ce que la décision **non prise** aurait changé. Techniquement, cela suppose de
rejouer la partie en faisant varier un paramètre (une carte non jouée) et de comparer les deux
trajectoires — l'ablation. Le moteur étant déterministe, c'est faisable : il suffit de le lancer
deux fois. Coûteux à l'affichage, mais c'est exactement la démonstration que le site veut faire.

### Équilibrage à vérifier

Le critère posé par le document de conception — *le deck frappe profonde doit battre le deck
attrition frontale sur seize tours* — n'a pas été vérifié systématiquement. Il faudrait faire
tourner des parties d'ablation par doctrine et comparer.

### Vers la V2

- La pioche indexée sur la santé des flux : le camp qui décroche pioche moins, donc décroche plus
  vite. La spirale de mort d'un jeu de cartes **est** la spirale d'effondrement de 1918.
- Le capital politique comme vraie contrainte : une carte inabordable reste en main.
- Le deckbuilding comme geste stratégique : chaque deck est une théorie de la victoire explicite.

---

## Statut des chiffres

Inchangé : ce sont des **ordres de grandeur de travail**, posés pour que le moteur produise des
courbes discutables. Ils ne sont pas sourcés un par un et ne doivent pas être cités comme des
faits. Les trimestres 2026 du calendrier pétrolier sont des hypothèses, pas des observations.
