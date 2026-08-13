# Theory of Victory — modèle de jeu

> Simulation de la guerre en Ukraine comme **compétition de génération de force**, et non comme
> succession de batailles. Outil pédagogique inspiré des travaux de **Phillips P. O'Brien**
> (*How the War Was Won*, 2015 ; *The Strategists*, 2024 ; sa lettre hebdomadaire sur l'Ukraine).

État : conception arrêtée, hors calibration chiffrée. Document de référence du projet.

Deux documents le complètent et sont postérieurs à sa rédaction :
[`04-calibration-effectifs.md`](04-calibration-effectifs.md) fixe l'unité, la trajectoire et les
besoins engendrés par les effectifs humains, et fait autorité sur ce point ;
[`05-composantes-armee.md`](05-composantes-armee.md) spécifie les cinq composantes d'armée — terre,
air, mer, drones, missiles — et le théâtre naval.

---

## 1. La thèse à mettre en scène

O'Brien soutient que les guerres industrielles ne se décident pas sur le champ de bataille mais
en amont : dans la production, dans l'acheminement, et dans la destruction du matériel adverse
*avant* qu'il n'atteigne le front. La « bataille décisive » est une reconstruction narrative
a posteriori. Ce qui décide, c'est la capacité à régénérer de la force trimestre après trimestre.

| Principe | Traduction en règle |
|---|---|
| Le front est un **thermomètre**, pas un moteur | Un hexagone se prend parce que le flux net l'a permis, jamais par un bon jet de dé |
| L'attrition se joue **en profondeur** | Une raffinerie, une centrale ou une usine valent plus de terrain qu'une tranchée |
| L'effondrement est un **seuil**, pas une pente | Tant que les deux camps régénèrent, rien ne bouge ; sous le seuil, tout cède d'un coup |

Message pédagogique final : **une égalité de génération de force produit un front figé ; un déficit
durable produit un effondrement total.** Le joueur doit pouvoir perdre la guerre en gagnant des
hexagones à chaque tour.

**Signature du modèle** : partout où c'est possible, un effet est un **seuil**, jamais une pente —
effondrement militaire, délestage électrique, chute d'un régime. Et partout où c'est possible, un
avantage **s'érode seul** et doit être entretenu — innovation, sanctions, dégâts sur les raffineries.
Rien d'acquis ne reste acquis.

---

## 2. Phasage

| Version | Contenu | Objet |
|---|---|---|
| **V1.0** | Partie **déterministe** pré-calculée, sans joueur, rejouée à l'écran tour par tour | Donner à voir la modélisation et sa lisibilité en plateau |
| **V1.1** | Même moteur, tirages probabilistes réactivés | Montrer la dispersion des trajectoires |
| **V2** | Deux joueurs, pioche et jeu de cartes façon Magic | Faire vivre l'arbitrage |
| **V3** | IA de doctrine | Jouer seul |

La V1.0 ne comporte **aucun aléa** : les cartes tombent à des tours écrits d'avance, les allocations
suivent une doctrine fixée par le scénario. Le moteur est néanmoins écrit pour que la source
aléatoire et la couche de décision se rebranchent sans réécriture.

**Démonstration V1** : le même départ est rejoué **trois fois**, seules changent les cartes que
l'Occident décide de jouer. *L'Occident joue ses cartes* → l'Ukraine l'emporte par asphyxie
financière de l'adversaire, sans prendre un hexagone de plus. *Le soutien tient, sans plus* → front
figé, personne ne gagne. *Le soutien s'arrête* → rien pendant deux tours, puis tout cède. Une partie
ne prouve rien ; une comparaison prouve tout.

**Leçon de calibration, apprise en construisant le déroulé de victoire** : tant que le budget de
guerre était plafonné en simple part du PIB, couper les recettes pétrolières ne changeait
strictement rien. Il a fallu adosser l'effort de guerre à ce que les recettes du trimestre
**financent réellement** — impôt affectable, pétrole, aide, ponction sur les réserves — pour que le
baril redevienne ce qu'il est dans la réalité : le robinet de la guerre.

---

## 3. Grammaire des ressources : stocks contre flux

**Stocks** — s'épuisent, se reconstituent lentement : réservoir démographique, parc de matériel,
trésorerie et réserves, dépôts, capacité de génération électrique installée.

**Flux** — la seule chose qui compte au front : recrues formées, armes sorties d'usine, barils
exportés, milliards perçus, tonnes acheminées, térawattheures produits.

> **Le front consomme des flux.** Un stock énorme mal alimenté ne tient pas ; un stock faible bien
> alimenté tient indéfiniment.

Les trois flux consommés au front : **Armes · Carburant · Nourriture**, plus **l'argent** qui les
achète, le **pétrole** qui fait l'argent et le **PIB** qui le produit.

**Les hommes ne sont pas un flux de plus.** Un flux consommé a un besoin, donc un taux de
couverture ; l'effectif n'en a pas, parce qu'il n'existe aucun besoin exogène auquel le comparer —
c'est lui qui dimensionne le front, et donc lui qui **fabrique** le besoin en obus, en carburant et
en vivres. Les recrues formées restent un flux, celui qui répare les pertes ; les hommes tenus en
ligne sont autre chose, et le §4 dit quoi. Le moteur les compte en milliers, l'affichage montre
toujours l'homme réel : « 560 000 hommes », jamais « 560 ».

Élégance à exploiter : **le pétrole est à la fois monnaie et consommable.** La Russie le vend pour
financer sa guerre, les deux camps le brûlent pour bouger. Le même cube change de nature selon qui
le tient.

---

## 4. La règle du minimum

Règle de combat unique, tenable à la main sur un plateau :

> **Ta puissance sur un secteur est celle de ta ressource la plus rare.**

Chaque flux consommé est exprimé en taux de couverture du besoin, et le minimum porte sur ces
trois-là. Trois millions d'obus ne compensent pas un dépôt de carburant vide ; cinq cent mille
hommes sans obus ne percent rien. Minimum strict en V1 — c'est plus démonstratif et directement
jouable au plateau.

**Le tonneau, énoncé proprement.** Les hommes tenus en ligne sont la **taille** du tonneau : la
puissance leur est proportionnelle, linéairement, et ils ne sont jamais une douve. Les douves sont
les trois flux que le front consomme, et la plus courte fixe le niveau quelle que soit la taille.
Un déficit d'effectif est puni deux fois plutôt qu'une, sans être déguisé en couverture manquante :
**le tonneau rétrécit**, et **il fuit** — une unité qui tient le même terrain sous son effectif
théorique le tient avec des lignes plus minces, sans relève ni réserve, et se bat donc moins bien
que son seul effectif ne le dirait.

En V2, le minimum strict pourra être adouci par une agrégation à substituabilité faible et réglable :
mettre le curseur au maximum montre en direct pourquoi « avoir beaucoup de tout » ne remplace pas
« ne manquer de rien ».

---

## 5. L'énergie : l'intrant des intrants

L'électricité ne va **jamais** au front. Elle conditionne tout ce qui permet d'y aller.

| Consommateur | Effet d'une coupure |
|---|---|
| Usines d'armement | ↓ production livrée |
| Raffinage | ↓ carburant et ↓ recettes d'export |
| Chemins de fer électrifiés | ↓ intégrité logistique |
| Économie civile | ↓ PIB, donc ↓ recettes |
| Chauffage | ↓ moral |

**Le délestage est un seuil.** Tant que la génération couvre la demande, l'effet est nul : détruire
10 % d'un réseau qui dispose de 15 % de marge ne change rien. Sous le seuil, on coupe par priorité —
le civil d'abord, l'industrie ensuite. Les dix pour cent suivants font dix fois plus de dégâts que
les dix premiers. Frapper sans atteindre le seuil, c'est gaspiller toute sa campagne.

**La saison décide.** Un tour sur quatre est l'hiver et la demande y explose. Les mêmes dégâts ne
valent rien en juillet et sont une crise nationale en janvier. D'où un rythme annuel authentique :
on prépare la campagne de frappes à l'automne pour qu'elle morde en hiver.

**Réparable contre irréparable.** Distinction décisive :

- **Sous-stations et transformateurs** — remplacés en semaines. Dégât réversible : il faut y revenir
  sans cesse.
- **Turbines et alternateurs de centrales thermiques** — pièces uniques, fabricants étrangers,
  délais en années. Perte permanente.

C'est pourquoi la Russie est passée du réseau de transport aux salles des machines. Viser juste
inflige une perte définitive ; viser mal crée du travail à refaire.

*Détail fourni par le réel, à retenir* : on ne frappe pas une centrale nucléaire, on frappe sa
sous-station de raccordement. Une centrale débranchée ne produit rien, sans franchir le tabou.

**Asymétrie des cibles.** Le réseau russe est vaste, redondant et excédentaire — quasi impossible à
faire tomber. Mais son raffinage est concentré et ses exportations passent par peu de terminaux.
Frapper l'Ukraine veut dire viser l'électricité ; frapper la Russie veut dire viser le raffinage et
l'export. Même logique, deux géographies opposées.

**Arbitrage des intercepteurs.** Protéger le réseau consomme les munitions antiaériennes qui
manquent au front. Défendre Kiev ou défendre Pokrovsk : le même intercepteur ne peut pas être aux
deux endroits. Sans doute le plus bel arbitrage du jeu, et il est entièrement réel.

---

## 6. Économie

### 6.1 Le piège keynésien de guerre

Deux compteurs distincts, et c'est volontaire :

- **PIB apparent** — gonflé par la dépense militaire, les commandes, les primes d'engagement. Il
  *monte*. Affiché en gros sur le tableau de bord.
- **Capacité productive soutenable** — érodée par le retrait de main-d'œuvre, la décapitalisation,
  l'inflation, la ponction des réserves. Elle *baisse*. Affichée en petit.

Le camp qui optimise le premier se ruine à trois ans. C'est le piège central du scénario.

### 6.2 Mobilisation

**Les hommes ont trois plafonds, jamais un taux de couverture** — démographique (le réservoir
mobilisable), politique (ce qu'un régime ose exiger) et économique (chaque mobilisé quitte
l'économie productive). Celui de la Russie n'est pas démographique : il est **politique et fiscal**.
C'est pourquoi elle a acheté ses soldats par primes au lieu de les réquisitionner, et pourquoi son
recrutement retombe dès qu'elle réduit la prime.

**Le coût en PIB est marginalement croissant.** Les premiers mobilisés ne coûtent presque rien —
régions périphériques, faible productivité, chômage. Les suivants coûtent de plus en plus cher,
jusqu'à toucher les ouvriers qualifiés des régions productives, c'est-à-dire ceux qui fabriquent les
obus. La première vague est quasi gratuite, la troisième est ruineuse. Le modèle reproduit ainsi
tout seul le comportement observé : primes ciblées sur les régions pauvres, évitement de Moscou et
Saint-Pétersbourg, refus obstiné d'une seconde mobilisation générale.

**Deux monnaies pour acheter des soldats :**

| Voie | Coût | Effet |
|---|---|---|
| Recrutement sous contrat | Argent — primes d'engagement élevées | Aucun coût politique |
| Mobilisation forcée | Capital politique et moral | Gratuite en trésorerie |

La Russie a massivement choisi la première pour éviter la seconde. Le joueur découvrira que les
primes assèchent la trésorerie qui aurait acheté ses munitions.

**La formation bâclée s'auto-alimente.** Mobiliser au tour T donne des soldats à T+1 ou T+2.
Presser le cycle dégrade la qualité, donc augmente les pertes, donc oblige à remobiliser plus tôt.
La mobilisation précipitée fabrique le besoin de la suivante.

**Le piège majeur.** Si la ressource la plus rare est l'obus, mobiliser 300 000 hommes n'apporte
rien au front, ampute le PIB, donc les recettes, donc la production d'obus.

> **Mobiliser au mauvais moment n'est pas inefficace : c'est activement suicidaire.**

Contre-intuitif, séduisant à chaque tour, et au cœur de la thèse. Le jeu doit permettre d'y tomber,
puis de voir le front céder six tours plus tard par manque d'obus, avec plus d'hommes que jamais.

**Asymétrie ukrainienne** : réservoir démographique bien plus faible, et chaque mobilisé pèse
proportionnellement trois à quatre fois plus lourd dans une économie plus petite. La Russie peut
gaspiller des hommes ; l'Ukraine paye chaque mobilisation deux fois — au front et à l'usine.

---

## 7. Le pétrole : une variable, quatre canaux

Formation du prix : marche aléatoire à retour à la moyenne, secouée par les cartes. En V1.0, le
calendrier des prix est écrit d'avance.

1. **Recette russe** — `volume exportable × (Brent − décote) × (1 − friction sanctions)`, où le
   volume dépend de l'intégrité du raffinage et des terminaux, attaquable et réparable.
2. **Coût ukrainien** — importatrice nette : baril cher, carburant militaire cher, mobilité en berne
   à budget constant.
3. **Lassitude occidentale** — baril cher, inflation chez les soutiens, volonté politique érodée.
4. **Survie du régime russe** — le baril cher finance le niveau de vie, tient les élites
   satisfaites, stabilise le pouvoir.

Une seule variable, quatre canaux, **tous dans le même sens**. Le baril à 100 dollars est
probablement la pire nouvelle possible pour l'Ukraine — et le jeu le démontre sans avoir à l'écrire.

---

## 8. Sanctions : trois canaux, pas un malus de PIB

Modéliser « sanctions → −X % de PIB » serait faux. Elles frappent trois canaux, et le PIB **suit** :

| Canal | Effet | Vitesse |
|---|---|---|
| **Prix** | Creuse la décote sur le baril | Immédiat |
| **Friction** | Taxe permanente sur tout ce qui franchit la frontière | Rapide, modéré |
| **Composants** | Plafonne la capacité de production d'armes — machines-outils, roulements, optique, électronique | Lent, décisif |

Leçon : la sanction efficace n'est pas celle qui punit, c'est celle qui coupe un intrant physique.
Empiler des sanctions financières spectaculaires rapporte moins qu'un embargo discret sur les
machines-outils.

**Elles s'érodent.** Une sanction posée au tour 3 ne vaut presque plus rien au tour 12 : les circuits
de contournement se construisent. L'effet décroît seul sauf resserrement. Sanctionner n'est pas un
acte, c'est un entretien.

**Elles coûtent au sanctionneur.** Chaque train consomme du capital politique européen et renchérit
l'énergie, donc nourrit l'inflation, donc érode la volonté — qui réduit *à la fois* les sanctions
futures *et* l'aide à l'Ukraine.

> **Sanctionner la Russie affaiblit mécaniquement le soutien à l'Ukraine.** Pas par idéologie : par
> la facture énergétique.

**Miroir ukrainien : la conditionnalité.** L'Ukraine n'est pas sanctionnée, elle est *conditionnée*.
Si sa corruption monte, la conditionnalité se durcit et le flux se réduit. La Russie a des
sanctions, l'Ukraine a des conditions : symétriques dans leur fonction, opposées dans leur forme.

---

## 9. Soutien étranger : donner contre vendre

L'asymétrie structurante du jeu.

| | Ukraine — on **donne** | Russie — on **vend** |
|---|---|---|
| Coût | Nul en argent | Élevé, en devises |
| Dépend de | La volonté d'un tiers | Sa propre trésorerie |
| Risque | **Arrêt net** sur une élection ou une lassitude | Aucun : tant qu'elle paie, elle est servie |
| Plafond | Politique | Comptable |
| Contrepartie | Conditionnalité, dépendance technique | Dépendance stratégique, concessions, prix imposés |

Un flux gratuit qui peut disparaître, contre un flux payant qui ne disparaît jamais. La chaîne
russe est courte et directe : **Brent → recettes → obus nord-coréens au front.**

Corollaire pédagogique : couper le soutien à l'Ukraine est une décision politique qui prend une
journée ; couper celui de la Russie suppose d'agir sur son argent, donc lentement, donc par le
pétrole. Deux leviers de nature radicalement différente.

**Compteur de dépendance** côté russe : chaque achat l'alourdit, elle se paie en concessions.

**Acheter puis internaliser.** L'achat étranger se convertit en capacité domestique moyennant
investissement et délai — drones achetés, licence payée, production de masse chez soi. À l'inverse,
côté ukrainien, l'aide apporte des systèmes qu'on ne sait ni produire ni entretenir seul : une
capacité qui s'évapore si le donateur se retire. **Reçu n'est pas possédé.**

---

## 10. Innovation : elle ne multiplie pas la puissance, elle déplace le goulot

Un multiplicateur de puissance contournerait la règle du minimum et casserait le modèle.

> **Innover, c'est atteindre le même effet avec une autre ressource — celle qu'on a.**

Le drone n'ajoute pas de la puissance de feu : il permet de l'obtenir **sans obus**. Quand le goulot
est l'artillerie, investir en drones le déplace ailleurs. La règle du minimum est préservée, et
l'innovation devient le seul moyen de sortir d'une pénurie qu'on ne peut pas combler par la
production.

C'est aussi le vrai arbitrage industriel : beaucoup d'armes médiocres, ou moins d'armes meilleures ?
Les deux camps ont tranché différemment.

**La Reine Rouge.** Toute avance se dégrade seule à chaque tour, parce que l'adversaire s'adapte —
guerre électronique contre drones, puis fibre optique contre guerre électronique. Cesser d'investir,
c'est retomber en deux ou trois tours.

**Asymétrie des sources.** L'Ukraine innove par le bas : cycles courts, petites structures, sauts
fréquents mais difficiles à passer à l'échelle. La Russie innove par le haut : lente à adopter, mais
quand elle adopte, c'est à l'échelle industrielle. Le même investissement ne produit pas la même
courbe selon le camp.

---

## 11. Corruption et coefficient de transmission

```
Transmission = (1 − fuite budgétaire) × intégrité logistique × (1 − interdiction adverse)
```

En **V1**, la corruption est un curseur unique entre l'argent dépensé et les armes qui arrivent au
front. Rien de plus — le reste n'a de sens qu'avec un joueur qui décide sous incertitude.

En **V2**, elle se déploie en quatre effets, dont un qui change la nature du jeu :

1. Fuite budgétaire — une part de l'argent n'achète rien
2. Inflation des coûts unitaires — le même obus payé deux ou trois fois son prix
3. Dégradation qualitative — la ressource existe mais ne performe pas
4. **Brouillard sur ses propres chiffres** — plus le camp est corrompu, plus son tableau de bord
   ment. Il voit deux millions d'obus, il en existe 1,2, et il le découvre le jour où il en a besoin

La corruption est **endogène** : elle monte avec l'argent injecté vite et sans contrôle, elle baisse
si on investit en audit — investissement qui coûte du budget, met trois tours à produire son effet
et déclenche une crise politique immédiate. Assainir coûte avant de rapporter.

---

## 12. Stabilité politique

### 12.1 Russie — deux jauges, et c'est la seconde qui tue

Les régimes autoritaires ne tombent presque jamais par la rue seule : ils tombent quand une
fraction de l'appareil bascule.

- **Mécontentement populaire** — mobilisation, morts, niveau de vie. Spectaculaire, rarement fatal.
- **Cohésion des élites** — la guerre rapporte-t-elle encore à ceux qui comptent ? Silencieuse,
  invisible jusqu'au dernier moment, bien plus dangereuse.

Les sanctions et la baisse du PIB rongent précisément la seconde, en rendant la guerre non rentable
pour l'appareil. C'est la théorie de la victoire de ceux qui misent sur les sanctions — le jeu
permet de la tester au lieu d'en débattre.

**Paradoxe de la répression** : elle fait baisser la probabilité de révolte visible et monter la
tension sous-jacente. Fermer la soupape repousse l'échéance en aggravant la magnitude. Le régime
paraît stable jusqu'au tour où il ne l'est plus du tout — même signature que l'effondrement
militaire.

**C'est la défaite visible qui déclenche, pas l'attrition.** Ce n'est pas le nombre de morts qui
renverse un régime, c'est l'évidence publique que la guerre ne peut plus être gagnée. Un recul de
front important provoque un saut de tension ; l'usure lente n'en provoque presque aucun.

### 12.2 Ukraine — l'épuisement de la volonté

Le pendant n'est pas la révolution mais la bascule vers un accord négocié. Un camp risque le
renversement de son régime, l'autre une capitulation par les urnes. Formes opposées, fonction
identique : la guerre s'arrête par l'arrière, pas par le front.

---

## 13. Le front hexagonal

- **1 hexagone = 10 km**, calibré sur le rythme réel : quelques kilomètres par mois sur les axes
  actifs, soit 1 à 2 hexagones par tour en cas de poussée réussie
- Front découpé en **secteurs** (Kharkiv, Koupiansk, Lyman, Siversk, Tchassiv Iar, Pokrovsk,
  Vouhledar, Zaporijjia, Kherson/Dniepr…), chacun avec son terrain et ses fortifications
- La puissance s'alloue **par secteur**, jamais par unité : aucune micro-gestion tactique

```
Ratio = puissance attaquant / (puissance défenseur × terrain × fortification × urbanisation)
```

| Ratio | Résultat | Coût pour l'attaquant |
|---|---|---|
| < 1,2 | Aucun mouvement, usure réciproque | Élevé, pour rien |
| 1,2 – 2,0 | Grignotage : 0 à 1 hexagone | 3 à 5 × les pertes du défenseur |
| 2,0 – 3,0 | Avance : 1 à 3 hexagones | 2 à 3 × |
| > 3,0 | Percée, puis exploitation si réserves disponibles | ≈ 1 × |
| Défenseur en rupture | Effondrement, avance non bornée jusqu'à la ligne d'arrêt | Minime |

Deux règles portent le message : **l'attaque coûte 3 à 5 fois la défense**, et **l'avance dégrade sa
propre logistique** — le gain de terrain porte sa propre sanction.

---

## 14. Structure d'un tour (3 mois)

| # | Phase | Contenu |
|---|---|---|
| 1 | **Énergie** | Génération contre demande saisonnière, délestage par priorité |
| 2 | **Revenus** | PIB, fiscalité, pétrole, aide reçue ou achats étrangers |
| 3 | **Allocation** | Ravitaillement prioritaire, puis répartition du budget (scriptée en V1, jouée en V2) |
| 4 | **Production** | Commandes échues, capacités qui montent en charge, recrues formées |
| 5 | **Logistique** | Coefficient de transmission : ce qui atteint réellement le front |
| 6 | **Événements** | Cartes du tour (calendrier fixe en V1.0, pioche en V2) |
| 7 | **Frappes en profondeur** | Saturation, interception, rapport d'échange, dégâts sur l'arrière |
| 8 | **Front** | Répartition de la puissance par secteur, résolution, mouvement |
| 9 | **Attrition** | Pertes, moral, usure, érosion des sanctions et de l'avance technologique |
| 10 | **Contrôle** | Ratios de génération, seuils d'effondrement, stabilité politique, fin de partie |

Deux règles d'allocation, apprises en calibrant le moteur, méritent d'être notées :
**le ravitaillement est une charge, pas une décision** — on nourrit et on carbure les troupes avant
de choisir quoi que ce soit ; et **une armée recrute pour combler sa cible d'effectif, jamais pour
dépenser son budget** — sans ce frein, elle grossit au-delà de ce qu'elle peut armer et s'affame
elle-même de munitions.

---

## 15. Les cartes

Format figé dès la V1 — coût, type, timing, effets — même si la V1.0 les joue sans coût à des tours
écrits d'avance. Une carte est une **donnée**, jamais du code : en ajouter une ne doit pas demander
de recompiler le moteur.

### 15.1 Principe

**Pas de points de vie.** On ne réduit pas l'adversaire à zéro : on assèche ses flux de génération,
et c'est l'assèchement qui provoque, un tour plus tard, l'effondrement de tout le reste d'un seul
coup. Aucune carte n'inflige de dégâts ; **toutes déplacent un robinet**.

### 15.2 Typologie

| Type | Analogie Magic | Exemple |
|---|---|---|
| **Permanent** | Terrain, enchantement | Nouvelle chaîne d'obus, réseau de contournement des sanctions, filière de recrutement |
| **Éphémère** | Instantané | Frappe sur une raffinerie, livraison d'urgence, coup diplomatique |
| **Rituel lent** | Sort à effet différé | Mobilisation : payée maintenant, effet dans deux tours |
| **Contre-carte** | Contresort | Guerre électronique, contre-batterie, contre-espionnage |

Les contre-cartes créent le bluff : l'adversaire a deux cartes en main, j'attaque quand même ?

### 15.3 Le « mana » : le capital politique

Payer les cartes en argent les mettrait en concurrence directe avec les achats d'armes — arbitrage
plat. Seconde ressource, à **deux moteurs de génération asymétriques** :

- **Russie** — production régulière par la centralisation, sans avoir à convaincre personne, mais
  qui s'effondre d'un bloc si le moral casse
- **Ukraine** — production irrégulière par la diplomatie et la résistance visible, dépendante de
  l'extérieur, avec des pics quand elle marque des coups

Cartes militaires payées en argent, cartes politiques en capital politique, certaines dans les deux.

### 15.4 La pioche indexée sur la santé

On ne pioche pas une carte par tour d'office : **on pioche selon l'état de ses flux**. Économie saine
et moral haut, deux ou trois cartes ; flux qui s'assèchent, une seule, puis aucune.

Le camp qui décroche subit donc de plus en plus d'événements sans pouvoir y répondre, et décroche
plus vite. La spirale de mort d'un jeu de cartes **est** la spirale d'effondrement de 1918. La
non-linéarité n'est plus cachée au fond du moteur : elle est dans la main du joueur, qui la voit se
vider.

### 15.5 Le deck est la théorie de la victoire

Le deckbuilding est le vrai geste stratégique, et il justifie le titre du jeu. Deck **attrition**
(production, profondeur de banc), deck **frappe profonde** (couper les flux adverses), deck
**politique** (épuiser la volonté adverse et celle de ses soutiens). Chaque deck est une théorie de
la victoire explicite, et le jeu répond à la question d'O'Brien : laquelle tient la distance ?

> Critère d'équilibrage : **le deck frappe profonde doit battre le deck attrition frontale sur seize
> tours**, sinon le jeu dit le contraire de sa thèse.

### 15.6 Familles

- **Politique occidentale** — bascule électorale américaine, déblocage des avoirs gelés, fatigue
  budgétaire (probabilité croissante avec l'inflation, donc avec le prix du pétrole)
- **Politique interne** — mobilisation, fracture au sommet, décapitation
- **Économique** — choc pétrolier, durcissement ou érosion des sanctions, épuisement des réserves
- **Énergie** — campagne contre le réseau, frappe sur les salles des machines, hiver rigoureux
- **Militaire et technologique** — saut sur les drones, percée en guerre électronique, trou dans la
  couverture antiaérienne
- **Externe** — nouveau fournisseur, transfert de licence, détournement d'attention, pression chinoise

---

## 16. Conditions de fin

| Issue | Déclencheur | Leçon |
|---|---|---|
| **Front figé** | Les deux ratios de génération restent ≥ 1 | L'égalité industrielle produit l'enlisement, pas la paix |
| **Effondrement militaire** | Un ratio sous le seuil pendant N tours | L'armée qui a envahi peut se désagréger entièrement — 1918 |
| **Effondrement politique** | Chute du régime ou capitulation négociée | On perd aussi par l'arrière — 1917 |
| **Épuisement mutuel** | Les deux camps sous le seuil | Armistice sur la ligne atteinte |
| **Rupture du soutien** | Aide à zéro pendant N tours | La dépendance extérieure est une vulnérabilité structurelle |

La partie « normale » finit en **front figé**. C'est voulu : l'effondrement n'arrive jamais en
poussant plus fort, seulement en coupant les flux de l'autre.

---

## 17. Portage vers le plateau

- Chaque flux devient un **cube de couleur** ; l'argent, le PIB et le capital politique sont des jetons
- Chaque conversion devient une table à deux entrées ou un ratio d'échange fixe
- La règle de combat tient en une phrase sur la carte de référence
- La résolution de secteur devient une table de ratio, plus deux dés en V2
- Le deck est déjà physique par nature

---

## 18. Statut de la calibration

Les valeurs numériques du scénario sont des **ordres de grandeur de travail**, posées pour que le
moteur tourne et produise des courbes discutables. Elles ne sont pas sourcées une par une et ne
doivent pas être présentées comme des faits.

Méthode de validation : faire tourner le scénario depuis février 2022 et vérifier que le modèle,
sans y être forcé, retrouve les grandes inflexions observées — échec de la poussée initiale, crise
des munitions de fin 2023, grignotage lent de 2024-2025, campagnes hivernales contre le réseau.
**Un modèle qui ne retrouve pas le passé n'a rien à dire sur l'avenir.**

Chaque paramètre porte dans le scénario sa source, ou la mention explicite `estimation`.
