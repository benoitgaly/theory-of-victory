# Calendrier proposé — dix-neuf trimestres, deux à trois cartes par camp

> Proposition de densification du calendrier de `UkraineScenario.BuildCalendar`. Le deck étant passé
> de 41 à 92 cartes, le déroulé peut enfin montrer ce que chaque camp décide **chaque** trimestre, au
> lieu d'une carte isolée entourée de remplissage.
>
> **Statut : vérifié.** Le calendrier ci-dessous a été rejoué sur le moteur courant. Les trois issues
> tombent exactement où elles doivent tomber — `Resolve` T19, `Holds` T19, `Collapses` T10 — et les
> assertions des tests portant sur les trois déroulés passent, duel de decks compris. Détail au bas
> du document.

---

## 1. Correspondance tour / trimestre

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

## 2. Lecture chronologique du socle

Ce que chaque trimestre raconte, avant de donner les codes.

| Tour | Ce que fait l'Ukraine et ses soutiens | Ce que fait la Russie | Ce que fait le monde |
|---|---|---|---|
| T1 | Premier train de sanctions, renseignement allié, filières de formation | Verrouillage intérieur et saturation de l'espace médiatique | La boue de mars |
| T2 | L'aide s'industrialise, la conditionnalité s'installe, le recrutement se réforme | Réquisition des chaînes, bascule vers l'économie de guerre | |
| T3 | HIMARS, puis les dépôts reculent de quatre-vingts kilomètres | Prisons ouvertes, premiers drones achetés à l'étranger | Un gazoduc saute |
| T4 | Kharkiv et Kherson ; les drones navals entrent en mer Noire | Mobilisation partielle, missiles achetés, premières vagues de leurres | |
| T5 | Plafonnement du baril, campagne diplomatique | Campagne contre le réseau, bombes planantes, barils réorientés vers l'Asie | Hiver clément : la campagne tombe dans le vide |
| T6 | Embargo sur les machines-outils, interception à bas coût | L'embargo est contourné le trimestre même ; le brouillage passe à l'échelle | Mutinerie armée |
| T7 | L'offensive d'été s'enlise ; la production nationale de drones démarre | La chaîne de montage domestique, les ports céréaliers | L'inflation ronge les soutiens |
| T8 | Le train de sanctions est reconduit — une sanction est un entretien | Obus étrangers, primes d'engagement, flotte fantôme | Une autre guerre capte l'attention |
| T9 | Loi de mobilisation, interception à bas coût, ateliers de drones | Réseau, leurres, primes | |
| T10 | Raffineries, coalition drones, brouillage des kits de guidage | Licence transférée, bombes planantes, assauts à découvert | Bascule électorale européenne |
| T11 | Rail coupé, nouvelle frappe profonde | La contre-batterie annule la frappe : la carte est jouée pour rien | |
| T12 | Prêt gagé sur les avoirs gelés, réseaux îlotés | Fibre optique, brouillage, salles des machines — les deux sont contrées | |
| T13 | Sanctions sur la flotte fantôme | Le réseau de contournement répond le trimestre même | Hiver rigoureux |
| T14 | Crise anticorruption, rail coupé de nouveau | Bataillons ferroviaires, achat de complaisance diplomatique | Trou dans la couverture antiaérienne |
| T15 | Coalition drones, ateliers | Tour de vis, assauts, drones étrangers | Fracture au sommet, pourparlers sans lendemain |
| T16 | Interception, transparence, coalition | Ports céréaliers, leurres, réorientation des flux | |
| T17 | Formation, ateliers | Primes, propagande, répression | |
| T18 | Interception, coalition, ateliers | Bombes planantes, missiles achetés, propagande | |
| T19 | *(variantes)* | | |

---

## 3. Socle commun — à coller tel quel

```csharp
// T1 · hiver 2022 — l'invasion
new ScheduledCard { Turn = 1, CardCode = "sanctions_package_1" },
new ScheduledCard { Turn = 1, CardCode = "allied_intelligence" },
new ScheduledCard { Turn = 1, CardCode = "nato_training_pipeline" },
new ScheduledCard { Turn = 1, CardCode = "state_propaganda_surge" },
new ScheduledCard { Turn = 1, CardCode = "domestic_repression" },
new ScheduledCard { Turn = 1, CardCode = "rasputitsa" },

// T2 · printemps 2022 — le repli du nord, l'aide s'organise
new ScheduledCard { Turn = 2, CardCode = "western_aid_opens" },
new ScheduledCard { Turn = 2, CardCode = "transparency_reform" },
new ScheduledCard { Turn = 2, CardCode = "recruitment_reform" },
new ScheduledCard { Turn = 2, CardCode = "industrial_requisition" },
new ScheduledCard { Turn = 2, CardCode = "war_economy_conversion" },

// T3 · été 2022 — HIMARS
new ScheduledCard { Turn = 3, CardCode = "himars_deep_strike" },
new ScheduledCard { Turn = 3, CardCode = "depot_strikes" },
new ScheduledCard { Turn = 3, CardCode = "prison_recruitment" },
new ScheduledCard { Turn = 3, CardCode = "foreign_drones" },
new ScheduledCard { Turn = 3, CardCode = "pipeline_sabotage" },

// T4 · automne 2022 — Kharkiv, Kherson, mobilisation
new ScheduledCard { Turn = 4, CardCode = "counter_offensive_2022" },
new ScheduledCard { Turn = 4, CardCode = "naval_drones_black_sea" },
new ScheduledCard { Turn = 4, CardCode = "partial_mobilisation" },
new ScheduledCard { Turn = 4, CardCode = "foreign_ballistic_missiles" },
new ScheduledCard { Turn = 4, CardCode = "decoy_saturation" },

// T5 · hiver 2023 — la campagne contre le réseau, et l'hiver qui la dément
new ScheduledCard { Turn = 5, CardCode = "oil_price_cap" },
new ScheduledCard { Turn = 5, CardCode = "diplomatic_campaign" },
new ScheduledCard { Turn = 5, CardCode = "grid_campaign" },
new ScheduledCard { Turn = 5, CardCode = "glide_bombs" },
new ScheduledCard { Turn = 5, CardCode = "oil_export_rerouting" },
new ScheduledCard { Turn = 5, CardCode = "mild_winter" },

// T6 · printemps 2023 — l'embargo posé, l'embargo contourné
new ScheduledCard { Turn = 6, CardCode = "component_embargo" },
new ScheduledCard { Turn = 6, CardCode = "cheap_interception" },
new ScheduledCard { Turn = 6, CardCode = "component_smuggling" },
new ScheduledCard { Turn = 6, CardCode = "electronic_warfare_scaling" },
new ScheduledCard { Turn = 6, CardCode = "armed_mutiny" },

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
new ScheduledCard { Turn = 8, CardCode = "foreign_shells" },
new ScheduledCard { Turn = 8, CardCode = "contract_recruitment_drive" },
new ScheduledCard { Turn = 8, CardCode = "shadow_fleet" },
new ScheduledCard { Turn = 8, CardCode = "attention_elsewhere" },
new ScheduledCard { Turn = 8, CardCode = "rasputitsa" },

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
new ScheduledCard { Turn = 15, CardCode = "domestic_repression" },
new ScheduledCard { Turn = 15, CardCode = "meat_assault" },
new ScheduledCard { Turn = 15, CardCode = "foreign_drones" },
new ScheduledCard { Turn = 15, CardCode = "elite_fracture" },
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

## 4. Variante `Holds` — le soutien tient, sans plus

```csharp
new ScheduledCard { Turn = 9, CardCode = "aid_blocked" },
new ScheduledCard { Turn = 11, CardCode = "aid_unblocked" },
new ScheduledCard { Turn = 13, CardCode = "budget_fatigue" },
new ScheduledCard { Turn = 17, CardCode = "parliament_veto" },
```

La crise des munitions de 2023-2024 est conservée telle quelle. S'y ajoutent deux frottements qui ne
rompent rien : le soutien s'amaigrit et se conditionne, il ne s'arrête jamais.

## 5. Variante `Collapses` — le soutien s'arrête

```csharp
new ScheduledCard { Turn = 6, CardCode = "us_election_swing" },
new ScheduledCard { Turn = 6, CardCode = "aid_collapse" },
new ScheduledCard { Turn = 8, CardCode = "budget_fatigue" },
```

Rien avant T6 : les cinq premiers tours doivent rester **strictement identiques** à ceux de `Holds`,
c'est la démonstration elle-même. La fatigue budgétaire arrive à T8 et non à T5 pour cette raison.

## 6. Variante `Resolve` — l'Occident joue ses cartes

```csharp
new ScheduledCard { Turn = 9, CardCode = "aid_blocked" },
new ScheduledCard { Turn = 10, CardCode = "aid_unblocked" },
new ScheduledCard { Turn = 10, CardCode = "component_embargo_total" },
new ScheduledCard { Turn = 11, CardCode = "aid_predictable" },
new ScheduledCard { Turn = 12, CardCode = "refinery_campaign_sustained" },
new ScheduledCard { Turn = 13, CardCode = "frozen_assets_released" },
new ScheduledCard { Turn = 15, CardCode = "refinery_campaign_sustained" },
new ScheduledCard { Turn = 16, CardCode = "aid_predictable" },
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
paquet de T19 n'est pas une victoire militaire, c'est une caisse qui se ferme.

---

## 7. Ce que le calendrier corrige au passage

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

## 8. Cartes volontairement laissées hors calendrier

Douze cartes ne sont jouées à aucun tour. Ce n'est pas un oubli.

| Carte | Raison |
|---|---|
| `war_tax_rise`, `sovereign_fund_draw` (hors T19), `mobilisation_wave_two` | Leur bénéfice repose sur `TreasuryDelta` ou sur une ressource que le vocabulaire d'effets ne sait pas encore atteindre ; jouées dans le socle, elles seraient un coût net pour la Russie. Voir la note ci-dessous. |
| `oil_price_spike`, `global_recession` | `OilPriceDelta` décale **définitivement** tout le calendrier pétrolier. Dans un déroulé historique où `OilPriceCalendar` encode déjà la trajectoire réelle du baril, elles font doublon. Excellentes cartes de deck, mauvaises cartes de chronique. |
| `dam_breach`, `demographic_wall` | Réservoir d'aléa pour la V1.1 et la V2. |
| `refinery_air_defence`, `counter_intelligence` | Contre-cartes dont la cible n'est pas jouée dans le socle ; elles existent pour le bluff de la V2. |
| `air_defence_surge`, `decapitation_strike`, `drone_swarm_scaling`, `chinese_pressure` | Déjà hors calendrier avant cette proposition ; les trois premières servent au duel de decks. |

---

## 9. Résultats observés

Rejoué sur le moteur courant, calendrier ci-dessus substitué au calendrier actuel.

| Variante | Issue | Tour de décision | Lecture |
|---|---|---|---|
| `Resolve` | `regime_collapse`, victoire ukrainienne | **T19** | Puissance russe à 30 % de son pic, écart de financement 0,46, réserves 302 → 40 Md, ratio de génération ukrainien 1,00, +1 022 km² encore aux mains de l'envahisseur |
| `Holds` | `frozen_front` | **T19** | Personne ne rompt, la partie va au bout |
| `Collapses` | `military_collapse`, victoire russe | **T10** | Couverture ukrainienne à 1,00 en T6 et T7, puis 0,15 en T8 : la latence de deux tours est intacte |

Duel de decks rejoué sur la base `Holds` densifiée : frappe profonde **gagne**, attrition frontale
**ne gagne pas**, et l'attrition prend vingt fois plus de terrain que la frappe profonde (7 772 km²
contre 376). Le critère d'équilibrage du document de conception tient.

Le nombre de cartes par camp et par tour va de **deux à trois**. L'écran de génération de force cesse
d'afficher une main dont une seule carte a été jouée.

---

## 10. Point de vigilance : le capital politique

Densifier triple le nombre de cartes payées. Avec une génération de deux à trois points par tour et
un plafond de trente, les deux camps terminent en découvert profond — de l'ordre de cent points côté
russe, deux cents côté ukrainien. La V1.0 joue son calendrier quoi qu'il arrive et n'enregistre que
le découvert, donc rien ne casse ; mais l'économie de mana telle qu'elle est calibrée ne supporterait
pas la V2 avec cette densité. Deux leviers, à trancher avant la V2 : relever la génération par tour,
ou multiplier les cartes qui en rendent — `state_propaganda_surge` et `diplomatic_campaign` le font
déjà, chacune de son côté du plateau, et c'est exactement l'asymétrie décrite dans le modèle.
