# Le front historique — vingt trimestres sourcés

> `src/TheoryOfVictory.Engine/data/front-history.json` donne la position réelle du front à la fin de
> chaque trimestre, de l'automne 2021 à l'été 2026. Ce document justifie chaque ligne : ce qui s'est
> passé, à quelle date, d'après quelle source, et avec quel degré de confiance.
>
> **Ce fichier n'est pas une sortie du modèle.** C'est une chronique. Le site étant public et la
> guerre en cours, la règle posée en [§14 du calendrier](06-calendrier-propose.md) s'applique ici sans
> exception : ce qui est reconstitué doit être distinguable, à l'écran, de ce que le moteur calcule.

---

## 1. Ce que le fichier contient, et ce qu'il ne contient pas

Vingt trimestres, un par tour de T1 à T20 du calendrier de jeu. Chaque trimestre porte la position à
sa **fin**, trois listes de zones, un titre, ses sources et un degré de confiance.

Le documenté s'arrête à l'**été 2026, arrêté au 13 août 2026** — un trimestre incomplet, marqué
`"confidence": "moyenne"` pour cette seule raison. Le dernier trimestre entièrement révolu et
documenté est le **printemps 2026**. Au-delà, c'est le modèle qui projette, et ce n'est plus le
domaine de ce fichier.

Le fichier ne donne pas de kilomètres carrés. Il donne un état de contrôle par zone, parce que c'est
ce que la carte du jeu sait afficher. Les surfaces citées dans les titres — 120 000 km² en mars 2022,
1 000 km² à Koursk, 370 km² pour la contre-offensive de 2023 — sont là pour l'échelle, pas pour être
additionnées.

---

## 2. Trois conventions, dont deux qui pourraient surprendre

**Les saisons suivent le contrat : `Winter` = janvier-mars.** Donc printemps = avril-juin, été =
juillet-septembre, automne = octobre-décembre. Cette convention a une conséquence qu'il faut voir
venir : **la percée de Kharkiv du 6 au 12 septembre 2022 tombe dans l'`été 2022`**, pas dans
l'automne. Le [calendrier de jeu](06-calendrier-propose.md) §14 la place au T5, automne 2022, en
raisonnant sur un automne qui commence en septembre. Les deux lectures sont défendables ; le fichier
suit la définition du contrat, parce que c'est elle qui sera lue par le code. Même décalage, en sens
inverse, pour le retrait de Kherson rive droite du 11 novembre, qui reste bien dans l'automne 2022.

**`kursk_incursion` est marqué `heldByInvader` partout sauf pendant l'incursion.** La règle du
contrat — toute zone absente des trois listes est réputée tenue par l'Ukraine — est fausse pour cette
zone, qui est du territoire russe. L'omettre reviendrait à afficher l'oblast de Koursk comme ukrainien
en 2021. Il est donc explicitement dans `heldByInvader` de l'automne 2021 au printemps 2024, dans
`heldByDefender` à l'été et à l'automne 2024, `contested` à l'hiver 2025, puis de nouveau
`heldByInvader`. C'est le seul endroit du fichier où « tenu par l'envahisseur » signifie « chez lui ».

**Une zone contestée n'est pas une zone à moitié prise.** `contested` est utilisé dans deux cas
distincts : le combat urbain en cours (Marioupol à l'hiver 2022, Bakhmout de l'été 2022 à l'hiver
2023) et la divergence de sources (Koupiansk depuis l'automne 2025, où Moscou revendique la ville
alors que ses propres blogueurs militaires n'y décrivent que des poches). Dans les deux cas, la règle
est la même : **en cas de doute, `contested`, jamais un arbitrage.**

---

## 3. Le prologue et l'année 2022

### Automne 2021 — la ligne de 2014

Rien ne bouge. La Crimée et les deux enclaves du Donbass, environ 43 000 km². Ce qui se passe se
passe dans les dépôts : les concentrations de forces russes sont rapportées dès novembre 2021 et
suivies quotidiennement par l'ISW jusqu'à l'invasion. **Confiance haute** — c'est le trimestre le
mieux établi du fichier, puisque rien n'y est disputé.

### Hiver 2022 — l'invasion, et le pic

Le 24 février, quatre axes. Au nord, Hostomel dès le premier jour, Boutcha occupée à partir du
27 février, Irpin disputée jusqu'au 28 mars. Au sud, **Melitopol le 1er mars, Kherson le 2** —
première grande ville tombée, et la seule capitale d'oblast que la Russie prendra de toute la guerre.
Marioupol est encerclée le 2 mars. Le pic d'occupation approche 120 000 km² à la mi-mars.

Trois précisions que la liste ne peut pas porter et qui comptent :

- `kyiv_axis`, `chernihiv_axis` et `sumy_axis` sont marqués `heldByInvader` pour le **corridor**
  occupé. **Ni Kyiv, ni Tchernihiv, ni Soumy ne sont tombées.** Tchernihiv et Soumy ont été
  contournées et assiégées, jamais prises. La liste dit l'emprise territoriale, pas la chute des
  villes.
- `kharkiv_north` de même : la bande frontalière et les faubourgs nord sont occupés, la ville de
  Kharkiv ne l'est jamais.
- **Au 31 mars, le retrait du nord est déjà décidé** — annoncé le 29 mars, achevé le 6 avril. Le
  trimestre est donc daté sur son état dominant, la mi-mars, et non sur sa dernière semaine. C'est le
  seul trimestre du fichier où la position de fin de période et l'état caractéristique divergent, et
  c'est signalé ici plutôt que masqué.

**Confiance haute.** Sources : ISW, assessments quotidiens de mars 2022 ; BBC et Reuters pour Kherson ;
cartes DeepState.

### Printemps 2022 — le retrait est une décision, pas une rupture

Entre le 1er et le 6 avril, les forces russes évacuent les oblasts de Kyiv, Tchernihiv et Soumy.
**Aucune ligne n'a été enfoncée.** La manœuvre de décapitation avait échoué : elle n'avait ni pris la
capitale, ni détruit l'armée ukrainienne, ni renversé le gouvernement, et la logistique de la colonne
nord ne la soutenait plus. Moscou redéploie sur le Donbass. C'est un abandon d'objectif, pas un
effondrement de front — et c'est exactement la distinction que le jeu existe pour rendre lisible : la
puissance était déjà partie, le terrain n'a fait que suivre.

Le reste du trimestre va dans l'autre sens : **Izioum le 1er avril**, Lyman le 27 mai, **la capitulation
d'Azovstal le 20 mai** qui achève Marioupol et referme le corridor terrestre vers la Crimée,
**Sievierodonetsk le 25 juin**. `severodonetsk` est `contested` au 30 juin parce que Lyssytchansk, sa
ville jumelle sur l'autre rive du Donets, tiendra encore trois jours.

**Confiance haute.**

### Été 2022 — Lyssytchansk, puis Kharkiv

**Lyssytchansk tombe le 3 juillet** et achève la conquête de l'oblast de Louhansk ; la Russie annonce
une pause opérationnelle qu'elle ne saura pas exploiter. La bataille de Bakhmout s'ouvre le 1er août.

Puis **la percée de Kharkiv, du 6 au 12 septembre** : Balakliia le 8, Izioum et Koupiansk le 10, environ
6 000 km² repris en une semaine — le mouvement le plus rapide de toute la guerre, dans les deux sens.
Il ne se produit pas parce que l'Ukraine était plus forte, mais parce que le secteur était **vide** :
les réserves russes étaient parties tenir Kherson. C'est la démonstration littérale de la règle de
densité du modèle.

`koupiansk` reste `contested` au 30 septembre : la ville est reprise, la rive gauche de l'Oskil ne
l'est pas encore. `lyman` est `contested` pour la même raison — la ville sera reprise le 1er octobre,
le lendemain de la fin du trimestre.

**Confiance haute.**

### Automne 2022 — le Dniepr devient la ligne

Lyman le 1er octobre. Puis, le **11 novembre**, l'évacuation russe de Kherson rive droite : le seul
chef-lieu d'oblast repris de toute la guerre, et l'aveu qu'une force ne peut pas être soutenue de
l'autre côté d'un fleuve dont les ponts sont détruits. Le front sud ne bougera plus pendant deux ans.

**Confiance haute.**

---

## 4. 2023 — l'année où rien ne s'est décidé sur le terrain

**Hiver 2023.** Soledar en janvier, Bakhmout presque encerclée mais tenue. L'offensive d'hiver russe
s'épuise à Vouhledar, où deux brigades de marine sont détruites sans gagner un village. La ligne est
immobile partout ailleurs.

**Printemps 2023.** **Bakhmout tombe le 20 mai**, après dix mois et des pertes russes sans équivalent
dans la guerre. La contre-offensive ukrainienne s'ouvre le 4 juin et bute dès les premiers jours sur
les champs de mines de la ligne Sourovikine.

**Été 2023 — le résultat réel de la contre-offensive.** **Robotyne est atteint le 22 août** et
l'avance s'arrête là. Le bilan est d'environ **370 km² sur une dizaine de kilomètres de profondeur en
quatre mois** : la deuxième ligne de défense russe n'a jamais été atteinte, et la troisième n'a jamais
été vue. Sur les flancs de Bakhmout, Andriïvka et Klichtchiïvka sont reprises à la mi-septembre — des
villages. `zaporijjia_south` passe `contested` et n'ira pas plus loin. C'est le seul trimestre du
fichier où l'Ukraine gagne du terrain en 2023, et le gain est marginal.

**Automne 2023.** La contre-offensive s'éteint. La Russie reprend l'initiative sur **Avdiïvka le
10 octobre**. L'Ukraine établit une tête de pont à **Krynky**, sur la rive gauche du Dniepr : quelques
kilomètres carrés tenus au prix fort d'octobre 2023 à juillet 2024. Elle **n'est pas** reflétée dans
`kherson_left`, qui reste `heldByInvader` — marquer toute la rive gauche `contested` pour une tête de
pont de cette taille dirait quelque chose de faux sur le contrôle du fleuve. La mention est ici, pas
dans les données.

**Confiance haute sur les quatre trimestres.**

---

## 5. 2024 — Avdiïvka, Kharkiv, Koursk

**Hiver 2024.** **Avdiïvka tombe le 17 février**, faute d'obus. Le blocage de l'aide américaine au
Congrès est à son creux, la garnison se retire sous le feu. C'est le trimestre qui valide le modèle
sans qu'on le lui demande : la ville ne cède pas parce que l'assaut a réussi, elle cède parce que le
flux qui l'alimentait s'est tari six mois plus tôt à Washington.

**Printemps 2024.** Le **10 mai**, la Russie ouvre un front transfrontalier au nord de Kharkiv —
Vovtchansk et Lyptsi — pour étirer la défense ukrainienne. L'avance s'arrête à quelques kilomètres de
la frontière et passe en défense active dès le 23 mai, au prix de près de 2 500 hommes pour une
surface dérisoire. `kharkiv_north` devient `contested` et **le restera jusqu'à la fin du fichier** :
ce secteur ne s'est jamais refermé. L'aide américaine est votée le 24 avril.

**Été 2024 — l'incursion de Koursk.** Le **6 août**, l'Ukraine franchit la frontière et prend Soudja :
environ **1 000 km² en une semaine**, 28 localités reconnues par Moscou. C'est le seul terrain russe
jamais tenu par le défenseur, et la seule ligne `heldByDefender` du fichier. Dans le même trimestre,
la Russie reprend **l'intégralité de Robotyne** — confirmé par imagerie géolocalisée fin juillet,
revendiqué dès le 15 mai — effaçant le seul gain de la contre-offensive de 2023, et avance sur
Pokrovsk. L'Ukraine évacue Krynky.

**Automne 2024.** **Vouhledar tombe le 1er octobre**, après deux ans de sièges manqués. Le saillant de
Koursk se réduit : appuyée par des troupes nord-coréennes engagées à partir de novembre, la Russie en
a repris environ la moitié avant la fin du mois. `kursk_incursion` reste `heldByDefender` : l'Ukraine
tient encore la moitié du saillant au 31 décembre.

**Confiance haute sur les quatre trimestres.**

---

## 6. 2025 — la Russie avance, et le paie

**Hiver 2025.** Kourakhove le 6 janvier, Velyka Novossilka le 26 : la poche sud du Donetsk se referme,
puis le secteur se stabilise. Aucune des deux localités n'est dans le vocabulaire — elles se situent
entre `vouhledar` et `pokrovsk`, et le titre les nomme. Le **11 mars**, l'Ukraine perd Soudja et
l'essentiel du saillant de Koursk ; la contre-attaque russe s'était intensifiée le 6 mars, **au
lendemain de la suspension de l'aide et du renseignement américains**. Guerassimov annonce le 12 mars
avoir repris 86 % du saillant. `kursk_incursion` passe `contested` — l'Ukraine tient encore des
positions frontalières.

**Printemps 2025.** Le **26 avril**, Guerassimov déclare l'oblast de Koursk entièrement reconquis.
La déclaration est contestée le jour même par l'état-major ukrainien, **et par Poutine lui-même le
30 avril**, qui admet la présence de soldats ukrainiens ; l'ISW relève encore une avance ukrainienne
dans le sud de Tetkino le 7 mai. Le fichier bascule néanmoins `kursk_incursion` en `heldByInvader`
pour ce trimestre : au 30 juin, l'incursion est finie en tant qu'opération, ce qui reste est du
harcèlement frontalier. **C'est le seul basculement du fichier où la date exacte se discute d'un
trimestre**, et la nuance est ici.

En miroir, la Russie ouvre une zone tampon dans l'**oblast de Soumy** — attaques continues depuis le
début mars le long de la ligne Volodymyrivka-Jouravka-Novenké, plus de **200 km² tenus en juin**, à une
vingtaine de kilomètres de la ville. `sumy_axis` passe `contested` et le reste jusqu'à la fin du
fichier. L'ordre de Poutine d'étendre la zone tampon en 2026, rapporté par Guerassimov, confirme que
le secteur n'est pas refermé.

**Été 2025.** **Tchassiv Yar tombe le 31 juillet** et ouvre la « ceinture des forteresses » du
Donetsk. Le **11 août**, une infiltration russe pousse quinze kilomètres vers **Dobropillia**, au nord
de Pokrovsk, en exploitant une couture entre deux brigades ; elle menace la route
Dobropillia-Kramatorsk et donc la logistique de tout le secteur. L'Ukraine y engage ses réserves
d'élite. La bataille de Koupiansk s'ouvre à l'est ; `koupiansk` passe `contested`.

**Automne 2025.** Le **saillant de Dobropillia est entièrement effacé le 29 novembre** — toutes les
localités reviennent sous contrôle ukrainien, plus de cinquante prisonniers russes à Koutchériv Yar.
C'est le seul succès offensif ukrainien de l'année, et il est défensif par nature. Dans le même
trimestre, la Russie revendique **Koupiansk le 20 novembre** et **Pokrovsk le 2 décembre** ; l'Ukraine
contre-attaque dans les deux villes et affirme le 12 décembre que les assaillants de Koupiansk sont
coupés. En décembre, les troupes russes entrent dans **Lyman** et prennent vraisemblablement Siversk ;
au sud, elles poussent vers Houliaïpole. Cinq zones `contested` — c'est le trimestre le plus disputé du
fichier.

**Confiance haute sur les quatre trimestres**, avec la réserve explicite sur la date de bascule de
Koursk au printemps.

---

## 7. 2026 — la prise de Pokrovsk, et l'arrêt du documenté

**Hiver 2026 — Pokrovsk.** La chronologie est instructive et elle est la raison pour laquelle le
fichier ne suit pas les revendications. Moscou revendique Pokrovsk le 2 décembre 2025 ; Guerassimov
avait annoncé en décembre que sa prise serait « décisive » pour tout le Donetsk. **L'ISW ne conclut à
la prise que le 25 février 2026**, sur l'absence totale d'activité ukrainienne observée dans la ville
depuis le **28 janvier**. Myrnohrad tombe avant cette date — un officier l'avait rapportée prise à
Poutine dès octobre 2025, alors que les combats y dureront encore des mois. `pokrovsk` bascule donc
`heldByInvader` à l'hiver 2026, un trimestre après la revendication russe.

**Et il ne s'en suit rien.** L'ISW note que la valeur logistique de Pokrovsk avait été détruite par les
frappes russes dès juillet 2025 : la ville prise n'était plus l'objectif qu'elle avait été. Deux ans
d'assauts pour une localité de 60 000 habitants avant-guerre, sans percée opérationnelle.

À Koupiansk, les revendications s'annulent : le commandement russe dit tenir toute la ville en
janvier, ses propres blogueurs militaires n'y décrivent que « quelques poches de défense », et les
évaluations quotidiennes de l'ISW la donnent toujours sous contrôle ukrainien en août 2026.
`contested` est la seule lecture honnête. À Lyman, les infiltrations russes de février échouent ; des
groupes de deux ou trois hommes tentent de franchir le Donets gelé.

Au sud, **les contre-attaques ukrainiennes de fin janvier sur les axes Houliaïpole et Oleksandrivka**
bloquent la jonction que le commandement russe avait annoncée le 29 décembre entre les axes d'Orikhiv
et de Houliaïpole.

**Printemps 2026 — l'offensive qui n'a pas eu lieu.** Le chiffre porte le trimestre à lui seul :
**30,42 km² gagnés en juin 2026 contre 481,25 km² en juin 2025**, soit 1,01 km² par jour au lieu de
16,04. Un rapport de un à seize. Aucune zone ne change de main. Kostiantynivka est menacée
d'enveloppement par le nord et le sud, Lyman tient, le front sud est stabilisé.

**Été 2026 — trimestre incomplet, arrêté au 13 août 2026.** La Russie paie en juillet son **taux de
pertes le plus élevé de 2026** sans accélérer d'un kilomètre. Lyman tient toujours le 18 juillet,
l'axe de Houliaïpole est bloqué au 12 août, et l'ISW juge le 7 août « extrêmement irréaliste »
l'objectif russe de prendre tout le Donbass avant la fin de l'année. La Russie contrôle environ
**19 % du territoire ukrainien**, Crimée comprise — soit un peu moins de 117 000 km² selon les
décomptes pro-ukrainiens.

**Confiance moyenne** sur ce seul trimestre, et pour une seule raison : il n'est pas fini. Rien de ce
qui y est écrit n'est incertain ; ce qui est incertain, c'est ce qui s'y passera entre le 13 août et
le 30 septembre.

---

## 8. Ce qui n'a pas pu être sourcé, et ce qui est assumé

**Les dates de bascule au trimestre près sont solides ; les fractions de zone ne le sont pas.** Le
fichier dit qu'une zone est tenue, disputée ou libre. Il ne dit pas « 89 % de Velyka Novossilka »,
alors que c'est le chiffre qu'avait établi la géolocalisation en janvier 2025. Ce niveau de finesse
existe dans les sources et n'a pas de place dans le contrat.

**Trois localités majeures ne sont pas dans le vocabulaire** et n'apparaissent que dans les titres :
Kourakhove et Velyka Novossilka (janvier 2025), Tchassiv Yar (juillet 2025), Kostiantynivka et Siversk
(2025-2026), Dobropillia (août-novembre 2025). Elles se situent dans les interstices du vocabulaire,
entre `bakhmout`, `avdiivka`, `vouhledar` et `pokrovsk`. Ce n'est pas une lacune de sourçage, c'est
une limite de résolution de la carte — la même que celle qu'assume déjà le
[calendrier](06-calendrier-propose.md) §14 pour les axes de Kyiv, Tchernihiv et Soumy.

**Un point reste vraiment ouvert : la date exacte de sortie de Koursk.** Entre la déclaration russe du
26 avril 2025, l'aveu de Poutine du 30 avril et l'avance ukrainienne à Tetkino du 7 mai, le
basculement peut se défendre au printemps ou à l'été 2025. Le fichier a tranché pour le printemps et
le dit.

**Aucune ligne n'a été inventée.** Là où les sources divergent — Koupiansk depuis novembre 2025,
Pokrovsk entre décembre 2025 et février 2026 — le fichier a retenu `contested` et attendu la
convergence, quitte à décaler une prise d'un trimestre par rapport à la revendication de celui qui la
proclame. C'est la règle qui a produit le décalage Pokrovsk, et c'est le bon décalage.

---

## 9. Sources

**Principales.**

- **Institute for the Study of War** — *Russian Offensive Campaign Assessment*, quotidien depuis
  février 2022, publié avec les cartes de Critical Threats : `criticalthreats.org/analysis/`.
  C'est la source de référence du fichier, et la seule qui couvre les vingt trimestres sans
  discontinuité. Les évaluations citées nommément : 2 et 31 mars 2022, 29 mars et 6 avril 2022,
  25 juin 2022, 3 juillet 2022, 10-12 septembre 2022, 1er octobre 2022, 9-11 novembre 2022,
  16 janvier 2023, 20-21 mai 2023, 4-8 juin 2023, 22 août 2023, 15-17 septembre 2023,
  10 octobre 2023, 17 février 2024, 10-12 mai 2024, 6-12 août 2024, fin juillet 2024, 1-2 octobre
  2024, 6-7 et 26 janvier 2025, 24 septembre 2025, 26 et 31 décembre 2025, 2, 6 et 25 février 2026,
  9 mars 2026, 1er juin 2026, 1er juillet 2026, 1er et 7 août 2026.
- **DeepState** (`deepstatemap.live`) — cartographie ukrainienne quotidienne, utilisée pour les
  positions de 2022 et pour les avances contestées de 2025.
- **Wikipedia**, articles de bataille, eux-mêmes sourcés : *Kyiv offensive (2022)*, *Siege of
  Mariupol*, *Battle of Sievierodonetsk (2022)*, *2022 Kharkiv counteroffensive*, *Kherson
  counteroffensive*, *Battle of Bakhmut*, *2023 Ukrainian counteroffensive*, *Battle of Avdiivka
  (2023-2024)*, *2024 northeastern Ukraine offensive*, *Kursk offensive (2024-present)*, *Ukrainian
  occupation of Kursk Oblast*, *2025 Sumy Oblast incursion*, *Battle of Chasiv Yar*, *Dobropillia
  offensive*, *Pokrovsk offensive*, *Battle of Kostiantynivka*.

**Presse de référence citée dans les données.** Reuters, BBC, AP, Al Jazeera, CNN, Euronews, Meduza,
Kyiv Independent, Euromaidan Press, EUobserver, Lviv Herald, Russia Matters (*Russia-Ukraine War
Report Card*, bimensuel, pour les décomptes de surface).

**Ce qui n'a pas été utilisé.** Aucun communiqué du ministère russe de la Défense ni de l'état-major
ukrainien n'est retenu **seul** comme preuve d'un changement de contrôle. Ils sont cités dans les
titres comme revendications, datées, et systématiquement confrontées à une évaluation indépendante.
C'est ce qui explique l'essentiel des écarts entre ce fichier et les cartes qu'on trouve ailleurs.
