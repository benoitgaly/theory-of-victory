# Audit de réalisme du modèle

> Confrontation de ce que le moteur produit à ce que la guerre a produit, de février 2022 à
> l'été 2026. Le modèle de référence est [`01-modele-de-jeu.md`](01-modele-de-jeu.md) ; les
> effectifs sont traités séparément dans [`04-calibration-effectifs.md`](04-calibration-effectifs.md)
> et ne sont pas refaits ici.

État : audit conduit sur l'instantané publié le 13 août 2026. Aucun code modifié.

---

## 1. Méthode, et ce qu'elle ne peut pas faire

Les sorties du moteur proviennent de la page publiée
(<https://benoitgaly.github.io/theory-of-victory/>), qui embarque les trois parties complètes,
tour par tour, dans `window.tovGames`. Rien n'a été compilé ni exécuté : le code C# est en cours
de modification, et une compilation aurait capturé un état intermédiaire plutôt que celui que le
public voit. **L'audit porte donc exactement sur ce qui est en ligne**, ce qui est la bonne
référence puisque c'est ce qui est lu.

La partie de référence est **« Le soutien tient, sans plus »** (`ukraine_2022_holds`) : son
calendrier de cartes est celui de la guerre réelle — aide bloquée au T9, débloquée au T11 —
et son issue est le front figé. Les deux autres servent de contrôle.

Trois limites, qu'il faut poser avant les résultats.

**Ce que personne ne sait.** Les pertes humaines, les stocks de munitions réellement détenus et
les cadences de production effectives ne sont pas connus à mieux qu'un facteur deux. Les bilans
publiés par les belligérants sont des instruments de guerre. Cet audit ne tranche aucune de ces
trois grandeurs : il indique où passe la limite du savoir et s'arrête là.

**Ce qui n'a pas de mesure du tout.** La corruption, le coefficient de transmission, la cohésion
des élites et la volonté extérieure sont des constructions du modèle. On peut juger leur
comportement plausible ou non ; on ne peut pas le confronter à une série.

**Faux contre simplifié.** Un modèle pédagogique a le droit d'être grossier. Il n'a pas le droit
d'être faux **dans le sens qu'il démontre**. Les deux catégories sont séparées ci-dessous, et la
seconde vient en premier.

---

## 2. Synthèse par gravité

| # | Écart | Nature | Gravité |
|---|---|---|---|
| 1 | La règle du minimum ne mord jamais dans la partie de référence | **Subi** | Critique |
| 2 | La saturation des défenses ne se produit jamais, l'interception est à 100 % | **Subi** | Critique |
| 3 | Le délestage électrique n'a lieu dans aucune des trois parties | **Subi** | Critique |
| 4 | La crise des munitions de 2023-2024 n'apparaît pas | **Subi** | Critique |
| 5 | « 3 061 km² pris depuis février 2022 » — le réel est de l'ordre de 73 000 km² | Volontaire, **mal étiqueté** | Élevée |
| 6 | Volumes de frappe quatre à cinq fois trop faibles | Subi | Élevée |
| 7 | Réseau ukrainien deux fois trop grand pour sa demande | Subi | Élevée |
| 8 | Le PIB ukrainien décroît sans fin au lieu de s'effondrer puis de repartir | Subi | Moyenne |
| 9 | La décote Urals s'annule alors qu'elle s'est réélargie | Subi | Moyenne |
| 10 | Le raffinage russe se répare trop vite et n'est jamais durablement touché | Subi | Moyenne |
| 11 | Réserves souveraines russes de départ presque triples du réel liquide | Volontaire | Faible |
| 12 | La solde disparaît du tableau d'allocation : 63 % de la dépense est invisible | **Subi (bug)** | Faible mais visible |
| 13 | Les parts « du PIB » rapportent un flux trimestriel à un PIB annuel | Subi | Faible, interne |

Les quatre premières lignes forment un seul dossier : **les mécanismes que le jeu annonce comme
son cœur ne se déclenchent pas.** C'est le résultat principal de cet audit.

---

## 3. Les mécanismes centraux ne se déclenchent pas

### 3.1 La règle du minimum ne mord jamais

Le document de modèle en fait sa règle de combat unique : *ta puissance sur un secteur est celle
de ta ressource la plus rare*. La direction artistique lui consacre sa pièce maîtresse, le tonneau
de Liebig, et pose que *le goulot d'étranglement se voit avant d'être lu*.

Or, sur les dix-neuf tours de la partie de référence, et pour les deux camps :

| Partie | Tours où la couverture minimale descend sous 0,99 |
|---|---|
| Le soutien tient, sans plus | **aucun** — 0 sur 38 lectures camp-tour |
| L'Occident joue ses cartes | 3 (Russie, T17 à T19) |
| Le soutien s'arrête | 4 (Ukraine, T8 à T11) |

Dans la partie que le visiteur ouvre par défaut, la couverture vaut 1,00 pour les armes, le
carburant et la nourriture, à chaque tour, des deux côtés. Le tonneau est plein en permanence.

Conséquence directe, et c'est elle qui rend le site attaquable : **le « goulot d'étranglement »
nommé à l'écran est un artefact.** Le moteur prend le minimum de trois valeurs toutes égales à
1,00 et retient la première, c'est-à-dire les armes ; l'étiquette rouge « GOULOT » désigne donc
une ressource qui n'en est pas un. Le tour 15 la déplace sur le carburant, le tour 18 sur le
carburant encore, sans qu'aucune pénurie n'existe. Un lecteur attentif verra une douve pleine
coiffée d'un sceau qui annonce une pénurie.

**Cinq tours sur dix-neuf portent une étiquette qui désigne une ressource pleine** — le carburant
au T9, T11, T15 et T18, la nourriture au T5 — sans qu'aucune valeur ne bouge. Ce n'est pas une
erreur de calibration, c'est une règle d'affichage manquante.

*Correction recommandée* — deux gestes, l'un cosmétique, l'autre de fond.

1. `FrontPhase.ComputeCombatPower` : ne renseigner `BottleneckCode` que si la couverture la plus
   basse est inférieure à un seuil (0,97 conviendrait). Au-dessus, aucun goulot n'existe et
   l'écran doit le dire — « aucune contrainte matérielle ce trimestre » est une information, pas
   un vide.
2. Le fond est traité en 3.2 et 3.4 : si la couverture ne descend jamais, c'est que les dépôts ne
   se vident jamais.

### 3.2 La saturation ne se produit jamais, et l'interception est parfaite

Le §7 du modèle pose que *la saturation précède la pénétration* et que l'interception ne doit
jamais être un taux constant. `StrikeResolver` est écrit dans cet esprit. Le résultat produit ne
l'est pas :

| Partie | Vagues saturées | Vagues interceptées à moins de 99 % |
|---|---|---|
| Le soutien tient | **jamais** | T1 (68 %), T2 (94 %) |
| L'Occident joue ses cartes | **jamais** | T1, T2 |
| Le soutien s'arrête | **jamais** | T1, T2, puis T8 à T11 (96 %) |

À partir du tour 3, **toutes les frappes des deux camps sont interceptées à 100 %**, pendant
dix-sept tours consécutifs. Aucune vague ne sature jamais aucun magasin, dans aucune des trois
parties. Le mécanisme le plus mis en avant du jeu ne s'observe littéralement nulle part.

La réalité, elle, se situe entre 80 et 97 % selon les mois : 94 à 97 % d'août 2024 à février 2025,
puis une chute à 82 % en mai 2025 et 86 % en juin, remontée à 89 % en août. Chaque point perdu
représente des dizaines de vecteurs qui passent, et ce sont eux qui font les campagnes contre le
réseau.

**La cause est identifiée et elle est arithmétique.** Les stocks d'intercepteurs bas coût
ukrainiens passent de 11 906 unités au T1 à **534 276 unités au T19**. Ce n'est pas une dérive
lente, c'est une injection régulière :

```
aide effective              11,3 Md$ / trimestre
part livrée en nature       × 0,62              = 7,0 Md$
part « défense bas coût »   × 0,12              = 0,84 Md$
coût unitaire de référence  ÷ 0,02 M$           = 42 000 unités par trimestre
```

Contre une capacité industrielle nationale de 3 850 unités par trimestre et un plafond de dépôt
volontairement posé à six trimestres de production, soit 23 100 unités. **L'aide en nature verse
onze fois la capacité nationale, chaque trimestre, hors de tout plafond** — elle est ajoutée
directement au stock dans `RevenuePhase`, sans passer par `AllocationPhase.Order` qui porte la
règle de plafonnement. Le stock final dépasse de vingt-trois fois le plafond que le jeu s'est
lui-même fixé, et c'est ce mur d'intercepteurs qui rend toute saturation impossible.

C'est exactement le piège déjà rencontré trois fois et consigné dans
[`03-gameplay.md`](03-gameplay.md) : *on produit pour couvrir un besoin, jamais pour dépenser un
budget*. Quatrième occurrence, quatrième endroit.

*Correction recommandée* — `RevenuePhase`, les trois lignes qui convertissent `inKind` en unités :
appliquer le même plafond de dépôt que `AllocationPhase` (`StockQuartersHeld = 6` fois la capacité
installée), ou mieux, router l'aide en nature par la même fonction d'ordre que les achats, de
sorte qu'il n'existe qu'un seul endroit où un dépôt peut être rempli. Le surplus non absorbable
doit alors être converti en autre chose, ou perdu — un donateur qui livre ce que le receveur ne
peut pas stocker est un fait réel, pas une anomalie.

### 3.3 Le délestage n'a jamais lieu

Le §5 du modèle est explicite : l'électricité est *l'intrant des intrants*, le délestage est *un
seuil*, et *la saison décide* — on prépare la campagne de frappes à l'automne pour qu'elle morde
en hiver.

Résultat produit : `gridShortfall` vaut **zéro à tous les tours, pour les deux camps, dans les
trois parties**. Il n'y a jamais eu une seule coupure en dix-neuf tours. Le rythme annuel que le
modèle décrit comme *authentique* n'existe pas dans les sorties.

La cause est une marge trop confortable, et elle se chiffre :

| | Modèle | Réel |
|---|---|---|
| Capacité nominale ukrainienne | 36 GW | ~55 GW installés, mais **~21 GW réellement opérables** début 2022 |
| Demande de base | 15,5 GW | ~13 GW hors hiver |
| Demande d'hiver | 23,3 GW (× 1,5) | ~18 GW de pointe |
| Marge avant seuil | **2,3 ×** hors hiver, 1,5 × en hiver | **~1,2 ×** en pointe d'hiver |
| Dégâts permanents atteints | 12,1 GW au maximum (T13) | 9 GW perdus pour la seule année 2024 ; 17,5 GW opérables en 2025, ~11,5 GW en février 2026 |

Le modèle inflige donc des dégâts d'ampleur réaliste — 12 GW, c'est l'ordre de grandeur observé —
à un réseau dimensionné pour les absorber. Avec 36 GW installés contre 23,3 GW de demande
hivernale, il faudrait détruire 12,7 GW **avant même** que la première coupure n'apparaisse, et le
plafond de perte permanente du modèle (`MaxPermanentLossShare = 0,55`) autorise au plus 19,8 GW.
Le seuil est atteignable en théorie et jamais atteint en pratique.

La réalité est l'inverse : dès l'automne 2022, les frappes ont produit des coupures nationales ;
en 2024, l'Ukraine a perdu environ 9 GW, soit un tiers de sa consommation d'avant-guerre ; à
l'hiver 2025-2026, Kiev a connu jusqu'à seize heures par jour sans électricité, après plus de
612 attaques sur les infrastructures énergétiques dans l'année.

*Correction recommandée* — `UkraineScenario.BuildUkraine`, deux constantes :

```
ukraine.Grid.NominalCapacityGw = 36   →  22     // capacité opérable, pas installée
ukraine.Grid.BaseDemandGw      = 15.5 →  13
                                                // hiver ×1,45 → 18,9 GW contre 22 disponibles
                                                // marge 14 % : trois gigawatts de dégâts mordent
```

Avec ces valeurs, la marge passe de 55 % à 14 % en pointe d'hiver, et une campagne soutenue —
celle que les cartes `grid_campaign` déclenchent déjà aux tours 5, 9 et 13 — franchit le seuil au
premier hiver, exactement comme en 2022. La règle du seuil devient enfin démontrable : les mêmes
dégâts ne coûtent rien en juillet et provoquent une crise nationale en janvier.

Le réseau russe, lui, est correctement modélisé comme insensible — 245 GW pour 148 GW de demande.
C'est fidèle : ce qui est vulnérable en Russie, ce n'est pas l'électricité, c'est le raffinage.

### 3.4 La crise des munitions de 2023-2024 n'apparaît pas

Le §18 du modèle pose son propre critère de validation : faire tourner le scénario depuis
février 2022 et vérifier que le modèle retrouve, **sans y être forcé**, l'échec de la poussée
initiale, *la crise des munitions de fin 2023*, le grignotage de 2024-2025 et les campagnes
hivernales contre le réseau.

Deux de ces quatre inflexions ne sont pas retrouvées : la crise des munitions et les campagnes
hivernales (traitée en 3.3).

Dans la partie de référence, la carte `aid_blocked` tombe au T9 et `aid_unblocked` au T11. L'effet
est visible sur le dépôt ukrainien — 1 141 unités au T9, 795 au T10, 458 au T11 — et **nul sur la
couverture**, qui reste à 1,00 tout du long. Le dépôt a absorbé le choc sans que le front s'en
aperçoive. Or c'est précisément l'inverse qui s'est produit : le rationnement ukrainien a fait
tomber la cadence de tir de 6 000 coups par jour à 2 000, soit un rapport de un à trois avec
l'artillerie russe, et c'est ce déséquilibre qui a permis la prise d'Avdiïvka en février 2024.

La latence que le modèle cherche à produire — *rien pendant deux tours, puis tout cède* — n'est
donc obtenue que dans la variante `collapse`, où l'aide tombe à zéro pour de bon. Une réduction
temporaire ne produit rien du tout. C'est une perte pédagogique nette : le blocage de l'aide
américaine d'octobre 2023 à avril 2024 est l'événement qui illustre le mieux la thèse du jeu, et
le moteur le traverse sans frémir.

*Correction recommandée* — c'est le même correctif qu'en 3.2, appliqué aux armes : tant que
l'aide en nature remplit les dépôts sans plafond, un dépôt ne peut pas se vider. Une fois le
plafond appliqué, le stock de départ de 2 400 unités — l'héritage soviétique, voulu et justifié —
reste suffisant pour porter 2022, et un blocage de deux tours mord au troisième.

---

## 4. Chiffres affichés à confronter

### 4.1 Le territoire : le nombre est honnête, l'étiquette ne l'est pas

La page affiche, littéralement : **« 3 061 km² pris depuis février 2022 »** (variante par défaut ;
4 107 km² dans la variante « le soutien tient »).

Le réel, au 1ᵉʳ janvier 2026 : la Russie occupe **116 165 km²**, soit 19,25 % de l'Ukraine. Elle
en tenait de l'ordre de 43 000 avant février 2022 — Crimée (~27 000 km²) et Donbass occupé
(~16 000 km²). **Le gain net depuis février 2022 est donc de l'ordre de 73 000 km²**, avec un pic
à environ 27 % du pays en mars 2022, puis un reflux : la seule contre-offensive de Kharkiv a repris
12 000 km² en septembre 2022. Le modèle chiffre l'ensemble des contre-offensives ukrainiennes de
2022 à **−352 km²**.

L'écart est d'un facteur vingt-quatre sur le total et de trente sur les contre-offensives. Aucun
lecteur informé ne le laissera passer.

**Et pourtant le nombre n'est pas faux — il mesure autre chose que ce qu'il annonce.** Deux
raisons, toutes deux structurelles et parfaitement défendables :

- **Le front modélisé fait 480 km**, pas 1 200. Huit secteurs de 5 à 7 hexagones de large,
  48 hexagones à 10 km, tous situés sur l'arc Kharkiv-Kherson. Le reste de la ligne de contact
  n'existe pas dans le modèle.
- **Le jeu ne simule pas la phase de manœuvre de 2022.** Il démarre sur la ligne de contact de
  février 2022 et applique immédiatement une table de mouvement plafonnée à trois hexagones par
  trimestre. Les quelque 120 000 km² qui ont bougé pendant les dix mois de manœuvre de 2022 ne
  sont ni pris ni repris : ils sont hors du domaine de validité du modèle, et c'est assumé — le
  jeu porte sur la guerre d'usure, pas sur l'invasion.

Rapporté au front qu'il modélise, le rythme du moteur est du bon ordre : 4 107 km² sur 480 km de
front et 4,75 ans font **1,8 km de profondeur par an**, contre 3,3 km/an si l'on rapporte les
4 000 km²/an réels à toute la ligne de contact, et davantage si l'on ne compte que les secteurs
réellement actifs. Le modèle est lent d'un facteur deux à cinq sur le front actif — un écart de
calibration ordinaire, pas une erreur de nature.

*Correction recommandée* — la moins coûteuse et la plus honnête est **une correction d'étiquette,
pas de physique**. Dans `board.js`, remplacer « pris depuis février 2022 » par une formule qui dit
ce qui est mesuré, par exemple « pris sur les huit secteurs simulés depuis le début de la partie ».
Si l'on veut conserver une grandeur comparable au réel, ajouter une constante de scénario portant
la position de départ (le territoire occupé au 24 février 2022) et afficher un total, en précisant
qu'il est reconstitué. La pire option serait d'accélérer l'avance pour retrouver 73 000 km² : cela
casserait la table de mouvement, qui est juste, pour réparer un libellé.

### 4.2 Les volumes de frappe sont quatre à cinq fois trop faibles

| Grandeur | Modèle (par trimestre) | Réel | Écart |
|---|---|---|---|
| Drones de frappe russes lancés | 2 300 à 2 460 | ~10 400 (plus de 38 000 de janvier à novembre 2025) | **× 4,2** |
| Missiles russes lancés | 110 à 124 | de l'ordre de 600 sur les trimestres de grandes vagues | **× 5** |
| Drones de frappe ukrainiens | 680 à 2 180 | plusieurs milliers, en forte croissance en 2025 | × 2 à 4 |

Le modèle fait donc lancer à la Russie vingt-six drones par jour là où elle en lançait cent
soixante-dix à l'été 2025. Comme la production suit la consommation dans le moteur, l'écart n'est
pas une pénurie modélisée : c'est un calibrage de capacité industrielle.

Cet écart aggrave celui de la section 3.2 : peu de vecteurs face à un mur d'intercepteurs, la
saturation devient doublement impossible. Corriger la capacité sans corriger le plafond de dépôt
ne servirait à rien ; corriger les deux ensemble rend le mécanisme opérant.

*Correction recommandée* — `UkraineScenario`, capacités de production de `StrikeDrones` : Russie
900 → 3 000 par trimestre, Ukraine 700 → 1 800, en laissant le plafond d'expansion à 3,5 ×. Pour
les missiles, l'écart d'unité signalé au §11 de [`05-composantes-armee.md`](05-composantes-armee.md)
doit être tranché d'abord : 130 unités par trimestre valent 520 par an, contre environ
2 500 missiles commandés pour 2025.

### 4.3 Le PIB ukrainien décroît quand il devrait s'effondrer puis repartir

Le modèle fait décroître le PIB ukrainien de façon monotone : 193 Md$ au T1, 147 Md$ au T19, soit
−24 % étalés sur dix-neuf trimestres, sans aucune inflexion.

Le réel a une tout autre forme : **−28,8 % en 2022**, une chute brutale en une seule année, puis
+5,5 % en 2023, +2,9 % en 2024, +2,1 % en 2025 — soit 190,7 Md$ en 2024 et 214 Md$ en 2025,
au-dessus du niveau d'avant-guerre en dollars courants.

Le modèle se trompe donc à la fois sur la forme (une pente au lieu d'une falaise suivie d'une
reprise) et sur le signe des quatre dernières années. C'est d'autant plus dommage que la
« signature du modèle » revendiquée au §1 est précisément *un effet est un seuil, jamais une
pente*. Ici, le modèle applique une pente là où la réalité offrait un seuil parfait.

*Correction recommandée* — `ukraine.Economy.CivilianGrowthPerTurn = −0,02` est une décroissance
permanente de 2 % par trimestre. La remplacer par un choc initial concentré sur les quatre
premiers tours puis une croissance légèrement positive reproduirait la vraie courbe. C'est une
carte de scénario plutôt qu'une constante : un « effondrement de guerre » au T1-T4, puis le retour
à `+0,005`. Cela renforce la thèse du jeu au lieu de l'affaiblir — une économie de guerre qui
repart est un fait plus intéressant qu'une économie qui s'étiole.

---

## 5. Écarts de calibration à resserrer

### 5.1 La décote Urals s'annule au lieu de se réélargir

La décote implicite du modèle, calculée en divisant la recette pétrolière par les volumes
exportés, tombe de **14,6 $ le baril au T2 à 1,2 $ au T19**. Elle disparaît, parce que
`SanctionsPriceDelta` s'érode selon la règle voulue — *sanctionner n'est pas un acte, c'est un
entretien* — et parce qu'aucune carte ne la resserre après le T13. La friction sanctionnaire
tombe elle aussi à 0,00 dès le T13.

Le réel : la décote Urals était de 30 à 35 $ en 2022-2023, s'est resserrée à 10-12 $ en 2024, puis
**s'est réélargie à environ 20 $ — près de 23 % sous le Brent — en novembre 2025**, après
l'inscription de Rosneft et Loukoïl sur la liste noire américaine.

Le modèle produit donc, sur les deux dernières années, l'inverse du mouvement observé. La règle
d'érosion est juste ; ce qui manque, c'est le durcissement de 2025, qui n'a pas de carte. À noter
également : au T1, la décote implicite est nulle alors qu'elle était déjà de 25 à 30 $ au premier
trimestre 2022.

*Correction recommandée* — une carte de scénario au T15 ou T16 (`major_oil_sanctions`, effet
`SanctionsPriceDelta` de +0,5 sur l'envahisseur) plutôt qu'un changement de constante : le fait
historique est un événement daté, pas un paramètre. Et poser une décote initiale non nulle dans
`SanctionsRegime`, puisqu'elle existait dès le premier tour.

### 5.2 Le raffinage russe se répare trop vite

`RefiningIntegrity` ne descend qu'à 87 % (T10), remonte à 92 % au tour suivant et à 100 % en trois
tours. Sur dix-neuf tours, le raffinage russe est intact seize fois.

Le réel : 17 % de la capacité de raffinage russe hors service en août 2025 ; **38 % des unités de
distillation primaire à l'arrêt au 28 septembre 2025** ; environ un quart encore à l'arrêt en
mai 2026 ; deux interdictions d'exportation d'essence, en avril 2025 et au printemps 2026.

Le modèle sous-estime donc l'ampleur d'un facteur deux à trois, et surtout la durée : là où la
réalité montre une dégradation entretenue sur plus d'un an, il montre deux creux transitoires.
`RefiningRepairPerTurn = 0,4` — 40 % du dommage réparé par trimestre — est trop généreux au regard
des délais réels d'approvisionnement en équipements de raffinage sous sanctions.

*Correction recommandée* — `UkraineScenario` : `russia.Economy.RefiningRepairPerTurn` de 0,4 à
0,18, et dans `DeepStrikePhase.ApplyDamage`, le coefficient `damage × 0,09` porté à 0,15 pour la
cible `Refining`. Le paramétrage actuel a l'inconvénient de rendre inutile la carte
`refinery_campaign_sustained`, qui est pourtant l'arme centrale de la variante victorieuse.

### 5.3 Les réserves souveraines de départ

Le modèle démarre la Russie à 310 Md$ de réserves et la conduit à 40 Md$ au T19. La forme est
juste — le fonds est réellement liquidé, et c'est la correction majeure consignée dans
[`03-gameplay.md`](03-gameplay.md). Le niveau de départ correspond au fonds souverain **total**,
part illiquide et avoirs gelés compris. La part réellement mobilisable début 2022 était de l'ordre
de 113 Md$, et c'est elle qui s'est épuisée.

Écart volontaire ou non, il est sans conséquence sur la démonstration puisque `ReserveDrawRate`
compense en limitant la ponction à 12 % par trimestre. Signalé pour mémoire, correction non
prioritaire.

### 5.4 La solde disparaît du tableau d'allocation

Observation vérifiable dans les données publiées : la ligne `payroll` est **absente des dix-neuf
tours, dans les trois parties**. Au dernier tour de la partie de référence, les lignes affichées
totalisent 16,0 Md$ contre une dépense militaire de 43,3 Md$. **Soixante-trois pour cent de la
dépense militaire russe n'apparaît nulle part** dans la vue économique.

Le montant est pourtant calculé et débité : 668 000 hommes à 0,056 Md$ les mille font environ
37 Md$ par trimestre. C'est de loin la première ligne du budget, et le commentaire du code le dit
lui-même. Elle est écrite dans `AllocationThisTurn` par `PayTroops`, puis effacée quelques lignes
plus bas par le `Clear()` qui prépare la ventilation discrétionnaire.

*Correction recommandée* — `AllocationPhase.Allocate` : déplacer `AllocationThisTurn.Clear()`
avant l'appel à `PayTroops`. Un caractère de diff, et la vue économique retrouve les deux tiers
manquants. C'est aussi ce qui explique visuellement pourquoi le budget discrétionnaire ukrainien
tombe à 0,1 Md$ par trimestre en fin de partie : la solde absorbe tout, mais l'écran ne montre que
le résidu, ce qui donne l'impression d'un pays qui n'achète plus rien sans dire pourquoi.

### 5.5 Les parts « du PIB » sont des trimestres divisés par des années

`WarBudgetCeilingShare = 0,038` est documenté comme une part du PIB. Il rapporte une dépense
trimestrielle à un PIB annuel : la part réelle d'effort de guerre du modèle est donc quatre fois
celle qu'il affiche, soit environ 9 % du PIB et non 2,3 %. Même remarque pour
`Economy.WarEffortShare`.

Cela n'a **aucune conséquence sur les sorties** — le plafond fonctionne, il est simplement mal
nommé — et le site n'affiche jamais ce ratio, seulement des montants. Mais toute personne lisant
le code y verra une erreur, et le jour où l'on voudra afficher « part du PIB consacrée à la
guerre », le chiffre sortira faux d'un facteur quatre.

*Correction recommandée* — renommer en `WarBudgetCeilingShareOfQuarterlyGdp`, ou diviser le PIB
par quatre au point d'usage. À faire avant, et non après, que le chiffre n'atteigne l'écran.

---

## 6. Ce que le modèle réussit

Un audit qui ne trouverait que des fautes serait suspect. Plusieurs grandeurs sont bien calibrées,
et certaines remarquablement.

**Les obus, qui sont le cœur matériel du jeu, sont justes.** La capacité russe passe de 700 à
1 774 unités par trimestre, soit de 2,8 à **7,1 millions de coups par an** — le renseignement
estonien chiffre la production russe 2025 à environ 7 millions d'obus, dont 3,4 millions de
calibres d'artillerie. La consommation russe modélisée, 4,2 millions par an, encadre correctement
les ~10 000 coups par jour observés en 2024-2025. Le plafond d'expansion à 3,5 fois le niveau
d'avant-guerre, posé après coup pour brider une boucle non bornée, se révèle avoir été posé au bon
endroit.

**Les recettes pétrolières sont du bon ordre.** Le modèle produit 186 Md$ la première année et
115 Md$ la dernière ; les recettes budgétaires pétro-gazières russes sont passées de 11,6 billions
de roubles en 2022 (~170 Md$) à 8,8 billions en 2023 (~100 Md$) puis 8,32 billions attendus en
2025 (~104 Md$). La trajectoire et les niveaux concordent. *Réserve d'interprétation* : si
`oilRevenue` est lu comme des **recettes d'exportation** et non comme la part budgétaire, il est
deux fois trop bas. Il faudrait trancher lequel des deux il représente et le dire à l'écran.

**La dépense militaire russe est presque exacte** : environ 185 Md$ par an au rythme des derniers
tours, contre 190 Md$ estimés pour 2025.

**Le PIB russe est bien tenu** : +11 % nominal sur la période modélisée, contre environ +7 à +8 %
de croissance réelle cumulée (−1,2 % en 2022, +3,6 % en 2023, +4,1 % en 2024, ~+1 % en 2025). Le
double compteur — PIB apparent qui monte, capacité productive qui baisse de 13,5 % — est une
construction du modèle qu'aucune série ne peut valider, mais elle raconte le bon phénomène.

**L'aide occidentale est bien dimensionnée** : 11,3 Md$ par trimestre, soit environ 45 Md$ par an,
contre une moyenne de 41,6 Md€ par an alloués entre 2022 et 2024 et 32,5 Md€ en 2025. Le mécanisme
de conditionnalité et de décaissement partiel (`DisbursementRate`, `Conditionality`) est
exactement la bonne forme : la réalité est bien un écart entre le promis et le versé.

**La géographie du front est juste.** À la fin de la partie de référence, le secteur le plus
enfoncé est **Pokrovsk** (5,9 hexagones), suivi de Bakhmout-Tchassiv Iar (1,2), les six autres
étant figés ou en léger recul. C'est la hiérarchie réelle : près de 80 % des gains russes de 2025
se sont concentrés sur six axes dont Pokrovsk, et le Donbass est le seul théâtre qui bouge. Le
modèle retrouve seul cette concentration, à partir des seules pondérations de doctrine.

**La forme de l'issue est juste.** Un front figé quand les deux camps régénèrent, un effondrement
brutal quand un flux est coupé, une victoire par asphyxie financière sans gain de terrain : ce sont
les trois enseignements que le jeu veut transmettre, et le moteur les produit.

---

## 7. Ce que cet audit ne peut pas trancher

| Grandeur | Pourquoi elle échappe |
|---|---|
| **Pertes humaines** | Les bilans des belligérants sont gonflés d'un facteur deux à trois sur l'adversaire et minorés sur soi. Le modèle produit 685 000 pertes russes cumulées contre 208 000 ukrainiennes ; le CSIS estime 1,4 million contre 525 000 à 625 000. Le rapport modélisé (0,30) est du même ordre que le rapport estimé (~0,42), ce qui est le seul contrôle possible. Voir [`04-calibration-effectifs.md`](04-calibration-effectifs.md) §5 |
| **Stocks de munitions réellement détenus** | Aucun des deux camps ne publie, et les estimations occidentales portent sur les flux, pas sur les dépôts. Le stock ukrainien de départ (2 400 unités, l'héritage soviétique) est une hypothèse de travail assumée |
| **Cadences de production réelles** | Les chiffres russes viennent du renseignement estonien ou ukrainien ; ceux de l'Ukraine sont classifiés. Le facteur d'incertitude est d'au moins deux |
| **Part en ligne de contact** | Le paramètre le plus faible de tout le modèle, à ±30 % — déjà signalé comme tel dans `04` |
| **Corruption, transmission, cohésion des élites, volonté extérieure** | Constructions sans référent mesurable. On peut juger leur comportement, pas leur niveau |
| **Prix effectif du baril russe** | La décote Urals est publiée par plusieurs agences avec des écarts de 5 à 10 $ selon la méthode et le point de livraison. Les ordres de grandeur du §5.1 restent robustes ; le détail au dollar près, non |

---

## 8. Plan de correction, par ordre de rendement

Les quatre premières corrections font toutes partie du même dossier — les dépôts qui ne se vident
jamais — et se tiennent : appliquées ensemble, elles rendent opérants les trois mécanismes que le
jeu annonce et rétablissent le critère de validation du §18.

| Ordre | Correction | Fichier | Geste |
|---|---|---|---|
| 1 | Plafonner l'aide en nature au plafond de dépôt | `Engine/Phases/RevenuePhase.cs` | Router les trois `Stock.Add` par la règle de `AllocationPhase.Order`, plafond `StockQuartersHeld = 6` |
| 2 | Réduire la marge du réseau ukrainien | `Engine/Scenarios/UkraineScenario.cs` | `NominalCapacityGw` 36 → 22 ; `BaseDemandGw` 15,5 → 13 |
| 3 | Relever les capacités de vecteurs de frappe | `Engine/Scenarios/UkraineScenario.cs` | `StrikeDrones` : Russie 900 → 3 000, Ukraine 700 → 1 800 |
| 4 | Ne nommer un goulot que s'il en existe un | `Engine/Phases/FrontPhase.cs` | Seuil à 0,97 sur `BottleneckCode` |
| 5 | Corriger l'étiquette du territoire | `Web/wwwroot/js/board.js` ligne 1227 | « pris sur les secteurs simulés depuis le début de la partie » |
| 6 | Rendre la solde visible | `Engine/Phases/AllocationPhase.cs` | Déplacer `AllocationThisTurn.Clear()` avant `PayTroops` |
| 7 | Ralentir la réparation du raffinage | `Engine/Scenarios/UkraineScenario.cs`, `DeepStrikePhase.cs` | `RefiningRepairPerTurn` 0,4 → 0,18 ; dommage `× 0,09` → `× 0,15` |
| 8 | Donner sa forme au PIB ukrainien | `Engine/Scenarios/UkraineScenario.cs` + une carte | Choc concentré en 2022, croissance légère ensuite |
| 9 | Ajouter le durcissement des sanctions de 2025 | `Engine/data/cards.fr.json` | Carte au T15-T16, `SanctionsPriceDelta` +0,5 |
| 10 | Nommer correctement les parts de PIB | `Core/Economy.cs` | Renommage, ou division par quatre au point d'usage |

**Après les corrections 1 à 3, les trois issues devront être revérifiées tour par tour** : ce sont
les seules qui touchent la dynamique. Les corrections 1 et 2 vont dans le sens d'une guerre plus
dure pour l'Ukraine ; il est probable qu'il faille relever son stock de départ ou l'aide pour
retrouver la chute au T10 de la variante `collapse` et le front figé de la variante `holds` aux
tours qu'ils occupent aujourd'hui.

---

## 9. Sources

Territoire :

- [La Russie a pris plus de 4 300 km² en 2025 — DeepState, via The Kyiv Independent](https://kyivindependent.com/russia-captured-over-4-300-square-kilometers-of-ukraine-in-2025-deepstate-reports/)
- [Bilan territorial au 1ᵉʳ janvier 2026 : 116 165 km² occupés, 19,25 % du pays — Russia Matters](https://www.russiamatters.org/news/russia-ukraine-war-report-card/russia-ukraine-war-report-card-dec-23-2025)
- [3 604 km² pris en 2024 — ISW](https://www.yahoo.com/news/russian-forces-suffered-more-420-113719773.html)
- [Contre-offensive de Kharkiv : 12 000 km² repris en septembre 2022 — Wikipedia](https://en.wikipedia.org/wiki/2022_Kharkiv_counteroffensive)
- [Cartographie des gains territoriaux et pic d'occupation de mars 2022 — Al Jazeera](https://www.aljazeera.com/news/2026/2/24/mapping-russian-attacks-and-territorial-gains-across-ukraine)

Munitions :

- [Production russe de 7 millions d'obus par an, renseignement estonien — Euromaidan Press](https://euromaidanpress.com/2026/02/11/russian-shell-production-reaches-7-million-annually-17-times-pre-invasion-levels-estonian-intelligence/)
- [Ventilation de la production 2025 et livraisons nord-coréennes — Defense Express](https://en.defence-ua.com/news/in_2025_russia_broke_its_ammunition_output_record_producing_7m_shells_worth_106b-17489.html)
- [La Corée du Nord réduit de moitié ses livraisons — bne IntelliNews](https://www.intellinews.com/north-korea-halves-shell-supply-to-russia-as-stockpiles-run-low-says-ukraine-411593/)

Énergie :

- [Bilan d'avant-hiver du système énergétique ukrainien — Agence internationale de l'énergie](https://www.iea.org/reports/ukraines-energy-security/a-pre-winter-assessment)
- [Capacité perdue, coupures jusqu'à seize heures par jour — CEPA](https://cepa.org/article/a-rebirth-in-flame-ukraines-beleaguered-energy-system/)
- [État et perspectives du système électrique ukrainien — The Ukrainian Review](https://theukrainianreview.info/power-outages-as-the-new-reality-the-state-and-prospects-of-ukraines-energy-system/)

Frappes et interception :

- [Analyse mensuelle du déploiement des Shahed contre l'Ukraine en 2025 — Institute for Science and International Security](https://isis-online.org/isis-reports/a-comprehensive-analytical-review-of-russian-shahed-type-uavs-deployment-against-ukraine-in-2025)
- [Taux d'interception et montée en puissance des intercepteurs — Forbes](https://www.forbes.com/sites/davidhambling/2025/08/01/russia-ramps-up-shahed-attacks-but-interceptors-take-them-down/)
- [Coût des missiles russes : Kalibr, Iskander, Kh-101 — Militarnyi](https://militarnyi.com/en/articles/from-kalibr-to-kinzhal-how-much-do-russian-missiles-really-cost/)

Économie et pétrole :

- [Recettes pétro-gazières russes 2025, −25 % — The Moscow Times](https://www.themoscowtimes.com/2025/12/03/russias-oil-and-gas-revenues-fall-34-year-on-year-in-november-a91328)
- [Décote Urals élargie à 20 $ après l'inscription de Rosneft et Loukoïl — Forbes](https://www.forbes.com/sites/rrapier/2025/11/28/russias-oil-revenues-are-falling-fast-as-fiscal-pressure-mounts/)
- [Budget militaire russe 2026 — SIPRI](https://www.sipri.org/publications/2026/sipri-insights-peace-and-security/budget-fifth-year-war-military-spending-russias-budget-2026)
- [Croissance du PIB ukrainien après la chute de 28,8 % en 2022 — Banque mondiale](https://data.worldbank.org/indicator/NY.GDP.MKTP.KD.ZG?locations=UA)

Raffinage :

- [17 % de la capacité de raffinage russe hors service, août 2025 — Militarnyi](https://militarnyi.com/en/news/reuters-ukrainian-strikes-on-10-oil-refineries-knock-out-17-of-russia-s-refining-capacity/)
- [38 % des unités de distillation primaire à l'arrêt, septembre 2025 — Militarnyi](https://militarnyi.com/en/news/drone-strikes-halt-nearly-40-of-russia-s-oil-refining-capacity/)
- [Crise des carburants 2025-2026 — Wikipedia](https://en.wikipedia.org/wiki/2025%E2%80%932026_Russian_fuel_crisis)

Aide occidentale :

- [Ukraine Support Tracker, quatre ans de guerre — Institut de Kiel](https://www.kielinstitut.de/publications/news/ukraine-support-after-4-years-of-war-europe-steps-up/)
- [L'Europe ne compense pas le retrait américain — Institut de Kiel](https://www.kielinstitut.de/publications/news/ukraine-support-tracker-europe-fails-to-offset-us-aid-drop/)

---

## 10. Conclusion

Le modèle est **plus juste sur les grandeurs qu'il calcule que sur les mécanismes qu'il
revendique**. Les obus, les recettes pétrolières, la dépense militaire, le PIB russe, l'aide
occidentale et la géographie du front sont bien calibrés, parfois à quelques pour cent près. Mais
la règle du minimum ne mord jamais, la saturation ne se produit jamais, le délestage n'a jamais
lieu, et la crise des munitions que le modèle s'était donné pour critère de validation n'apparaît
pas.

Ces quatre défauts ont **une cause unique et corrigible** : des dépôts que l'aide en nature
remplit sans plafond, et un réseau électrique dimensionné pour ne jamais tomber. Aucun d'eux ne
tient à une erreur de conception — les règles sont bonnes, ce sont les conditions de leur
déclenchement qui ne sont jamais réunies.

Le seul chiffre qui rende aujourd'hui le site franchement attaquable est le territoire, et c'est
le plus facile à traiter : le nombre est honnête, l'étiquette ne l'est pas. Un lecteur informé qui
lit « 3 061 km² pris depuis février 2022 » n'ira pas plus loin, et n'apprendra jamais que le
modèle a raison sur les obus.
