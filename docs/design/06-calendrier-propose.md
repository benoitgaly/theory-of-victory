# Calendrier — vingt-six trimestres, deux à quatre cartes par camp

> Le calendrier de `UkraineScenario.BuildCalendar`. Le deck étant passé de 41 à 101 cartes, le
> déroulé montre ce que chaque camp décide **chaque** trimestre, au lieu d'une carte isolée entourée
> de remplissage. La partie ouvre à l'automne 2021, un trimestre avant l'invasion, et se poursuit
> au-delà de la décision jusqu'à l'armistice.
>
> **Statut : intégré et vérifié.** Le calendrier est dans le scénario et les trois issues tombent où
> elles doivent tomber — le régime russe cède au **T23**, printemps 2027 ; le front reste figé jusqu'au
> **T26** ; l'Ukraine s'effondre au **T11**, printemps 2024. Détail et mesures en §11.
>
> Ce sont les cartes qui fixent ces dates, pas les constantes : un balayage systématique du moteur
> n'avait jamais réussi à déplacer la chute russe hors de la fourchette T17-T19. Le régime tombe
> quand tombe l'amas de fin — fonds vidé, fracture des élites, baril effondré, fournisseur retiré.

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

---

## 2. Le prologue : la guerre se prépare avant de se jouer

La partie ouvre à l'**automne 2021**, un trimestre avant l'invasion. C'est le seul tour du déroulé où
aucun coup de feu n'est tiré, et c'est le plus démonstratif de tous : le joueur y voit une guerre se
gagner ou se perdre dans les dépôts, avant que la carte ne bouge d'un hexagone.

Six cartes s'y jouent, trois par camp, et leur asymétrie **est** le message.

La Russie masse cent mille hommes, remplit les dépôts avancés en obus et en carburant, couvre le tout
d'un exercice annoncé et régulier, puis adresse à l'OTAN un ultimatum dont elle sait qu'il sera
refusé — le refus n'est pas l'échec de la démarche, il en est le produit. `force_concentration` est
un rituel lent : les hommes arrivent au front **au tour suivant**, c'est-à-dire le jour de
l'invasion. La force est constituée un trimestre avant d'être employée, et cela se lit à l'écran.

En face, le renseignement allié donne la date, les axes et les effectifs, et il ne se passe rien :
`intelligence_warning` produit une promesse d'aide d'un milliard et cinq points de volonté, pas une
armée. Les premières livraisons sont des missiles antichars portables — de quoi n'être pas complice,
pas de quoi dissuader. Et l'Ukraine ne mobilise pas : `no_mobilisation_yet` préserve son économie et
son consentement, et n'ajoute **pas un homme** au front.

> Le prologue oppose donc un camp qui convertit son économie en force et un camp qui préserve son
> économie. Au tour deux, l'un a cent mille hommes de plus sur la ligne et l'autre a un PIB intact.
> C'est toute la thèse du jeu en un seul écran, sans un coup de feu.

Les six textes portent une même leçon, et elle n'est pas datée : **tout était visible.** Les convois
se comptaient, les hôpitaux de campagne montaient vers la frontière, le renseignement donnait la date
et les axes. Ce qui a manqué n'est pas l'information, c'est la décision — croire engage, douter ne
coûte rien, jusqu'au matin où le doute se paie.

### Ce que le moteur doit faire pour que le prologue tienne sa promesse

**Le tour 1 ne doit produire ni combat, ni mouvement, ni frappe en profondeur.** Ce n'est pas acquis :
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

## 4. Lecture chronologique du socle

| Tour | Ce que joue l'Ukraine et ses soutiens | Ce que joue la Russie |
|---|---|---|
| **T1** | **L'avertissement que personne ne croit, les premières livraisons défensives, l'ordre de mobilisation qui n'est pas donné** | **L'amassement, les manœuvres d'automne, l'ultimatum de décembre** |
| T2 | Premier train de sanctions, renseignement allié, filières de formation, la boue de mars | Verrouillage intérieur, saturation de l'espace médiatique |
| T3 | L'aide s'industrialise, la conditionnalité s'installe, le recrutement se réforme | Réquisition des chaînes, bascule vers l'économie de guerre |
| T4 | HIMARS, les dépôts reculent de quatre-vingts kilomètres, un gazoduc saute | Prisons ouvertes, premiers drones achetés à l'étranger |
| T5 | Kharkiv et Kherson, les drones navals entrent en mer Noire | Mobilisation partielle, missiles achetés, premières vagues de leurres |
| T6 | Plafonnement du baril, campagne diplomatique, un hiver clément qui annule la campagne d'en face | Frappes sur le réseau, bombes planantes, barils réorientés vers l'Asie |
| T7 | Embargo sur les machines-outils, interception à bas coût, mutinerie armée à Rostov | L'embargo est contourné le trimestre même, le brouillage passe à l'échelle |
| T8 | L'offensive d'été s'enlise, la production nationale de drones démarre | La chaîne de montage domestique, les ports céréaliers, l'inflation chez les donateurs |
| T9 | Le train de sanctions est reconduit — une sanction est un entretien —, la raspoutitsa | Obus étrangers, primes d'engagement, flotte fantôme, une autre guerre capte l'attention |
| T10 | Loi de mobilisation, interception à bas coût, ateliers de drones | Réseau, leurres, primes |
| T11 | Raffineries, coalition drones, brouillage des kits de guidage | Licence transférée, bombes planantes, assauts à découvert, bascule électorale européenne |
| T12 | Rail coupé, nouvelle frappe profonde | La contre-batterie annule la frappe : la carte est jouée pour rien |
| T13 | Prêt gagé sur les avoirs gelés, réseaux îlotés | Fibre optique, brouillage, postes de raccordement — les deux sont contrées |
| T14 | Sanctions sur la flotte fantôme | Le réseau de contournement répond le trimestre même, l'hiver est rigoureux |
| T15 | Crise anticorruption, rail coupé de nouveau | Bataillons ferroviaires, complaisance diplomatique achetée, trou dans la couverture antiaérienne |
| T16 | Coalition drones, ateliers, fracture au sommet à Moscou | Tour de vis, assauts, drones étrangers, pourparlers sans lendemain |
| T17 | Interception, transparence, coalition | Ports céréaliers, leurres, réorientation des flux |
| T18 | Formation, ateliers | Primes, propagande, répression |
| T19 | Interception, coalition, ateliers | Bombes planantes, missiles achetés, propagande |
| T20 | *(variantes — la décision)* | |
| T21 à T24 | *(l'après-chute — voir §15)* | |

---

## 5. Socle commun — à coller tel quel

```csharp
// T1 · automne 2021 — le prologue : de la génération de force, et rien d'autre
new ScheduledCard { Turn = 1, CardCode = "force_concentration" },
new ScheduledCard { Turn = 1, CardCode = "zapad_exercises" },
new ScheduledCard { Turn = 1, CardCode = "ultimatum_to_nato" },
new ScheduledCard { Turn = 1, CardCode = "intelligence_warning" },
new ScheduledCard { Turn = 1, CardCode = "first_defensive_deliveries" },
new ScheduledCard { Turn = 1, CardCode = "no_mobilisation_yet" },

// T2 · hiver 2022 — l'invasion
new ScheduledCard { Turn = 2, CardCode = "sanctions_package_1" },
new ScheduledCard { Turn = 2, CardCode = "allied_intelligence" },
new ScheduledCard { Turn = 2, CardCode = "nato_training_pipeline" },
new ScheduledCard { Turn = 2, CardCode = "state_propaganda_surge" },
new ScheduledCard { Turn = 2, CardCode = "domestic_repression" },
new ScheduledCard { Turn = 2, CardCode = "rasputitsa" },

// T3 · printemps 2022 — le repli du nord, l'aide s'organise
new ScheduledCard { Turn = 3, CardCode = "western_aid_opens" },
new ScheduledCard { Turn = 3, CardCode = "transparency_reform" },
new ScheduledCard { Turn = 3, CardCode = "recruitment_reform" },
new ScheduledCard { Turn = 3, CardCode = "industrial_requisition" },
new ScheduledCard { Turn = 3, CardCode = "war_economy_conversion" },

// T4 · été 2022 — HIMARS
new ScheduledCard { Turn = 4, CardCode = "himars_deep_strike" },
new ScheduledCard { Turn = 4, CardCode = "depot_strikes" },
new ScheduledCard { Turn = 4, CardCode = "prison_recruitment" },
new ScheduledCard { Turn = 4, CardCode = "foreign_drones" },
new ScheduledCard { Turn = 4, CardCode = "pipeline_sabotage" },

// T5 · automne 2022 — Kharkiv, Kherson, mobilisation
new ScheduledCard { Turn = 5, CardCode = "counter_offensive_2022" },
new ScheduledCard { Turn = 5, CardCode = "naval_drones_black_sea" },
new ScheduledCard { Turn = 5, CardCode = "partial_mobilisation" },
new ScheduledCard { Turn = 5, CardCode = "foreign_ballistic_missiles" },
new ScheduledCard { Turn = 5, CardCode = "decoy_saturation" },

// T6 · hiver 2023 — la campagne contre le réseau, et l'hiver qui la dément
new ScheduledCard { Turn = 6, CardCode = "oil_price_cap" },
new ScheduledCard { Turn = 6, CardCode = "diplomatic_campaign" },
new ScheduledCard { Turn = 6, CardCode = "grid_campaign" },
new ScheduledCard { Turn = 6, CardCode = "glide_bombs" },
new ScheduledCard { Turn = 6, CardCode = "oil_export_rerouting" },
new ScheduledCard { Turn = 6, CardCode = "mild_winter" },

// T7 · printemps 2023 — l'embargo posé, l'embargo contourné
new ScheduledCard { Turn = 7, CardCode = "component_embargo" },
new ScheduledCard { Turn = 7, CardCode = "cheap_interception" },
new ScheduledCard { Turn = 7, CardCode = "component_smuggling" },
new ScheduledCard { Turn = 7, CardCode = "electronic_warfare_scaling" },
new ScheduledCard { Turn = 7, CardCode = "armed_mutiny" },

// T8 · été 2023 — l'offensive s'enlise, le blé devient une cible
new ScheduledCard { Turn = 8, CardCode = "failed_offensive" },
new ScheduledCard { Turn = 8, CardCode = "depot_strikes" },
new ScheduledCard { Turn = 8, CardCode = "domestic_drone_industry" },
new ScheduledCard { Turn = 8, CardCode = "shahed_plant" },
new ScheduledCard { Turn = 8, CardCode = "grain_port_strikes" },
new ScheduledCard { Turn = 8, CardCode = "inflation_surge" },

// T9 · automne 2023 — le train reconduit, les obus étrangers arrivent
new ScheduledCard { Turn = 9, CardCode = "component_embargo" },
new ScheduledCard { Turn = 9, CardCode = "naval_drones_black_sea" },
new ScheduledCard { Turn = 9, CardCode = "foreign_shells" },
new ScheduledCard { Turn = 9, CardCode = "contract_recruitment_drive" },
new ScheduledCard { Turn = 9, CardCode = "shadow_fleet" },
new ScheduledCard { Turn = 9, CardCode = "attention_elsewhere" },
new ScheduledCard { Turn = 9, CardCode = "rasputitsa" },

// T10 · hiver 2024 — la disette d'obus, la loi de mobilisation
new ScheduledCard { Turn = 10, CardCode = "cheap_interception" },
new ScheduledCard { Turn = 10, CardCode = "conscription_law" },
new ScheduledCard { Turn = 10, CardCode = "domestic_drone_industry" },
new ScheduledCard { Turn = 10, CardCode = "grid_campaign" },
new ScheduledCard { Turn = 10, CardCode = "decoy_saturation" },
new ScheduledCard { Turn = 10, CardCode = "contract_recruitment_drive" },

// T11 · printemps 2024 — les raffineries, les bombes planantes et leur brouillage
new ScheduledCard { Turn = 11, CardCode = "refinery_strikes" },
new ScheduledCard { Turn = 11, CardCode = "drone_coalition" },
new ScheduledCard { Turn = 11, CardCode = "electronic_warfare_ukraine" },
new ScheduledCard { Turn = 11, CardCode = "licence_transfer" },
new ScheduledCard { Turn = 11, CardCode = "glide_bombs" },
new ScheduledCard { Turn = 11, CardCode = "meat_assault" },
new ScheduledCard { Turn = 11, CardCode = "european_election_swing" },

// T12 · été 2024 — la frappe profonde est annulée par la contre-batterie
new ScheduledCard { Turn = 12, CardCode = "rail_interdiction" },
new ScheduledCard { Turn = 12, CardCode = "himars_deep_strike" },
new ScheduledCard { Turn = 12, CardCode = "nato_training_pipeline" },
new ScheduledCard { Turn = 12, CardCode = "counter_battery" },
new ScheduledCard { Turn = 12, CardCode = "meat_assault" },

// T13 · automne 2024 — deux contres le même trimestre
new ScheduledCard { Turn = 13, CardCode = "frozen_assets_windfall" },
new ScheduledCard { Turn = 13, CardCode = "cheap_interception" },
new ScheduledCard { Turn = 13, CardCode = "decentralised_generation" },
new ScheduledCard { Turn = 13, CardCode = "fibre_optic_drones" },
new ScheduledCard { Turn = 13, CardCode = "electronic_warfare" },
new ScheduledCard { Turn = 13, CardCode = "substation_strikes" },

// T14 · hiver 2025 — la flotte fantôme sanctionnée, puis contournée
new ScheduledCard { Turn = 14, CardCode = "shadow_fleet_sanctions" },
new ScheduledCard { Turn = 14, CardCode = "cheap_interception" },
new ScheduledCard { Turn = 14, CardCode = "grid_campaign" },
new ScheduledCard { Turn = 14, CardCode = "evasion_network" },
new ScheduledCard { Turn = 14, CardCode = "decoy_saturation" },
new ScheduledCard { Turn = 14, CardCode = "harsh_winter" },

// T15 · printemps 2025 — le rail coupé, le rail réparé
new ScheduledCard { Turn = 15, CardCode = "anticorruption_crisis" },
new ScheduledCard { Turn = 15, CardCode = "rail_interdiction" },
new ScheduledCard { Turn = 15, CardCode = "nato_training_pipeline" },
new ScheduledCard { Turn = 15, CardCode = "rail_repair_brigades" },
new ScheduledCard { Turn = 15, CardCode = "diplomatic_complaisance" },
new ScheduledCard { Turn = 15, CardCode = "air_defence_gap" },

// T16 · été 2025 — la fracture au sommet, les pourparlers
new ScheduledCard { Turn = 16, CardCode = "drone_coalition" },
new ScheduledCard { Turn = 16, CardCode = "domestic_drone_industry" },
new ScheduledCard { Turn = 16, CardCode = "domestic_repression" },
new ScheduledCard { Turn = 16, CardCode = "meat_assault" },
new ScheduledCard { Turn = 16, CardCode = "foreign_drones" },
new ScheduledCard { Turn = 16, CardCode = "ceasefire_talks" },

// T17 · automne 2025
new ScheduledCard { Turn = 17, CardCode = "cheap_interception" },
new ScheduledCard { Turn = 17, CardCode = "transparency_reform" },
new ScheduledCard { Turn = 17, CardCode = "drone_coalition" },
new ScheduledCard { Turn = 17, CardCode = "grain_port_strikes" },
new ScheduledCard { Turn = 17, CardCode = "decoy_saturation" },
new ScheduledCard { Turn = 17, CardCode = "oil_export_rerouting" },

// T18 · hiver 2026
new ScheduledCard { Turn = 18, CardCode = "nato_training_pipeline" },
new ScheduledCard { Turn = 18, CardCode = "domestic_drone_industry" },
new ScheduledCard { Turn = 18, CardCode = "contract_recruitment_drive" },
new ScheduledCard { Turn = 18, CardCode = "state_propaganda_surge" },
new ScheduledCard { Turn = 18, CardCode = "domestic_repression" },

// T19 · printemps 2026
new ScheduledCard { Turn = 19, CardCode = "cheap_interception" },
new ScheduledCard { Turn = 19, CardCode = "drone_coalition" },
new ScheduledCard { Turn = 19, CardCode = "domestic_drone_industry" },
new ScheduledCard { Turn = 19, CardCode = "glide_bombs" },
new ScheduledCard { Turn = 19, CardCode = "foreign_ballistic_missiles" },
new ScheduledCard { Turn = 19, CardCode = "state_propaganda_surge" },

// T20 · été 2026 — le présent
new ScheduledCard { Turn = 20, CardCode = "depot_strikes" },
new ScheduledCard { Turn = 20, CardCode = "elite_fracture" },
new ScheduledCard { Turn = 20, CardCode = "cheap_interception" },
new ScheduledCard { Turn = 20, CardCode = "meat_assault" },
new ScheduledCard { Turn = 20, CardCode = "decoy_saturation" },

// T21 · automne 2026
new ScheduledCard { Turn = 21, CardCode = "drone_coalition" },
new ScheduledCard { Turn = 21, CardCode = "nato_training_pipeline" },
new ScheduledCard { Turn = 21, CardCode = "glide_bombs" },
new ScheduledCard { Turn = 21, CardCode = "meat_assault" },

// T22 · hiver 2027
new ScheduledCard { Turn = 22, CardCode = "domestic_drone_industry" },
new ScheduledCard { Turn = 22, CardCode = "cheap_interception" },
new ScheduledCard { Turn = 22, CardCode = "harsh_winter" },
new ScheduledCard { Turn = 22, CardCode = "decoy_saturation" },

// T23 · printemps 2027 — la décision
new ScheduledCard { Turn = 23, CardCode = "depot_strikes" },
new ScheduledCard { Turn = 23, CardCode = "meat_assault" },
```

---

## 6. Variante `Holds` — le soutien tient, sans plus

```csharp
new ScheduledCard { Turn = 10, CardCode = "aid_blocked" },
new ScheduledCard { Turn = 12, CardCode = "aid_unblocked" },
new ScheduledCard { Turn = 14, CardCode = "budget_fatigue" },
new ScheduledCard { Turn = 18, CardCode = "parliament_veto" },
```

La crise des munitions de 2023-2024 est conservée telle quelle : la Russie coupe le robinet à T9,
l'Ukraine le rouvre à T11. S'y ajoutent deux frottements russes qui ne rompent rien — le soutien
s'amaigrit et se conditionne, il ne s'arrête jamais.

## 7. Variante `Collapses` — le soutien s'arrête

```csharp
new ScheduledCard { Turn = 7, CardCode = "us_election_swing" },
new ScheduledCard { Turn = 7, CardCode = "aid_collapse" },
new ScheduledCard { Turn = 9, CardCode = "budget_fatigue" },
```

Rien avant T6 : les cinq premiers tours doivent rester **strictement identiques** à ceux de `Holds`,
c'est la démonstration elle-même. La fatigue budgétaire arrive à T8 et non à T5 pour cette raison.
Les trois cartes appartiennent à la Russie : couper un flux gratuit n'est pas un accident du calendrier
électoral, c'est le rendement d'un effort d'influence de trois ans.

## 8. Variante `Resolve` — l'Occident joue ses cartes

```csharp
new ScheduledCard { Turn = 10, CardCode = "aid_blocked" },
new ScheduledCard { Turn = 11, CardCode = "aid_unblocked" },
new ScheduledCard { Turn = 11, CardCode = "component_embargo_total" },
new ScheduledCard { Turn = 12, CardCode = "aid_predictable" },
new ScheduledCard { Turn = 13, CardCode = "refinery_campaign_sustained" },
new ScheduledCard { Turn = 14, CardCode = "frozen_assets_released" },
new ScheduledCard { Turn = 17, CardCode = "aid_predictable" },
new ScheduledCard { Turn = 17, CardCode = "major_oil_sanctions" },
new ScheduledCard { Turn = 20, CardCode = "shadow_fleet_sanctions" },
new ScheduledCard { Turn = 21, CardCode = "component_embargo_total" },
new ScheduledCard { Turn = 21, CardCode = "oil_price_cap" },
new ScheduledCard { Turn = 21, CardCode = "conscription_law" },
new ScheduledCard { Turn = 22, CardCode = "refinery_campaign_sustained" },
new ScheduledCard { Turn = 22, CardCode = "currency_collapse" },
new ScheduledCard { Turn = 23, CardCode = "supplier_withdraws" },
new ScheduledCard { Turn = 23, CardCode = "oil_price_crash" },
new ScheduledCard { Turn = 23, CardCode = "sovereign_fund_empty" },
new ScheduledCard { Turn = 23, CardCode = "elite_break" },
new ScheduledCard { Turn = 23, CardCode = "refinery_campaign_sustained" },
new ScheduledCard { Turn = 23, CardCode = "sovereign_fund_draw" },
```

La campagne sur le raffinage revient tous les trois trimestres, jamais tous les trimestres : le dégât
cicatrise entre deux passages, et c'est cet intervalle qui fait durer l'étranglement des années. Le
paquet de T19 n'est pas une victoire militaire, c'est une caisse qui se ferme — et depuis que rien
n'est plus « subi », ces cinq cartes sont dans la main ukrainienne. Le trimestre final est la seule
main de six cartes du déroulé, ce qui est exactement le propos : c'est le moment où le camp qui a
patiemment coupé les flux abat tout ce qu'il a construit.

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

Placement au **T16**, soit le quatrième trimestre 2025 : le trimestre exact de la désignation. C'est
aussi, à la mesure, le seul placement qui fasse tomber le régime russe par sa caisse plutôt que son
armée par le front — reculée à T17 ou T18, la carte laisse l'issue basculer en `military_collapse`,
ce qui raconte l'inverse de la thèse du jeu.

Elle est réservée à la variante `Resolve`. Un durcissement de cette ampleur n'a aucun sens dans un
monde où le soutien « tient sans plus », et la partie est déjà finie au T10 dans la variante où il
s'arrête.

---

## 9. Ce que le calendrier corrige au passage

**`harsh_winter` déplacée de T11 à T13.** T11 est un tour d'été. Un hiver rigoureux en juillet
contredit la règle saisonnière que le modèle met en scène ; T13 est le premier hiver disponible.

**`conscription_law` placée à T9 et non au premier tour.** Le décret de mobilisation générale de
février 2022 est déjà supposé par l'état de départ ; ce que la carte représente — l'abaissement de
l'âge et l'élargissement de l'assiette — date d'avril 2024, soit T9. La placer à T1 vidait par
ailleurs le dépôt ukrainien un trimestre trop tôt et faisait disparaître la latence de deux tours qui
fait toute la démonstration de la variante `Collapses`.

**Deux cartes reconduites plutôt que jouées une fois.** `component_embargo` est joué à T6 puis à T8 :
le premier est contré le trimestre même par `component_smuggling`, le second aboutit. Une sanction
qu'on n'entretient pas se contourne — la règle est désormais visible à l'écran plutôt qu'écrite dans
un document.

---

## 10. Cartes volontairement laissées hors calendrier

Quatorze cartes ne sont jouées à aucun tour. Ce n'est pas un oubli : le deck de la V2 doit être plus
large que la chronique de la V1.

| Carte | Raison |
|---|---|
| `war_tax_rise`, `mobilisation_wave_two` | Leur bénéfice repose sur `TreasuryDelta` ou sur une ressource que le vocabulaire d'effets ne sait pas encore atteindre ; jouées dans le socle, elles seraient un coût net pour la Russie. Voir §11. |
| `oil_price_spike`, `global_recession` | `OilPriceDelta` décale **définitivement** tout le calendrier pétrolier. Dans un déroulé où `OilPriceCalendar` encode déjà la trajectoire réelle du baril, elles font doublon. Excellentes cartes de deck, mauvaises cartes de chronique. |
| `dam_breach`, `demographic_wall` | Réservoir de tension pour la V1.1 et la V2. |
| `refinery_air_defence`, `counter_intelligence`, `air_defence_surge` | Contre-cartes dont la cible n'est pas jouée dans le socle ; elles existent pour le bluff de la V2. |
| `air_base_strikes`, `security_guarantees` | Cartes ukrainiennes ajoutées pour équilibrer les deux decks à quarante-sept ; les insérer déplacerait les issues déjà calées. |
| `decapitation_strike`, `drone_swarm_scaling`, `chinese_pressure` | Déjà hors calendrier avant cette proposition ; les deux premières servent au duel de decks. |

---

## 11. Résultats observés

Mesuré sur le scénario intégré.

| Variante | Issue | Tour de décision | Lecture |
|---|---|---|---|
| `Resolve` | chute du régime, puis **armistice**, victoire ukrainienne | **T23** | Cohésion des élites à zéro, puissance russe à 13 % de son pic, écart de financement 0,50, réserves 310 → 44 Md |
| `Holds` | `frozen_front` | **T26** | Personne ne rompt, la partie va au bout des vingt-six trimestres |
| `Collapses` | effondrement ukrainien, puis armistice, victoire russe | **T11** | Le flux gratuit coupé au T7, deux trimestres de latence, puis tout cède |

**La tension monte sans plateau sur les trois derniers trimestres**, ce qui était la condition
posée. L'écart de financement russe passe de 0,19 au T18 à 0,21, 0,30, 0,33, 0,39 puis 0,45 ; les
réserves de 91 à 48 Md ; la puissance de combat de 301 000 à 37 000 hommes ; et la cohésion des
élites de 76 à zéro. Rien ne s'affaisse et rien ne saute : le régime cède le trimestre où ce qu'il
doit financer est devenu ce qu'il ne peut plus financer.

**L'épilogue ne joue aucune carte.** Aux tours 24 à 26, les deux mains sont vides et la ligne bouge
quand même : l'armée qui n'est plus payée fond, et l'Ukraine reprend du terrain sans monter un seul
assaut. C'est la meilleure démonstration que le jeu puisse offrir de sa propre thèse, et elle ne
coûte pas une carte.

### Deux constantes ont dû bouger avec le calendrier

Le calendrier fixe **quand** le coup porte ; les constantes décident **s'il** porte. Deux ont été
ajustées, toutes deux dans le sens du réalisme.

`WarBudgetCeilingShare` passe de 0,038 à **0,028**. À 0,038 l'armée russe cessait d'être payée vers
le T19 et le déroulé s'achevait en effondrement **militaire** — le front décidant de la guerre, ce
que ce scénario ne doit jamais dire. À 0,028, c'est le régime qui cède le premier. La valeur colle
aussi mieux aux sources : elle implique environ 202 Md par an de plafond de guerre, contre les
~190 Md estimés pour 2025, là où 0,038 impliquait 274 Md. La borne basse est connue — à 0,027 la
variante du front figé casse, l'envahisseur tenant alors assez confortablement pour l'emporter.

`RefiningRepairPerTurn` revient de 0,4 à **0,18**, la valeur que l'audit demandait et que les sources
soutiennent. Elle avait été écartée parce qu'elle avançait l'effondrement russe d'un trimestre sans
compensation disponible ; cette contrainte a disparu, puisque la date de la chute est désormais tenue
par le calendrier. La concession documentée dans `04-calibration-effectifs.md` §12 peut être retirée.

Une campagne sur le raffinage a été retirée du T19 en contrepartie : avec une réparation deux fois
plus lente, chaque passage pèse plus lourd et il en faut moins pour le même étranglement.

---

## 12. Points de vigilance

**Le capital politique ne suit pas.** Densifier multiplie par cinq le nombre de cartes payées, et la
suppression des cartes sans propriétaire ajoute vingt-six cartes désormais facturées à un camp. Avec
une génération de deux à trois points par tour et un plafond de trente, les deux camps terminent en
découvert profond — de l'ordre de cent trente points côté russe, deux cent soixante-dix côté
ukrainien. La V1.0 joue son calendrier quoi qu'il arrive et n'enregistre que le découvert, donc rien
ne casse ; mais l'économie de mana telle qu'elle est calibrée ne supporterait pas la V2 avec cette
densité. Deux leviers, à trancher avant la V2 : relever la génération par tour, ou multiplier les
cartes qui en rendent — `state_propaganda_surge` et `diplomatic_campaign` le font déjà, chacune de
son côté du plateau, et c'est exactement l'asymétrie décrite dans le modèle.

**`elite_fracture` reste au coût zéro** alors que toutes les autres cartes réattribuées ont reçu un
prix. Ce n'est pas un choix de conception : le deck « Épuisement politique » de `DeckDuel` est
calibré à exactement quarante-quatre points de capital, et cette carte y compte pour rien. Lui donner
un coût casse le test `NoDeckIsDominant_TheyAreComparedAtEqualPoliticalCost` tant que `DeckDuel.cs`
n'est pas rééquilibré en regard.

**Le vocabulaire d'effets manque de trois entrées** pour que les cartes budgétaires russes
fonctionnent : `ReservesDelta` (le fonds souverain n'est pas atteignable), `FiscalCaptureDelta` (un
impôt de guerre ne peut pas augmenter ce que le trimestre finance), et un moyen de brûler des hommes.
`TreasuryDelta` est par ailleurs quasi inerte côté russe : le budget de guerre est plafonné par le
PIB et par les recettes du trimestre, et la trésorerie est réinitialisée à chaque tour.

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
franchissent le seuil. Le calendrier en programme trois, aux tours 5, 9 et 13 — tous des hivers —
donc les coupures nationales apparaissent au deuxième, ce qui est le rythme que le modèle décrit et
qu'aucune partie ne produisait. Le plafond de perte permanente autorise 14,3 GW, que les trois
passages atteignent presque exactement : la marge de manœuvre pour ajouter d'autres cartes de réseau
est désormais nulle, et `dam_breach` comme `substation_strikes` doivent rester des jeux uniques.

**Une bonne nouvelle, enfin.** Les cartes de mobilisation — `partial_mobilisation`,
`mobilisation_wave_two`, `prison_recruitment`, `conscription_law` — ne coûtaient jusqu'ici que du
consentement, puisque la règle du minimum ne mordait jamais et que le front ne manquait de rien.
Une fois les dépôts capables de se vider, elles produisent enfin ce que le modèle annonce depuis le
début : mobiliser quand le goulot est l'obus ampute le PIB, donc les recettes, donc la production
d'obus. Le piège central du scénario devient jouable.

---

## 14. Le front de 2022 doit bouger — ce que le modèle peut, et ce qu'il ne peut pas

La demande est juste : un joueur qui ouvre la frise sur 2022 doit voir la ruée, puis le reflux. C'est
le trimestre le plus connu de la guerre et le seul où la carte a vraiment bougé. Aujourd'hui elle ne
bouge pas.

**Les bons rapports de force n'y suffiront pas, et l'arithmétique le dit avant toute simulation.**

Les huit secteurs modélisés totalisent quarante-huit hexagones de largeur, soit quatre cent
quatre-vingts kilomètres de front. Un hexagone d'avance sur toute la ligne vaut donc 4 800 km².
`FrontPhase.MovementFor` plafonne l'avance à **trois hexagones par trimestre** — trente kilomètres,
et le commentaire précise que c'est déjà une percée historique. Même en portant le rapport de force
au-delà de trois pour un sur les huit secteurs à la fois, le maximum atteignable est de
**14 400 km² par trimestre**. Si la défense rompt, le multiplicateur d'effondrement porte ce plafond
à 50 400 km².

Or le pic de mars 2022 est de l'ordre de **120 000 km²** au-delà des lignes de 2014. Il faudrait
vingt-cinq hexagones de profondeur, soit deux cent cinquante kilomètres, sur tout le front et en un
seul trimestre. Un effondrement ukrainien total, sur les huit secteurs simultanément, en produirait
quarante-deux pour cent.

Trois causes se cumulent, et **aucune ne relève des cartes ni du calendrier** :

1. **La géographie manque.** Les axes de mars 2022 — Kyiv, Tchernihiv, Soumy, Jytomyr — ne sont pas
   des secteurs du modèle. Les huit secteurs couvrent l'arc Kharkiv-Kherson, c'est-à-dire le théâtre
   de la guerre d'usure, pas celui de la manœuvre. Une part substantielle du pic est hors domaine.
2. **Le plafond de mouvement est calibré pour l'usure.** Trente kilomètres par trimestre, quand les
   colonnes russes ont couvert cent à deux cents kilomètres en trois semaines vers Kherson et
   Melitopol.
3. **La table de ratio ne peut pas être franchie assez fort.** Il faut dépasser trois pour un pour
   toucher le plafond, et la doctrine d'ouverture n'y parvient sur aucun secteur.

### Ce que je recommande : la densité, pas un script

Le mécanisme qui manque n'est pas un multiplicateur, c'est **la densité de défense au kilomètre de
front**. En février 2022, l'Ukraine tenait environ mille deux cents kilomètres avec deux cent
cinquante mille hommes et aucune fortification au sud : la percée n'a pas été un exploit tactique,
elle a eu lieu là où il n'y avait personne. Et le reflux de l'automne s'explique exactement de la
même façon — la Russie tenait ces cent vingt mille kilomètres carrés avec la même densité dérisoire,
et Kharkiv est tombé en une semaine parce que la ligne était vide.

Le moteur a déjà tout ce qu'il faut pour le calculer : `Manpower.InContact`, `sector.Width` et
`sector.DefenderFortification`. Il lui manque seulement de laisser le plafond de `MovementFor`
dépendre de cette densité au lieu d'être une constante. Une ligne tenue à deux cents hommes au
kilomètre ne cède pas de trente kilomètres ; une ligne tenue à vingt hommes au kilomètre, sans
tranchées, cède de cent.

C'est de loin la meilleure réponse, pour une raison qui n'est pas technique : **le modèle démontre
alors ce qu'il affirme** au lieu de le réciter. La ruée de 2022 et le reflux de l'automne sortent du
même mécanisme, sans qu'aucune date ne soit écrite à la main, et la leçon — *on perd du terrain
parce qu'on n'a pas assez d'hommes pour le tenir, pas parce que l'adversaire a été brillant* — est
précisément celle du jeu.

### Si le scriptage est préféré

Il reste acceptable, mais alors la règle est absolue et sans exception : **le passé rejoué et
l'avenir simulé ne sont pas la même chose, et le site est public.** Tout mouvement scripté doit être
marqué comme tel dans les données, affiché comme tel à l'écran, et distingué des trimestres que le
modèle calcule. Un visiteur informé doit pouvoir savoir, en un coup d'œil, quels tours sont une
reconstitution et quels tours sont un résultat. Un scénario maquillé en sortie de modèle
discréditerait tout le reste, y compris ce que le moteur calcule justement.

Cette question ne se règle ni dans `cards.fr.json` ni dans le calendrier : elle appartient à
`FrontPhase` et à `BuildSectors`. Je la documente ici parce qu'elle conditionne le prologue — un tour
d'ouverture sans mouvement n'a de sens que si les tours suivants en produisent.

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
