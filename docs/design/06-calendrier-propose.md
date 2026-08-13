# Calendrier — vingt-six trimestres, une carte par camp et par trimestre

> Le calendrier de `UkraineScenario.BuildCalendar`. **Un camp joue une carte par trimestre, jamais
> deux.** Le déroulé en programmait cent cinquante-deux sur vingt-six trimestres, soit près de six
> par tour : la main affichée à l'écran mentait sur la mécanique, et le joueur ne voyait pas une
> décision, il voyait un déluge. Il en programme désormais **quarante-six** — vingt-trois trimestres,
> deux camps, une carte chacun — et chacune est celle qui décide du trimestre.
>
> **Statut : intégré et vérifié.** Les trois issues n'ont pas bougé d'un trimestre : le régime russe
> cède au **T23**, printemps 2027, et l'armistice tombe à l'**automne 2027** ; le front reste figé
> jusqu'au **T26** ; l'Ukraine s'effondre au **T11**, printemps 2024. Toute la suite de tests passe, dont
> quatre nouveaux qui verrouillent la règle elle-même.
>
> Les cartes sorties du calendrier **ne sont pas sorties du deck** : les cent une y sont toujours.
> Elles sont la main dans laquelle la V2 fera piocher, et c'est précisément pour cela qu'on les
> garde. Quarante-sept codes distincts sont joués sur les trois déroulés réunis ; cinquante-quatre
> cartes ne le sont jamais et attendent leur joueur.
---

## 1. Personne ne subit : tout le monde joue

Le deck ne comporte plus **aucune** carte sans propriétaire. Il n'y a plus d'événement qui tombe du
ciel, plus de rubrique « subi ce trimestre » : les cent une cartes se répartissent en cinquante
russes et cinquante et une ukrainiennes, et chacune est jouée par quelqu'un.

La règle d'attribution est simple : **une carte appartient au camp qu'elle sert**, celui qui aurait
intérêt à la jouer. Elle ne dit pas qui a provoqué l'événement dans le monde réel — elle dit qui
tient la carte en main.

| Ce que la carte fait | À qui elle appartient |
|---|---|
| Sanctions, embargos, avoirs gelés, aide, conditionnalité | Ukraine — ce sont ses soutiens qui les décident |
| Asphyxie économique de l'envahisseur : krach du baril, fonds souverain à sec, rupture de l'appareil, décrochage de la monnaie | Ukraine — l'étranglement est une manœuvre, pas un accident |
| Lassitude occidentale : blocage parlementaire, bascule électorale, inflation chez les donateurs, guerre qui capte l'attention ailleurs | Russie — c'est le produit de son effort d'influence |
| Saison, terrain, démographie | Le camp que la saison sert ce trimestre-là : l'hiver rigoureux est russe, l'hiver clément et la raspoutitsa sont ukrainiens |

Les vingt-six anciennes cartes sans propriétaire ont été réparties selon cette règle, treize de
chaque côté, et non supprimées.

| Passées côté russe | Passées côté ukrainien |
|---|---|
| `attention_elsewhere`, `aid_blocked`, `aid_collapse`, `us_election_swing`, `budget_fatigue`, `parliament_veto`, `european_election_swing`, `inflation_surge`, `ceasefire_talks` | `aid_unblocked`, `elite_fracture`, `elite_break`, `oil_price_crash`, `sovereign_fund_empty`, `supplier_withdraws`, `currency_collapse`, `global_recession` |
| `harsh_winter`, `air_defence_gap`, `oil_price_spike`, `dam_breach` | `mild_winter`, `rasputitsa`, `demographic_wall`, `pipeline_sabotage`, `armed_mutiny` |

Chacune a reçu un coût en capital politique proportionné à la manœuvre qu'elle représente. Une seule
exception, documentée en §12 : `elite_fracture` reste à zéro.

Cette attribution est ce qui rend la règle d'une carte par camp et par trimestre **comptable** : sans
propriétaire, on ne saurait pas de qui est le trimestre. Les deux chantiers se tiennent.

---

## 2. Le prologue : la guerre se prépare avant de se jouer

La partie ouvre à l'**automne 2021**, un trimestre avant l'invasion. C'est le seul tour du déroulé où
aucun coup de feu n'est tiré, et c'est le plus démonstratif de tous : le joueur y voit une guerre se
gagner ou se perdre dans les dépôts, avant que la carte ne bouge d'un hexagone.

Deux cartes s'y jouent, une par camp, et leur asymétrie **est** le message.

La Russie joue **L'amassement**. Cent mille hommes, les dépôts avancés remplis en obus et en
carburant : c'est un rituel lent, les hommes n'arrivent au front qu'**au tour suivant**,
c'est-à-dire le jour de l'invasion. La force est constituée un trimestre avant d'être employée, et
cela se lit à l'écran. Les manœuvres d'automne et l'ultimatum de décembre restent dans le deck : ce
sont la couverture et le prétexte de la manœuvre, pas la manœuvre.

En face, l'Occident joue **Les premières livraisons défensives** : des missiles antichars portables,
de quoi n'être pas complice, pas de quoi dissuader. L'avertissement du renseignement allié et l'ordre
de mobilisation que l'Ukraine ne donne pas restent eux aussi dans le deck — le premier parce qu'un
renseignement que personne ne croit ne décide de rien, le second parce qu'une décision de ne pas
agir n'occupe pas le trimestre de celui qui la prend.

> Le prologue oppose donc un camp qui convertit son économie en force et un camp qui livre de quoi
> ne pas mourir tout de suite. Au tour deux, l'un a cent mille hommes de plus sur la ligne et l'autre
> a un PIB intact. C'est toute la thèse du jeu en deux cartes, sans un coup de feu.

Les deux textes portent une même leçon, et elle n'est pas datée : **tout était visible.** Les convois
se comptaient, les hôpitaux de campagne montaient vers la frontière, le renseignement donnait la date
et les axes. Ce qui a manqué n'est pas l'information, c'est la décision — croire engage, douter ne
coûte rien, jusqu'au matin où le doute se paie.

### Ce que le moteur doit faire pour que le prologue tienne sa promesse

**Le tour 1 ne doit produire ni combat, ni mouvement, ni frappe en profondeur.** Ce ne l'était pas :
`FrontPhase` et `DeepStrikePhase` s'exécutent à chaque tour, et l'envahisseur entre en jeu avec cent
quatre-vingt-dix mille hommes déjà en théâtre et une posture offensive de 0,62. Laissé tel quel, le
prologue ouvrirait la partie sur une bataille qui n'a pas eu lieu.

Les deux postures offensives sont mises à zéro au tour 1, et cela suffit pour le mouvement : **aucun
hexagone ne bouge**, c'est mesuré. Cela ne suffit pas pour les pertes. `ResolveSector` les applique
**avant** de regarder le résultat — le taux de perte du défenseur est une constante et le
multiplicateur d'attaque vaut cinq quand le ratio est inférieur à 1,1. Le trimestre le plus pacifique
du déroulé coûte donc, à la mesure, **vingt-neuf mille hommes à la Russie et huit mille à l'Ukraine**,
et les deux camps lancent des frappes en profondeur. C'est faux, c'est visible à l'écran, et cela
reste à corriger.

Il faut donc **sauter les deux phases**, pas les neutraliser. Un drapeau de scénario — par exemple
`CombatStartsOnTurn = 2` — qui fait passer `FrontPhase` et `DeepStrikePhase` tant que le tour lui est
inférieur. Les huit autres phases tournent normalement : c'est justement le propos du trimestre que
l'énergie, les revenus, l'allocation et la production continuent de fonctionner pendant qu'aucun coup
n'est tiré.

La ligne de départ n'appelle en revanche aucun changement : les huit secteurs sont déjà posés sur la
ligne de contact de 2014, Crimée et Donbass compris, et aucun hexagone ne bouge tant que la phase de
front est sautée.

---

## 3. Correspondance tour / trimestre

`StartYear = 2021`, `StartSeason = Autumn`, et **`TurnCount = 26`** — voir §15 pour la justification
de la durée. Le décalage d'un cran fait basculer la saison de départ, si bien que les hivers restent
alignés : ce sont **T2, T6, T10, T14, T18 et T22**. Les campagnes contre le réseau, programmées aux
tours 6, 10 et 14, tombent donc toujours en hiver, et se préparent à l'automne qui précède.

| Tour | Trimestre | Saison | Tour | Trimestre | Saison |
|---|---|---|---|---|---|
| **T1** | **2021 T4** | **automne** | T13 | 2024 T4 | automne |
| T2 | 2022 T1 | hiver | T14 | 2025 T1 | hiver |
| T3 | 2022 T2 | printemps | T15 | 2025 T2 | printemps |
| T4 | 2022 T3 | été | T16 | 2025 T3 | été |
| T5 | 2022 T4 | automne | T17 | 2025 T4 | automne |
| T6 | 2023 T1 | hiver | T18 | 2026 T1 | hiver |
| T7 | 2023 T2 | printemps | T19 | 2026 T2 | printemps |
| T8 | 2023 T3 | été | T20 | 2026 T3 | été |
| T9 | 2023 T4 | automne | T21 | 2026 T4 | automne |
| T10 | 2024 T1 | hiver | T22 | 2027 T1 | hiver |
| T11 | 2024 T2 | printemps | T23 | 2027 T2 | printemps |
| T12 | 2024 T3 | été | T24 | 2027 T3 | été |
| | | | T25 | 2027 T4 | automne |
| | | | T26 | 2028 T1 | hiver |

Le calendrier pétrolier a gagné sept entrées : une en tête pour l'automne 2021 — le Brent tenait
alors environ **80 dollars** — et six en queue, autour de soixante dollars, pour les trimestres
d'étranglement et d'après-guerre.

---

## 4. La règle : une carte par camp et par trimestre

Un camp joue **une** carte par trimestre. Pas deux, pas cinq. C'est une règle de jeu, pas une
préférence d'affichage : une main de six cartes au-dessus d'un trimestre qui en joue six ne présente
aucun choix, et le tonneau de Liebig n'a plus rien à arbitrer si tout arrive en même temps. Le
trimestre est l'unité de décision du jeu ; il ne peut porter qu'une décision.

### Comment la règle est tenue

Le calendrier n'est plus une liste, c'est **une table de créneaux par camp** : le trimestre est la
clé, et écrire une deuxième carte dessus **remplace** la première au lieu de s'y ajouter. La règle
n'est donc pas une discipline à respecter, c'est la forme de la donnée. Une variante ne s'ajoute pas
au calendrier : elle **réécrit** les créneaux où l'Occident a décidé autrement, ce qui rend les trois
déroulés comparables carte à carte.

Quatre tests la verrouillent malgré tout (`CalendarRuleTests`), parce qu'une forme de donnée reste
une convention et qu'une convention est à un remaniement près d'être perdue : aucun trimestre ne
porte deux cartes du même camp, aucune carte jouée n'est sans propriétaire, aucun code programmé
n'est absent du deck — une faute de frappe programmerait un trimestre vide sans que rien ne le
signale — et le deck reste au moins deux fois plus large que ce qui est joué.

### Ce qui compte comme une décision

Le critère d'arbitrage, appliqué trimestre par trimestre : **la carte est-elle ce qui a changé la
génération de force ce trimestre-là ?** Une mobilisation, un paquet d'aide, un tour de vis sur les
composants, une conversion industrielle, un fournisseur étranger qui s'ouvre — oui. Le reste, non.

| Écarté du calendrier | Pourquoi |
|---|---|
| La météo et la saison — `harsh_winter`, `mild_winter`, `rasputitsa` | Une saison n'est pas une décision. Elle ne peut pas consommer le trimestre de celui qui la subit. |
| Les conséquences — `failed_offensive`, `armed_mutiny` | Une offensive qui s'enlise est le résultat d'une posture, et la posture est déjà dans le décalage de doctrine du T8. Une mutinerie dans le camp d'en face n'est pas une carte qu'on joue. |
| La couverture et le prétexte — `zapad_exercises`, `ultimatum_to_nato`, `intelligence_warning` | Ils habillent la manœuvre, ils ne la sont pas. |
| Le saupoudrage répété — `decoy_saturation` (six fois), `state_propaganda_surge` (quatre), `domestic_repression` (trois) | Six vagues de leurres en six trimestres, c'est la routine de la guerre, pas six décisions. |
| Les frottements diplomatiques — `diplomatic_complaisance`, `attention_elsewhere`, `european_election_swing`, `inflation_surge` | Ils pèsent sur la volonté des soutiens, mais aucun ne décide d'un trimestre à lui seul. |

---

## 5. Le socle commun — les deux tables de créneaux

Vingt-trois trimestres jouent ; les trois derniers sont l'épilogue et ne jouent rien.

| Tour | Trimestre | Ce que joue l'Ukraine et ses soutiens | Ce que joue la Russie |
|---|---|---|---|
| **T1** | automne 2021 | Les premières livraisons défensives | L'amassement |
| T2 | hiver 2022 | Premier train de sanctions | Tour de vis intérieur |
| T3 | printemps 2022 | L'aide occidentale s'organise | Conversion en économie de guerre |
| T4 | été 2022 | Frappe de précision longue portée | Drones achetés à l'étranger |
| T5 | automne 2022 | Contre-offensives d'automne | Mobilisation partielle |
| T6 | hiver 2023 | Plafonnement du prix du baril | Campagne contre le réseau électrique |
| T7 | printemps 2023 | Embargo sur les machines-outils | Les composants passent par ailleurs |
| T8 | été 2023 | Drones navals en mer Noire | La chaîne de montage domestique |
| T9 | automne 2023 | Embargo sur les machines-outils *(reconduit)* | Fournisseur étranger de munitions |
| T10 | hiver 2024 | Production nationale de drones | Blocage budgétaire du principal soutien |
| T11 | printemps 2024 | Drones sur les raffineries | Bombes planantes |
| T12 | été 2024 | Coalition drones | Transfert de licence |
| T13 | automne 2024 | Prêt gagé sur les avoirs gelés | Drones à fibre optique |
| T14 | hiver 2025 | Frappes sur les dépôts | Campagne contre le réseau électrique |
| T15 | printemps 2025 | Abaissement de l'âge de mobilisation | Montée en gamme du brouillage |
| T16 | été 2025 | Drones sur les raffineries | Prime d'engagement |
| T17 | automne 2025 | Interception à bas coût | Flotte fantôme |
| T18 | hiver 2026 | Production nationale de drones | Pourparlers sans lendemain |
| T19 | printemps 2026 | Interdiction du rail | Assauts d'infanterie à découvert |
| T20 | été 2026 | Interception à bas coût | Assauts d'infanterie à découvert |
| T21 | automne 2026 | Frappes sur les dépôts | Ponction sur le fonds souverain |
| T22 | hiver 2027 | Interception à bas coût | Impôt de guerre |
| T23 | printemps 2027 | Drones sur les raffineries | Assauts d'infanterie à découvert |

### Trois trimestres qui portent une leçon à eux seuls

**T7 et T9 : le duel de l'embargo.** L'Ukraine pose l'embargo sur les machines-outils au T7 ; la
Russie répond le trimestre même par « Les composants passent par ailleurs », une contre-carte, et
l'embargo ne produit rien — il est joué, il est vu, il ne sert à rien. L'Ukraine le reconduit au T9,
la Russie a dépensé son trimestre ailleurs, et cette fois il aboutit. **Une sanction qu'on
n'entretient pas se contourne** : la règle est désormais visible à l'écran plutôt qu'écrite dans un
document. C'est la seule contre-carte du socle, et elle suffit à rendre le mécanisme atteignable.

**T5 : la mobilisation partielle.** Le seul trimestre où la Russie ajoute trois cent mille hommes
d'un coup. Rien d'autre ne se joue de son côté ce trimestre-là, et c'est juste : mobiliser est une
décision qui mange son trimestre.

**T16 : la prime d'engagement.** L'armée de contrat se rachète tous les trimestres, et chaque hausse
de la prime retire un homme au budget des obus. C'est la carte qui, à elle seule, empêche l'armée
russe de gonfler jusqu'à un format que sa caisse ne peut plus alimenter — voir §11.

---

## 6. Variante `Holds` — le soutien tient, sans plus

```csharp
defender[12] = "aid_unblocked";
invader[15] = "budget_fatigue";
invader[18] = "parliament_veto";
```

Trois créneaux réécrits sur quarante-six. La crise des munitions de 2023-2024 est conservée telle
quelle : la Russie coupe le robinet au T10 — c'est sa carte du trimestre —, l'Ukraine le rouvre au
T12. S'y ajoutent deux frottements russes qui ne rompent rien : le soutien s'amaigrit et se
conditionne, il ne s'arrête jamais.

## 7. Variante `Collapses` — le soutien s'arrête

```csharp
invader[7] = "aid_collapse";
invader[10] = "meat_assault";
```

Rien avant le T7 : les six premiers trimestres restent **strictement identiques** à ceux de `Holds`,
c'est la démonstration elle-même. La coupure prend le créneau russe du T7, et la bascule électorale
américaine qui l'accompagnait retourne au deck — `aid_collapse` porte déjà les trente-cinq points de
volonté perdus, et deux cartes pour dire la même chose n'en font pas une décision de plus.

Au T10, il n'y a plus d'aide à bloquer : la Russie dépense ce trimestre-là sur le front. C'est la
seule divergence de calendrier hors coupure, et elle est une conséquence de la coupure.

## 8. Variante `Resolve` — l'Occident joue ses cartes

```csharp
defender[11] = "aid_unblocked";
defender[12] = "component_embargo_total";
defender[13] = "refinery_campaign_sustained";
defender[14] = "frozen_assets_released";
defender[16] = "refinery_campaign_sustained";
defender[17] = "major_oil_sanctions";
defender[19] = "refinery_campaign_sustained";
defender[21] = "sovereign_fund_empty";
defender[22] = "oil_price_crash";
defender[23] = "elite_break";
```

**Dix trimestres sur vingt-trois.** C'est le prix d'une théorie de la victoire quand un trimestre ne
porte qu'une décision — et c'est la meilleure chose que la règle ait apportée au jeu. Ce que
l'Occident ne joue pas se lit aussi bien que ce qu'il joue : ces dix trimestres sont dix trimestres
d'intercepteurs, d'ateliers de drones et de frappes sur les dépôts que l'Ukraine n'a pas, et elle
tient la ligne sur les huit qui lui restent. **L'arbitrage est le jeu.** Dans l'ancien calendrier,
l'Occident jouait l'étranglement *en plus* du reste : cela ne coûtait rien, donc cela ne décidait de
rien.

La campagne sur le raffinage revient **tous les trois trimestres** — T13, T16, T19 — et jamais tous
les trimestres : le dégât cicatrise entre deux passages, et c'est cet intervalle qui fait durer
l'étranglement des années. Les trois derniers trimestres sont l'endgame, et il est financier : le
fonds qui comblait le trou (T21), puis le baril (T22), puis l'appareil (T23). Aucune de ces cartes ne
prend un hexagone.

Deux cartes de l'ancien amas final sont retournées au deck, `supplier_withdraws` et
`currency_collapse` : le trimestre de la décision était une main de six cartes, il en porte une, et
c'est `elite_break` — la seule qui dise ce qui se passe réellement, à savoir que l'appareil se
fracture parce qu'il n'est plus payé.

### Le durcissement de 2025, et pourquoi +0,5

`major_oil_sanctions` — « Les deux majors désignées » — comble le trou signalé par l'audit de
réalisme : la décote sur l'Oural s'annulait dans le modèle alors qu'elle s'est réélargie dans le réel.

L'ampleur a été vérifiée aux sources plutôt que reprise de l'audit, et elle les confirme. La décote
Oural / Brent tenait **11 à 12 dollars** le baril avant le 22 octobre 2025 ; la désignation de
Rosneft et de Lukoil par l'OFAC l'a portée à **20 dollars** en quelques semaines, puis à **23,51
dollars** en novembre — le plus large écart depuis mars 2023 — les acheteurs réclamant entre 23 et
35 dollars pour accepter le risque. En parallèle, le dix-huitième paquet européen du 19 juillet 2025
abaissait le plafond de 60 à 47,60 dollars au 3 septembre, avec révision automatique à 15 % sous le
marché tous les six mois.

Le durcissement vaut donc **9 à 12 dollars de décote supplémentaire**. Dans le moteur,
`ExportDiscountPerBarrel = PriceSeverity × 22` : un `SanctionsPriceDelta` de **+0,5** ajoute très
exactement onze dollars au baril, soit le milieu de la fourchette observée. La carte porte en plus un
`SanctionsFrictionDelta` de +0,2, parce que ce qui s'est produit n'est pas seulement un rabais
consenti : les raffineurs indiens et chinois ont réduit leurs enlèvements, et cela se lit en volume,
pas en prix.

Placement au **T17**, soit le quatrième trimestre 2025 : le trimestre exact de la désignation. Elle
est réservée à la variante `Resolve` — un durcissement de cette ampleur n'a aucun sens dans un monde
où le soutien « tient sans plus », et la partie est déjà finie au T11 dans celui où il s'arrête.

---

## 9. Cartes volontairement laissées hors calendrier

**Cinquante-quatre cartes sur cent une ne sont jouées à aucun tour d'aucun déroulé.** Ce n'est pas un
oubli, c'est le but : le deck de la V2 doit être beaucoup plus large que la chronique de la V1, sinon
il n'y a rien à piocher et rien à choisir. Le rapport est aujourd'hui d'une carte jouée pour deux
cartes en main.

Elles se répartissent en cinq familles, et les trois premières viennent du passage à une carte par
trimestre.

| Famille | Exemples | Raison |
|---|---|---|
| Le décor du trimestre | `harsh_winter`, `mild_winter`, `rasputitsa`, `zapad_exercises`, `ultimatum_to_nato`, `intelligence_warning`, `no_mobilisation_yet` | Elles habillent le trimestre sans le décider — voir le critère en §4 |
| Les doublons d'intensité | `industrial_requisition`, `prison_recruitment`, `foreign_ballistic_missiles`, `decoy_saturation`, `state_propaganda_surge`, `allied_intelligence`, `nato_training_pipeline`, `transparency_reform`, `recruitment_reform`, `diplomatic_campaign` | Une carte du même créneau dit déjà la même chose, en plus lourd. Leur poids a été reversé dans celle qui reste — voir §11 |
| Les conséquences | `failed_offensive`, `armed_mutiny`, `anticorruption_crisis`, `european_election_swing`, `inflation_surge`, `attention_elsewhere`, `diplomatic_complaisance`, `us_election_swing` | Elles constatent, elles ne décident pas |
| Les contre-cartes sans cible | `refinery_air_defence`, `counter_intelligence`, `air_defence_surge`, `electronic_warfare`, `electronic_warfare_ukraine`, `counter_battery`, `evasion_network`, `rail_repair_brigades`, `decentralised_generation` | Leur cible n'est plus jouée le même trimestre. Elles existent pour le bluff de la V2, où c'est le joueur qui choisira de dépenser son trimestre à répondre |
| Le réservoir de la V1.1 et de la V2 | `oil_price_spike`, `global_recession`, `dam_breach`, `demographic_wall`, `mobilisation_wave_two`, `air_base_strikes`, `security_guarantees`, `decapitation_strike`, `drone_swarm_scaling`, `chinese_pressure` | Déjà hors calendrier avant ce chantier ; `oil_price_spike` et `global_recession` décalent définitivement tout le calendrier pétrolier et feraient doublon avec lui |

Une seule contre-carte survit dans le socle, `component_smuggling`, et c'est assez : le mécanisme
reste atteignable, le test qui le verrouille passe, et le duel du T7 le montre à l'écran.

---

## 10. Le duel de decks obéit désormais à la même règle

`DeckDuel` compare trois théories de la victoire à budget politique égal sur le déroulé du front
figé. Il **ajoutait** son deck au calendrier du défenseur ; il en **prend** désormais les créneaux,
comme n'importe quel joueur.

Le changement n'est pas cosmétique, il répare une mesure fausse. Empilé par-dessus, le deck
d'attrition frontale gagnait le duel pour la raison la plus triviale qui soit : il était sept cartes
**de plus**, pas sept cartes **autres**. On mesurait un budget, pas une théorie. Une fois qu'il doit
payer ses sept trimestres, il perd — et la frappe profonde gagne, ce que le document de conception
pose comme critère d'équilibre.

---

## 11. Résultats observés et réétalonnage

Mesuré sur le scénario intégré, suite de tests au vert.

| Variante | Issue | Tour de décision | Armistice |
|---|---|---|---|
| `Resolve` | chute du régime, puis armistice, victoire ukrainienne | **T23** — printemps 2027 | **T25** — automne 2027 |
| `Holds` | `frozen_front` | **T26** — personne ne rompt | — |
| `Collapses` | effondrement militaire ukrainien, puis armistice | **T11** — printemps 2024 | **T13** |

Les trois issues sont **exactement celles d'avant**, au trimestre près. C'est le résultat qui
comptait : la règle n'a pas coûté la démonstration.

### Ce que le réétalonnage a coûté

Passer de cent cinquante-deux cartes à quarante-six retire de l'effet cumulé des deux côtés, et
jamais dans les mêmes proportions. Cinq valeurs de cartes et une constante de scénario ont bougé.
Aucune carte n'a été ajoutée au calendrier pour compenser : **une décision par trimestre qui pèse
lourd est plus juste qu'un saupoudrage**, et c'est la règle qui a guidé chaque ajustement.

| Ce qui a changé | Avant | Après | Pourquoi |
|---|---|---|---|
| `contract_recruitment_drive` — multiplicateur de coût de recrutement | 1,5 | **2,4** | La carte était jouée trois fois (1,5³ ≈ 3,4), elle l'est une. Sans cela l'armée de contrat russe ne coûte plus assez cher, la Russie tient sept cent mille hommes que sa caisse ne peut plus munitionner, et le front figé se met à bouger. **C'est l'ajustement le plus structurant de tous.** |
| `meat_assault` — obus russes détruits | 320 | **480** | Sept passages dans l'ancien calendrier, trois désormais. Un trimestre d'assauts à découvert consomme un trimestre d'obus, pas un dixième |
| `western_aid_opens` — aide promise | 11 Md | **16 Md** | L'aide occidentale de 2022 arrivait en six cartes étalées sur quatre trimestres ; elle arrive en une |
| `refinery_campaign_sustained` — intégrité du raffinage | −0,34 | **−0,40** | Quatre passages dans l'ancien calendrier, trois désormais, et l'étranglement doit rester assez profond pour que la puissance russe tombe sous la moitié de son pic avant la chute |
| `oil_price_crash` — cohésion des élites | −9 | **−5** | La carte doit couper la caisse, pas fracturer l'appareil : c'est le travail d'`elite_break` au trimestre suivant. À −9 le régime tombait au T22, un trimestre trop tôt |
| `ukraine.Industry.DepotQuartersHeld` | 3 | **3,2** | Les deux trimestres de latence que promet la variante `Collapses` viennent de ce dépôt. Une fois le remplissage retiré, il manquait le second **de quatre obus** — le trimestre à couvrir est celui de l'offensive d'été 2023, dont la posture augmente la consommation d'un quart |

`major_oil_sanctions` **n'a pas bougé** et reste à +0,5, la valeur calée sur sources en §8 : onze
dollars de décote supplémentaire, le milieu de la fourchette observée après la désignation de
Rosneft et de Lukoil. Il aurait été commode de la pousser à +0,7 pour gagner de la marge sur
l'étranglement ; c'eût été sortir de ce que les sources soutiennent pour un confort de calibration.
La profondeur manquante a été prise sur la campagne de raffinage, dont la valeur est de travail.

### Ce qui a bougé malgré nous

**La puissance de combat russe forme un plateau au lieu d'une pente**, du T16 au T20, à trois cent
trente-trois mille hommes. L'armée est à son plafond d'effectif et sa couverture matérielle est
encore pleine : rien ne la fait descendre tant que le dépôt tient. Elle chute ensuite d'un bloc —
333 000 au T19, 291 000 au T20, 189 000 au T21, 163 000 au T22, 52 000 au T23. L'ancien calendrier produisait une décrue plus
régulière, parce que sept passages d'assauts à découvert et trois de frappes sur les dépôts la
grignotaient trimestre après trimestre. La nouvelle courbe est plus brutale et, à la réflexion, plus
fidèle au modèle : **l'effondrement est un seuil, pas une pente**, et c'est le dépôt qui décide du
moment où il se franchit.

**La ruée de 2022 va mille kilomètres carrés plus loin** — 2 365 km² au T8 contre 1 285 dans
l'ancien calendrier : l'Ukraine ne joue plus, en 2022, les trois ou quatre cartes de soutien qui l'aidaient à
tenir le sud. L'écart est dans le bruit d'un modèle qui reconnaît lui-même ne pas couvrir les axes de
Kyiv, Tchernihiv et Soumy — mais il est réel et il est signalé.

**Le déroulé « Épuisement politique » du duel de decks ne gagne plus.** Avec des créneaux à payer, la
décapitation et la fracture au sommet ne suffisent plus à faire tomber le régime dans le temps
imparti. Aucun test ne l'exigeait ; c'est néanmoins un déplacement d'équilibre à connaître, et il
mérite d'être rejugé quand la V2 rendra les decks jouables.

---

## 12. Points de vigilance

**Le capital politique redevient soutenable, et c'est la bonne surprise.** L'ancienne densité laissait
les deux camps en découvert profond — de l'ordre de cent trente points côté russe et trois cents côté
ukrainien —, ce qui condamnait l'économie de mana à être refondue avant la V2. Avec une carte par
trimestre, le découvert tombe à **trente-sept points côté russe et soixante-cinq côté ukrainien** sur
le déroulé de l'asphyxie. Ce n'est pas encore payable, mais l'écart n'est plus d'un ordre de grandeur :
relever la génération de deux à quatre points par tour suffirait à rendre le calendrier jouable tel
quel. La règle a fait, au passage, la moitié du travail que la V2 devait faire.

**`elite_fracture` reste au coût zéro** alors que toutes les autres cartes réattribuées ont reçu un
prix. Ce n'est pas un choix de conception : le deck « Épuisement politique » de `DeckDuel` est calibré
à exactement quarante-quatre points de capital, et cette carte y compte pour rien. Lui donner un coût
casse le test `NoDeckIsDominant_TheyAreComparedAtEqualPoliticalCost` tant que `DeckDuel.cs` n'est pas
rééquilibré en regard.

**Le vocabulaire d'effets manque de trois entrées** pour que les cartes budgétaires russes
fonctionnent : `ReservesDelta` (le fonds souverain n'est pas atteignable), `FiscalCaptureDelta` (un
impôt de guerre ne peut pas augmenter ce que le trimestre finance), et un moyen de brûler des hommes.
`TreasuryDelta` est par ailleurs quasi inerte côté russe : le budget de guerre est plafonné par le PIB
et par les recettes du trimestre, et la trésorerie est réinitialisée à chaque tour. `war_tax_rise`,
jouée au T22, ne rapporte donc presque rien — elle est au calendrier pour ce qu'elle coûte en
consentement, ce qui est un demi-usage et se voit.

**La marge de l'asphyxie est mince.** La puissance russe à la veille de la chute vaut quarante-neuf
pour cent de son pic, contre les cinquante que le test exige au maximum. Un point. Toute modification
du moteur qui touche à la production, au budget de guerre ou au dépôt doit être suivie d'une
relecture de ce chiffre, et non du seul « les tests passent ».

---

## 13. Ce que l'audit de réalisme change pour le deck

L'audit corrige les conditions dans lesquelles les cartes agissent, sans toucher aux cartes. Une
carte se lit donc autrement selon ce qui l'entoure, et trois familles changent de poids.

**Le doublon corrigé.** `machine_hall_strikes` reprenait mot pour mot la thèse de `grid_campaign` —
« on ne vise plus les sous-stations mais les salles des machines » — c'est-à-dire la même carte
écrite deux fois. Elle est devenue `substation_strikes`, « Les postes de raccordement », sur la seule
cible que le modèle décrit et que rien ne couvrait : on ne frappe pas la centrale nucléaire, on
frappe le poste qui la relie au pays. Ses dégâts passent de 4,5 à 3,0 GW, l'ordre de grandeur de deux
ou trois tranches débranchées.

**Les magasins d'intercepteurs lourds : rien à changer, et pour une bonne raison.** Un magasin
d'intercepteurs ne se dimensionne pas sur ce que l'usine produit mais sur **ce qu'il tire** — sinon
la défense antiaérienne ukrainienne serait calibrée sur douze pièces par trimestre et le ciel serait
ouvert. Le plafond retient le maximum entre production et consommation constatée, et le magasin lourd
ukrainien évolue en pratique entre 76 et 525 unités. Les quatre-vingt-dix unités que retire
`decoy_saturation`, les cent soixante d'`air_defence_gap`, les cent quatre-vingts
d'`attention_elsewhere` et les deux cent vingt d'`air_defence_surge` prélèvent donc environ trente
pour cent d'un magasin plein — et la totalité d'un magasin déjà entamé. C'est mieux qu'un pourcentage
fixe : la même carte fait mal quand la défense est usée et coûte peu quand elle vient d'être
reconstituée.

**Les cartes de frappe ont été rescalées d'un facteur trois à six.** Un magasin de drones est
intégralement consommé et reconstitué à chaque tour : sa taille vaut une production trimestrielle, et
celle-ci a triplé côté russe. Même logique pour l'interception à bas coût ukrainienne, dont la
capacité passe de 1 100 à 7 000 par trimestre — l'ancienne valeur représentait douze pièces par jour
quand l'Ukraine en livre mille à mille cinq cents. Les valeurs ont donc été remises à l'échelle pour
conserver leur poids relatif :

| Carte | Ressource | Avant | Après |
|---|---|---|---|
| `decoy_saturation` | drones russes | −600 | **−2 000** |
| `grain_port_strikes` | drones russes | −350 | **−1 150** |
| `foreign_drones` | drones russes | +500 | **+1 650** |
| `decoy_saturation` | interception bas coût | −450 | **−2 850** |
| `air_defence_gap` | interception bas coût | −500 | **−3 200** |
| `cheap_interception` | interception bas coût | +1 400 | **+8 900** |
| `depot_strikes` | drones ukrainiens | −220 | **−550** |
| `air_base_strikes` | drones ukrainiens | −250 | **−650** |

**Les cartes anti-raffinage mordent plus fort à l'impact, pas plus longtemps.** Contrairement à ce
que cette section affirmait, la réparation trimestrielle **n'a pas** été ramenée à 25 % : elle reste
à 40 %, et c'est délibéré — c'était le seul point de l'audit qui déplaçait l'effondrement russe d'un
trimestre sans compensation possible. Ce qui a doublé, c'est la profondeur de chaque vague, le
coefficient de dommage sur le raffinage passant de 0,09 à 0,18. Les sept centièmes d'intégrité que
retirent `naval_drones_black_sea` et `pipeline_sabotage` continuent donc de cicatriser vite et **ne
s'accumulent pas** : ces cartes gagnent en impact immédiat, pas en persistance. Une campagne sur le
raffinage reste un entretien, jamais un acquis — ce qui est précisément la règle que le modèle
défend.

**Un écart mineur, signalé sans être corrigé.** La capacité russe en obus est passée de 700 à 560 par
trimestre, ce qui renchérit d'un quart le poids relatif des cartes qui détruisent des obus russes —
`meat_assault` et `depot_strikes`. Vingt-cinq pour cent, contre deux cent trente et cinq cent trente
pour les rescalages ci-dessus : ce n'est pas du même ordre, et la corriger déplacerait le déroulé
pour un gain de justesse négligeable.

**Le réseau ukrainien est enfin franchissable.** Avec 26 GW opérables contre 18,85 GW de demande
hivernale, la marge est de 7,15 GW : un seul passage de `grid_campaign` ne coupe rien, deux
franchissent le seuil. Le calendrier en programme exactement **deux**, aux tours 6 et 14 — deux
hivers —, donc les coupures nationales apparaissent au second, ce qui est le rythme que le modèle
décrit et qu'aucune partie ne produisait. Les deux passages retirent 9 GW sur les 14,3 que le
plafond autorise ; la marge restante est de cinq gigawatts, c'est-à-dire d'un seul jeu de
`substation_strikes`, et il n'y a pas de place pour un troisième passage de campagne.

C'est le seul endroit où la règle d'une carte par trimestre a rendu une lecture *moins* dure qu'avant
— trois campagnes contre deux — et la coupure hivernale continue pourtant de se produire. Le seuil
est franchi, ce qui est ce que le modèle avait à démontrer ; il l'est avec moins de marge.

**Une bonne nouvelle, enfin.** Les cartes de mobilisation — `partial_mobilisation`,
`mobilisation_wave_two`, `prison_recruitment`, `conscription_law` — ne coûtaient jusqu'ici que du
consentement, puisque la règle du minimum ne mordait jamais et que le front ne manquait de rien.
Une fois les dépôts capables de se vider, elles produisent enfin ce que le modèle annonce depuis le
début : mobiliser quand le goulot est l'obus ampute le PIB, donc les recettes, donc la production
d'obus. Le piège central du scénario devient jouable.

---

## 14. Le front de 2022 — la densité, et non un script

### Le défaut qui a lancé ce chantier

La carte du tour 1, automne 2021, affichait le territoire occupé de 2024 : le corridor terrestre
complet vers la Crimée, Kherson et Zaporijjia sous contrôle russe, des pions engagés à Bakhmout,
Pokrovsk et Vouhledar, une légende « Conquis depuis février 2022 » — trois mois avant l'invasion — et
un bandeau d'arrière russe annonçant la saturation d'une campagne de frappes sur le raffinage.

Ce que ce tour devrait montrer : **la ligne de 2014, et rien d'autre.** La Crimée et les deux
enclaves du Donbass, de l'ordre de quarante-trois mille kilomètres carrés.

### La cause : cinq ancrages sur huit sont posés sur la ligne d'aujourd'hui

`hexmap.js` trace la ligne de contact à partir des ancrages de secteur — `lon + pushLon × hexes` — et
colore en occupé tout ce qui se trouve à l'est. Au tour 1 le cumul d'hexagones vaut zéro : la carte
affiche donc exactement les ancrages. Or le commentaire de `FrontSector` les annonce sur « la ligne
de contact, février 2022 » alors qu'ils sont posés, pour la plupart, sur la ligne actuelle.

| Secteur | Ancrage actuel | Ancrage de février 2022 | Écart |
|---|---|---|---|
| `kharkiv` | 36,95 / 50,05 | 36,95 / **50,35** — la frontière d'État | ~33 km |
| `kupiansk` | 37,75 / 49,55 | **38,20** / 49,55 — la frontière d'État | ~33 km |
| `lyman` | 38,05 / 49,00 | **38,70** / 49,00 — à l'est de Kreminna | ~47 km |
| `bakhmut` | 38,15 / 48,60 | **38,45** / 48,55 — Debaltseve | ~22 km |
| `pokrovsk` | 37,80 / 48,25 | 37,75 / **48,14** — Avdiïvka | ~12 km |
| `vuhledar` | 37,30 / 47,75 | 37,40 / 47,80 — Marïnka–Chyrokyne | ~9 km |
| `zaporizhzhia` | 35,90 / 47,45 | **aucun front n'existait** | — |
| `kherson` | 33,40 / 46,75 | **33,70 / 46,16** — l'isthme de Perekop | ~67 km |

Coordonnées de travail, justes à une dizaine de kilomètres : la ligne de 2014 est connue, son tracé
exact ne l'est pas au degré près. Deux secteurs — `pokrovsk` et `vuhledar` — sont déjà presque au bon
endroit, ce qui explique que le Donbass soit à peu près crédible et que le sud ne le soit pas du tout.

### La trajectoire de 2022

| Trimestre | Ce qui se passe | Occupé au-delà de la ligne de 2014 |
|---|---|---|
| T1 · automne 2021 | Rien. La ligne de 2014 | position de départ |
| T2 · hiver 2022 | L'invasion sur quatre axes : Kyiv, Kharkiv, le Donbass, le sud depuis la Crimée | pic à ~120 000 km² à la mi-mars |
| T3 · printemps 2022 | Retrait du nord — Kyiv, Tchernihiv, Soumy — et chute de Marioupol : le corridor terrestre se referme | ~90 000 km² |
| T4 · été 2022 | Front figé, HIMARS, les dépôts reculent de quatre-vingts kilomètres | ~90 000 km² |
| T5 · automne 2022 | Kharkiv repris en une semaine, puis retrait de Kherson rive droite | ~78 000 km² |

### Les rapports de force ne suffiront pas, et l'arithmétique le dit avant toute simulation

Les huit secteurs totalisent quarante-huit hexagones de largeur, soit quatre cent quatre-vingts
kilomètres de front. Un hexagone d'avance sur toute la ligne vaut donc 4 800 km².
`FrontPhase.MovementFor` plafonne l'avance à **trois hexagones par trimestre** — trente kilomètres,
et le commentaire précise que c'est déjà une percée historique. Même en portant le rapport de force
au-delà de trois pour un sur les huit secteurs à la fois, le maximum atteignable est de
**14 400 km² par trimestre** ; si la défense rompt, le multiplicateur d'effondrement porte ce plafond
à 50 400 km².

Le pic de mars 2022 demanderait vingt-cinq hexagones de profondeur — deux cent cinquante kilomètres —
sur tout le front et en un seul trimestre. **Un effondrement ukrainien total, sur les huit secteurs
simultanément, en produirait quarante-deux pour cent.** Et il ne se produirait pas : le multiplicateur
de rupture ne s'applique qu'à un défenseur déjà passé sous le seuil pendant plusieurs trimestres, ce
qui n'est pas le cas au deuxième tour d'une partie.

Trois causes se cumulent, et aucune ne relève des cartes ni du calendrier :

1. **La géographie manque.** Les axes de mars 2022 — Kyiv, Tchernihiv, Soumy — ne sont pas des
   secteurs. Zaporijjia et Kherson, eux, existent comme secteurs alors qu'aucun front n'y passait.
2. **Le plafond de mouvement est calibré pour l'usure.** Trente kilomètres par trimestre, quand les
   colonnes russes ont couvert cent à deux cents kilomètres en trois semaines vers Kherson et
   Melitopol.
3. **La table de ratio ne peut pas être franchie assez fort** par la doctrine d'ouverture.

### Ce qui a été fait, et ce que cela produit

Les trois étapes sont en place et mesurées.

**Les huit ancrages sont reposés sur la ligne de février 2022.** Correction de données, aucune
physique : le tour 1 dessine désormais la Crimée et les deux enclaves du Donbass, et rien d'autre.
Les vecteurs de poussée ont été réajustés pour que quatre ans d'avance modélisée ramènent la ligne
là où elle se trouve aujourd'hui.

**`CombatStartsOnTurn` saute la phase de front et la phase de frappe** tant que le tour lui est
inférieur. Le prologue ne résout plus aucun secteur, ne lance aucune frappe et ne coûte plus un
homme — mesuré : zéro résolution, zéro perte, aucune campagne. Les huit autres phases tournent
normalement, ce qui est justement le propos du trimestre. Le drapeau vaut un par défaut, donc un
scénario qui ouvre sur sa guerre ne change pas de comportement.

**Le plafond de pénétration dépend de la densité.** `FrontPhase.PenetrationCeiling` lit les hommes
en contact du tenant par kilomètre de secteur, les compare à une ligne tenue, et divise par ce qui
donne de la profondeur — les tranchées et les drones. Le résultat plafonne le mouvement au-delà d'un
rapport de trois pour un, et **il ne peut qu'ouvrir le plafond, jamais le fermer** : trois hexagones
restent le plancher, donc la guerre d'usure est protégée par construction.

#### Deux obstacles trouvés en codant, et que la conception n'avait pas vus

**La doctrine offensive n'est pas une doctrine défensive.** `DefensiveCover` répartissait la
couverture uniformément ; brancher la densité sur `SectorEffort` revenait à lire, côté russe, où
elle *attaque* comme si c'était là qu'elle *tient*. Résultat : l'Ukraine perçait partout où la
Russie n'attaquait pas. Il a fallu un poids distinct, `Doctrine.SectorDefence`, vide par défaut —
et vide, il redonne exactement l'ancienne répartition uniforme, donc le changement est neutre tant
qu'un scénario ne dit rien.

**Une ligne vide ne suffit pas si les réserves arrivent toujours.** La couverture défensive déplace
jusqu'à 45 % de sa puissance vers la pression ennemie, ce qui annulait la concentration : le sud
recevait des réserves à l'instant même où il était attaqué, et le ratio ne franchissait jamais 1,1.
Or personne n'a redéployé en février 2022 — le sud est tombé en trois semaines, bien à l'intérieur
du délai de déplacement d'une réserve, et nul ne savait quel axe était la feinte.
`Doctrine.ReserveMobility` multiplie cette réactivité ; elle vaut un partout sauf au trimestre de
l'invasion, où elle tombe à 0,15.

#### Ce que le déroulé raconte maintenant

| Tour | Trimestre | km² cumulés | Ce qui se passe |
|---|---|---|---|
| T1 | automne 2021 | 0 | Le prologue. Aucun combat, aucune frappe |
| T2 | hiver 2022 | **+4 183** | La ruée : Zaporijjia 3,5 hexagones, Kherson 2,9 — le sud, jamais le Donbass |
| T3–T4 | 2022 | 4 183 | La ligne s'est formée, les réserves sont arrivées, plus rien ne bouge |
| T5 | automne 2022 | **+570** | Le reflux : Kharkiv repris sur 3 hexagones, Kherson ramené de 2,9 à 1,0 |
| T8–T22 | 2023-2026 | 2 365 → 2 463 | **Quinze trimestres, cent kilomètres carrés.** Le front est redevenu un thermomètre |
| T23 | printemps 2027 | 513 | Le régime tombe, et le terrain commence à revenir |
| T25 | automne 2027 | −3 796 | L'armistice : l'armée qui n'est plus payée a rendu tout ce qu'elle tenait |

**La séquence est le résultat qui compte.** Entre le pic de puissance russe au T16 et la veille de
la chute au T22, l'envahisseur passe de 333 000 à 163 000 hommes de puissance de combat **et la ligne
ne bouge pas d'un kilomètre** — 2 463 km² au T20 comme au T22. Le terrain ne revient qu'après. Le front n'est pas devenu le moteur de
l'histoire : il bouge pendant la manœuvre de 2022, se fige pendant toute la guerre d'usure, et ne
rebouge qu'à l'effondrement de l'arrière.

Témoin de non-régression, celui qui vérifie qu'on n'a pas cassé ce qui marchait : **Pokrovsk finit
à 2,3 hexagones**, exactement sa valeur d'avant le changement.

Les trois issues sont intactes — `Resolve` T23, `Holds` front figé au T26, `Collapses` T11 — et les
tous les tests passent, dont cinq qui verrouillent la ruée, le reflux, le prologue muet,
l'usure figée et l'ordre puissance-puis-terrain, et quatre qui verrouillent la règle d'une carte par
camp et par trimestre.

#### Ce que cela ne fait toujours pas, et qu'il faut assumer

Le pic de 2022 culmine à **4 183 km²** quand le réel approche 120 000. L'écart n'est pas dans le
mécanisme, il est dans la carte : les huit secteurs couvrent l'arc Kharkiv-Kherson, et les axes de
Kyiv, Tchernihiv et Soumy — l'essentiel du pic de mars — n'existent pas comme secteurs. Le modèle
raconte désormais la bonne histoire à la bonne date, avec la bonne cause, sur le théâtre qu'il
modélise. Il ne prétend pas mesurer un pays entier.

Reste aussi une imprécision de tracé : entre l'isthme de Perekop et la mer d'Azov, une ligne droite
à deux points passe à l'intérieur des terres, si bien que Berdiansk apparaît occupée au tour 1
alors qu'elle était ukrainienne. C'est la limite d'une ligne à huit points ; la corriger demanderait
un neuvième secteur côtier ou un tracé dans `geography.js`.

### Le scriptage a été écarté, et la règle si l'on devait y revenir

L'option a été examinée puis écartée : un script aurait produit la même carte en racontant le
contraire — que le terrain se perd parce que l'adversaire a été brillant, quand il se perd parce
qu'on n'a pas assez d'hommes pour le tenir. Si un mouvement devait malgré tout être scripté un jour,
la règle serait absolue et sans exception : **le passé rejoué et l'avenir
simulé ne sont pas la même chose, et le site est public.** Tout mouvement scripté doit être marqué
comme tel dans les données, affiché comme tel à l'écran, et distingué des trimestres que le modèle
calcule. Un visiteur informé doit pouvoir savoir, en un coup d'œil, quels tours sont une
reconstitution et quels tours sont un résultat. Un scénario maquillé en sortie de modèle
discréditerait tout le reste, y compris ce que le moteur calcule justement.

### Les deux défauts d'affichage annexes sont réglés

Le bandeau d'arrière russe annonçait « SATURATION » sur une campagne de frappes contre le raffinage,
et l'arrière ukrainien « réseau : 2,7 GW perdus », au tour 1 — sans qu'aucune frappe ait eu lieu.
C'était le même défaut que les vingt-neuf mille pertes relevées en §2, et `CombatStartsOnTurn` les a
réglés tous les trois d'un coup.

---

## 15. La durée de la partie : `TurnCount = 26`

Le décompte se lit en quatre blocs.

| Bloc | Tours | Contenu |
|---|---|---|
| Prologue | T1 | Automne 2021, la génération de force avant le premier coup de feu |
| La guerre | T2 à T20 | Les dix-neuf trimestres de l'invasion au présent, été 2026 |
| L'étranglement | T21 à T23 | Trois trimestres où la tension monte sans plateau, jusqu'à la chute du régime |
| L'après | T24 à T26 | Trois trimestres où plus aucune carte n'est jouée et où la ligne bouge quand même |

Vingt-six tours mènent à l'hiver 2028, et cette durée est **bornée aux deux bouts, sans marge**.
Trop courte, elle couperait l'après-chute avant l'armistice : celui-ci n'est pas un délai fixe mais
un seuil — l'armée brisée doit tomber sous 6 % de son effectif théorique. Trop longue, elle
détruirait la démonstration du front figé : porté à vingt-huit tours, le déroulé « le soutien tient »
finit par céder au T27, l'Ukraine s'effondrant à son tour. Vingt-six est la seule valeur où le
déroulé d'asphyxie atteint sa paix et où le front figé le reste — vérifié en jouant à vingt-huit,
où le premier s'arrête toujours au T26.

Le calendrier pétrolier a été porté de dix-neuf à
vingt-six entrées : quatre-vingts dollars en tête pour l'automne 2021, puis les dix-neuf trimestres
observés, puis six trimestres autour de soixante dollars — étant entendu que `oil_price_crash`,
joué au tour de la décision, y ajoute son propre décalage permanent.

Les décalages de doctrine ont glissé d'un cran eux aussi : les contre-offensives de Kharkiv et
Kherson au tour 5, l'offensive d'été au tour 8, le passage russe au grignotage au tour 10. Deux
décalages ont été ajoutés au tour 1 pour annuler les postures offensives des deux camps, et deux au
tour 2 pour les rétablir le jour de l'invasion.

### Les tours 24 à 26, et ce que le deck n'y joue pas

**L'essentiel de ces trois trimestres ne vient d'aucune carte, et c'est mesuré :** aux tours 24 à 26
les deux mains sont vides. Une armée qui n'est plus payée
rétrécit toute seule — `PayableForceSize` le fait déjà — et un front se dénoue sans assaut dès que la
puissance de l'un s'effondre pendant que celle de l'autre tient. Charger l'après-chute de cartes
reviendrait à scripter une conséquence que le modèle produit tout seul, ce qui est exactement ce
qu'il faut éviter : le meilleur épilogue est celui où le joueur ne voit **plus une seule carte
tomber** et regarde la ligne bouger quand même.

Trois cartes du deck y ont malgré tout leur place, et une seule est déjà programmée ailleurs :

| Carte | Camp | Ce qu'elle fait après la chute |
|---|---|---|
| `security_guarantees` | Ukraine | Aujourd'hui hors calendrier, elle attendait ce moment : une promesse écrite ne livre pas un obus, elle change ce qu'un pays ose planifier — c'est la carte de l'après, pas celle de la guerre |
| `ceasefire_talks` | Russie | Reprise du même code, dans un rapport de force inversé : cette fois c'est le camp qui perd qui a besoin que la ligne se fige |
| `currency_collapse` | Ukraine | Si elle n'a pas été jouée au T19, elle appartient à l'épilogue plutôt qu'à la guerre |

Il manquera vraisemblablement **une carte de sortie de guerre côté ukrainien** — le règlement
négocié depuis une position de force, distinct de `ceasefire_talks` qui est une manœuvre dilatoire.
Je ne l'écris pas encore : sa forme dépend entièrement de la façon dont le mécanisme de fin de guerre
décide qu'une guerre est finie, et l'inventer avant de l'avoir lu produirait une carte qui ne
s'accroche à rien.
