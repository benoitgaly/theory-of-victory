# Les cinq composantes d'armée

> Spécification de conception. Le modèle faisant autorité reste
> [`01-modele-de-jeu.md`](01-modele-de-jeu.md) : ce document s'y branche, ne le remplace pas et
> n'en contredit aucune règle. Vocabulaire, phases et ressources sont ceux du moteur existant.

État : conception arrêtée, calibration à faire. Aucun code écrit.

---

## 1. Pourquoi cinq composantes plutôt qu'un total

Le moteur agrège aujourd'hui la puissance en un seul nombre par camp,
`SustainableCombatPower`, plafonné par la ressource la plus rare. C'est juste pour le front, et
c'est faux pour la guerre : cela laisse entendre qu'une armée est une masse, et qu'une masse plus
grande est une armée plus forte. Or l'argument central d'O'Brien est l'inverse — ce sont l'air,
les missiles et la mer qui décident du sort d'une guerre industrielle, et l'infanterie est le
dernier maillon, pas le premier.

Trois raisons de découper, et une seule suffirait :

**Les composantes ne sont pas substituables.** Un million d'hommes ne remplace pas une défense
antiaérienne. Aucune quantité de fantassins ne rouvre un port. Additionner des composantes, ou
les faire se compenser, c'est affirmer le contraire de la thèse du jeu.

**Elles ne se reconstituent pas au même rythme.** Un drone se remplace en jours, un missile en
semaines, un soldat en deux trimestres, un avion en trois ans, un navire jamais à l'échelle d'une
partie. Une armée n'est donc pas un stock mais **cinq stocks à cinq horloges différentes**, et
c'est cette différence d'horloge qui décide de ce qu'on peut se permettre de perdre.

**Elles agissent les unes sur les autres, jamais côte à côte.** Les drones vident les magasins
d'intercepteurs, les missiles passent dans la brèche, l'aviation ne vole que là où la défense a
déjà cédé, la terre n'avance que si les trois ont travaillé, et la mer ne touche jamais le front —
elle coupe l'argent. Une composante n'ajoute pas de la puissance : elle **ouvre ou ferme une porte**
pour une autre. C'est la traduction exacte, à l'échelle de l'armée, de ce que le modèle dit déjà
des ressources : *ta puissance est celle de ta ressource la plus rare*.

> **Règle de conception directrice.** Une composante n'est jamais un terme d'addition. Elle est
> soit un **parc** (un capital, avec son horloge de reconstitution), soit une **porte** (une
> condition d'emploi d'une autre composante). Tout ce qui ne rentre pas dans l'une de ces deux
> cases ne mérite pas d'être modélisé.

Cela interdit d'emblée le pentagone de statistiques et le graphique en radar : une aire de radar
se compense, donc elle dit exactement le mensonge qu'on veut éviter.

---

## 2. Ce que le moteur possède déjà

Trois des cinq composantes existent, sans porter leur nom. La spécification les nomme et complète.

| Composante | Ce qui existe aujourd'hui | Ce qui manque |
|---|---|---|
| **Terre** | `Manpower`, `FrontPhase`, `SustainableCombatPower` | Rien de structurel — seulement la porte de préparation (§6) |
| **Drones** | `ResourceKind.StrikeDrones`, `Innovation.TacticalDroneEdge`, `CheapInterceptors` | L'unification des trois emplois sous un seul parc |
| **Missiles** | `ResourceKind.Missiles`, `StrikeResolver`, `MissilePermanentShare` | Le plafond de tir lié aux plateformes (dont navales) |
| **Air** | `AirDefenceSystem` (le volet défensif uniquement) | Tout le volet offensif : parc, porte d'usure, pertes définitives |
| **Mer** | **rien** | Tout — et c'est là que se trouve la meilleure démonstration du jeu |

Le travail utile est donc concentré : **la mer d'abord, l'aviation ensuite, le reste est de la
mise en forme.** L'étape 1 du chemin d'implémentation (§10) livre à elle seule l'argument le plus
frappant du jeu.

---

## 3. Anatomie commune d'une composante

Chaque composante porte les mêmes six attributs. C'est ce qui la rend comparable aux quatre
autres, et lisible sur une seule pièce à l'écran.

| Attribut | Type | Rôle |
|---|---|---|
| `Park` | unités | Le capital détenu. En navires, avions, milliers d'hommes, unités de vecteur |
| `UnitValueMillions` | M$ | Ce qu'une unité vaut. Fixe le rapport d'échange face aux autres composantes |
| `ReplacementTurns` | tours | Délai de remplacement d'une unité perdue. **L'attribut décisif** |
| `UpkeepShare` | part/tour | Fraction de la valeur du parc payée chaque tour pour le garder vivant |
| `Readiness` | 0..1 | Part du parc réellement employable ce trimestre, dérivée des flux consommés |
| `Engagement` | 0..1 | Part du parc engagée ce trimestre — décidée par la doctrine |

### 3.1 Ce que chaque composante consomme et produit

Consommations exprimées dans les ressources existantes (`ResourceKind`) et dans le budget
(`Doctrine`). Les valeurs sont des **ordres de grandeur de travail**, au sens du §18 du modèle.

| Composante | Consomme par tour | Produit | Coût unitaire | Reconstitution | Entretien |
|---|---|---|---|---|---|
| **Terre** | Armes · Carburant · Nourriture · solde | Tenue et prise de terrain | 0,022 à 0,056 Md$ / 1 000 hommes et par trimestre (déjà dans `Manpower.UpkeepCostPerThousand`) | 1 à 2 tours (formation) | La solde est l'entretien |
| **Drones** | Argent · capacité industrielle · innovation (entretien de l'avance) | Saturation, interdiction tactique, interception basse, frappe navale | 0,02 à 0,25 M$ | **0 à 1 tour** | Quasi nul : un parc de drones non employé ne coûte rien |
| **Missiles** | Argent · composants sous sanctions · capacité industrielle | Dégâts **permanents** sur cibles durcies | 1,6 M$ | 1 tour, mais plafond de capacité dur | Faible, stockage |
| **Air** | Carburant · munitions guidées · pilotes · maintenance lourde | Appui-feu au sol, à condition que la porte soit ouverte | 45 M$ l'avion | **10 à 12 tours** — hors horizon | 4 % de la valeur du parc par tour, **même au sol** |
| **Mer** | Carburant · entretien de coque · bases · escorte | Blocus adverse, plateformes de tir de missiles, protection du corridor propre | 90 M$ le navire (moyenne d'un parc réel) | **12 à 20 tours** — définitivement hors horizon | 5 % de la valeur du parc par tour |

Deux lignes de ce tableau portent toute la leçon, et il faut qu'elles soient lisibles à l'écran
sans commentaire : **le drone se reconstitue en zéro tour et ne coûte rien à garder ; le navire
ne se reconstitue jamais et coûte cher à ne pas utiliser.** Le reste du tableau est du décor
autour de cette opposition.

### 3.2 Régénérable contre définitif

Le modèle a déjà cette distinction pour le réseau électrique — sous-stations réparables contre
salles des machines perdues. Elle se généralise ici sans rien inventer :

- **Composantes régénérables** — terre, drones, missiles. Une perte se comble, la question est le
  débit. Elles entrent dans le ratio de génération de force (§7).
- **Composantes de capital** — air, mer. Une perte est définitive à l'échelle de la partie. Elles
  n'entrent **pas** dans le ratio de génération (sinon un camp qui perd un navire s'effondrerait
  au tour suivant, ce qui est absurde), mais dans un compteur distinct : **le capital détruit**.

Cette séparation est ce qui autorise la démonstration navale : l'Ukraine détruit du capital russe
irremplaçable avec du consommable qu'elle refabrique en une semaine. Ce n'est pas un échange
avantageux, c'est un échange **d'une nature différente**.

---

## 4. La matrice des interactions

Le cœur de la mécanique. Chaque case est un effet exercé, jamais une addition de puissance. Le
rapport d'échange indique ce que coûte à l'agresseur un euro de dégât infligé — c'est la seule
mesure honnête d'une composante.

| Agit ↓ · sur → | Terre | Air (défense) | Air (offensif) | Mer | Missiles | Économie |
|---|---|---|---|---|---|---|
| **Drones** | Interdiction tactique : renchérit toute attaque, des deux côtés | **Sature les magasins**, vide les intercepteurs chers | — | **Coule le parc naval** | — | — |
| **Missiles** | — | Épuise les intercepteurs lourds | Détruit les avions **au sol** | Coule les gros bâtiments | — | Raffinage, terminaux, salles des machines |
| **Air (offensif)** | **Appui-feu**, seulement si la porte est ouverte | — | — | Frappe les bâtiments à quai | — | — |
| **Air (défense)** | — | — | — | — | **Intercepte** drones et missiles | Protège le réseau et le raffinage |
| **Mer** | — | — | — | — | **Plateformes de tir** : plafonne la salve | **Blocus** : ferme le corridor d'export adverse |
| **Terre** | Prend et tient le terrain | — | Prive l'adversaire de ses bases par l'avance | Prive l'adversaire de ses ports | — | Occupe des ressources |

### 4.1 Les rapports d'échange, avec leurs ordres de grandeur réels

Ce sont les chiffres qui doivent apparaître sur les flèches du dessin (§9). Ils sont réels et ce
sont eux qui font la démonstration.

| Échange | Coût de l'agresseur | Coût du défenseur | Rapport |
|---|---|---|---|
| Drone d'attaque contre intercepteur lourd | 20 à 50 k$ le Shahed / Geran-2 | 3 à 4,5 M$ le PAC-3 MSE | **1 : 90 à 1 : 200** en faveur de l'attaquant |
| Drone d'attaque contre drone intercepteur | 20 à 50 k$ | 1 à 5 k$ | **10 : 1** en faveur du défenseur — *c'est l'innovation qui a renversé la table* |
| Drone naval contre navire de combat | 250 k$ le Magura V5 ou le Sea Baby | 50 à 90 M$ la corvette, 750 M$ le croiseur | **1 : 200 à 1 : 3 000** |
| Missile de croisière contre salle des machines | 1,5 à 2,4 M$ (Iskander-K, Kh-101) | Turbine irremplaçable, délai en années | Sans commune mesure |
| Bombe planante contre position fortifiée | 20 à 30 k$ le kit UMPK, porté par un avion à 45 M$ | Position tenue | Favorable **tant que l'avion rentre** |

La ligne à retenir, et à mettre en scène : **le même camp perd la guerre des coûts contre les
drones aériens et la gagne d'un facteur mille contre les navires.** Ce n'est pas une question de
richesse, c'est une question de rapport entre le coût du vecteur et le coût de la cible. Le jeu
n'a rien d'autre à démontrer sur ce point.

### 4.2 Les trois portes

Non-substituabilité rendue mécanique. Une porte fermée ne réduit pas l'effet : elle **l'annule**.
C'est ce qui distingue une composante d'une douve du tonneau. Une douve courte plafonne ; une
porte fermée met à zéro, et tout l'argent versé dans la composante est perdu sec.

**Porte 1 — l'aviation ne franchit pas une défense intacte.**

```
UsureAA(adverse) = 1 − magasins d'intercepteurs adverses / besoin d'interception du tour
si UsureAA < SeuilFranchissement → SortiesUtiles = 0
sinon SortiesUtiles = Park × Readiness × (UsureAA − Seuil) / (1 − Seuil)
SeuilFranchissement = 0,35, abaissé à 0,15 par l'innovation « bombe planante »
```

Le contournement est le point intéressant : plutôt que de gagner la supériorité aérienne, on
allonge la portée pour tirer depuis 60 à 70 km et ne jamais entrer dans l'enveloppe adverse. La
Russie larguait plus de 160 bombes planantes par jour à l'été 2025 sans avoir jamais obtenu la
supériorité aérienne. C'est **exactement** la définition de l'innovation posée au §10 du modèle :
obtenir le même effet avec une autre ressource — ici, le kit à 25 k$ au lieu de la campagne de
suppression des défenses. La carte « bombes planantes » abaisse le seuil ; elle n'ajoute aucune
puissance.

**Porte 2 — les missiles ne passent que dans la brèche ouverte par les drones.**

Déjà implémentée dans `StrikeResolver` : les intercepteurs lourds brûlés sur les drones qui fuient
(`HeavyWasteOnDrones`) sont ceux que les missiles ne rencontreront pas. Rien à changer. La
spécification se contente de **nommer** ce mécanisme comme une porte et de le montrer sur le
dessin, où il est aujourd'hui invisible.

**Porte 3 — la terre n'avance que sur un terrain préparé.**

Voir §6. C'est la porte la plus délicate à calibrer, parce qu'elle touche la résolution du front,
donc les trois issues du scénario.

---

## 5. La mer, traitée à fond

C'est l'argument le plus frappant du jeu : **une composante entière peut être vaincue par une
autre, à un coût dérisoire, par un camp qui ne possède pas cette composante.** L'Ukraine n'a pas
de marine. Elle a repoussé la flotte de la mer Noire, rouvert son corridor céréalier, et restauré
une part de son PIB — avec des embarcations à 250 000 $.

### 5.1 Le fait à modéliser

- Le Magura V5 et le Sea Baby coûtent environ **250 000 $** l'unité.
- En un an d'emploi, les Magura V5 ont détruit huit bâtiments russes et en ont endommagé six,
  pour plus de **500 M$** de dégâts revendiqués. Au 8 juin 2024, la flotte russe en mer Noire
  comptait 22 bâtiments détruits et 20 endommagés.
- Le gros de la flotte a quitté Sébastopol pour Novorossiisk : la mer n'est pas conquise, elle
  est **rendue inutilisable**.
- Le corridor ouvert par l'Ukraine après le retrait russe de l'initiative céréalière a porté les
  exportations 2024 à **41,6 Md$ pour 131,2 Mt**, soit +23,6 % de volume sur 2023. L'agroalimentaire
  y pèse **près de 60 % des ventes de biens à l'étranger, 24,7 Md$**.

Il n'y a aucune bataille navale à modéliser. Il y a un capital adverse qu'on détruit et un accès
qu'on rouvre.

### 5.2 Structure : `NavalTheatre`

Une classe par camp, quatre champs. C'est tout ce qu'il faut.

```
Park              // navires de combat détenus. Ukraine : 0, et c'est le sujet
Denial            // 0..1 : interdiction exercée sur l'accès adverse
Blockade          // 0..1 : blocus imposé au corridor adverse, dérivé du parc
CapitalDestroyed  // valeur cumulée du parc adverse coulé, en M$. Le compteur du joueur
```

Le point structurant, et il doit être écrit tel quel dans le code :

> **`Denial` ne dépend pas de `Park`.** L'interdiction se produit avec des vecteurs consommables ;
> le blocus se produit avec des navires. Un camp sans marine peut interdire ; il ne peut jamais
> bloquer.

### 5.3 Résolution navale

Nouvelle phase `NavalPhase`, insérée **entre** `DeepStrikePhase` et `FrontPhase`. Elle réutilise
la grammaire de `StrikeResolver` sans la dupliquer.

```
1. Vecteurs engagés
   navals   = Stock.Consume(NavalDrones, Engagement × stock)
   missiles = part antinavire de la doctrine × missiles disponibles

2. Efficacité
   eff = IsrFactor × (1 + Innovation.StrikeEdge) / (1 + DéfenseDePoint(adverse))
   IsrFactor = 0,40 en renseignement propre
             = 1,00 si le soutien étranger fournit le ciblage (ExternalWill ≥ seuil)

   → la traque de navires est conditionnée par le soutien allié : couper l'aide ferme
     aussi la mer, deux tours plus tard. La boucle est réelle et elle est gratuite à écrire.

3. Coups au but
   navires touchés = min(Park_adverse, (navals × 0,06 + missiles × 0,22) × eff)
   ≈ 16 drones navals engagés par navire mis hors de combat

4. Perte de capital
   Park_adverse       −= navires touchés            (ReplacementTurns = 20 → définitif)
   CapitalDestroyed   += navires touchés × 90 M$

5. Interdiction et blocus
   Denial_propre     = clamp(vecteurs disponibles × portée / surface du théâtre, 0, 1)
   Blockade_adverse  = clamp(Park_adverse / ParkRéférence, 0, 1)
   CorridorOuvert    = 1 − max(0, Blockade_adverse − Denial_propre)
```

La ligne 5 contient toute la démonstration. `Park = 0` pour l'Ukraine : `Blockade` ukrainien est
nul, mais `Denial` ukrainien monte avec chaque lot de drones navals produit. Dès que
`Denial ≥ Blockade` russe, `CorridorOuvert` revient à 1 — **sans qu'un seul navire ukrainien ait
jamais existé**. Le joueur voit un blocus tomber sans flotte, et c'est exactement ce qui s'est
passé.

### 5.4 Mer → exportations → PIB → budget de guerre

Le circuit demandé, branché sur l'économie existante sans mécanisme nouveau.

```
Economy.SeaExportShare        // part des recettes d'exportation qui passe par la mer
Economy.ExportCapacityWeight  // poids de l'export dans la capacité productive
```

À la phase 2 (`RevenuePhase`), deux effets, dans cet ordre :

```
a) Recette immédiate — le trimestre même
   Ukraine : fiscal        ×= 1 − SeaExportShare × (1 − CorridorOuvert)
   Russie  : oilRevenue    ×= 1 − TerminalShare  × (1 − CorridorOuvert)

b) Capital productif — avec deux tours de latence
   ProductiveCapacityBillions ×= 1 − SeaExportShare × ExportCapacityWeight × (1 − CorridorOuvert)
```

Le second effet est le plus important et il ne demande **aucun code nouveau** : `AttritionPhase`
plafonne déjà le PIB apparent à `ProductiveCapacity × 1,35`. Frapper la capacité productive tire
donc le PIB apparent vers le bas d'elle-même, avec le décalage voulu, et de là la recette fiscale,
et de là le budget de guerre. La chaîne **mer → exportations → capacité productive → PIB → recettes
→ effort de guerre finançable** est bouclée en réutilisant le mécanisme déjà en place.

Valeurs de départ proposées :

| Paramètre | Ukraine | Russie | Justification |
|---|---|---|---|
| `SeaExportShare` | 0,60 | — | environ 60 % des exportations agroalimentaires passent par les ports de la mer Noire |
| `TerminalShare` (part maritime de l'export pétrolier) | — | 0,45 | Novorossiisk et les terminaux de la Baltique |
| `ExportCapacityWeight` | 0,22 | 0,15 | poids de l'export dans la capacité productive |
| `Park` initial | **0** | 45 navires | flotte de la mer Noire, valeur ≈ 4 Md$ |
| `NavalDrones` capacité/tour | 0 puis 40 dès l'innovation | 0 | l'arme n'existe pas au T1, elle est inventée en cours de partie |

**Cible de calibration** : corridor fermé, l'Ukraine perd de l'ordre de **8 % de son PIB apparent
en deux tours et 12 % de son financement de guerre ordinaire**. C'est sensible sans être décisif
à soi seul — ce qui est la vérité, et il ne faut pas le gonfler. La démonstration ne porte pas sur
l'ampleur de l'effet mais sur **le rapport entre l'effet et son prix** : quelques dizaines de
millions de dollars de drones navals contre quelques milliards de recettes restaurées par
trimestre.

### 5.5 Le second canal : la mer plafonne les missiles

Les bâtiments de la mer Noire sont des plateformes de tir de missiles de croisière. Les couler ou
les repousser réduit la salve disponible.

```
PlafondSalveMissiles = capacité terrestre + Park × 1,2 missiles par navire et par tour
```

Conséquence en jeu, entièrement réelle : une campagne de drones navals à 250 k$ l'unité **réduit
le nombre de missiles qui frapperont le réseau électrique l'hiver suivant**. Le joueur découvre
qu'il a défendu Kiev en mer Noire. C'est la plus belle boucle du jeu et elle coûte une ligne.

### 5.6 Ce qu'il ne faut pas modéliser en mer

Sous-marins, types de coque, ports individuels, guerre des mines, escortes, combat de surface.
Aucun de ces objets n'ajoute une décision : le joueur choisit combien de vecteurs il envoie et
contre quoi. Tout le reste est de la texture, et la texture se met dans le texte des cartes, pas
dans le moteur.

---

## 6. La résolution du front, modifiée

### 6.1 Contrainte absolue

Les trois issues du scénario ne bougent pas : victoire ukrainienne au **T19**, front figé, chute
ukrainienne vers **T10** dans la variante où le soutien s'arrête. Toute modification de
`FrontPhase` doit donc être **neutre sur la trajectoire historique** et ne mordre que quand le
joueur s'écarte de cette trajectoire. Cela impose une seule forme possible : un terme **normalisé
à 1,00** sur le calibrage de référence.

### 6.2 La formule

Actuellement :

```
ratio = push / (hold × terrain × (1 + urbanisation) × (1 + fortification) × frictionDrones / saison)
```

Proposé — un seul facteur ajouté, au numérateur, borné :

```
ratio = (push × Préparation) / résistance

Préparation = clamp(
      0,40
    + 0,25 × AppuiAérien / AppuiAérienRéférence
    + 0,20 × UsureAAAdverse / UsureAARéférence
    + 0,15 × InterdictionLogistique / InterdictionRéférence,
    0,75, 1,25)
```

Les trois valeurs de référence sont mesurées une fois sur la trajectoire 2022-2026 et gelées comme
constantes de scénario, de sorte que `Préparation = 1,00` sur cette trajectoire. Le facteur ne
fait donc rien tant que le joueur joue l'histoire, et il mord dès qu'il s'en écarte :

- Un camp qui abandonne les frappes profondes tombe vers **0,75** : ses offensives ne percent
  plus, quel que soit le nombre d'hommes. C'est la leçon, et elle est enfin mécanique.
- Un camp qui use réellement la défense adverse monte vers **1,25** : dans la table de mouvement
  existante, c'est le passage du grignotage à l'avance, jamais de rien à la percée. Les bornes
  sont serrées exprès.

**Un seul facteur, pas trois.** L'empilement de multiplicateurs est le meilleur moyen de rendre la
résolution impossible à calibrer et illisible sur une carte de référence de jeu de plateau.

### 6.3 La mer n'entre pas dans le front

À souligner dans le code comme dans l'interface : `NavalTheatre` n'apparaît **nulle part** dans
`FrontPhase`. La mer agit sur la recette, la recette sur le budget, le budget sur les munitions,
les munitions sur la couverture, la couverture sur la puissance. Quatre tours de latence, aucune
ligne de code entre les deux. C'est la meilleure illustration possible de la thèse du jeu : *une
composante entière peut décider de la guerre sans jamais toucher le front.*

---

## 7. Le ratio de génération de force, décomposé

`ControlPhase` calcule aujourd'hui un ratio unique, lui-même un minimum. On le décompose sans
changer son seuil ni son comportement.

```
RatioComposante(c) = valeur reconstituée ce tour (c) / valeur perdue ce tour (c)

RatioGénération = min sur les composantes RÉGÉNÉRABLES et ENGAGÉES
                  ( terre, drones, missiles )
                  ∪ { menRatio, materielRatio, CoverageFloor }   ← inchangé
```

Trois règles de garde, chacune indispensable :

1. **Une composante non engagée ne contraint pas.** `Engagement = 0` la sort du minimum. Sans
   cela, l'Ukraine sans marine s'effondrerait au premier tour, ce qui serait à la fois faux et
   ridicule.
2. **Une composante de capital n'entre jamais dans le minimum.** Air et mer ont un
   `ReplacementTurns` supérieur à l'horizon : leur ratio est structurellement nul dès la première
   perte. Elles alimentent un compteur séparé, `CapitalDestroyed`, affiché mais non bloquant.
3. **Le seuil et la durée ne changent pas** : `CollapseThreshold = 0,75`, `TurnsBeforeCollapse = 3`.
   La décomposition est une lecture plus fine du même nombre, pas un nouveau nombre.

Ce que le joueur gagne : au lieu de « ton ratio est à 0,71 », il lit « ta composante missiles
régénère à 0,4 : c'est elle qui te tue, et cela fait deux tours ». C'est la même information,
enfin actionnable.

---

## 8. Ce que la doctrine gagne comme décisions

Une composante qui n'ajoute pas de décision intéressante ne mérite pas d'exister. Voici les
décisions ajoutées, et elles sont peu nombreuses — c'est voulu.

| Décision | Arbitrage réel | Ajout à `Doctrine` |
|---|---|---|
| Répartition des drones entre les trois emplois | Saturer l'arrière adverse, tenir le front, ou couler des navires — le même parc, trois emplois exclusifs | `DroneStrikeShare`, `DroneTacticalShare`, `DroneNavalShare` |
| Part antinavire des missiles | Un missile qui coule un navire ne frappe pas une raffinerie | `AntiShipMissileShare` |
| Engagement de l'aviation | Voler sur une défense encore vive, c'est perdre des avions irremplaçables pour un appui nul | `AirEngagement` |
| Entretien du parc de capital | Payer l'entretien d'une flotte qui ne sert plus, ou laisser filer et se retrouver sans plateformes | `CapitalUpkeepShare` |

L'arbitrage des intercepteurs entre l'arrière et le front (`RearDefenceShare`) existe déjà et
reste, comme prévu, le plus bel arbitrage du jeu. Les quatre ci-dessus le complètent sans le
concurrencer.

**Ce qui n'est pas ajouté, et pourquoi** :

- *Pas de combat air-air.* L'aviation ne survit pas au-dessus d'une défense intacte : c'est déjà
  dit par la porte, et un duel d'avions n'ajouterait qu'un jet de dés.
- *Pas de sous-composantes* (chasse / bombardement, surface / sous-marin, FPV / longue portée).
  Un parc, une horloge, un emploi réparti par doctrine.
- *Pas de composante ISR ou cyber.* Le renseignement entre déjà comme condition du ciblage naval,
  par un seul coefficient conditionné au soutien étranger. Une sixième composante n'ajouterait
  qu'un curseur de plus à lire.
- *Pas de géographie navale.* Un théâtre, un corridor, un chiffre d'accès.

---

## 9. Comment ça se voit

C'est un jeu, pas un tableur. Trois pièces, dessinées au SVG dans `board.js`, dans la même langue
visuelle que le tonneau : papier `#f2efe7`, carton `#fbf9f4`, filets `#d9d1be`, encre chaude
`#1a1815`, chiffres en sérif, libellés en petites capitales 9,5 px à `letter-spacing: 0.13em`.
Deux teintes nouvelles seulement : **mer `#2f6f7f`** et **air `#5b6b7c`** ; les autres réutilisent
les couleurs déjà posées (drones `#8e5878`, missiles `#a8322a`, terre `#7a6a55`, défense `#3f7f93`).

### 9.1 Pièce A — les cinq parcs

`viewBox="0 0 860 300"`. Cinq socles de 140 px de large, ancrés à x = 40, 200, 360, 520, 680.

**Chaque composante a sa silhouette**, jamais un rectangle générique — c'est ce qui rend la rangée
lisible d'un coup d'œil, comme cinq pièces différentes d'un jeu de plateau :

| Composante | Silhouette | Construction |
|---|---|---|
| Terre | un bloc bas et large | `rect` 96 × 34, posé sur la ligne de sol |
| Air | un delta | triangle isocèle, base 72, pointe en haut |
| Mer | une coque | `path` : fond arrondi, pont plat, une superstructure |
| Drones | une nuée | 7 hexagones de 14 px en quinconce, pas un bloc |
| Missiles | un fuseau | `rect` 18 × 74 à bouts arrondis, coiffé d'une pointe |

**L'aire de la silhouette est proportionnelle à la valeur du parc en milliards**, pas au nombre
d'unités — échelle en racine carrée pour rester lisible. C'est cette règle qui produit l'image
juste : quatre millions de drones occupent moins de surface qu'une flotte de quarante-cinq
navires. La nuée de drones est **petite et nombreuse**, la coque est **grande et unique**.

**L'encoche d'usure** : la part du parc détruite depuis le début est découpée dans la silhouette
par un `clipPath` et remplie d'une hachure `#a8322a` à 0,45 d'opacité, avec un liseré gravé sur la
ligne de coupe. On voit le morceau manquant, on ne lit pas un pourcentage.

**Le sablier**, à y = 210, sous chaque silhouette : une petite forme de 16 × 22 portant de un à
cinq grains selon `ReplacementTurns`. Quand la reconstitution dépasse l'horizon de la partie, le
sablier est **remplacé par un cadenas** de 14 × 16 en encre pleine. Le cadenas dit « définitif »
mieux que n'importe quel chiffre, et il n'apparaît que sur l'air et la mer — ce qui est
précisément la leçon.

Sous le sablier : le libellé en petites capitales, puis la valeur du parc en sérif 19 px.

### 9.2 Pièce B — la nappe : qui use quoi

`viewBox="0 0 860 240"`, directement sous la rangée, ancrages alignés sur les mêmes x.

- Des **courbes de Bézier gravées** relient les composantes, d'épaisseur 1,5 à 7 px selon l'effet
  réellement exercé ce trimestre. Une flèche épaisse est un effet massif ; il n'y a rien à lire.
- Au milieu de chaque trait, un **cartouche crème** `#f5f1e6` portant le rapport d'échange en
  sérif 12 px : « 1 : 96 », « 1 : 2 400 ». Ce sont les chiffres du §4.1, et ce sont eux qui font
  la démonstration.
- Un lien **non exploité ce tour** passe en pointillé gris à 0,3 d'opacité : le joueur voit ce
  qu'il n'utilise pas, ce qui est la moitié de l'intérêt du dessin.
- Un lien **bloqué par une porte fermée** porte une **barrière** posée en travers du trait — deux
  montants et une barre diagonale, en encre pleine — et le trait au-delà s'éteint. Infobulle :
  « l'aviation ne franchit pas une défense intacte ».
- La seule flèche qui **sort du cadre** part de la mer et descend vers le bandeau du corridor.
  C'est le geste graphique qui dit que la mer n'agit pas sur le front.

### 9.3 Pièce C — le corridor

`viewBox="0 0 860 110"`, un bandeau à part entière, placé dans l'écran économique et non dans
l'écran militaire — parce que c'est là qu'agit la mer.

- Une bande horizontale `#2f6f7f` à 0,18 d'opacité pour la mer, un trait de côte irrégulier en
  haut, deux ports à gauche (petits carrés gravés), l'horizon à droite.
- **Blocus tenu** : une chaîne dessinée en travers du bandeau — maillons ovales gravés, cadenas au
  centre — et trois silhouettes de coque adverses posées sur la ligne.
- **Blocus rompu** : la chaîne pend en deux tronçons, le maillon central est **ouvert**, les coques
  adverses sont repoussées vers le bord droit, et une file de cinq cargos traverse de gauche à
  droite. Ce basculement d'un tour à l'autre est le moment le plus spectaculaire du jeu ; il mérite
  le seul mouvement de tout le plateau.
- Le chiffre-héros du bandeau en sérif 34 px : la part des recettes d'exportation qui passe. En
  dessous, en petites capitales : « corridor rouvert au tour T — recettes restaurées ».
- En bas à droite, **la balance** : deux jetons face à face, « 0,25 M$ le drone naval » contre
  « 90 M$ le navire », reliés par un fléau qui penche. C'est l'image que le visiteur doit emporter.

### 9.4 Où ça vit

- Pièces A et B : écran « Génération de force », **sous** le tonneau. Le tonneau dit ce que le
  front consomme ; la rangée dit ce que l'armée est. L'ordre importe : la couverture d'abord, les
  composantes ensuite.
- Pièce C : écran économique, dans la chaîne des maillons, entre la recette et le budget — à
  l'endroit exact où la mer agit.

Le tonneau n'est **pas** modifié. Ajouter cinq douves de composantes ferait exactement l'erreur
que le modèle a déjà refusée pour les effectifs : une composante n'est pas une couverture, il
n'existe pas de « besoin en marine » à comparer à une livraison.

---

## 10. Chemin d'implémentation

Cinq étapes, ordonnées par le rapport entre la leçon apportée et le risque de recalibrage. Les
étapes 1 et 2 se suffisent : elles donnent l'argument central et ne touchent pas à la résolution
du front, donc pas aux trois issues.

### Étape 1 — la mer seule *(le minimum qui apporte la leçon)*

Aucune autre composante, aucun changement au front. On gagne l'argument le plus frappant du jeu.

| Fichier | Modification |
|---|---|
| `src/TheoryOfVictory.Core/NavalTheatre.cs` | **nouveau** — `Park`, `Denial`, `Blockade`, `CapitalDestroyed` |
| `src/TheoryOfVictory.Core/ResourceKind.cs` | **un seul** ajout : `NavalDrones`, famille `naval`, 0,25 M$ |
| `src/TheoryOfVictory.Core/Belligerent.cs` | propriété `Naval` |
| `src/TheoryOfVictory.Core/Economy.cs` | `SeaExportShare`, `TerminalShare`, `ExportCapacityWeight`, `CorridorOpen` |
| `src/TheoryOfVictory.Core/Doctrine.cs` | `DroneNavalShare`, `AntiShipMissileShare` |
| `src/TheoryOfVictory.Engine/Phases/NavalPhase.cs` | **nouveau** — résolution du §5.3 |
| `src/TheoryOfVictory.Engine/TurnEngine.cs` | insertion entre `DeepStrikePhase` et `FrontPhase` |
| `src/TheoryOfVictory.Engine/Phases/RevenuePhase.cs` | les deux effets du §5.4 |
| `src/TheoryOfVictory.Core/EventCard.cs` | `EffectKind.SeaDenialDelta`, `EffectKind.NavalParkDelta` |
| `src/TheoryOfVictory.Engine/data/cards.fr.json` | 3 cartes : campagne de drones navals, corridor rouvert, retrait de la flotte |
| `src/TheoryOfVictory.Engine/Scenarios/UkraineScenario.cs` | parc russe 45, parc ukrainien 0, calendrier des cartes |
| `src/TheoryOfVictory.Web/wwwroot/js/board.js` | bandeau du corridor (§9.3) |
| `tests/…/ModelRulesTests.cs` | `ASideWithoutANavy_CanStillReopenItsCorridor` · `SinkingTheFleet_RestoresRevenue_WithoutTakingGround` · les trois issues inchangées |

### Étape 2 — les cinq parcs comme lecture *(aucune règle nouvelle)*

Agrégation de ce qui existe déjà, publication et dessin. **Aucun changement de simulation**, donc
aucun risque sur les trois issues : les tests de non-régression doivent passer au bit près.

`ArmyComponent.cs` (nouveau) · `Belligerent.cs` · `GameState.cs` (`SideSnapshot.Components`) ·
`TurnEngine.Capture` · `board.js` (pièces A et B) · `_Layout.cshtml.css`.

### Étape 3 — l'aviation et sa porte

`AirForce.cs` (nouveau) · `AirDefenceSystem.cs` (mesure de `UsureAA`) · `Doctrine.AirEngagement` ·
`FrontPhase` (appui) · `UkraineScenario` (parcs initiaux) · innovation « bombe planante » dans
`cards.fr.json`. **Première étape qui touche la résolution du front** : recalibrage à vérifier
tour par tour sur les trois variantes.

### Étape 4 — le facteur de préparation

`FrontPhase` uniquement, plus trois constantes de référence dans `UkraineScenario`. Se fait après
l'étape 3, puisqu'elle en consomme la sortie. Test dédié : `Préparation` vaut 1,00 ± 0,02 à chaque
tour de la trajectoire historique.

### Étape 5 — la décomposition du ratio de génération

`ControlPhase` · `SideSnapshot` · `board.js`. Purement une lecture plus fine ; le seuil et la durée
d'effondrement ne changent pas.

### Ce qui alourdirait le jeu sans rien apporter

À écarter explicitement, pour que la question ne se repose pas :

- **Cinq douves de composantes dans le tonneau.** Une composante n'a pas de taux de couverture ;
  ce serait la même erreur de catégorie que celle déjà commise puis corrigée pour les effectifs.
- **Un parc naval ukrainien.** Il n'y en a pas, et c'est tout l'intérêt. Lui en donner un
  détruirait la démonstration.
- **Des sous-types d'unités**, un combat air-air, une guerre des mines, des ports individuels.
- **Une composante par emploi de drone.** Un parc, trois emplois, une clé de répartition.
- **Un radar à cinq branches.** L'aire d'un radar se compense : c'est le dessin qui affirme
  précisément la substituabilité qu'on veut réfuter.
- **Une résolution navale par bâtiment nommé.** Le joueur ne décide de rien à ce niveau.

---

## 11. Points de calibration à vérifier

À examiner au moment d'implémenter, sans les trancher ici :

1. **Capacité missile.** Le scénario donne 130 unités par tour à la Russie, soit environ 520 par
   an. Les commandes réelles pour 2025 approchent 2 500 missiles, tous types confondus. L'écart
   est peut-être volontaire — une « unité » du modèle n'est pas nécessairement un missile — mais
   il devra être tranché avant de brancher le plafond de salve du §5.5, qui suppose la même unité
   des deux côtés.
2. **Valeur moyenne d'un navire.** 90 M$ est une moyenne de parc, entre une corvette Tarantul-III
   et un croiseur. Si l'on veut que le coulage du navire amiral se distingue, il faudra une
   valeur par unité et non une moyenne — ce qui ajoute une donnée, pas une règle.
3. **`ExportCapacityWeight`.** C'est le paramètre qui décide si la fermeture du corridor est un
   inconvénient ou une catastrophe. Le viser sur la cible du §5.4 (−8 % de PIB apparent en deux
   tours) et ne pas céder à la tentation de le gonfler : la mer doit être décisive **par son
   rapport coût/effet**, pas par son ampleur brute.
4. **Seuil de franchissement aérien.** 0,35 est posé pour que l'aviation russe ne serve à rien
   avant l'apparition de la bombe planante, puis serve beaucoup. À vérifier sur la trajectoire.

Comme au §18 du modèle : ces valeurs sont des ordres de grandeur de travail. Celles du §4.1 et du
§5.1, en revanche, sont sourcées et doivent rester telles quelles — ce sont elles qui portent la
démonstration.
