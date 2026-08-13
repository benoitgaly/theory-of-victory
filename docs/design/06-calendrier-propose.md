# Calendrier proposé — dix-neuf trimestres, deux à quatre cartes par camp

> Proposition de densification du calendrier de `UkraineScenario.BuildCalendar`. Le deck étant passé
> de 41 à 95 cartes, le déroulé peut enfin montrer ce que chaque camp décide **chaque** trimestre, au
> lieu d'une carte isolée entourée de remplissage.
>
> **Statut : vérifié.** Le calendrier ci-dessous a été rejoué sur le moteur courant. Les trois issues
> tombent exactement où elles doivent tomber — `Resolve` T19, `Holds` T19, `Collapses` T10 — et les
> assertions des tests portant sur les trois déroulés passent, duel de decks compris. Détail en §9.

---

## 1. Personne ne subit : tout le monde joue

Le deck ne comporte plus **aucune** carte sans propriétaire. Il n'y a plus d'événement qui tombe du
ciel, plus de rubrique « subi ce trimestre » : les quatre-vingt-quinze cartes se répartissent en
quarante-sept russes et quarante-huit ukrainiennes, et chacune est jouée par quelqu'un.

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
exception, documentée en §10 : `elite_fracture` reste à zéro.

---

## 2. Correspondance tour / trimestre

`StartYear = 2022`, `StartSeason = Winter`, dix-neuf tours. Les hivers sont donc **T1, T5, T9, T13 et
T17** — c'est sur eux que les campagnes contre le réseau doivent mordre, et c'est à l'automne
précédent qu'elles se préparent.

| Tour | Trimestre | Saison | Tour | Trimestre | Saison |
|---|---|---|---|---|---|
| T1 | 2022 T1 | hiver | T11 | 2024 T3 | été |
| T2 | 2022 T2 | printemps | T12 | 2024 T4 | automne |
| T3 | 2022 T3 | été | T13 | 2025 T1 | hiver |
| T4 | 2022 T4 | automne | T14 | 2025 T2 | printemps |
| T5 | 2023 T1 | hiver | T15 | 2025 T3 | été |
| T6 | 2023 T2 | printemps | T16 | 2025 T4 | automne |
| T7 | 2023 T3 | été | T17 | 2026 T1 | hiver |
| T8 | 2023 T4 | automne | T18 | 2026 T2 | printemps |
| T9 | 2024 T1 | hiver | T19 | 2026 T3 | été |
| T10 | 2024 T2 | printemps | | | |

---

## 3. Lecture chronologique du socle

| Tour | Ce que joue l'Ukraine et ses soutiens | Ce que joue la Russie |
|---|---|---|
| T1 | Premier train de sanctions, renseignement allié, filières de formation, la boue de mars | Verrouillage intérieur, saturation de l'espace médiatique |
| T2 | L'aide s'industrialise, la conditionnalité s'installe, le recrutement se réforme | Réquisition des chaînes, bascule vers l'économie de guerre |
| T3 | HIMARS, les dépôts reculent de quatre-vingts kilomètres, un gazoduc saute | Prisons ouvertes, premiers drones achetés à l'étranger |
| T4 | Kharkiv et Kherson, les drones navals entrent en mer Noire | Mobilisation partielle, missiles achetés, premières vagues de leurres |
| T5 | Plafonnement du baril, campagne diplomatique, un hiver clément qui annule la campagne d'en face | Frappes sur le réseau, bombes planantes, barils réorientés vers l'Asie |
| T6 | Embargo sur les machines-outils, interception à bas coût, mutinerie armée à Rostov | L'embargo est contourné le trimestre même, le brouillage passe à l'échelle |
| T7 | L'offensive d'été s'enlise, la production nationale de drones démarre | La chaîne de montage domestique, les ports céréaliers, l'inflation chez les donateurs |
| T8 | Le train de sanctions est reconduit — une sanction est un entretien —, la raspoutitsa | Obus étrangers, primes d'engagement, flotte fantôme, une autre guerre capte l'attention |
| T9 | Loi de mobilisation, interception à bas coût, ateliers de drones | Réseau, leurres, primes |
| T10 | Raffineries, coalition drones, brouillage des kits de guidage | Licence transférée, bombes planantes, assauts à découvert, bascule électorale européenne |
| T11 | Rail coupé, nouvelle frappe profonde | La contre-batterie annule la frappe : la carte est jouée pour rien |
| T12 | Prêt gagé sur les avoirs gelés, réseaux îlotés | Fibre optique, brouillage, salles des machines — les deux sont contrées |
| T13 | Sanctions sur la flotte fantôme | Le réseau de contournement répond le trimestre même, l'hiver est rigoureux |
| T14 | Crise anticorruption, rail coupé de nouveau | Bataillons ferroviaires, complaisance diplomatique achetée, trou dans la couverture antiaérienne |
| T15 | Coalition drones, ateliers, fracture au sommet à Moscou | Tour de vis, assauts, drones étrangers, pourparlers sans lendemain |
| T16 | Interception, transparence, coalition | Ports céréaliers, leurres, réorientation des flux |
| T17 | Formation, ateliers | Primes, propagande, répression |
| T18 | Interception, coalition, ateliers | Bombes planantes, missiles achetés, propagande |
| T19 | *(variantes)* | |

---

## 4. Socle commun — à coller tel quel

```csharp
// T1 · hiver 2022 — l'invasion
new ScheduledCard { Turn = 1, CardCode = "sanctions_package_1" },
new ScheduledCard { Turn = 1, CardCode = "allied_intelligence" },
new ScheduledCard { Turn = 1, CardCode = "nato_training_pipeline" },
new ScheduledCard { Turn = 1, CardCode = "rasputitsa" },
new ScheduledCard { Turn = 1, CardCode = "state_propaganda_surge" },
new ScheduledCard { Turn = 1, CardCode = "domestic_repression" },

// T2 · printemps 2022 — le repli du nord, l'aide s'organise
new ScheduledCard { Turn = 2, CardCode = "western_aid_opens" },
new ScheduledCard { Turn = 2, CardCode = "transparency_reform" },
new ScheduledCard { Turn = 2, CardCode = "recruitment_reform" },
new ScheduledCard { Turn = 2, CardCode = "industrial_requisition" },
new ScheduledCard { Turn = 2, CardCode = "war_economy_conversion" },

// T3 · été 2022 — HIMARS
new ScheduledCard { Turn = 3, CardCode = "himars_deep_strike" },
new ScheduledCard { Turn = 3, CardCode = "depot_strikes" },
new ScheduledCard { Turn = 3, CardCode = "pipeline_sabotage" },
new ScheduledCard { Turn = 3, CardCode = "prison_recruitment" },
new ScheduledCard { Turn = 3, CardCode = "foreign_drones" },

// T4 · automne 2022 — Kharkiv, Kherson, mobilisation
new ScheduledCard { Turn = 4, CardCode = "counter_offensive_2022" },
new ScheduledCard { Turn = 4, CardCode = "naval_drones_black_sea" },
new ScheduledCard { Turn = 4, CardCode = "partial_mobilisation" },
new ScheduledCard { Turn = 4, CardCode = "foreign_ballistic_missiles" },
new ScheduledCard { Turn = 4, CardCode = "decoy_saturation" },

// T5 · hiver 2023 — la campagne contre le réseau, et l'hiver qui la dément
new ScheduledCard { Turn = 5, CardCode = "oil_price_cap" },
new ScheduledCard { Turn = 5, CardCode = "diplomatic_campaign" },
new ScheduledCard { Turn = 5, CardCode = "mild_winter" },
new ScheduledCard { Turn = 5, CardCode = "grid_campaign" },
new ScheduledCard { Turn = 5, CardCode = "glide_bombs" },
new ScheduledCard { Turn = 5, CardCode = "oil_export_rerouting" },

// T6 · printemps 2023 — l'embargo posé, l'embargo contourné
new ScheduledCard { Turn = 6, CardCode = "component_embargo" },
new ScheduledCard { Turn = 6, CardCode = "cheap_interception" },
new ScheduledCard { Turn = 6, CardCode = "armed_mutiny" },
new ScheduledCard { Turn = 6, CardCode = "component_smuggling" },
new ScheduledCard { Turn = 6, CardCode = "electronic_warfare_scaling" },

// T7 · été 2023 — l'offensive s'enlise, le blé devient une cible
new ScheduledCard { Turn = 7, CardCode = "failed_offensive" },
new ScheduledCard { Turn = 7, CardCode = "depot_strikes" },
new ScheduledCard { Turn = 7, CardCode = "domestic_drone_industry" },
new ScheduledCard { Turn = 7, CardCode = "shahed_plant" },
new ScheduledCard { Turn = 7, CardCode = "grain_port_strikes" },
new ScheduledCard { Turn = 7, CardCode = "inflation_surge" },

// T8 · automne 2023 — le train reconduit, les obus étrangers arrivent
new ScheduledCard { Turn = 8, CardCode = "component_embargo" },
new ScheduledCard { Turn = 8, CardCode = "naval_drones_black_sea" },
new ScheduledCard { Turn = 8, CardCode = "rasputitsa" },
new ScheduledCard { Turn = 8, CardCode = "foreign_shells" },
new ScheduledCard { Turn = 8, CardCode = "contract_recruitment_drive" },
new ScheduledCard { Turn = 8, CardCode = "shadow_fleet" },
new ScheduledCard { Turn = 8, CardCode = "attention_elsewhere" },

// T9 · hiver 2024 — la disette d'obus, la loi de mobilisation
new ScheduledCard { Turn = 9, CardCode = "cheap_interception" },
new ScheduledCard { Turn = 9, CardCode = "conscription_law" },
new ScheduledCard { Turn = 9, CardCode = "domestic_drone_industry" },
new ScheduledCard { Turn = 9, CardCode = "grid_campaign" },
new ScheduledCard { Turn = 9, CardCode = "decoy_saturation" },
new ScheduledCard { Turn = 9, CardCode = "contract_recruitment_drive" },

// T10 · printemps 2024 — les raffineries, les bombes planantes et leur brouillage
new ScheduledCard { Turn = 10, CardCode = "refinery_strikes" },
new ScheduledCard { Turn = 10, CardCode = "drone_coalition" },
new ScheduledCard { Turn = 10, CardCode = "electronic_warfare_ukraine" },
new ScheduledCard { Turn = 10, CardCode = "licence_transfer" },
new ScheduledCard { Turn = 10, CardCode = "glide_bombs" },
new ScheduledCard { Turn = 10, CardCode = "meat_assault" },
new ScheduledCard { Turn = 10, CardCode = "european_election_swing" },

// T11 · été 2024 — la frappe profonde est annulée par la contre-batterie
new ScheduledCard { Turn = 11, CardCode = "rail_interdiction" },
new ScheduledCard { Turn = 11, CardCode = "himars_deep_strike" },
new ScheduledCard { Turn = 11, CardCode = "nato_training_pipeline" },
new ScheduledCard { Turn = 11, CardCode = "counter_battery" },
new ScheduledCard { Turn = 11, CardCode = "meat_assault" },

// T12 · automne 2024 — deux contres le même trimestre
new ScheduledCard { Turn = 12, CardCode = "frozen_assets_windfall" },
new ScheduledCard { Turn = 12, CardCode = "cheap_interception" },
new ScheduledCard { Turn = 12, CardCode = "decentralised_generation" },
new ScheduledCard { Turn = 12, CardCode = "fibre_optic_drones" },
new ScheduledCard { Turn = 12, CardCode = "electronic_warfare" },
new ScheduledCard { Turn = 12, CardCode = "machine_hall_strikes" },

// T13 · hiver 2025 — la flotte fantôme sanctionnée, puis contournée
new ScheduledCard { Turn = 13, CardCode = "shadow_fleet_sanctions" },
new ScheduledCard { Turn = 13, CardCode = "cheap_interception" },
new ScheduledCard { Turn = 13, CardCode = "grid_campaign" },
new ScheduledCard { Turn = 13, CardCode = "evasion_network" },
new ScheduledCard { Turn = 13, CardCode = "decoy_saturation" },
new ScheduledCard { Turn = 13, CardCode = "harsh_winter" },

// T14 · printemps 2025 — le rail coupé, le rail réparé
new ScheduledCard { Turn = 14, CardCode = "anticorruption_crisis" },
new ScheduledCard { Turn = 14, CardCode = "rail_interdiction" },
new ScheduledCard { Turn = 14, CardCode = "nato_training_pipeline" },
new ScheduledCard { Turn = 14, CardCode = "rail_repair_brigades" },
new ScheduledCard { Turn = 14, CardCode = "diplomatic_complaisance" },
new ScheduledCard { Turn = 14, CardCode = "air_defence_gap" },

// T15 · été 2025 — la fracture au sommet, les pourparlers
new ScheduledCard { Turn = 15, CardCode = "drone_coalition" },
new ScheduledCard { Turn = 15, CardCode = "domestic_drone_industry" },
new ScheduledCard { Turn = 15, CardCode = "elite_fracture" },
new ScheduledCard { Turn = 15, CardCode = "domestic_repression" },
new ScheduledCard { Turn = 15, CardCode = "meat_assault" },
new ScheduledCard { Turn = 15, CardCode = "foreign_drones" },
new ScheduledCard { Turn = 15, CardCode = "ceasefire_talks" },

// T16 · automne 2025
new ScheduledCard { Turn = 16, CardCode = "cheap_interception" },
new ScheduledCard { Turn = 16, CardCode = "transparency_reform" },
new ScheduledCard { Turn = 16, CardCode = "drone_coalition" },
new ScheduledCard { Turn = 16, CardCode = "grain_port_strikes" },
new ScheduledCard { Turn = 16, CardCode = "decoy_saturation" },
new ScheduledCard { Turn = 16, CardCode = "oil_export_rerouting" },

// T17 · hiver 2026
new ScheduledCard { Turn = 17, CardCode = "nato_training_pipeline" },
new ScheduledCard { Turn = 17, CardCode = "domestic_drone_industry" },
new ScheduledCard { Turn = 17, CardCode = "contract_recruitment_drive" },
new ScheduledCard { Turn = 17, CardCode = "state_propaganda_surge" },
new ScheduledCard { Turn = 17, CardCode = "domestic_repression" },

// T18 · printemps 2026
new ScheduledCard { Turn = 18, CardCode = "cheap_interception" },
new ScheduledCard { Turn = 18, CardCode = "drone_coalition" },
new ScheduledCard { Turn = 18, CardCode = "domestic_drone_industry" },
new ScheduledCard { Turn = 18, CardCode = "glide_bombs" },
new ScheduledCard { Turn = 18, CardCode = "foreign_ballistic_missiles" },
new ScheduledCard { Turn = 18, CardCode = "state_propaganda_surge" },
```

---

## 5. Variante `Holds` — le soutien tient, sans plus

```csharp
new ScheduledCard { Turn = 9, CardCode = "aid_blocked" },
new ScheduledCard { Turn = 11, CardCode = "aid_unblocked" },
new ScheduledCard { Turn = 13, CardCode = "budget_fatigue" },
new ScheduledCard { Turn = 17, CardCode = "parliament_veto" },
```

La crise des munitions de 2023-2024 est conservée telle quelle : la Russie coupe le robinet à T9,
l'Ukraine le rouvre à T11. S'y ajoutent deux frottements russes qui ne rompent rien — le soutien
s'amaigrit et se conditionne, il ne s'arrête jamais.

## 6. Variante `Collapses` — le soutien s'arrête

```csharp
new ScheduledCard { Turn = 6, CardCode = "us_election_swing" },
new ScheduledCard { Turn = 6, CardCode = "aid_collapse" },
new ScheduledCard { Turn = 8, CardCode = "budget_fatigue" },
```

Rien avant T6 : les cinq premiers tours doivent rester **strictement identiques** à ceux de `Holds`,
c'est la démonstration elle-même. La fatigue budgétaire arrive à T8 et non à T5 pour cette raison.
Les trois cartes appartiennent à la Russie : couper un flux gratuit n'est pas un accident du calendrier
électoral, c'est le rendement d'un effort d'influence de trois ans.

## 7. Variante `Resolve` — l'Occident joue ses cartes

```csharp
new ScheduledCard { Turn = 9, CardCode = "aid_blocked" },
new ScheduledCard { Turn = 10, CardCode = "aid_unblocked" },
new ScheduledCard { Turn = 10, CardCode = "component_embargo_total" },
new ScheduledCard { Turn = 11, CardCode = "aid_predictable" },
new ScheduledCard { Turn = 12, CardCode = "refinery_campaign_sustained" },
new ScheduledCard { Turn = 13, CardCode = "frozen_assets_released" },
new ScheduledCard { Turn = 15, CardCode = "refinery_campaign_sustained" },
new ScheduledCard { Turn = 16, CardCode = "aid_predictable" },
new ScheduledCard { Turn = 16, CardCode = "major_oil_sanctions" },
new ScheduledCard { Turn = 17, CardCode = "oil_price_cap" },
new ScheduledCard { Turn = 17, CardCode = "domestic_drone_industry" },
new ScheduledCard { Turn = 17, CardCode = "conscription_law" },
new ScheduledCard { Turn = 18, CardCode = "shadow_fleet_sanctions" },
new ScheduledCard { Turn = 18, CardCode = "currency_collapse" },
new ScheduledCard { Turn = 18, CardCode = "depot_strikes" },
new ScheduledCard { Turn = 19, CardCode = "supplier_withdraws" },
new ScheduledCard { Turn = 19, CardCode = "oil_price_crash" },
new ScheduledCard { Turn = 19, CardCode = "sovereign_fund_empty" },
new ScheduledCard { Turn = 19, CardCode = "elite_break" },
new ScheduledCard { Turn = 19, CardCode = "refinery_campaign_sustained" },
new ScheduledCard { Turn = 19, CardCode = "sovereign_fund_draw" },
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

## 8. Ce que le calendrier corrige au passage

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

## 9. Cartes volontairement laissées hors calendrier

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

## 10. Résultats observés

Rejoué sur le moteur, calendrier ci-dessus substitué au calendrier actuel.

> **À revalider.** Ces mesures ont été prises avant la passe de réalisme qui recalibre en ce moment
> la réparation du raffinage russe, la capacité de drones d'attaque et le réseau électrique ukrainien.
> Ces trois grandeurs sont précisément celles sur lesquelles le calendrier a été calé : le nombre de
> passages de `decoy_saturation`, de `grid_campaign` et de `machine_hall_strikes` devra être rejoué
> une fois la recalibration figée. Le banc qui a servi à caler le calendrier reconstruit les trois
> variantes, substitue le calendrier et rejoue toutes les assertions, duel de decks compris ; il
> suffit de le relancer.

| Variante | Issue | Tour de décision | Lecture |
|---|---|---|---|
| `Resolve` | `regime_collapse`, victoire ukrainienne | **T19** | Puissance russe à 30 % de son pic, écart de financement 0,46, réserves 302 → 40 Md, ratio de génération ukrainien 1,00, +1 022 km² encore aux mains de l'envahisseur |
| `Holds` | `frozen_front` | **T19** | Personne ne rompt, la partie va au bout |
| `Collapses` | `military_collapse`, victoire russe | **T10** | Couverture ukrainienne à 1,00 en T6 et T7, puis 0,15 en T8 : la latence de deux tours est intacte |

Duel de decks rejoué sur la base `Holds` densifiée : frappe profonde **gagne**, attrition frontale
**ne gagne pas**, et l'attrition prend vingt fois plus de terrain que la frappe profonde (7 772 km²
contre 376). Le critère d'équilibrage du document de conception tient.

**Densité obtenue** : de deux à quatre cartes par camp et par trimestre sur presque tout le déroulé.
Deux dépassements assumés — l'Ukraine monte à cinq ou six aux tours 10, 17, 18 et 19, quand
l'étranglement qu'elle a préparé pendant quinze trimestres arrive à échéance. Un seul creux, T19 côté
russe, où le régime n'a plus qu'une carte à jouer : c'est la conclusion, pas un trou.

---

## 11. Points de vigilance

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
