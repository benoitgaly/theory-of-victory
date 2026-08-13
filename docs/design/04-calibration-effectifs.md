# Calibration des effectifs

> Document de calibration des effectifs humains du scénario Ukraine, de février 2022 à l'été 2026.
> Il fixe l'unité, la trajectoire de chaque camp trimestre par trimestre, les rythmes de recrutement,
> les pertes, et les besoins matériels que ces effectifs engendrent.

État : calé sur sources ouvertes, avec fourchettes d'incertitude assumées. Complète
`01-modele-de-jeu.md`, qui reste le document de référence du modèle.

---

## 1. L'effectif est la variable structurante, pas une ressource à couvrir

C'est la correction de fond que ce document accompagne, et elle prime sur tous les chiffres qui
suivent.

Les obus, le carburant et la nourriture sont des **flux consommés** : ils se brûlent chaque
trimestre, ils ont un besoin, et ce besoin peut être couvert ou non. Les hommes, non. Un effectif
n'a pas de « taux de couverture du besoin », parce qu'il n'existe aucun besoin exogène auquel le
comparer : **c'est l'effectif qui dimensionne le front, et c'est donc lui qui fabrique le besoin en
obus, en carburant et en nourriture.** Un million d'hommes en ligne consomme le double de ce que
consomme un demi-million. L'inverse n'a pas de sens.

Dans le tonneau de Liebig que dessine le plateau :

| Élément | Rôle |
|---|---|
| Hommes tenus en ligne | La **taille** du tonneau. La puissance y est proportionnelle, linéairement |
| Obus, carburant, nourriture | Les **douelles**. La plus courte fixe le niveau, quelle que soit la taille |

La règle du minimum reste donc entière — et elle reste le cœur pédagogique du jeu : un front
largement pourvu en hommes mais sans obus ne perce rien. Ce qui disparaît, c'est uniquement le
*taux de couverture des hommes*, qui était une erreur de catégorie.

Un déficit d'effectif reste puni, deux fois plutôt qu'une, sans qu'on ait à le déguiser en couverture
manquante :

1. **Le tonneau rétrécit** — chaque homme manquant retire de la puissance, proportionnellement.
2. **Il fuit** — une unité qui tient le même terrain sous son effectif théorique le tient avec des
   lignes plus minces, sans relève et sans réserve : elle se bat moins bien que son seul effectif ne
   le dirait. C'est le facteur de cohésion.

Les hommes n'ont pas de plafond de couverture. Ils ont **trois plafonds réels**, et c'est ce que le
modèle doit reproduire :

| Plafond | Nature | Ce qui le fait mordre |
|---|---|---|
| **Démographique** | Le réservoir mobilisable | Ukraine : petit, et il se vide. Russie : large, il ne mord jamais vraiment |
| **Politique** | Ce qu'un régime ose mobiliser | Russie : refus obstiné d'une seconde mobilisation générale après septembre 2022 |
| **Économique** | Chaque mobilisé quitte l'économie productive | Coût marginal croissant : la première vague est presque gratuite, la troisième est ruineuse |

Le plafond russe n'est pas démographique — il est politique et fiscal. C'est pour cela que la Russie
a acheté ses soldats par primes plutôt que de les réquisitionner, et c'est ce que le modèle doit
faire découvrir au joueur : les primes assèchent la trésorerie qui aurait acheté les munitions.

---

## 2. Unité de compte

**Le moteur compte en milliers d'hommes.** `AtFront = 560` signifie 560 000 soldats déployés dans le
théâtre d'opérations, ligne de contact et arrières immédiats compris.

**L'affichage, lui, se fait toujours en hommes réels.** « 560 » à l'écran ne veut rien dire pour un
lecteur ; « 560 000 hommes » se lit immédiatement. Toute valeur exposée à la vue doit donc être
multipliée par mille côté C#, pour que la page n'ait aucune conversion à faire.

Trois niveaux d'effectif, à ne jamais confondre :

| Niveau | Définition | Ordre de grandeur, début 2025 |
|---|---|---|
| **Sous les drapeaux** | Tout ce qui porte l'uniforme : théâtre, arrières, formation, défense du territoire, marine, aviation | Ukraine ≈ 880 000 |
| **Au théâtre** | Le groupement de forces engagé contre l'adversaire | Russie ≈ 600 000, Ukraine ≈ 500 000 |
| **En ligne de contact** | Les unités de combat du groupement — l'infanterie qui tient réellement la ligne | Ukraine ≈ 300 000, dont l'infanterie de contact n'est qu'une fraction |

L'écart entre les deux derniers est le point le plus mal documenté de cette guerre, et c'est aussi
celui qui explique le mieux la crise d'infanterie ukrainienne : une armée de 880 000 hommes peut
manquer de fantassins. Le rapport retenu est une **estimation** (voir § 6), pas une donnée.

---

## 3. Trajectoire des effectifs au théâtre, trimestre par trimestre

Le scénario compte 19 tours de trois mois : T1 = hiver 2022 (l'invasion), T19 = été 2026.

| Tour | Période | Russie au théâtre | Ukraine sous les drapeaux | Repère historique |
|---|---|---|---|---|
| T1 | 2022 Q1 | ~190 000 | ~261 000 | Force d'invasion massée aux frontières ; effectif ukrainien d'avant-guerre |
| T2-T3 | 2022 Q2-Q3 | 150 000 – 200 000 | ~700 000 | Échec de la poussée initiale ; mobilisation générale ukrainienne, garde nationale et gardes-frontières inclus |
| T4 | 2022 Q4 | ~300 000 | ~700 000 | Mobilisation partielle russe : 300 000 réservistes, annoncée le 21 septembre, déclarée remplie le 28 octobre |
| T5-T8 | 2023 | 350 000 – 450 000 | 750 000 – 800 000 | Montée en charge du recrutement sous contrat russe ; contre-offensive d'été ukrainienne |
| T9-T12 | 2024 | 470 000 – 575 000 | ~800 000 | Objectif russe de 690 000 pour fin 2024, non atteint ; abaissement de l'âge de mobilisation ukrainien à 25 ans |
| T13 | 2025 Q1 | ~600 000 | ~880 000 | Chiffres donnés par Zelensky le 15 janvier 2025 |
| T15 | 2025 Q3 | > 700 000 | ~880 000 | Poutine, 18 septembre 2025 : « plus de 700 000 sur la ligne de front » |
| T17-T19 | 2026 | 700 000 – 720 000 | 800 000 – 880 000 | Syrskyi évoque plus de 721 000 hommes russes concentrés en Ukraine ; le recrutement russe recule de 20 % au premier trimestre 2026 |

**Incertitude.** Les effectifs de théâtre sont des estimations de renseignement, pas des états
nominatifs. Les chiffres russes viennent soit du Kremlin, qui a intérêt à les gonfler pour afficher
sa puissance, soit du renseignement ukrainien, qui a intérêt à les gonfler pour justifier ses
demandes d'aide. Les deux sources convergent ici autour de 600 000 – 720 000 pour 2025-2026, ce qui
est le seul motif sérieux de leur faire confiance. Fourchette honnête : **± 15 %**.

Le chiffre ukrainien de 880 000 est un total sous les drapeaux, revendiqué par le pouvoir politique :
il n'est comparable au 600 000 russe qu'à condition de se souvenir que le second est un effectif de
théâtre et le premier non. Les comparer directement, comme le font régulièrement les communiqués des
deux camps, n'a aucun sens.

---

## 4. Flux de recrutement

C'est le flux, pas le stock, qui décide — et le recrutement est le seul robinet par lequel un camp
répare ses pertes.

### Russie

| Période | Rythme | Source du chiffre |
|---|---|---|
| Sept.-oct. 2022 | 300 000 en six semaines | Mobilisation partielle, chiffre officiel de clôture |
| 2023 | ~400 000 sur l'année | Revendication du ministère de la Défense, invérifiable |
| 2024 | 407 000 – 450 000 sur l'année, soit ~1 700/jour fin 2024 | Estimation indépendante à partir des données budgétaires régionales, recoupée par les annonces officielles |
| 2025 | 280 000 (renseignement ukrainien, octobre) à 417 000 (Medvedev, décembre) | Écart de 1 à 1,5 entre les deux sources : à retenir tel quel |
| 2026 Q1 | En baisse de ~20 % | Recul concomitant de la réduction des primes régionales |

Le point structurant : **la Russie a acheté ses soldats plutôt que de les réquisitionner.** La prime
d'engagement fédérale passe de 195 000 à 400 000 roubles en août 2024, et Moscou y ajoute 1,9 million
de roubles — c'est cette surenchère qui produit le flux, et c'est elle qui le fait retomber dès
qu'on la réduit. Le recrutement russe est un prix, pas une conscription : il est donc sensible à la
trésorerie, exactement comme le modèle le suppose.

### Ukraine

| Période | Rythme | Repère |
|---|---|---|
| 2022 | Mobilisation générale décrétée dès le 24 février | L'effectif triple en quelques mois |
| Avril 2024 | ~30 000/mois | Loi signée le 2 avril 2024 abaissant l'âge de mobilisation de 27 à 25 ans |
| Automne 2024 | ~20 000/mois | Le gain de l'abaissement d'âge ne tient pas |
| 2025 | 25 000 – 27 000/mois | Chiffre avancé par Zelensky, en regard de 40 000 – 45 000 côté russe |

L'asymétrie est le sujet : à population et à économie beaucoup plus petites, l'Ukraine mobilise
environ deux fois moins vite que la Russie ne recrute, et chaque mobilisé lui coûte
proportionnellement trois à quatre fois plus cher en capacité productive perdue.

---

## 5. Pertes : les chiffres publics des deux camps sont de la propagande

Il faut l'écrire sans détour. Les bilans que publient Kyiv sur les pertes russes et Moscou sur les
pertes ukrainiennes sont des instruments de guerre, pas des mesures ; ils sont invariablement gonflés
d'un facteur deux à trois. Les bilans que chaque camp publie sur **ses propres** pertes sont
minorés pour la même raison. Aucun des quatre ne sert à calibrer quoi que ce soit.

Le modèle retient donc des **estimations occidentales et indépendantes**, en l'assumant, avec leurs
fourchettes.

### Morts russes

| Source | Estimation | Méthode | Période |
|---|---|---|---|
| Mediazona / Meduza / BBC Russie | ~352 000 morts | Registre successoral russe croisé avec une liste nominative vérifiée (217 808 noms confirmés en mai 2026) | Fév. 2022 – fin 2025 |
| CSIS | 400 000 – 450 000 morts, ~1,4 million de pertes totales | Agrégation de sources de renseignement | Fév. 2022 – juin 2026 |

L'estimation Mediazona se décompose en ~261 000 décès « ordinaires » et ~90 000 décès « tardifs »
(déclarés par jugement ou enregistrés avec plus de 180 jours de retard) ; la seconde moitié est la
plus fragile, et les auteurs le disent eux-mêmes.

### Morts ukrainiennes

| Source | Estimation | Période |
|---|---|---|
| Zelensky | 46 000 morts (février 2025), 55 000 morts (février 2026) | Déclaration officielle, plancher |
| CSIS | 125 000 – 150 000 morts, 525 000 – 625 000 pertes totales | Fév. 2022 – juin 2026 |

L'écart entre le chiffre officiel ukrainien et l'estimation occidentale est d'un facteur deux à
trois. Retenir le premier reviendrait à croire un belligérant sur ses propres pertes ; retenir le
second sans le dire reviendrait à le présenter comme un fait. Le modèle retient la fourchette
occidentale **en la signalant comme fourchette**.

### Ce que le modèle en fait

Le rapport d'échange n'est pas une constante : il dépend du sens de l'attaque, ce qui est précisément
la règle que le jeu enseigne — **attaquer coûte trois à cinq fois ce que coûte tenir**. Sur les
pertes totales cumulées, les estimations CSIS donnent un rapport russe/ukrainien d'environ 1,4 sur
l'ensemble du conflit et jusqu'à 8 pour 1 sur le premier semestre 2026, quand la Russie assaille en
permanence. Le moteur reproduit cet écart par le multiplicateur de coût d'attaque, jamais par un
coefficient national : aucun camp n'a de « meilleurs soldats » dans ce modèle.

**Signal de fin de partie, à retenir pour 2026** : le CSIS estime les pertes russes à
30 000 – 34 000 par mois en 2026 pour un recrutement d'environ 27 000. Le flux de régénération passe
sous le flux de consommation — l'armée russe rétrécit alors même qu'elle avance. C'est très
exactement la situation que le ratio de génération de force est censé rendre visible, et le scénario
`Resolve` doit la retrouver seul.

---

## 6. Ce que les effectifs consomment

Puisque l'effectif dimensionne le besoin, chaque taux ci-dessous est un **besoin par homme et par
trimestre**, et le total en découle mécaniquement.

### Obus

L'artillerie russe tire environ 10 000 coups par jour en 2024-2025, pour un groupement d'environ
650 000 hommes : soit **≈ 1,4 coup par homme et par trimestre**. L'Ukraine, rationnée, tire entre
2 000 et 6 000 coups par jour à effectif comparable, soit 0,3 à 0,9. Les premiers mois de 2022 sont
une aberration à part, avec des pointes annoncées jusqu'à 60 000 coups russes par jour.

La valeur retenue par le moteur se situe sur le régime de grignotage 2024-2025, qui est celui que
dix-neuf des vingt trimestres simulés connaissent réellement. **Estimation, ± 40 %.**

Vérification de cohérence, dans l'autre sens : 700 000 hommes russes au taux retenu, en posture
offensive, consomment de l'ordre de 4 millions de coups par an, pour une production russe estimée
entre 3 et 4 millions par an, approvisionnement nord-coréen compris. Le déficit qui en résulte est
fidèle — il est comblé par les stocks soviétiques, et c'est bien ce qui les vide. Le modèle est donc
physiquement tenable à effectif réaliste ; il ne l'était pas à 420 000 hommes, où la consommation
tombait sous la production et où le stock ne servait plus à rien.

### Carburant et nourriture

Environ 6 kg de carburant et 4,6 kg de vivres et d'eau par homme et par jour, ordres de grandeur
d'une force mécanisée sur lignes de ravitaillement courtes. **Estimation non sourcée**, assumée comme
telle : ces deux flux servent moins à la précision qu'à rappeler que le ravitaillement est une
charge et non une décision. Contrairement aux obus, la nourriture ne dépend pas de l'intensité des
combats — un homme mange autant un trimestre calme.

### Part en ligne de contact

Le rapport entre le groupement de théâtre et les unités de combat qui tiennent la ligne est le
paramètre le plus faible de ce document. Les seuls repères publics : sur 880 000 Ukrainiens sous les
drapeaux, de l'ordre de 300 000 servent dans les unités de combat, et l'état-major a dû transférer
des milliers d'aviateurs vers l'infanterie pour combler les brigades. Aucun des deux camps ne publie
de ventilation. **Estimation, ± 30 %**, à traiter comme un ordre de grandeur pédagogique et non
comme une donnée.

---

## 7. Paramètres retenus dans le scénario

Toutes les valeurs sont en milliers d'hommes. Elles décrivent l'**effectif visé par le commandement**
au théâtre, jamais le total sous les drapeaux : l'effectif réellement présent le suit avec retard,
puisqu'il faut recruter, former, et remplacer les pertes.

| Paramètre | Russie | Ukraine | Justification |
|---|---|---|---|
| Effectif de départ au théâtre | 190 | 200 | Force d'invasion de février 2022 contre la part de l'armée ukrainienne engagée |
| Cible initiale | 190 | 250 | Ce que chaque commandement voulait tenir au premier trimestre |
| Croissance de la cible par trimestre | 28 | 25 | La guerre institutionnalise son propre recrutement plutôt que de sauter une fois |
| Plafond de la cible | 720 | 620 | Maximum observé : 700 000 – 721 000 côté russe, groupement de théâtre ukrainien sous un total de 880 000 |
| Capacité de formation par trimestre | 105 | 78 | ~35 000/mois contre ~26 000/mois |
| Réservoir mobilisable | 4 200 | 3 700 | Voir la réserve ci-dessous |

**Trajectoire produite, et son biais connu.** La cible russe passe ainsi par ~300 fin 2022,
~410 fin 2023, ~525 fin 2024, ~610 mi-2025 et atteint son plafond de 720 en fin de partie. Comparée
aux repères du § 3, elle est juste sur 2022 et 2023, et **en retard d'un à deux trimestres sur
2024-2025** (525 modélisés contre ~575 observés fin 2024). Ce retard est assumé : la croissance
linéaire est la forme la plus honnête qu'on puisse donner à une trajectoire dont les points
intermédiaires sont eux-mêmes des estimations à ± 15 %, et l'écart reste dans cette fourchette.

**Réserve sur le réservoir mobilisable.** Les deux valeurs sont larges, et c'est volontaire : elles
ne représentent pas la démographie brute mais le vivier réellement atteignable. Côté russe, le
réservoir ne mord jamais dans la partie, et c'est fidèle — ce qui limite la Russie n'est pas le
nombre d'hommes disponibles mais ce que le régime ose exiger et ce que la trésorerie peut payer.
Côté ukrainien, la contrainte est inverse et le réservoir doit finir par se faire sentir. Un
réservoir russe calibré comme une contrainte active serait une erreur de modèle.

---

## 8. Ce que la calibration doit reproduire sans y être forcée

Critère de validation, dans l'esprit du § 18 du modèle de jeu : **un modèle qui ne retrouve pas le
passé n'a rien à dire sur l'avenir.** La calibration des effectifs est bonne si le moteur retrouve
seul :

1. **L'échec de la poussée initiale** — 190 000 hommes ne suffisent pas à tenir un front de cette
   longueur, quelle que soit la couverture matérielle. Le tonneau est trop petit.
2. **La crise d'infanterie ukrainienne de 2024** — l'effectif total monte pendant que la ligne
   s'amincit : c'est l'écart entre « sous les drapeaux » et « en ligne de contact » qui le produit,
   pas une pénurie d'hommes au sens brut.
3. **Le piège de la mobilisation** — mobiliser quand le goulot est l'obus n'apporte rien au front,
   ampute la capacité productive, donc les recettes, donc la production d'obus. Le joueur doit
   pouvoir y tomber et voir le front céder six tours plus tard avec plus d'hommes que jamais.
4. **Le retournement de 2026** — pertes supérieures au recrutement côté russe, donc ratio de
   génération de force sous 1 alors même que le front avance encore.

---

## 9. Sources

Effectifs et groupements de forces :

- [Poutine : « plus de 700 000 soldats russes sur la ligne de front », 18 septembre 2025 — Al Arabiya English](https://english.alarabiya.net/News/world/2025/09/18/putin-says-more-than-700000-russian-soldiers-fighting-at-front-in-ukraine)
- [Zelensky : 880 000 soldats ukrainiens face à 600 000 Russes, 15 janvier 2025 — The Kyiv Independent](https://kyivindependent.com/ukraines-military-now-totals-880-000-soldiers-facing-600-000-russian-troops-zelensky-says/)
- [Syrskyi : plus de 721 000 soldats russes concentrés en Ukraine — Militarnyi](https://militarnyi.com/en/news/russia-concentrate-721000-troops-in-ukraine/)
- [Comparaison des forces russes et ukrainiennes avant l'invasion — Council on Foreign Relations](https://www.cfr.org/in-brief/comparing-size-and-capabilities-russian-and-ukrainian-militaries)
- [Combien de soldats la Russie a-t-elle en Ukraine ? — The National Interest](https://nationalinterest.org/blog/buzz/how-many-troops-does-russia-have-ukraine-sa-092225)

Mobilisation et recrutement :

- [Mobilisation partielle russe de septembre 2022, 300 000 réservistes — EUAA, Russian Federation Country Focus](https://www.euaa.europa.eu/russian-federation-country-focus/413-mobilisation)
- [Conséquences économiques et sociales de la mobilisation russe — OSW Centre for Eastern Studies](https://www.osw.waw.pl/en/publikacje/osw-commentary/2023-01-20/mobilisation-russia-societys-reactions-and-economic)
- [Données budgétaires : ~1 700 recrues par jour fin 2024 — Janis Kluge](https://janiskluge.substack.com/p/new-budget-data-russia-recruited)
- [Recrutement russe au premier semestre 2025 — Janis Kluge](https://janiskluge.substack.com/p/russian-recruitment-the-first-half)
- [417 000 engagés sous contrat en 2025 selon Medvedev — The Moscow Times](https://www.themoscowtimes.com/2025/12/24/russian-army-recruited-417k-contract-soldiers-in-2025-medvedev-claims-a91536)
- [280 000 engagés en 2025 selon le renseignement militaire ukrainien — The Kyiv Independent](https://kyivindependent.com/russia-has-recruited-280-000-contract-soldiers-in-2025-military-intelligence-says/)
- [Recul du recrutement russe de 20 % au premier trimestre 2026 — Militarnyi](https://militarnyi.com/en/news/recruitment-rates-for-russia-fell-by-2026/)
- [Ajustement de la politique de mobilisation ukrainienne, avril 2024 — OSW Centre for Eastern Studies](https://www.osw.waw.pl/en/publikacje/analyses/2024-04-17/ukraine-adjusts-its-mobilisation-policy)
- [Mobilisation, paix et dissuasion en Ukraine — International Crisis Group](https://www.crisisgroup.org/qna/europe-central-asia/eastern-europe/ukraine/mobilisation-peacemaking-and-deterrence-ukraine)

Pertes :

- [352 000 morts russes en quatre ans, méthode du registre successoral — Mediazona](https://en.zona.media/article/2026/05/09/losses)
- [Trois ans de morts : estimation Meduza / Mediazona — Meduza](https://meduza.io/en/feature/2025/02/24/three-years-of-death)
- [Le sang et le trésor russes : le coût croissant de la guerre de Poutine — CSIS](https://www.csis.org/analysis/russian-blood-and-treasure-ballooning-costs-putins-war)
- [Zelensky annonce 55 000 morts ukrainiens, février 2026 — Meduza](https://meduza.io/en/news/2026/02/05/zelensky-says-55-000-ukrainian-soldiers-have-died-in-the-full-scale-war-with-russia-open-source-data-suggests-a-higher-toll)
- [Estimations de pertes militaires des deux camps — Britannica](https://www.britannica.com/question/What-are-the-military-casualty-estimates-for-the-Russia-Ukraine-War)

---

## 10. Ce que ce document ne prétend pas être

Aucun des chiffres ci-dessus n'est une mesure. Ce sont des estimations issues de sources ouvertes,
produites pendant une guerre en cours, par des acteurs qui ont tous un intérêt dans le résultat —
y compris les meilleurs d'entre eux. Les fourchettes annoncées sont des fourchettes réelles, pas
des précautions de style : un effectif de théâtre à ± 15 % et des pertes à ± 40 % restent des ordres
de grandeur.

Ce qu'ils suffisent à faire, et c'est tout ce qu'on leur demande : produire des trajectoires dont la
**forme** est juste. Le jeu ne prétend pas dire combien d'hommes sont morts. Il prétend montrer
pourquoi une armée qui recrute moins vite qu'elle ne perd finit par céder d'un seul coup, plusieurs
trimestres après que le chiffre l'avait annoncé.
