# Calibration des effectifs

> Combien d'hommes, où, et depuis quelle source. Document de calibration du scénario Ukraine,
> de février 2022 à l'été 2026.

Trois niveaux de fiabilité sont marqués partout dans ce document, et tenus dans le code :

| Marque | Sens |
|---|---|
| **SOURCÉ** | Un chiffre publié, avec sa source nommée et sa fourchette |
| **EXTRAPOLATION** | Une déduction de ma part à partir de chiffres sourcés — jamais présentée autrement |
| **VALEUR DE JEU** | Un paramètre fixé par la démonstration à produire, qu'aucune source ne détermine |

Rien dans ce fichier n'est inventé puis présenté comme mesuré. Quand les sources se contredisent,
la fourchette est donnée, puis le chiffre retenu, puis la raison du choix.

Complète `01-modele-de-jeu.md`, qui reste le document de référence du modèle.

---

## 1. L'effectif est la variable structurante, pas une ressource à couvrir

C'est la correction de fond que cette calibration accompagne, et elle prime sur tous les chiffres
qui suivent.

Les obus, le carburant et la nourriture sont des **flux consommés** : ils se brûlent chaque
trimestre, ils ont un besoin, et ce besoin peut être couvert ou non. Les hommes, non. Un effectif
n'a pas de « taux de couverture du besoin », parce qu'il n'existe aucun besoin exogène auquel le
comparer : **c'est l'effectif qui dimensionne le front, et c'est donc lui qui fabrique le besoin en
obus, en carburant et en nourriture.** Un million d'hommes en ligne consomme le double de ce que
consomme un demi-million. L'inverse n'a pas de sens.

Dans le tonneau de Liebig que dessine le plateau :

| Élément | Rôle |
|---|---|
| Infanterie en ligne de contact | La **taille** du tonneau. La puissance y est proportionnelle |
| Obus, carburant, nourriture | Les **douelles**. La plus courte fixe le niveau, quelle que soit la taille |

La règle du minimum reste donc entière, et elle reste le cœur pédagogique du jeu : un front
largement pourvu en hommes mais sans obus ne perce rien. Ce qui disparaît, c'est uniquement le
*taux de couverture des hommes*, qui était une erreur de catégorie.

Un déficit d'effectif reste puni deux fois, sans qu'on ait à le déguiser en couverture manquante :
**le tonneau rétrécit** avec chaque homme manquant, et **il fuit** — une unité sous son effectif
théorique tient le même terrain avec des lignes plus minces, sans relève et sans réserve.

Les hommes n'ont pas de plafond de couverture. Ils ont **trois plafonds réels** :

| Plafond | Nature | Où il mord |
|---|---|---|
| **Démographique** | Le réservoir mobilisable | Ukraine : il mord. Russie : jamais vraiment |
| **Politique** | Ce qu'un régime ose exiger | Russie : refus obstiné d'une seconde mobilisation générale après 2022 |
| **Économique** | Chaque mobilisé quitte l'économie productive | Coût marginal croissant : la première vague est presque gratuite, la troisième est ruineuse |

Ce qui limite la Russie n'est pas le nombre d'hommes disponibles : c'est ce que le régime ose
exiger et ce que la trésorerie peut payer. Elle a acheté ses soldats par primes plutôt que de les
réquisitionner — et le joueur découvre que ces primes assèchent la trésorerie qui aurait acheté les
munitions.

---

## 2. Trois grandeurs, qu'on confond en permanence

L'écart entre elles est énorme, et le débat public les mélange constamment — y compris dans les
communiqués officiels, qui comparent volontiers un effectif total à un effectif de théâtre pour
faire parler les chiffres.

| Grandeur | Définition | Ce qu'elle fait dans le modèle |
|---|---|---|
| **Sous les drapeaux** | Tout ce qui porte l'uniforme : théâtre, arrières, formation, défense du territoire, marine, aviation | Rien. C'est le chiffre que les dirigeants annoncent, et le plus vague |
| **Au théâtre** | Le groupement de forces engagé sur le territoire ukrainien | **Consomme.** C'est le dénominateur de tous les besoins matériels |
| **En ligne de contact** | Les unités de combat qui tiennent la ligne | **Combat.** C'est la taille du tonneau, et c'est ce qui manque aux deux armées |

Conséquence tenue par un test : **grossir la queue ne sert à rien.** Plus d'hommes au théâtre à
infanterie de contact constante augmente les obus, le carburant et les vivres exigés sans ajouter
un gramme de puissance. C'est la crise ukrainienne de 2024-2026 énoncée en un rapport.

Le point d'ancrage de toute la distinction, et il est **SOURCÉ** : l'OSW estime que **pas plus de
300 000** des plus d'un million d'Ukrainiens sous les drapeaux sont déployés sur la ligne, et la
presse ukrainienne documente des brigades tombées à **30 % de leur effectif théorique**, avec un
besoin déclaré de **300 000 hommes** pour les recompléter. Une armée d'un million d'hommes peut
manquer d'infanterie : c'est cela qu'il fallait rendre modélisable.

---

## 3. Unité de compte

**Le moteur compte en milliers d'hommes** — `AtFront = 560` vaut 560 000 soldats. **Rien ne sort du
moteur en milliers** : le snapshot expose `MenUnderArms`, `MenInTheatre`, `MenInContact`,
`MenInTraining`, `MenMobilisable`, `MenLost`, `MenEstablishment` et `CombatPower` **en hommes**, de
sorte que la page n'ait aucune conversion à faire. Un tableau de bord qui affiche « 560 » n'apprend
rien ; « 560 000 hommes » est immédiatement une guerre.

**Tout effectif exposé est arrondi au millier**, et l'arrondi fait partie de l'honnêteté du chiffre :
afficher « 671 412 hommes » revendiquerait un recensement là où les sources donnent une estimation à
± 15 %. Les trois derniers chiffres seraient de l'invention déguisée en précision. Le millier est le
grain le plus fin que les sources de ce document soutiennent, et un test le vérifie sur chaque
effectif de chaque tour.

---

## 4. Trajectoire trimestre par trimestre

Le scénario compte **26 tours** de trois mois. Il s'ouvre sur un prologue — T1, automne 2021, les
forces s'amassent aux frontières et personne ne combat —, l'invasion tombe au T2, notre présent
(l'été 2026) au T20, et les six derniers tours sont ce que le modèle projette sans que personne
l'ait observé. Tous les repères historiques de ce document sont donc antérieurs au T20 ; au-delà,
il n'y a plus rien à comparer.

### 4.1 Groupement russe au théâtre

La série de référence est celle de **Janis Kluge**, reconstituée à partir des données budgétaires
russes de compléments de solde. C'est la seule qui ne provienne d'aucun des deux belligérants, et
c'est pour cela qu'elle sert d'étalon.

| Repère | Effectif | Statut | Source |
|---|---|---|---|
| Février 2022 | ≈ 190 000 | **SOURCÉ**, fourchette 150 000 – 190 000 | Estimations occidentales de la force massée aux frontières |
| Sept.-oct. 2022 | +300 000 mobilisés | **SOURCÉ** | Mobilisation partielle, clôture annoncée le 28 octobre 2022 |
| Mi-2023 | 523 548 | **SOURCÉ** | Kluge, données budgétaires |
| Mi-2024 | 667 114 | **SOURCÉ** | Kluge, données budgétaires |
| Mi-2025 | 723 477 | **SOURCÉ** | Kluge, données budgétaires |
| Septembre 2025 | « plus de 700 000 » | **SOURCÉ**, déclaratif | Poutine, 18 septembre 2025 |
| Juin 2026 | 721 300 | **SOURCÉ**, déclaratif | Syrskyi |

Les deux déclarations de belligérants encadrent la série budgétaire au lieu de la contredire, ce
qui est la seule raison sérieuse de leur accorder du crédit. **Chiffre retenu : la série Kluge**,
parce qu'elle est dérivée de dépenses engagées et non d'une annonce.

**Ce que le modèle produit** (variante « le soutien tient ») :

| Tour | Période | Modèle | Repère | Écart |
|---|---|---|---|---|
| T2 | 2022 Q1 | 293 000 | ≈ 190 000 | +54 % |
| T8 | été 2023 | 595 000 | 523 548 | +13,7 % |
| T12 | été 2024 | 586 000 | 667 114 | −12,2 % |
| T16 | été 2025 | 673 000 | 723 477 | −7,0 % |

Un test verrouille les trois points d'été à ± 15 %, qui est la tolérance honnête pour des
estimations de renseignement comparées à un tour large d'un trimestre.

**Le T2 est hors tolérance et c'est assumé** : le tour de l'invasion cumule la force massée pendant
le prologue et l'arrivée des premières recrues, là où les 190 000 sourcés désignent le seul
groupement d'assaut du 24 février. Le modèle ne sait pas distinguer les deux à l'intérieur d'un
trimestre de trois mois. L'écart se referme dès le tour suivant.

### 4.2 Ukraine

| Repère | Effectif | Statut | Source |
|---|---|---|---|
| Février 2022 | 196 600 d'active | **SOURCÉ** | IISS Military Balance 2022 |
| Février 2022, tout compris | ≈ 261 000 | **SOURCÉ**, déclaratif | Umerov, comparant au total ultérieur |
| Mi-2022 | ≈ 700 000 sous les drapeaux | **SOURCÉ**, déclaratif | Mobilisation générale, garde nationale et gardes-frontières inclus |
| Janvier 2025 | 880 000 sous les drapeaux | **SOURCÉ**, déclaratif | Zelensky, 15 janvier 2025 |
| 2025 | « plus d'un million » sous les drapeaux, **≤ 300 000 sur la ligne** | **SOURCÉ** | OSW |
| 2025-2026 | Brigades à 30 % de l'effectif théorique, besoin de 300 000 hommes | **SOURCÉ** | Presse ukrainienne, DeepState sur Pokrovsk |

**Contradiction à trancher** : l'IISS donne 575 000 aux forces terrestres en 2025, Zelensky annonce
800 000 à 980 000 selon les déclarations, l'OSW dit « plus d'un million ». Les trois mesurent des
choses différentes — forces terrestres, forces armées, tout ce qui porte l'uniforme y compris la
défense territoriale. **Chiffre retenu : ≈ 860 000 sous les drapeaux en 2025-2026**, au milieu de
l'écart et cohérent avec l'annonce la plus précise (880 000, janvier 2025).

**Ce que le modèle produit** :

| Tour | Période | Sous les drapeaux | En contact | Repère |
|---|---|---|---|---|
| T14 | 2025 Q1 | 892 000 | 292 000 | 880 000 annoncés, ≤ 300 000 sur la ligne |
| T16 | été 2025 | 852 000 | 268 000 | idem |
| T20 | été 2026 | 781 000 | 242 000 | idem |

Deux tests verrouillent ces deux ancrages : le total sous les drapeaux à ± 15 % des 880 000, et
l'infanterie de contact sous le plafond de 300 000 de l'OSW.

### 4.3 Écarts connus, assumés

Trois divergences que je préfère écrire plutôt que masquer.

1. **Le groupement russe monte d'un bloc au printemps 2023** — 382 000 au T7, 608 000 au T8. La
   réalité a étalé l'arrivée des 300 000 mobilisés de l'automne 2022 sur deux à trois trimestres,
   là où le moteur les forme en un seul. La marche reste encadrée par la série Kluge de part et
   d'autre, et l'aplanir supposerait d'allonger la file de formation pour toute la partie et pour
   les deux camps. **Non corrigé, documenté.**
2. **Le total ukrainien sous les drapeaux de 2022 est trop haut** — 447 000 modélisés dès le
   prologue contre ≈ 261 000 réels. Le modèle porte un rapport arrière/théâtre constant, alors que
   la queue logistique et territoriale s'est construite pendant la guerre. Les chiffres se rejoignent
   à partir de 2024, période où les sources sont de loin les plus fermes. **Non corrigé, documenté.**
3. **Le rapport d'échange des pertes favorise trop l'Ukraine** — 3,3 contre 1 en cumulé dans le
   modèle, quand le CSIS donne ≈ 1,4 contre 1 sur l'ensemble du conflit (et jusqu'à 8 contre 1 sur
   le seul premier semestre 2026). Cause : le modèle applique fidèlement sa règle « attaquer coûte
   trois à cinq fois tenir », et la Russie attaque presque tous les trimestres ; la réalité inclut
   des offensives ukrainiennes coûteuses en 2022 et 2023 et des pertes défensives sous l'artillerie
   et les bombes planantes. Corriger supposerait de contredire une règle posée par le document de
   modèle. **Non corrigé : c'est un choix de modèle, pas une erreur de calibration.**

---

## 5. Flux de recrutement

C'est le flux, pas le stock, qui décide.

### Russie

| Période | Rythme | Statut |
|---|---|---|
| Sept.-oct. 2022 | 300 000 en six semaines | **SOURCÉ** |
| 2024 | 407 000 – 450 000 sur l'année, ≈ 1 700/jour en fin d'année | **SOURCÉ** — reconstitution budgétaire (Kluge), recoupée par les annonces officielles |
| 2025 | 280 000 (renseignement militaire ukrainien) à 417 000 (Medvedev) | **SOURCÉ**, sources contradictoires |
| 2026 Q1 | En baisse d'environ 20 % | **SOURCÉ** |

**Contradiction 2025 à trancher** : l'écart de 1 à 1,5 entre les deux chiffres oppose un
belligérant à l'autre. **Retenu : ≈ 35 000 par mois**, soit le milieu, parce qu'il est cohérent
avec la reconstitution budgétaire de 2024 et avec le repli constaté début 2026.

Le point structurant : **la Russie achète ses soldats.** La prime fédérale passe de 195 000 à
400 000 roubles en août 2024, Moscou y ajoute 1,9 million — et le flux retombe dès qu'on réduit la
prime. Le recrutement russe est un prix, donc sensible à la trésorerie, exactement comme le modèle
le suppose.

### Ukraine

| Période | Rythme | Statut |
|---|---|---|
| Avril 2024 | ≈ 30 000/mois après l'abaissement de l'âge de 27 à 25 ans (loi du 2 avril 2024) | **SOURCÉ** |
| Automne 2024 | ≈ 20 000/mois | **SOURCÉ** — état-major |
| 2025 | 25 000 – 27 000/mois | **SOURCÉ**, déclaratif (Zelensky) |
| 2025-2026 | 17 000 – 24 000/mois, avec plus de 80 000 désertions cumulées | **SOURCÉ** |

L'asymétrie est le sujet : l'Ukraine mobilise environ deux fois moins vite que la Russie ne recrute,
et chaque mobilisé lui coûte proportionnellement trois à quatre fois plus cher en capacité
productive perdue.

---

## 6. Pertes : les chiffres publics des deux camps sont de la propagande

Il faut l'écrire sans détour. Les bilans que publie Kyiv sur les pertes russes et Moscou sur les
pertes ukrainiennes sont des instruments de guerre : ils sont gonflés d'un facteur deux à trois. Les
bilans que chaque camp publie sur **ses propres** pertes sont minorés pour la même raison. Aucun des
quatre ne sert à calibrer quoi que ce soit.

Le modèle retient des **estimations occidentales et indépendantes**, en l'assumant.

### Morts russes

| Source | Estimation | Méthode | Période |
|---|---|---|---|
| Mediazona / Meduza / BBC Russie | ≈ 352 000 morts | Registre successoral croisé avec une liste nominative vérifiée (217 808 noms confirmés en mai 2026) | Fév. 2022 – fin 2025 |
| CSIS | 400 000 – 450 000 morts, ≈ 1,4 M de pertes totales | Agrégation de sources de renseignement | Fév. 2022 – juin 2026 |

L'estimation Mediazona se décompose en ≈ 261 000 décès ordinaires et ≈ 90 000 décès « tardifs »
(déclarés par jugement ou enregistrés avec plus de 180 jours de retard) ; les auteurs signalent
eux-mêmes que la seconde moitié est la plus fragile.

### Morts ukrainiennes

| Source | Estimation | Statut |
|---|---|---|
| Zelensky | 46 000 (février 2025), 55 000 (février 2026) | Déclaratif, plancher |
| UALosses | Plus de 92 000 noms confirmés début 2026 | Comptage nominatif, plancher explicite |
| CSIS | 125 000 – 150 000 morts, 525 000 – 625 000 pertes totales | Estimation, fév. 2022 – juin 2026 |

**Fourchette retenue et raison du choix** : entre le chiffre officiel et l'estimation occidentale,
l'écart va de un à trois. Le comptage nominatif d'UALosses, qui ne recense que ce qui est
publiquement documenté et se déclare incomplet, dépasse déjà de 70 % le chiffre officiel — ce qui
tranche en faveur des estimations hautes. **Retenu : la fourchette CSIS, signalée comme fourchette.**

### Ce que le modèle en fait

Les pertes ne sont pas un paramètre national : elles sortent du multiplicateur de coût d'attaque.
Aucun camp n'a de « meilleurs soldats » dans ce modèle. Voir l'écart n° 3 du § 4.3 sur la limite de
cette approche.

**Signal de fin de partie** : le CSIS estime les pertes russes de 2026 à 30 000 – 34 000 par mois
pour un recrutement d'environ 27 000. Le flux de régénération passe sous le flux de consommation —
l'armée russe rétrécit alors qu'elle avance encore. C'est ce que le ratio de génération de force
doit rendre visible, et c'est ce que la variante « l'Occident joue ses cartes » retrouve seule.

---

## 7. Ce que les effectifs consomment

L'unité `Armes` du moteur vaut **mille coups d'artillerie de tube**. Le point important est que la
consommation **et** la production sont ancrées sur cette même nature de munition — les mélanger est
l'erreur la plus facile à commettre ici, puisque les sources publient tantôt les obus d'artillerie
seuls, tantôt toutes natures confondues.

### Consommation

L'artillerie russe tire de l'ordre de **10 000 coups par jour** en 2024-2025 pour ≈ 650 000 hommes
au théâtre, soit 10 000 × 91 ÷ 650 ≈ **1,40 coup par homme et par trimestre**, à la posture
offensive que la Russie a tenue tout du long. La constante du moteur se lit **avant** le
multiplicateur d'intensité, qui vaut 1,12 à cette posture : l'ancrage donne donc 1,40 ÷ 1,12 ≈
**1,25**, valeur retenue. L'Ukraine, rationnée, tire 2 000 à 6 000 coups par jour à groupement
comparable, soit 0,3 à 0,9 — que le modèle reproduit par sa posture défensive et par sa couverture,
et non par une seconde constante. **Estimation, ± 40 %.**

Les premiers mois de 2022, avec des pointes annoncées jusqu'à 60 000 coups russes par jour, sont
délibérément non calés : le régime de grignotage est celui que dix-neuf des vingt trimestres
simulés ont réellement vécu.

### Production, et le trou que la Corée du Nord comble

| Chiffre | Statut |
|---|---|
| Production russe domestique ≤ 2,3 M d'obus en 2024 | **SOURCÉ** — responsables ukrainiens et occidentaux |
| ≈ 7 M de coups toutes natures produits en 2025 (dont 3,4 M d'obusier) | **SOURCÉ** — renseignement estonien |
| 4 à 6 M d'obus livrés par la Corée du Nord depuis septembre 2023, ≈ la moitié des munitions d'artillerie tirées par la Russie | **SOURCÉ** — Reuters, RUSI |

C'est le rapprochement décisif de toute cette calibration : **700 000 hommes russes consomment de
l'ordre de 4 millions de coups par an, pour une production nationale plafonnée à 2,3 millions.** Le
modèle porte donc une capacité russe de 560 000 coups par trimestre, et l'écart doit être **acheté
à l'étranger, trimestre après trimestre**. Coupez l'argent, les obus s'arrêtent — l'asphyxie de la
variante de victoire ukrainienne n'est plus un réglage, c'est le mécanisme réel.

Le modèle en portait 700 000 par trimestre, soit 2,8 M par an de production nationale : au-dessus
de tout ce que les sources soutiennent, et cela rendait la Russie autosuffisante, donc insensible à
la coupure financière. **Corrigé.**

### Carburant et nourriture

Environ 6 kg de carburant et 4,6 kg de vivres et d'eau par homme et par jour. **EXTRAPOLATION**,
ordres de grandeur d'une force mécanisée sur lignes courtes, sans source utilisable. Contrairement
aux obus, la nourriture ne dépend pas de l'intensité des combats.

---

## 8. Paramètres retenus dans le scénario

En milliers d'hommes. Ils décrivent l'effectif **visé par le commandement** au théâtre ; l'effectif
présent le suit avec retard, puisqu'il faut recruter, former et remplacer les pertes.

| Paramètre | Russie | Ukraine | Statut |
|---|---|---|---|
| Effectif de départ au théâtre | 190 | 200 | **SOURCÉ** / **EXTRAPOLATION** côté ukrainien |
| Cible initiale | 190 | 250 | **EXTRAPOLATION** |
| Croissance de la cible par trimestre | 28 | 22 | **EXTRAPOLATION**, calée sur les séries du § 4 |
| Plafond de la cible | 720 | 560 | **SOURCÉ** côté russe (maximum observé) |
| Capacité de formation par trimestre | 105 | 78 | **SOURCÉ** (≈ 35 000 et ≈ 26 000 par mois) |
| Réservoir mobilisable | 4 200 | 3 700 | **EXTRAPOLATION** — voir ci-dessous |
| Rapport arrière / théâtre | 0,70 | 0,68 | **EXTRAPOLATION** depuis les totaux publiés |
| Part en ligne de contact | 0,55 | 0,55 | **EXTRAPOLATION** — voir ci-dessous |
| Coût d'entretien et de solde | 0,050 | 0,020 | **VALEUR DE JEU** |

**Part en ligne de contact.** Seul le côté ukrainien est ancré : ≈ 300 000 sur la ligne pour un
groupement de théâtre de l'ordre de 550 000. La Russie ne publie **aucune** ventilation
combattants/soutien, donc sa valeur est celle de l'Ukraine, reportée. Les deux camps portent
délibérément le même chiffre : inventer une asymétrie ici reviendrait à offrir un avantage
qu'aucune source ne soutient. Fourchette 0,45 – 0,65.

**Réservoir mobilisable.** Les deux valeurs sont larges et c'est volontaire : elles ne représentent
pas la démographie brute mais le vivier réellement atteignable. Côté russe le réservoir ne mord
jamais dans la partie, et c'est fidèle. Côté ukrainien, 3,7 millions d'hommes de 25 à 60 ans étaient
évalués mobilisables en mars 2024 — c'est un instantané de 2024 utilisé ici comme réservoir de 2022,
ce qui flatte légèrement l'Ukraine : **signalé, non corrigé.**

**Dépôt initial ukrainien : 400 000 coups, VALEUR DE JEU** — et la seule de la chaîne des munitions
qui le soit. Personne n'a publié ce que l'Ukraine détenait en février 2022. Il est fixé par la
démonstration qu'il doit porter : le dépôt achète exactement les deux trimestres de calme que le
modèle promet après une coupure de flux, et cette latence est toute la leçon.

---

## 9. Où le moteur s'écarte du réel pour préserver la démonstration

Il faut le dire plutôt que de laisser croire que le jeu est calé au plus près du réel. Le scénario
doit produire trois issues précises — victoire ukrainienne au T19, front figé au T19, effondrement
ukrainien au T10 — et **quand la fidélité historique et la démonstration entrent en conflit, c'est
la démonstration qui gagne.** Voici où, exactement.

| Ce qui s'écarte | Ce que dit le réel | Ce que fait le moteur, et pourquoi |
|---|---|---|
| **Dépôt initial ukrainien** | Aucune source publiée | 400 000 coups, **fixé par la latence** : le dépôt doit acheter exactement les deux trimestres de calme après une coupure de flux. Quand la consommation par homme a été corrigée à la baisse, ce chiffre a été réduit d'autant — c'est un paramètre asservi à la démonstration, pas une estimation |
| **Coûts de solde et d'entretien** | Non séparables des budgets publiés | **VALEUR DE JEU** : recalibrés pour que la masse salariale garde son poids dans le budget de guerre après l'agrandissement des effectifs. Ils ne prétendent pas au coût réel d'un soldat |
| **Marche du printemps 2023** | Une montée étalée sur deux à trois trimestres | Une marche en un seul tour, 382 000 puis 608 000. Aplanir supposait d'allonger la file de formation pour les deux camps et toute la partie. **Inexactitude connue, encadrée par la série de référence de part et d'autre** |
| **Part en ligne de contact identique des deux côtés** | Seul le côté ukrainien est documenté | 0,55 partout. Une asymétrie non sourcée aurait déplacé l'équilibre du front sans justification — le choix neutre est aussi celui qui ne perturbe pas les issues |
| **Rapport d'échange des pertes** | ≈ 1,4 contre 1 en cumulé (CSIS) | 3,3 contre 1, parce que le moteur applique la règle « attaquer coûte trois à cinq fois tenir » posée par le document de modèle. **Choix de modèle assumé** |

Deux corrections, à l'inverse, sont allées dans le sens du réel **contre** le confort du réglage, et
méritent d'être notées puisqu'elles ont cassé des tests avant de les réparer :

- la consommation d'obus par homme, ramenée à la valeur dérivée de l'artillerie russe observée
  plutôt qu'à une valeur d'ajustement ;
- la production d'obus russe, ramenée de 2,8 à 2,24 millions par an, ce que les sources plafonnent.
  Le moteur rendait la Russie autosuffisante en munitions, donc insensible à une coupure
  financière : l'asphyxie ne pouvait pas fonctionner. Elle fonctionne maintenant **parce que** le
  chiffre est juste, et non malgré lui.

C'est la règle que ce document se donne : une valeur fixée par la démonstration est marquée
**VALEUR DE JEU** et ne se déguise jamais en chiffre sourcé.

---

## 10. Ce que la calibration doit reproduire sans y être forcée

**Un modèle qui ne retrouve pas le passé n'a rien à dire sur l'avenir.** La calibration est bonne si
le moteur retrouve seul :

1. **L'échec de la poussée initiale** — 190 000 hommes ne tiennent pas un front de cette longueur,
   quelle que soit la couverture matérielle. Le tonneau est trop petit.
2. **La crise d'infanterie ukrainienne** — l'effectif total monte pendant que la ligne s'amincit.
   C'est l'écart entre « sous les drapeaux » et « en contact » qui le produit.
3. **La dépendance russe aux munitions achetées** — la production nationale ne couvre pas ce que le
   groupement consomme, et l'écart se paie en devises.
4. **Le piège de la mobilisation** — mobiliser quand le goulot est l'obus n'apporte rien au front et
   ampute la capacité productive, donc les recettes, donc la production d'obus.
5. **Le retournement de 2026** — pertes supérieures au recrutement côté russe, donc ratio de
   génération sous 1 alors que le front avance encore.

---

## 11. Ce que la correction des dépôts a déplacé

L'audit de réalisme ([`09-audit-realisme.md`](09-audit-realisme.md)) a montré que les quatre
mécanismes centraux du jeu ne se déclenchaient jamais, pour une cause unique : des dépôts que
l'aide en nature remplissait sans plafond. Les corriger a obligé à toucher des valeurs voisines.
Voici lesquelles, et sur quoi elles s'appuient.

### 11.1 Corrections appliquées

| Correction | Avant | Après | Appui |
|---|---|---|---|
| Plafond de dépôt unique, aide en nature comprise | Deux chemins, un seul plafond | Un seul plafond pour tout ce qui remplit un dépôt | Règle du modèle : on produit pour couvrir un besoin, jamais pour dépenser un budget |
| Trimestres de dépôt tenus | 6 pour tous | 6 côté russe, **3 côté ukrainien** | Une armée approvisionnée par don ne constitue pas de réserve de guerre : le donateur livre contre la consommation. C'est l'asymétrie « donner contre vendre » rendue physique |
| Réseau ukrainien | 36 GW pour 15,5 de demande | **26 GW pour 13**, hiver × 1,45 | AIE : ≈ 38 GW disponibles avant 2022, 19 GW perdus la première année dont Zaporijjia (6 GW, occupée, non modélisée) ; pointe hivernale 18,5 GW |
| Drones de frappe | RU 900, UA 700 | **RU 3 000, UA 1 800** | Plus de 44 000 drones Shahed lancés sur 2025, 170 par jour aux pointes, contre 26 par jour dans le moteur |
| Intercepteurs bas coût | RU 1 400, UA 1 100 | **RU 1 600, UA 7 000** | Conseil de sécurité ukrainien : 100 000 drones intercepteurs produits en 2025, 1 000 à 1 500 livrés par jour début 2026 |
| Profondeur des frappes sur le raffinage | 0,09 par vague | **0,18** | 20 % du raffinage russe à l'arrêt à l'automne 2025, 42,7 % mi-2026, quand le moteur ne descendait jamais sous 87 % d'intégrité |
| Réparation du raffinage | 40 % du dommage restant par trimestre | **18 %** | Une colonne de distillation revient en semaines, mais les compresseurs et catalyseurs occidentaux ne sont plus vendus ; l'AIE annonce un débit durablement bridé |
| Ce qu'une vague de mobilisation ajoute à l'effectif cible | 60 % de sa taille | **25 %** | La mobilisation russe de 2022 a produit 300 000 hommes et le groupement n'a pas grossi de 300 000 : l'essentiel remplace des pertes. Voir ci-dessous |
| Solde dans le tableau d'allocation | Effacée après écriture | Visible | Deux tiers de la dépense militaire russe n'apparaissaient nulle part |
| Part de PIB de l'effort de guerre | Trimestre divisé par une année | Annualisée | Le ratio se lisait quatre fois trop bas |

### 11.2 Une concession, puis son retrait

Le taux de réparation du raffinage a d'abord été **laissé à 40 %** contre l'avis des sources, et
c'était écrit ici comme un écart défavorable au réalisme. La raison : c'était la seule modification
qui déplaçait l'effondrement russe d'un trimestre, à une époque où le calendrier s'arrêtait au
trimestre présent et où il ne restait aucune marge pour l'absorber.

Le calendrier va désormais jusqu'en 2028 et porte lui-même la chronologie — la chute du régime est
fixée par le moment où tombent les dernières cartes, pas par une constante. **La concession est donc
retirée et la valeur que les sources soutiennent est appliquée.** C'est le gain de réalisme le plus
net de cette passe, et il ne coûte rien à la démonstration.

### 11.3 Ce qu'une mobilisation ajoute réellement à l'effectif cible

Le calendrier dense a révélé une erreur que le calendrier court masquait. Trois cartes de
mobilisation tombent dans les cinq premiers tours — concentration des forces, recrutement en prison,
mobilisation partielle — et chacune relevait l'effectif cible de 60 % de sa propre taille. Cumulées,
elles ajoutaient 267 000 hommes à la cible et l'armée russe atteignait son plafond de guerre dès
l'été 2023 : **671 000 modélisés contre 523 548 mesurés**, trois ans d'avance.

L'erreur était conceptuelle. Une vague de mobilisation ne grossit pas l'ordre de bataille de sa
propre taille : la majeure partie des hommes qu'elle produit **remplace des pertes**. À 25 %, les
mêmes trois cartes suivent la série budgétaire de Kluge à moins de 15 % sur les trois points de
contrôle — 595 000 contre 523 548 à l'été 2023, 586 000 contre 667 114 à l'été 2024, 673 000 contre
723 477 à l'été 2025.

### 11.4 Une conséquence à ne pas masquer

Plafonner l'aide en nature revenait à en détruire une partie, ce qui aurait transformé une règle de
stockage en coupure d'aide déguisée. La valeur refusée bascule donc **en aide financière** sur le
même don. Ce n'est pas un artifice comptable : le suivi de l'institut de Kiel montre exactement ce
glissement, la part militaire du soutien occidental reculant quand la part financière progresse.

### 11.5 Ce que cela a coûté en fidélité 2022

Deux capacités industrielles sont désormais **trop hautes pour 2022 afin d'être justes en 2025** :
les drones de frappe russes et les intercepteurs ukrainiens. Le moteur n'a pas de mécanisme pour
faire croître une ligne de production d'un facteur quarante en trois ans — l'expansion est plafonnée
à 3,5 fois le niveau initial. Il fallait donc choisir entre un 2022 juste et un 2025 juste, et la
saturation que le jeu existe pour démontrer se joue en 2025. **Signalé, non corrigé.**

---

## 12. Le front, désormais fonction des effectifs

Le mouvement du front dépend maintenant de la **densité d'hommes au kilomètre**, et non plus de la
seule puissance de combat. Le terrain devient donc une conséquence directe de la calibration des
effectifs, ce qui est la raison pour laquelle il est confronté au réel ici et non dans un document
séparé : la grandeur qui le pilote est `InContact`, l'infanterie en ligne de contact du § 2.

### 12.1 Ce que le modèle produit, et ce que la guerre a produit

Le front modélisé fait **480 km** — huit secteurs, 48 hexagones de 10 km — contre une ligne de
contact réelle de l'ordre de 1 200 km. Les axes du nord, Kyiv, Tchernihiv et Soumy, n'existent pas
dans le modèle. Toute comparaison d'amplitude doit être lue avec cet écart de périmètre en tête.

| Phase | Modèle (partie de référence) | Réel | Écart |
|---|---|---|---|
| **Ruée** — trimestre de l'invasion | +4 183 km² en un trimestre | ≈ +114 000 km² : de 7 % du pays occupé avant l'invasion à un pic de 26 % en mars 2022 | ÷ 27 |
| **Reflux** — automne 2022 | −3 613 km², soit **86 %** du gain initial rendu | ≈ −43 000 km² repris par l'Ukraine, soit **38 %** du gain initial | Reflux 2,3 fois trop profond en proportion |
| **Grignotage** — 2023 à 2028 | +1 021 km² sur 21 trimestres, soit ≈ 200 km²/an | Moins de 1 % du pays depuis novembre 2022, soit ≈ 2 000 km²/an | ÷ 10, et ÷ 4 seulement à périmètre de front égal |

**La forme est juste, et c'est ce qu'on demandait.** Les trois phases apparaissent, dans le bon
ordre, avec les bonnes proportions relatives : la ruée du premier trimestre est de loin le plus
grand mouvement de la guerre, le reflux d'automne 2022 lui répond, puis le front se fige en un
grignotage qui ne rend plus jamais l'échelle des deux premiers. **Aucune date n'est écrite à la
main** : les deux inflexions sortent du seul rapport de densité.

**Les amplitudes sont d'un ordre de grandeur trop faibles**, et c'est structurel. Le facteur 27 sur
la ruée tient surtout à ce que le modèle ignore les axes du nord, qui font l'essentiel de la surface
de mars 2022 — et qui ont d'ailleurs été *abandonnés* plutôt que perdus au combat, ce que le modèle
ne sait pas représenter. Le facteur 10 sur le grignotage se réduit à 4 une fois rapporté aux 480 km
réellement simulés, ce qui reste un écart mais un écart ordinaire de calibration.

**Le reflux est la seule anomalie de forme** : le modèle rend 86 % de ce qu'il a pris là où
l'Ukraine en a repris 38 %. La densité russe tombe trop bas trop vite après la ruée, faute d'un
mécanisme qui distingue le terrain tenu en profondeur du terrain simplement traversé. **Signalé,
non corrigé** : le resserrer demanderait un modèle de contrôle du territoire que le jeu n'a pas.

### 12.2 Ce que le changement de densité a apporté au reste du modèle

Un résultat qu'il faut noter parce qu'il valide la thèse plutôt qu'un paramètre. Dans le déroulé de
victoire, entre le pic de puissance russe et la veille de la chute du régime :

| Grandeur | Au pic | Veille de la chute |
|---|---|---|
| Puissance de combat russe | 294 000 | 30 000 (**−90 %**) |
| Infanterie de contact russe | 370 000 | 346 000 (−6 %) |
| Terrain tenu | 1 435 km² | 1 435 km² (**inchangé**) |

Le front est désormais libre de bouger, et il ne bouge pas. La raison est exactement celle que le
modèle veut enseigner : **l'envahisseur a perdu ses obus, pas son infanterie.** Une armée privée de
munitions tient le terrain sur lequel elle est posée — elle ne peut simplement plus en prendre. La
ligne ne cède qu'au trimestre où les hommes s'en vont, et les hommes s'en vont quand l'État cesse de
les payer. Le terrain reste une conséquence de l'assèchement, et un test le verrouille.

---

## 13. Sources

Effectifs et groupements :

- [Janis Kluge, composition des forces russes en Ukraine — Russianomics](https://janiskluge.substack.com/p/the-composition-of-russian-forces)
- [Poutine : « plus de 700 000 soldats sur la ligne de front », 18 septembre 2025 — Al Arabiya English](https://english.alarabiya.net/News/world/2025/09/18/putin-says-more-than-700000-russian-soldiers-fighting-at-front-in-ukraine)
- [Syrskyi : plus de 721 000 soldats russes concentrés en Ukraine — Militarnyi](https://militarnyi.com/en/news/russia-concentrate-721000-troops-in-ukraine/)
- [Zelensky : 880 000 soldats ukrainiens face à 600 000 Russes, 15 janvier 2025 — The Kyiv Independent](https://kyivindependent.com/ukraines-military-now-totals-880-000-soldiers-facing-600-000-russian-troops-zelensky-says/)
- [Comparaison des forces avant l'invasion — Council on Foreign Relations](https://www.cfr.org/in-brief/comparing-size-and-capabilities-russian-and-ukrainian-militaries)
- [Crise de l'infanterie ukrainienne : brigades à 30 %, ≤ 300 000 sur la ligne selon l'OSW — RFE/RL](https://www.rferl.org/a/ukraine-infantry-crisis-military-army-war/33497989.html)
- [Instabilité du front et manque d'effectifs — Re: Russia](https://re-russia.net/en/analytics/0240/)

Mobilisation et recrutement :

- [Mobilisation partielle russe de septembre 2022 — EUAA, Russian Federation Country Focus](https://www.euaa.europa.eu/russian-federation-country-focus/413-mobilisation)
- [Conséquences économiques de la mobilisation russe — OSW](https://www.osw.waw.pl/en/publikacje/osw-commentary/2023-01-20/mobilisation-russia-societys-reactions-and-economic)
- [≈ 1 700 recrues par jour fin 2024, données budgétaires — Janis Kluge](https://janiskluge.substack.com/p/new-budget-data-russia-recruited)
- [Recrutement russe au premier semestre 2025 — Janis Kluge](https://janiskluge.substack.com/p/russian-recruitment-the-first-half)
- [417 000 engagés en 2025 selon Medvedev — The Moscow Times](https://www.themoscowtimes.com/2025/12/24/russian-army-recruited-417k-contract-soldiers-in-2025-medvedev-claims-a91536)
- [280 000 engagés en 2025 selon le renseignement militaire ukrainien — The Kyiv Independent](https://kyivindependent.com/russia-has-recruited-280-000-contract-soldiers-in-2025-military-intelligence-says/)
- [Recul du recrutement russe de 20 % début 2026 — Militarnyi](https://militarnyi.com/en/news/recruitment-rates-for-russia-fell-by-2026/)
- [Ajustement de la mobilisation ukrainienne, avril 2024 — OSW](https://www.osw.waw.pl/en/publikacje/analyses/2024-04-17/ukraine-adjusts-its-mobilisation-policy)
- [Mobilisation, paix et dissuasion en Ukraine — International Crisis Group](https://www.crisisgroup.org/qna/europe-central-asia/eastern-europe/ukraine/mobilisation-peacemaking-and-deterrence-ukraine)

Pertes :

- [352 000 morts russes, méthode du registre successoral — Mediazona](https://en.zona.media/article/2026/05/09/losses)
- [Décompte Mediazona, mise à jour continue](https://en.zona.media/article/2026/07/03/casualties_eng-trl)
- [Le coût croissant de la guerre de Poutine — CSIS](https://www.csis.org/analysis/russian-blood-and-treasure-ballooning-costs-putins-war)
- [UALosses, comptage nominatif des morts ukrainiens](https://ualosses.org/en/about/)
- [Zelensky annonce 55 000 morts ukrainiens, février 2026 — Meduza](https://meduza.io/en/news/2026/02/05/zelensky-says-55-000-ukrainian-soldiers-have-died-in-the-full-scale-war-with-russia-open-source-data-suggests-a-higher-toll)

Munitions :

- [Production russe record en 2025 : 7 M de coups toutes natures — Defense Express, renseignement estonien](https://en.defence-ua.com/news/in_2025_russia_broke_its_ammunition_output_record_producing_7m_shells_worth_106b-17489.html)
- [La Corée du Nord fournit jusqu'à la moitié des obus russes — The Moscow Times, d'après Reuters](https://www.themoscowtimes.com/2025/04/15/north-korea-supplying-up-to-100-of-russian-artillery-shells-used-in-ukraine-a88745)
- [Évaluation de la contribution nord-coréenne — RUSI](https://www.rusi.org/explore-our-research/publications/commentary/brothers-arms-assessing-north-koreas-contribution-russias-war-ukraine)
- [Mesurer le débit de munitions russe — Modern War Institute](https://mwi.westpoint.edu/the-industrial-window-of-war-how-to-measure-russias-munitions-throughput-and-how-to-disrupt-it/)

---

## 14. Ce que ce document ne prétend pas être

Aucun chiffre ci-dessus n'est une mesure. Ce sont des estimations de sources ouvertes, produites
pendant une guerre en cours, par des acteurs qui ont tous un intérêt dans le résultat — y compris
les meilleurs. Les fourchettes annoncées sont réelles et non des précautions de style : un effectif
de théâtre à ± 15 %, des pertes et une consommation d'obus à ± 40 %, une part en ligne de contact à
± 30 % restent des ordres de grandeur.

Ce qu'ils suffisent à faire, et c'est tout ce qu'on leur demande : produire des trajectoires dont la
**forme** est juste. Le jeu ne prétend pas dire combien d'hommes sont morts. Il prétend montrer
pourquoi une armée qui recrute moins vite qu'elle ne perd finit par céder d'un coup, plusieurs
trimestres après que le chiffre l'avait annoncé.
