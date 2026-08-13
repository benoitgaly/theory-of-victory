# Le front comme wargame

> Spécification de la lecture et de la mécanique visuelle du front. Le modèle faisant autorité
> reste [`01-modele-de-jeu.md`](01-modele-de-jeu.md) ; les effectifs relèvent de
> [`04-calibration-effectifs.md`](04-calibration-effectifs.md), qui fait autorité sur la puissance
> des unités ; la langue visuelle est celle de [`02-direction-artistique.md`](02-direction-artistique.md).
> Ce document ne modifie aucune règle du moteur.

État : conception arrêtée, aucun code écrit.

---

## 1. Le problème, et le critère qui tranche

La carte hexagonale de `hexmap.js` est bonne et n'est pas en cause. Ce qui l'est, c'est **tout ce
qui l'entoure** : une légende écrite pour un lecteur de presse — « territoire occupé », « ligne de
contact », « pris depuis février 2022 » — et un panneau latéral qui traduit la résolution en
histogrammes horizontaux. On sait où est passée la ligne ; on ne sait pas **qui est fort où, à quel
prix, ni ce qui a été décidé ce trimestre**. Un wargamer regarde cet écran et n'y trouve aucune des
trois choses qu'il cherche : la force en présence, le rapport qui en découle, la table qui convertit
ce rapport en terrain.

Trois livrables suivent : une analyse des familles de mécaniques, un verdict sur les moteurs open
source, et une mécanique retenue. Un seul critère les départage, et il est brutal :

> **Une mécanique qui fait gagner par la manœuvre raconte l'inverse de ce que le jeu démontre.**

O'Brien soutient que la bataille décisive est une reconstruction a posteriori et que ce qui décide
est la capacité à régénérer de la force. Le §13 du modèle en tire la conséquence : *le front est un
thermomètre, pas un moteur*. Toute mécanique qui rend l'encerclement payant, qui récompense la
concentration astucieuse ou qui offre une percée obtenue par le placement plutôt que par le flux est
disqualifiée, quelle que soit son élégance. Le jeu doit permettre de **perdre la guerre en gagnant
des hexagones à chaque tour** ; une mécanique qui rend cela impossible est fausse ici, même si elle
est excellente ailleurs.

Deux contraintes dures encadrent le reste :

1. **La V1.0 est déterministe et sans joueur.** Aucun dé, aucun tirage. Une table de résultats
   indexée par le rapport de force est acceptable — le moteur en possède déjà une, `MovementFor` —
   un jet ne l'est pas. Les trois issues du scénario doivent rester atteignables : victoire
   ukrainienne au T19, front figé, effondrement ukrainien vers T10.
2. **Les hommes sont la taille du tonneau, jamais une douve.** La puissance de combat est déjà
   exprimée par le moteur en **hommes-équivalents d'infanterie pleinement ravitaillée**
   (`SideSnapshot.CombatPower`) : les hommes en ligne de contact, rabotés par la couverture la plus
   courte, par la qualité de formation et par la cohésion. Toute pièce dessinée doit hériter de cette
   unité et de cette logique, sans jamais réintroduire un « taux de couverture en hommes ».

---

## 2. Les familles de mécaniques, du plus simple au plus sophistiqué

Sept familles, dans l'ordre croissant de coût en règles. Pour chacune : ce qu'elle rend visible, ce
qu'elle coûte, et le verdict au regard du critère du §1.

### 2.1 Les blocs — Columbia Games

*Hammer of the Scots*, *Julius Caesar*, *Crusader Rex*, et la variante hexagonale
*Fields of Despair: France 1914-1918*.

L'unité est un bloc de bois posé debout, face au propriétaire. Deux idées, et deux seulement.
**La force est la rotation** : le bloc porte quatre valeurs sur ses quatre côtés, on le tourne d'un
quart de tour à chaque perte, et la force courante est le nombre lisible en haut. **Le brouillard de
guerre est physique** : l'adversaire ne voit qu'un dos vierge, il connaît la position et ignore la
force. C'est le système de réduction par paliers le plus économique jamais publié — là où un pion de
carton classique n'offre que deux états, plein et retourné, le bloc en offre quatre sans une ligne
de règle supplémentaire.

*Rend visible* : la force, instantanément, sans lire un chiffre — on voit combien de crans il reste.
L'usure devient une géométrie, pas une comptabilité.
*Coûte* : presque rien. C'est la mécanique la moins chère en règles de tout le panorama.
*Verdict* : **la meilleure idée disponible pour nous**, à une réserve près. La rotation est
adoptable telle quelle ; le brouillard de guerre ne l'est pas — une partie qui se joue toute seule,
déterministe, dont le spectateur voit les deux tableaux de bord, n'a rien à cacher à personne. Le
brouillard redeviendra pertinent en V2, où il est même la traduction naturelle du §11 du modèle
(« plus le camp est corrompu, plus son tableau de bord ment »).

### 2.2 Le hex-and-counter classique — Avalon Hill, SPI, et sa descendance

*Panzerblitz*, *Squad Leader*, *The Russian Campaign*, puis la lignée moderne
*Operational Combat Series* (MMP) et *Battalion Combat Series*.

Le corps de doctrine du genre : une grille hexagonale, des pions à valeurs imprimées
(attaque – défense – mouvement), des zones de contrôle, et une **table de résultats de combat**
indexée par le rapport de force — on additionne, on divise, on arrondit vers le bas, on lit la
colonne, on jette un dé, on applique le résultat. La CRT est l'invention structurante du genre : un
tableau imprimé sur la carte qui rend la résolution publique et vérifiable.

La branche OCS pousse la logistique au premier plan et mérite d'être citée à part : les points de
ravitaillement sont des pions physiques qu'il faut transporter par camion depuis la tête de voie
ferrée, et **on ne peut pas attaquer à pleine force sans les avoir amenés**. C'est le wargame
commercial qui se rapproche le plus de la thèse d'O'Brien — le joueur passe son temps à dégager des
voies ferrées et à constituer des dépôts, et l'offensive n'est que la dépense de ce qu'il a réussi à
acheminer.

*Rend visible* : le rapport de force, le coût de l'attaque, et — chez OCS — le fait que la
logistique précède le combat.
*Coûte* : cher. Zones de contrôle, empilement, retraites, avance après combat, ravitaillement tracé
sur la carte : c'est un corps de règles, pas une mécanique.
*Verdict* : **on garde la CRT, on jette tout le reste.** La table de résultats est exactement ce qui
manque à l'écran actuel, et le moteur en possède déjà une (§4.3). Les zones de contrôle, la retraite
et l'avance après combat sont de la manœuvre — donc disqualifiées. Le ravitaillement tracé sur la
carte serait une double modélisation : chez nous, la logistique est économique
(`TransmissionRate`, `LogisticsIntegrity`), pas géographique, et la dessiner deux fois créerait deux
autorités contradictoires sur la même information, ce que la direction artistique interdit.

### 2.3 Les jeux dirigés par cartes — Mark Herman et sa descendance

*We The People* (1993), *Hannibal*, *Paths of Glory*, *Twilight Struggle*.

L'invention : une carte à double emploi. Elle porte un **événement historique** et une **valeur
d'opérations** ; on ne peut utiliser qu'un des deux, et jouer une carte pour ses points offre son
événement à l'adversaire. Le tempo, l'initiative et le hasard sont ainsi entièrement portés par la
main, jamais par un dé de mouvement. *Paths of Glory* y ajoute une idée qui nous concerne
directement : les **niveaux de tranchée**, un marqueur à trois états posé sur l'hexagone, qui décale
la colonne de la CRT et rend l'attaque frontale progressivement absurde.

*Rend visible* : l'arbitrage, et la rareté du temps.
*Coûte* : modéré, et le jeu tient dans le deck.
*Verdict* : **déjà adopté**, et c'est la colonne vertébrale de la V2 telle que le §15 du modèle la
décrit. Rien à reprendre de plus ici, sauf le marqueur de tranchée, qui est repris au §4.1 sous
forme de crénelage.

### 2.4 La série COIN — GMT

*Andean Abyss*, *Fire in the Lake*, *A Distant Plain*.

Quatre factions asymétriques, une piste d'éligibilité qui interdit d'agir deux tours de suite, un
menu d'opérations et d'activités spéciales par faction. Le cœur en est le **coût d'opportunité
posé en dur** : choisir sa case dans la séquence détermine ce que l'adversaire pourra encore faire.

*Rend visible* : l'asymétrie des acteurs, et le fait qu'agir maintenant, c'est renoncer à agir
ensuite.
*Coûte* : très cher. C'est le genre le plus lourd du panorama, et il suppose trois à quatre joueurs.
*Verdict* : **écarté**. Le conflit est bilatéral, la V1 n'a aucun joueur, et l'éligibilité résout un
problème de table à quatre que nous n'avons pas. Une seule idée mérite d'être notée pour la V2 : le
menu d'actions court et nommé par faction — quatre verbes, pas douze — que le §4.4 reprend.

### 2.5 Les jeux à impulsions par zones

*Storm Over Arnhem*, *Breakout: Normandy*, *Unconditional Surrender! World War 2 in Europe*.

La carte est découpée en zones, pas en hexagones, et le tour est une succession d'impulsions courtes
plutôt qu'un bloc « je bouge tout, tu bouges tout ». *Unconditional Surrender!* est le plus proche
de nous par ses intentions : c'est un jeu stratégique où la production, la politique et le
ravitaillement décident, et où la carte à zones existe précisément pour **empêcher la finesse
opératoire** et forcer le joueur à raisonner en théâtres.

*Rend visible* : que le front est une affaire de secteurs, pas de positions.
*Coûte* : faible pour les zones, élevé pour les impulsions.
*Verdict* : **la zone est déjà notre modèle** — le moteur alloue la puissance par secteur et jamais
par unité, ce qui est exactement une carte à zones posée par-dessus une carte à hexagones. Les
impulsions sont écartées : elles n'ont de sens qu'avec un joueur qui réagit, et la V1 n'en a pas.

### 2.6 Les jeux de front continu — la ligne comme corde sous tension

*Fields of Despair: France 1914-1918*, *The Great War* de Ted Raicer, la série *Der Weltkrieg*, et,
côté numérique, *The Great War: Western Front* (Petroglyph, 2023).

Ici la ligne n'est pas la somme de duels indépendants : c'est un objet unique, continu, qui se
déforme. Pousser au centre étire les flancs ; un secteur qui cède entraîne ses voisins. Le
tracé lui-même est la pièce de jeu.

*Rend visible* : la solidarité entre secteurs, et la notion de saillant.
*Coûte* : c'est le piège. La déformation de la ligne appelle immédiatement des règles de flanc, de
saillant, de rectification — c'est-à-dire de la manœuvre, réintroduite par la géométrie.
*Verdict* : **l'image est reprise, la règle est écartée.** La carte dessine déjà une ligne continue
qui se déforme, et c'est très bien ainsi. Y adosser une règle — un malus au saillant, un bonus
d'appui entre secteurs voisins — ferait gagner par la forme du tracé, c'est-à-dire précisément par
la manœuvre. C'est le seul endroit du document où une idée séduisante est rejetée pour une raison
purement doctrinale, et il faut qu'elle reste écrite (§7).

### 2.7 Les jeux d'attrition — là où le terrain ne bouge pas

*Verdun: The Game of Attrition* (1972), la série *Der Weltkrieg*, et tout ce qui se joue à la
volonté nationale plutôt qu'à la ligne : *Fields of Despair* encore, où l'on gagne en épuisant la
volonté adverse, et où même une victoire coûte.

Le genre repose sur une inversion : la carte cesse d'être l'objectif et devient le lieu de la
dépense. On ne gagne pas en prenant du terrain mais en obligeant l'autre à en payer le prix, et la
condition de victoire est une jauge d'arrière — moral, volonté, épuisement — pas une ligne.

*Rend visible* : que le terrain pris et la guerre gagnée sont deux choses différentes.
*Coûte* : peu en règles, beaucoup en pédagogie — un joueur qui voit une carte veut la conquérir.
*Verdict* : **c'est exactement notre sujet, et le moteur l'implémente déjà.** La partie « normale »
finit en front figé, l'effondrement vient de `ControlPhase` et jamais de `FrontPhase`, et le §16 du
modèle l'écrit noir sur blanc. Ce qui manque n'est pas la mécanique : c'est **sa visibilité**. Rien
sur l'écran actuel ne dit au spectateur que la ligne qu'il regarde n'est pas ce qui décide.

### 2.8 Les traitements contemporains de l'Ukraine

Il en existe, et un seul mérite d'être étudié.

***Defiance: 2nd Russo-Ukrainian War 2022-?*** (DB Dockter et Mark Herman, GMT, volume 1 *Miracle on
the Dnipro*) couvre les campagnes de Kyiv et Tchernihiv de février à avril 2022. Sa note de
conception est directement utile : les auteurs abandonnent le rapport de force comme moteur unique
au profit de la **qualité des troupes augmentée par les appuis** (« Force Augmentation and Troop
Quality »), au motif que l'artillerie inflige *80 à 90 % des pertes* de ce conflit et que la
prolifération des drones bon marché a rendu la supériorité aérienne moins décisive que les modèles
des années 1990 ne le supposaient. Leur formule mérite d'être citée telle quelle : *« ce que la CRT
ne dit pas, c'est qu'avoir trois soldats contre un produit un avantage — c'est une mesure de
puissance de feu relative. »* Les pertes y sont la **conséquence** du résultat de combat, jamais sa
cause, et la cohésion s'effondre avant que l'unité ne disparaisse.

Les autres — *Ukraine 2022: Tabletop Wargame*, *2022: Ukraine*, *Brovary 2022* — sont tactiques ou
opératifs de courte durée et n'apportent rien à un jeu trimestriel de génération de force.

*Ce qu'on en retient* : deux confirmations. La cohésion précède l'élimination — le moteur le fait
déjà par `CohesionFactor`. Et le rapport de force est une mesure de puissance de feu relative, pas
un compte d'hommes — ce que la règle du minimum dit déjà, puisque la puissance est l'effectif *raboté
par la douve la plus courte*. Aucun emprunt de règle, deux validations.

### 2.9 Récapitulatif

| Famille | Ce qu'elle rend visible | Coût en règles | Sert ou trahit O'Brien |
|---|---|---|---|
| Blocs Columbia | La force comme crans, l'usure comme géométrie | Très faible | **Sert** — la force devient lisible sans chiffre |
| Hex-and-counter | Le rapport de force, le coût de l'attaque | Élevé | **Mixte** — la CRT sert, la manœuvre trahit |
| OCS (logistique) | Que l'acheminement précède le combat | Très élevé | **Sert**, mais double notre modèle économique |
| Dirigé par cartes | L'arbitrage, la rareté du temps | Modéré | **Sert** — déjà adopté pour la V2 |
| COIN | L'asymétrie, le coût d'opportunité | Très élevé | Hors sujet à deux camps |
| Impulsions par zones | Le secteur comme unité de décision | Faible | **Sert** — déjà notre modèle |
| Front continu | La solidarité de la ligne | Modéré, et glissant | **Trahit** dès qu'on en fait une règle |
| Attrition | Que prendre du terrain n'est pas gagner | Faible | **Sert** — c'est la thèse même |

---

## 3. Les moteurs open source : verdict

La question est légitime et mérite une réponse franche plutôt qu'une dépendance de politesse.

| Projet | Licence · langage | Poids | Ce qu'il faudrait abandonner |
|---|---|---|---|
| **TripleA** | GPLv3 · Java | Application de bureau complète | Tout. C'est un jeu à la *Axis & Allies* avec son moteur de combat, son IA et son format de carte XML. L'adopter, c'est remplacer le produit, pas l'outiller |
| **VASSAL** | LGPL · Java | Application de bureau, ~200 Mo avec la JVM | Tout, également. VASSAL est une table virtuelle pour jouer à des jeux publiés : il déplace des pions, il ne simule rien. Notre moteur *est* la simulation |
| **Battle for Wesnoth** | GPL · C++ | Jeu complet | Tout. Moteur tactique tour par tour, avec dés, unités et niveaux — l'exact contraire du modèle |
| **Freeciv** | GPL · C | Jeu complet | Idem |
| **boardgame.io** | MIT · JavaScript | ~50 Ko + React en pratique | Rien à gagner en V1 : c'est un cadre d'état de partie multijoueur, et la V1 n'a ni joueur, ni tour à négocier, ni serveur de partie. À réexaminer honnêtement pour la V2, où le 1v1 en aurait l'usage |
| **Rally the Troops** | Serveur public, modules sous licence des ayants droit · Node.js | Serveur complet | Non applicable : c'est une plateforme pour jouer en ligne à des wargames publiés, sous accord avec leurs éditeurs |
| **honeycomb-grid** | MIT · TypeScript | ~15 Ko | Rien, et c'est une bonne bibliothèque. Mais `hexmap.js` pose déjà son pavage pointy-top en une quarantaine de lignes, et **la géométrie n'est pas la partie difficile** : la partie difficile est la classification des hexagones contre la ligne de contact (`classifier`, `classify`), que nulle bibliothèque ne fait |
| **Phaser** | MIT · JavaScript | ~1 Mo | Le SVG dans le DOM, donc le texte sélectionnable, l'accessibilité, l'impression, et le rendu « papier » qui *est* l'identité du site. Un canevas WebGL rendrait ce plateau moins bon, pas meilleur |

**Verdict : aucune dépendance. Zéro.** Le plateau est du SVG écrit à la main dans une page
ASP.NET Core, sans ressource distante, et il fonctionne hors ligne — c'est une exigence de la
direction artistique, pas un accident. Le seul gain possible serait quelques dizaines de lignes de
géométrie hexagonale, contre une dépendance, un `package.json`, une chaîne de compilation et la
perte du fonctionnement hors ligne. Le calcul est défavorable et il n'est pas serré.

Ce qu'il faut emprunter au monde libre est de la **connaissance**, pas du code : la référence de
Red Blob Games sur les grilles hexagonales — dont `honeycomb-grid` s'inspire explicitement et dont
le code actuel porte visiblement la trace — et l'idiome des crans de Columbia, qui est une idée, pas
une bibliothèque.

---

## 4. La mécanique retenue — le pion de secteur et la butée

Une seule mécanique, trois pièces, quatre verbes. Elle ne consomme que des grandeurs que le moteur
calcule déjà et **n'ajoute aucune règle de simulation**.

Le principe tient en une phrase, et c'est celle qu'il faut pouvoir défendre devant un wargamer :

> **Chaque secteur porte deux pions à crans. Le rapport entre eux se lit dans une table imprimée sur
> la carte. Sur les quatre ordres possibles, un seul touche la ligne — et ce n'est pas celui qui
> gagne la guerre.**

### 4.1 Le pion de secteur

Huit secteurs, deux camps : **seize pions**, posés de part et d'autre de la ligne de contact, ancrés
sur les coordonnées que `board[]` publie déjà. Carton `#fbf9f4`, filet `#d9d1be`, encre chaude
`#1a1815`, bandeau supérieur à la couleur du camp (`#a8322a` / `#1e5fa8`). Environ 46 × 34 px.

Anatomie, cinq éléments et pas un de plus :

**a. Le cadre OTAN et l'échelon.** Rectangle à symbole d'infanterie — les deux diagonales croisées —
surmonté de `XXXX` (armée) ou `XXX` (corps) selon la puissance engagée. Le choix de l'infanterie
n'est pas décoratif : le moteur exprime littéralement sa puissance en *hommes-équivalents
d'infanterie pleinement ravitaillée*. Le pion dit donc la vérité de l'unité de compte, ce qu'aucun
histogramme ne fait.

**b. L'échelle de crans — la pièce maîtresse.** Sur le flanc gauche du pion, une échelle verticale de
huit encoches. Elle se lit en deux temps, et c'est ce qui fait toute la valeur du dessin :

- **Les crans pleins** — la puissance réellement engagée ce trimestre, celle que le moteur nomme
  `SustainableCombatPower` répartie par secteur : les hommes en ligne, rabotés par la douve la plus
  courte.
- **Les crans évidés, au-dessus** — les crans que l'effectif présent justifierait si la couverture
  était pleine. Ce sont **les hommes qui sont là et qui ne servent à rien**, faute d'obus, de
  carburant ou de vivres.

L'écart entre les deux est la règle du minimum, gravée sur le pion. Un camp peut afficher huit crans
d'hommes et n'en avoir que trois de pleins : c'est toute la thèse, lisible en un dixième de seconde,
au bon endroit — sur le front, là où le lecteur cherche la force. Le tonneau de Liebig dit la même
chose sur l'écran de génération ; le pion le redit là où ça se paie. Deux surfaces, une seule
autorité : les deux dérivent du même `MaterialCoverage`, jamais d'un calcul parallèle.

Les crans perdus au cours du trimestre sont barrés d'un trait gravé, dans la couleur du camp
adverse. On voit le morceau manquant, on ne lit pas un pourcentage — c'est la règle déjà posée pour
l'encoche d'usure des composantes d'armée.

**c. Le glyphe de goulot.** En bas à droite, un pictogramme de 9 px désignant la douve courte,
et seulement quand elle mord (couverture < 0,95) : obus pour `weapons` (`#b8860b`), bidon pour
`fuel` (`#8a5a2b`), miche pour `food` (`#3d7a51`). La donnée existe : `BottleneckCode`. Une seule
autorité, là encore — le glyphe nomme la ressource que le moteur a nommée, même si deux sont à
égalité.

**d. Le crénelage de retranchement.** Le long du bord tourné vers l'ennemi, la ligne crénelée
classique du symbole de tranchée : zéro à trois créneaux selon `FortificationOf(side)`, borné à 1,2
dans le moteur. C'est l'idiome de *Paths of Glory*, et il est immédiatement lu par n'importe quel
joueur. Un secteur retranché *ressemble* à un secteur retranché.

**e. Le chiffre de contrôle.** Sous l'échelle, en sérif 11 px à chiffres tabulaires, la puissance en
milliers d'hommes-équivalents. L'échelle est le coup d'œil ; le chiffre est la vérification. Jamais
l'inverse.

**L'étalon d'un cran.** Valeur de travail : **1 cran = 10 000 hommes-équivalents**, échelle de huit,
soit 80 000 par secteur avant saturation. À caler une fois pour toutes en relevant la puissance
maximale par secteur sur la trajectoire de référence et en divisant par sept — de sorte qu'aucun
pion ne sature dans une partie normale, la saturation étant signalée par un chevron gravé au-dessus
de la huitième encoche. C'est une constante d'affichage, jamais une constante de règle : la changer
ne doit modifier aucune sortie du moteur.

### 4.2 La butée et la flèche

Entre les deux pions d'un secteur, **un seul objet**, et il porte tout le résultat.

**Quand le rapport est inférieur à 1,1** — l'état normal du jeu, celui de la plupart des secteurs de
la plupart des trimestres — on ne dessine pas une flèche courte : on dessine une **butée**, deux
chevrons opposés qui se rencontrent, en encre pleine, avec les deux chiffres de pertes de part et
d'autre. Cette pièce est capitale et elle doit être *belle*, parce que le front figé est le résultat
normal du modèle et non un vide. Aujourd'hui, un secteur immobile n'est rien à l'écran : demain, il
sera **deux armées qui se poussent et ne bougent pas, en se saignant**. C'est la même information,
enfin dessinée.

**Au-delà de 1,1** — une flèche gravée, d'épaisseur croissante avec le rapport, pointant vers le
camp qui recule, portant en cartouche crème `#f5f1e6` le déplacement en kilomètres. La couleur est
celle du camp qui avance. C'est déjà, à peu de chose près, ce que fait `callouts()` : la flèche
remplace le trait de rappel et le cartouche, et gagne le poids qui manque.

**Quand la défense est rompue** (`HasCollapsed`), la flèche traverse plusieurs hexagones de lecture
d'un seul trait, sans cartouche, et le pion du camp effondré est barré d'une croix gravée. Le
moteur multiplie déjà le mouvement par 3,5 dans ce cas. Il faut que ce trimestre-là *ne ressemble à
aucun autre* : c'est le moment où le jeu prouve son propos, et il n'arrive jamais en poussant plus
fort.

### 4.3 La table de résolution, imprimée sur la carte

Rien ne dit « wargame » plus vite qu'une CRT dans le coin du plateau. Le moteur en possède déjà une,
déterministe, dans `MovementFor` et `AttackCostMultiplier` : elle n'a jamais été montrée. C'est une
perte sèche, parce qu'une table imprimée transforme un spectateur en lecteur — il **prévoit** le
résultat au lieu de le subir, et c'est exactement la tension que le §Gameplay cherche.

| Rapport | Résultat | Coût de l'attaquant |
|---|---|---|
| < 1,1 | **Butée** — aucun mouvement, usure réciproque | × 5 |
| 1,1 – 2,0 | **Grignotage** — 0 à 1 hex (0 à 10 km) | × 4 |
| 2,0 – 3,0 | **Avance** — 1 à 2 hex (10 à 20 km) | × 2,5 |
| > 3,0 | **Percée** — 2 à 3 hex, plafonné à 30 km | × 1,2 |
| Défense rompue | **Avance libre** — mouvement × 3,5 | minime |

Elle est reproduite telle quelle, en 9,5 px, dans le cartouche de légende. Sous la table, une ligne
en petites capitales qui est le cœur du jeu et qu'il faut lire à voix haute :
**« la défense ne se rompt jamais ici — elle se rompt en phase de contrôle, quand la génération de
force passe sous le seuil trois trimestres de suite. »**

Deux précisions d'échelle à porter dans la légende, pour que rien ne mente : l'hexagone dessiné est
un **hexagone de lecture de 40 km**, quand le moteur déplace des hexagones de résolution de 10 km ;
les déplacements sont donc libellés en kilomètres, jamais en hexagones.

### 4.4 Les quatre verbes

Quatre ordres, pas un de plus. Chacun a un coût lisible et une conséquence visible, et **trois des
quatre ne touchent pas la ligne**. C'est la structure du menu qui porte la thèse, avant même qu'on
ait joué.

| Verbe | Ce que le moteur fait déjà | Coût lisible | Conséquence sur la carte |
|---|---|---|---|
| **TENIR** | `OffensivePosture` faible sur le secteur | Aucun — c'est l'ordre par défaut | Le pion garde ses crans ; la butée s'installe |
| **PESER** | `SectorEffort` élevé sur le secteur | **3 à 5 fois les pertes du défenseur**, affiché sur la flèche | Flèche, kilomètres, crans barrés des deux côtés |
| **CREUSER** | `FortificationShare` du budget | Un budget qui n'achète pas d'obus | Un créneau de plus sur le pion, dès le trimestre suivant |
| **FRAPPER LOIN** | `StrikeVectorsShare` + `PrimaryStrikeTarget` | Des vecteurs, et **les intercepteurs qui manqueront au front** | Rien sur la ligne. Un impact sur le bandeau de profondeur, et deux tours plus tard, des crans qui s'évident sur **tous** les pions adverses |

La dernière ligne est le document tout entier. **Frapper loin ne déplace pas la ligne d'un mètre, et
c'est le seul ordre qui gagne la guerre.** L'effet est différé, indirect, et il se manifeste au seul
endroit où le joueur regardait la force : les crans pleins de l'adversaire deviennent des crans
évidés, sur toute la longueur du front, sans qu'aucune flèche n'ait été tracée. Le critère
d'équilibrage du §15.5 du modèle — *le deck frappe profonde doit battre le deck attrition frontale
sur seize tours* — cesse d'être une note de conception et devient une chose qu'on voit.

En V1.0, ces verbes ne sont pas des boutons : ce sont des **étiquettes de lecture**, dérivées de la
doctrine scriptée et posées sous chaque pion en petites capitales 8,5 px. Le spectateur lit l'ordre
donné, puis en voit le prix. En V2, les mêmes quatre étiquettes deviennent les seules actions
disponibles, sans que l'interface change de langue.

### 4.5 Le bandeau de profondeur

`FRAPPER LOIN` a besoin d'un endroit où se voir, et la carte n'en offre pas : la profondeur russe est
hors champ. Une bande de 26 px de haut, le long du bord droit de la carte pour la Russie et du bord
gauche pour l'Ukraine, séparée du plateau par un filet — **c'est l'arrière, et il n'est pas à
l'échelle**, ce que le dessin doit assumer plutôt que masquer.

Quatre cases, une par `StrikeTarget` : réseau électrique, raffinage et terminaux, usines
d'armement, nœuds logistiques. Dans chaque case, les impacts du trimestre en petits carrés de 6 px,
et **la distinction qui décide de tout**, déjà modélisée par `PermanentDamageShare` :

- **carré évidé** = dégât réparable, il faudra y revenir ;
- **carré plein en encre** = perte définitive — la salle des machines, la turbine qu'on ne
  refabrique pas.

Une seule ligne de texte sous les carrés : le rapport d'échange de la vague
(`StrikeResolution.ExchangeRatio`), en sérif, formulé « 1 : 96 ». Et lorsque `Saturated` est vrai, le
mot **SATURATION** gravé en travers de la case — parce que saturer les magasins est ce qui ouvre la
porte aux missiles, et que cela ne se voit nulle part aujourd'hui.

### 4.6 La légende, refaite pour un wargamer

Trois blocs, empilés dans le cartouche existant du coin bas-gauche, élargi à 210 px. Elle doit
apprendre à lire la carte en dix secondes ; elle ne commente rien.

**Bloc 1 — Le terrain.** Cinq états, renommés en vocabulaire de plateau. Le contenu ne change pas,
les mots si :

| Aujourd'hui | Demain |
|---|---|
| Tenu | **Sous contrôle ukrainien** |
| Occupé avant février 2022 | **Sous contrôle russe** (dont annexions antérieures) |
| Pris depuis février 2022 | **Conquis depuis février 2022** |
| Repris par l'Ukraine | **Reconquis** |
| Traversé par la ligne | **Contesté** — traversé par la ligne de contact |

« Contesté » est un mot de wargame ; « traversé par la ligne » est une légende de presse. C'est toute
la différence de registre que l'utilisateur pointe.

**Bloc 2 — L'anatomie du pion.** Un pion agrandi à 1,6 fois, avec quatre traits de rappel et quatre
libellés en petites capitales : *crans alimentés*, *crans à sec — hommes présents, non pourvus*,
*goulot*, *retranchement*. C'est le bloc le plus important de la légende, parce qu'il enseigne la
règle du minimum sans écrire une phrase.

**Bloc 3 — La table de résolution** du §4.3, plus l'échelle : la barre de 100 km existante et la
mention « 1 hex de lecture = 40 km · 1 cran = 10 000 hommes-équivalents ».

Et une ligne, en bas, hors bloc, en italique sérif — la seule phrase d'humeur autorisée, parce que
c'est celle que le visiteur doit emporter :

> *Le front est un thermomètre. Ce qui décide se passe derrière.*

---

## 5. Ce que le moteur doit publier

Rien de ce qui suit ne change une règle, un seuil ou une trajectoire. Ce sont des grandeurs déjà
calculées à l'intérieur de `FrontPhase` et jetées à la fin de la méthode. Les publier est la
condition d'existence des pièces du §4, et le test de non-régression est simple : **les trois issues
doivent rester identiques au bit près.**

**Dans `SectorResolution`** (`src/TheoryOfVictory.Core/FrontSector.cs`) :

| Champ | Pourquoi |
|---|---|
| `AttackerSideCode` | Aujourd'hui l'attaquant n'est déductible que du signe de `HexesMoved` — donc indéductible quand rien ne bouge, c'est-à-dire dans le cas le plus fréquent. La butée a besoin de savoir qui poussait |
| `InvaderPower` · `DefenderPowerRaw` | La puissance brute des deux camps sur le secteur, avant multiplicateurs. Les crans pleins des deux pions |
| `InvaderEstablishmentPower` · `DefenderEstablishmentPower` | La même à couverture pleine : `SustainableCombatPower / clamp(MaterialCoverage)`, réparti pareil. Les crans évidés |
| `TerrainMultiplier` · `Urbanisation` | Pour l'infobulle du rapport, et pour que la résistance soit décomposable |
| `InvaderFortification` · `DefenderFortification` | Les créneaux |
| `DroneFriction` · `SeasonModifier` | Les deux facteurs qui expliquent une butée que le seul rapport de puissance ne suffit pas à expliquer |

`DefenderPower` conserve son sens actuel — la résistance complète — et n'est pas touché : c'est lui
qui alimente le rapport affiché.

**Dans le tableau `board` du `GameController`** : `width`, `terrain`, `urbanisation`,
`strategicValue`. Quatre champs de `FrontSector` déjà disponibles, aujourd'hui non sérialisés.

**Dans `TurnSnapshot`** : les ordres du trimestre par camp et par secteur — `SectorEffort` normalisé
et `OffensivePosture` — pour dériver les quatre verbes sans que le JavaScript ait à deviner
l'intention de la doctrine.

Une seule règle de discipline sur cette section : **le JavaScript ne recalcule jamais une grandeur
du modèle.** S'il faut un chiffre, le C# le publie. C'est déjà la convention retenue pour les
effectifs, où la conversion en hommes réels se fait côté serveur.

---

## 6. Chemin d'implémentation

Cinq étapes. La règle et le dessin sont séparés nettement : **seule l'étape 0 touche du C#, et elle
ne change aucune valeur.** Les étapes 1 à 4 sont exclusivement du dessin, et chacune produit à elle
seule un écran meilleur que celui d'aujourd'hui.

### Étape 0 — Publication *(C# seul, aucune règle)*

| Fichier | Modification |
|---|---|
| `src/TheoryOfVictory.Core/FrontSector.cs` | Les champs du §5 sur `SectorResolution` |
| `src/TheoryOfVictory.Engine/Phases/FrontPhase.cs` | Les renseigner dans `ResolveSector` — aucune ligne de calcul nouvelle, seulement des affectations |
| `src/TheoryOfVictory.Core/GameState.cs` | Les ordres par secteur sur `TurnSnapshot` |
| `src/TheoryOfVictory.Engine/TurnEngine.cs` | Capture des ordres |
| `src/TheoryOfVictory.Web/Controllers/GameController.cs` | Les quatre champs de `board` |
| `tests/…/ModelRulesTests.cs` | `PublishingSectorDetail_ChangesNoOutcome` — les trois issues rejouées et comparées tour par tour |

### Étape 1 — Le pion *(dessin)*

Fichier **nouveau** : `src/TheoryOfVictory.Web/wwwroot/js/counters.js`, sur le modèle de
`hexmap.js` — il possède son fichier pour pouvoir être retravaillé sans toucher au reste, et
n'expose qu'une fonction, `window.tovCounters.draw(svg, turn, board, project)`.

`hexmap.js` l'appelle à la fin de `render()`, après les villes et **à la place de** `callouts()`,
dont il reprend la logique d'anti-collision — c'est la seule partie délicate, et elle est déjà
écrite. Aucune autre modification de `hexmap.js`.

### Étape 2 — La butée, la flèche et la table *(dessin)*

`counters.js` pour la butée et la flèche ; `hexmap.js` pour la fonction `legend()`, entièrement
réécrite selon le §4.6. Le panneau latéral « Rapport de force par secteur » de `board.js` perd ses
jauges — l'information est passée sur la carte — et se réduit à la liste des secteurs qui ont bougé,
avec leur résultat textuel. Moins de surface, plus d'information.

### Étape 3 — Le bandeau de profondeur *(dessin)*

`counters.js` ou un `depth.js` séparé si le fichier dépasse 300 lignes. Alimenté par
`turn.invaderStrike` et `turn.defenderStrike`, déjà publiés dans `TurnSnapshot` et aujourd'hui
inexploités par la carte.

### Étape 4 — Les quatre verbes *(dessin)*

Les étiquettes d'ordre sous chaque pion, et une ligne d'en-tête sous la carte rappelant les quatre
verbes avec leur coût. Dépend de l'étape 0 pour les ordres publiés.

### Étape 5 — Style *(dessin)*

`src/TheoryOfVictory.Web/Views/Shared/_Layout.cshtml.css` : les classes du panneau latéral allégé.
Aucune couleur nouvelle — les quatre teintes de flux et les deux teintes de camp existent déjà.

---

## 7. Ce qui est écarté, et pourquoi

Consigné pour que la question ne se repose pas.

**Le dé et la table aléatoire.** Contrainte V1.0. La table du §4.3 est indexée par le rapport et
rien d'autre. La V1.1 rebranchera la dispersion sans toucher au dessin — une flèche et une butée
disent la même chose, que le résultat vienne d'une table ou d'un tirage.

**Les pions d'unité — brigades, divisions, corps posés sur les hexagones.** Le modèle n'a pas
d'unités : il a huit secteurs et alloue la puissance par secteur, *jamais par unité*, et c'est écrit
au §13 du modèle. Dessiner quarante pions de brigade affirmerait une résolution qui n'existe pas et
inviterait à les déplacer — c'est-à-dire à manœuvrer.

**Les zones de contrôle, l'empilement, la retraite, l'avance après combat.** Le corps de règles du
hex-and-counter classique. Toutes sont des règles de manœuvre : chacune rend un placement habile
payant, et chacune trahit la thèse.

**Le ravitaillement tracé sur la carte, à la manière d'OCS.** Séduisant, parce que c'est le wargame
qui ressemble le plus à O'Brien. Mais notre logistique est déjà modélisée — `TransmissionRate`,
`LogisticsIntegrity`, `CoverageFloor` — de façon économique et non géographique. La dessiner créerait
deux autorités sur la même information, ce que la direction artistique interdit explicitement.

**Le brouillard de guerre par pion retourné.** Il n'y a personne à qui cacher quoi que ce soit : la
V1 se joue seule et le spectateur voit les deux tableaux de bord. À reprendre en V2, où il traduit
naturellement le §11 du modèle — un camp corrompu ne voit plus ses propres crans.

**La règle de saillant, l'appui entre secteurs voisins, la ligne comme corde sous tension.** C'est le
rejet le plus coûteux du document, parce que l'image est juste et que la carte la produit déjà
gratuitement. Mais dès qu'une règle s'y adosse, la forme du tracé devient payante : rectifier un
saillant, appuyer un voisin, exploiter une brèche. On aurait construit un jeu où l'on gagne par la
géométrie du front. **On garde le dessin, on refuse la règle**, et la ligne continue de se déformer
sans que cela ne rapporte rien à personne.

**Le radar à cinq branches et l'histogramme de secteur.** Le premier est déjà proscrit ; le second
est le défaut d'origine.

**Une seconde carte, un zoom opératif, une vue de secteur.** Le secteur est l'unité de décision : il
n'y a rien à voir en dessous, et un zoom promettrait une profondeur inexistante.

**Une bibliothèque hexagonale, un moteur de jeu, un canevas WebGL.** §3. Le gain est de quelques
dizaines de lignes, le coût est une dépendance et la perte du fonctionnement hors ligne.
