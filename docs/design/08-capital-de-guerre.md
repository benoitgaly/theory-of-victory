# Le capital de guerre

> Spécification de conception. Le modèle faisant autorité reste
> [`01-modele-de-jeu.md`](01-modele-de-jeu.md) ; la langue visuelle est fixée par
> [`02-direction-artistique.md`](02-direction-artistique.md) et les conventions de dessin par
> [`05-composantes-armee.md`](05-composantes-armee.md) §9, qu'on ne rouvre pas ici.

État : **implémenté**, étapes 1 à 5 du §8, puis **converti en dollars** — le §11 porte cette
révision, décidée après coup, et il l'emporte sur les unités citées dans les §3 et §7 ci-dessous.
Le bandeau ne parle plus qu'une seule langue : les sept postes se comptent en milliards de
dollars, les variations s'impriment en pourcentages, et le poste sous contrainte porte son nom.

Quatre écarts assumés par rapport au texte ci-dessous, et chacun est documenté à l'endroit du
code qui le porte :

1. **Le niveau de vie n'a que deux facteurs**, l'intégrité de l'appareil civil et l'électricité
   civile disponible. Le troisième terme du §4.2, l'érosion de la capacité productive, a été
   retiré : `AttritionPhase` le lit déjà sur la cohésion des élites, et le compter deux fois
   aurait inventé un coût politique que le modèle n'a jamais payé. C'est aussi ce qui rend la
   publication du poste strictement neutre — les trois déroulés ne bougent pas d'un bit.
2. **La ligne de mécontentement reste dans `EnergyPhase`**, exprimée à travers le niveau de vie
   plutôt que déplacée dans `AttritionPhase`. La déplacer aurait changé son rang par rapport à
   la répression et au décrément du trimestre, donc les issues — pour un gain nul.
3. **Le ruban parle en pourcentages**, pas en points d'indice : un poste dont le trimestre de
   base était petit imprimait « −400 pts » à côté d'un « −7 pts », et un ruban se lit d'un trait.
4. **La carte « Frappe sur les entrepôts » n'existe pas encore.** Le mécanisme est complet —
   `StrikeTarget.CivilianIndustry`, `EffectKind.CivilianIndustryDamage`, la chaîne jusqu'au
   capital politique — mais rien dans `cards.fr.json` ni dans les doctrines ne le vise, ce qui
   est exactement la raison pour laquelle rien ne bouge dans les trois déroulés. Le poste des
   usines civiles est donc plat tant que cette carte n'est pas écrite.

Calibration posée : capacité civile = 24 % de la capacité productive soutenable (≈ 413 Md côté
russe, 45 côté ukrainien), et un point de dégât emporte 16 % de cette base — de sorte qu'une
campagne soutenue de deux à trois trimestres coûte huit à douze points de mécontentement, ce
que le §9.2 demandait. Le seuil d'affichage « poste détruit » à 25 % est franchi dix-huit fois
sur les trois déroulés : le dessin le plus fort du bandeau sert, il n'est pas décoratif (§9.4).

---

## 1. Ce que le bandeau doit rendre évident

Le jeu repose sur une phrase, et cette phrase n'est aujourd'hui écrite nulle part à l'écran :

> **Le capital produit les éléments du front. Quand on détruit le capital, le front continue de
> paraître en forme pendant plusieurs trimestres — on avance même — puis il cède d'un bloc.**

C'est la thèse d'O'Brien portée au niveau de l'affichage. Le plateau montre aujourd'hui trois
choses : ce que le front consomme (le tonneau), ce qu'il produit (la carte, les secteurs), et une
chaîne économique par camp. Il ne montre nulle part **le stock de départ** — ce qui reste à brûler.
Or c'est le seul chiffre qui prédit le tour 15 depuis le tour 9.

Trois conséquences de conception, qui commandent tout le reste du document.

**Le bandeau dit le trimestre, le ciseau dit la guerre.** Le bandeau (§7) porte les huit postes et
leur variation du tour ; il ne porte aucune trajectoire. La trajectoire est une seconde pièce (§6),
et c'est elle qui met en scène le décalage. Mélanger les deux produirait sept petites courbes
illisibles et une pièce maîtresse diluée.

**On compare les masses ET les trajectoires — mais pas avec le même signe.** ~~Chaque poste est
dessiné en indice base 100 au T1 de son propre camp.~~ **Révisé au §11.4** : la masse est
désormais une valeur en dollars sur une règle partagée par les deux camps, et c'est le
**pourcentage** imprimé contre chaque masse qui porte la trajectoire. La longueur dit ce qu'on
possède, le pourcentage dit ce que le trimestre en a fait. L'argument d'origine — mettre 310 Md$
de réserves russes en face de 29 Md$ ukrainiens ne dirait qu'une chose, que la partie est jouée
d'avance — reste vrai et reste répondu, mais par le second chiffre plutôt qu'en renonçant au
premier.

**Un indicateur en retard n'est pas une décoration : c'est le sujet.** Le moteur produit déjà trois
couples où la mesure visible ment sur l'état réel — le PIB apparent contre la capacité productive,
le mécontentement réprimé contre la tension latente, la puissance au front contre les flux qui la
nourrissent. Le bandeau doit systématiquement montrer les deux termes de ces couples plutôt que le
seul qui rassure. C'est le même geste, répété sept fois.

---

> **Décision de l'utilisateur, après un aller-retour : le bandeau porte sept postes, et la fusion
> argumentée ci-dessous s'applique.** Le soutien étranger et le soutien international sont un seul
> capital vu à deux étages, et le poste fusionné s'appelle **soutien extérieur**. Il dit ce que le
> camp obtient du dehors : pour l'Ukraine un flux donné qui peut cesser du jour au lendemain, pour
> la Russie un flux acheté qui ne cesse jamais tant qu'il y a de l'argent.
>
> **Ce qui disparaît est la colonne, pas l'information.** Le moteur continue de mesurer la position
> diplomatique — la latitude commerciale laissée à l'envahisseur, lue sur le régime de sanctions,
> les composants comptant double comme partout ailleurs — et `CapitalReader` la publie toujours sur
> les deux camps, avec sa valeur commune et son delta de signe opposé. Deux tests la verrouillent
> (`TheDiplomaticPost_IsOneQuantity_ReadFromBothSidesOfTheTable`,
> `TheDiplomaticPost_CanFallWithNoCardPlayed_BecauseSanctionsErode`). Elle **descend dans
> l'infobulle du poste fusionné**, du point de vue du camp qui la lit, parce que c'est elle qui
> explique pourquoi le flux se resserre ou se relâche : la mesure reste, seul le cartouche a été
> retiré, et le moteur n'a pas été touché pour cela.

## 2. Sept postes pour huit demandés

Les huit postes demandés sont : réserves monétaires, centrales, pétrole, usines civiles, usines
d'armement, capital politique, soutien étranger, soutien international. **Le bandeau en dessine
sept**, parce que soutien étranger et soutien international ne sont pas deux capitaux mais un seul,
vu à deux étages.

**Pourquoi la fusion, et pourquoi elle ne perd rien.** Le moteur porte les deux termes et il les
relie par une causalité stricte : `PoliticalState.ExternalWill` (0 à 100, la volonté politique des
soutiens) commande `ForeignSupport.DisbursementRate`, qui commande `EffectiveGrantBillions`, le flux
du trimestre. Les dessiner en deux cartouches indépendants affirmerait qu'ils peuvent diverger
librement, ce qui est faux. Les dessiner en un seul cartouche à deux étages — **une cuve et un
robinet** — garde les deux valeurs, ajoute leur lien, et produit gratuitement l'image la plus
utile du poste : *le robinet coule encore alors que la cuve est déjà presque vide.* C'est la thèse
du jeu à l'échelle d'un cartouche, et c'est l'argument décisif de la fusion.

L'asymétrie est un acquis du projet (`01` §9) et elle prend ici sa forme visuelle :

| | Ukraine — on **donne** | Russie — on **vend** |
|---|---|---|
| La cuve | `Politics.ExternalWill` — volonté d'un tiers, tombe d'un coup sur une élection | `Economy.TreasuryBillions` face à `Foreign.SupplyCeilingBillions` — un plafond comptable, jamais politique |
| Le robinet | `Foreign.EffectiveGrantBillions`, dont `InKindShare` = 54 % arrive en matériel et contourne les usines | `Foreign.Purchase(budget)`, plafonné à 1,5 Md$/tour, majoré de `PricePremium` = 1,35 |
| Le prix caché | `Foreign.Conditionality`, qui se durcit avec la corruption | `Foreign.Dependency`, qui monte à chaque achat et se paie en concessions |
| Le risque | Arrêt net | Aucun tant qu'elle paie |

Le chiffre à mettre en scène : **4 Md$ par trimestre donnés à l'Ukraine contre 1,5 Md$ que la Russie
peut acheter.** Le flux gratuit est le plus gros des deux — et c'est celui qui peut disparaître en
une journée. Toute la tension du jeu tient dans cette ligne, et elle est lisible sans commentaire
dès que les deux cuves se font face.

---

## 3. Les sept postes

### 3.1 Ce qu'ils valent, ce qu'ils produisent, ce qui les détruit

Les valeurs de cette colonne sont celles d'origine, dans l'unité propre de chaque poste ; le §11.1
donne la conversion en dollars et les valeurs du T1.

| Poste | Ce qu'il vaut | Ce qu'il produit — le chemin vers le front | Ce qui le détruit |
|---|---|---|---|
| **Réserves monétaires** | Md$. RU 310, UA 29 au T1 | Comble l'écart entre ce que la recette finance et ce que la guerre veut dépenser. Seul poste dont l'emploi **est** la destruction | La ponction elle-même (12 %/tour RU, 9 % UA), et rien d'autre |
| **Centrales électriques** | GW installés contre GW demandés. RU 245/148, UA 36/15,5 | Ne va jamais au front. **Ouvre ou ferme** la production d'armes, le raffinage, le PIB, le chauffage | Frappes réversibles (sous-stations, 55 % réparés/tour) et permanentes (salles des machines, 6 %/tour, plafond 55 %) |
| **Pétrole** | RU : Md$ de recette/trimestre. UA : Md$ de facture/trimestre | RU : ≈ 46 Md$/trimestre au baril à 100, la première source du budget de guerre. UA : une charge nette qui ampute le même budget | Frappes sur le raffinage, décote et friction des sanctions, et le baril lui-même |
| **Usines civiles** | Md$ de capacité civile × intégrité. **N'existe pas dans le moteur** (§4) | Le niveau de vie, donc le consentement à la guerre | Frappes sur les entrepôts et les usines, délestage civil, mobilisation qui prend les ouvriers |
| **Usines d'armement** | Md$ de capacité installée par tour. RU 3,72, UA 0,33 — **onze contre un** | Les obus, les drones, les missiles, les intercepteurs. Le seul poste qui produit du matériel | Plafond de composants sous sanctions, délestage industriel, frappes, et le plafond structurel de 3,5 × la ligne d'avant-guerre |
| **Tenue du pouvoir** | Points de marge avant la rupture, sur 58 (§3.3) | Le capital politique du tour, qui paie les cartes | L'écart de financement (le coefficient le plus violent du moteur), les pertes, les sanctions sur composants, la défaite visible |
| **Soutien étranger** | Cuve : 0 à 100 (UA) ou Md$ disponibles (RU). Robinet : Md$/trimestre | UA : 54 % en matériel, qui contourne entièrement les usines nationales. RU : des armes hors capacité domestique | Une élection, le baril cher qui nourrit l'inflation chez les soutiens, la conditionnalité qui se durcit avec la corruption |

### 3.2 D'où vient la valeur dans le code

Aucune propriété n'est à créer, sauf pour les usines civiles.

Toutes les valeurs affichées sont converties en milliards de dollars par `CapitalReader.Measure`
(§11.1) ; la colonne ci-dessous nomme la mesure du moteur d'où chacune sort.

| Poste | Mesure du moteur | Repères et seuils |
|---|---|---|
| Réserves | `Economy.ReservesBillions` | `Economy.ReserveQuartersLeft` (le décompte, déjà calculé), `LastTurnReserveDrawBillions` (la ponction du tour) |
| Centrales | `Grid.AvailableCapacityGw` × 1,5 Md$/GW, seuil sur `Grid.DemandGw(Winter)` | `Grid.ShortfallRatio`, `PermanentDamageGw`, `WinterDemandMultiplier` |
| Pétrole | `Economy.LastTurnOilRevenueBillions` × 4, ou la facture dérivée de `OilImportMbd × 91,25 × OilPrice / 1000` × 4 | `Economy.RefiningIntegrity`, `Sanctions.ExportDiscountPerBarrel`, `Sanctions.FrictionRate`, `GameState.OilPrice` |
| Usines civiles | `CivilianIndustry.CapacityBillions × Integrity` | `CivilianIndustry.LivingStandard` |
| Usines d'armement | `Industry.TotalCapacityValueBillions()` × 4 | `Industry.GetCapacityPerTurn(kind)` contre `GetCapacityCeiling(kind)`, `Sanctions.ProductionCeilingMultiplier`, `Grid.IndustrialSupplyRatio(season)` |
| Tenue du pouvoir | `part × Economy.ProductiveCapacityBillions × (58 − Politics.RegimeStress) / 58` | `PopularDiscontent`, `EliteCohesion`, `LatentTension`, `Repression`, `PoliticalCapital` |
| Soutien étranger | Le robinet, annualisé : `Foreign.EffectiveGrantBillions × 4` ou le budget d'achat du tour × 4. La cuve — `Politics.ExternalWill` (UA), `min(TreasuryBillions, SupplyCeilingBillions × PricePremium × 4)` (RU) — descend en seconde lecture | `Foreign.Conditionality`, `Foreign.Dependency`, `Foreign.InKindShare` |

Le seuil de rupture du régime, **58**, est aujourd'hui une constante privée de `ControlPhase`
(`RegimeCollapseStress`). Le bandeau doit le dessiner : il faut donc la rendre publique. C'est la
seule modification de visibilité que la publication exige.

### 3.3 Le capital politique : « la tenue du pouvoir »

L'utilisateur a hésité sur le mot et sa description vaut mieux que n'importe quel terme abstrait :
*Poutine est menacé en permanence d'un coup d'État.* Deux objets du moteur se disputent le nom de
« capital politique », et il faut trancher lequel monte dans le bandeau.

- `Politics.PoliticalCapital` (0 à 30) est le **mana** : la monnaie qui paie les cartes en V2. Il est
  déjà à l'écran, sur les pastilles de coût des cartes.
- `Politics.RegimeStress` est **la menace** : la distance au moment où l'appareil se fracture. C'est
  ce que l'utilisateur décrit, et ce n'est nulle part.

Le bandeau porte la seconde, et la première y entre comme son **rendement**. C'est cohérent avec la
grammaire de tous les autres postes : un capital vaut quelque chose et produit un flux. La tenue du
pouvoir produit littéralement le capital politique du trimestre — `AttritionPhase` le calcule déjà
ainsi, `3 × Morale/100` pour un régime autoritaire, `2 × ExternalWill/100 × Morale/100` pour une
démocratie. Le régime qui se fissure ne paie plus ses cartes : la boucle est déjà écrite, elle n'est
pas montrée.

**Nom retenu : « Tenue du pouvoir ».** Il couvre les deux formes sans en trahir aucune, et il dit
une action continue plutôt qu'un état — ce qui est exactement le point : *tenir* est un effort de
chaque trimestre. Le sous-titre du cartouche diffère par camp, et cette différence est la leçon :

- Russie — « fracture de l'appareil ». Le régime tombe de l'intérieur, jamais par la rue.
- Ukraine — « épuisement de la volonté ». Le régime ne tombe pas : le pays négocie. Lassitude,
  élections suspendues, corruption qui durcit la conditionnalité.

**La menace doit se lire comme permanente, jamais comme un score.** Deux règles de dessin en
découlent, et elles suffisent :

1. Le cartouche ne montre pas la valeur, il montre **la marge restante avant 58**. Une marge se lit
   comme un danger ; un score se lit comme une note.
2. Les deux jauges qui la composent sont dessinées séparément et **l'élite compte double**, comme
   dans le moteur : `RegimeStress = (visible + 2 × fracture des élites + tension latente) / 3,2`. La
   rue est spectaculaire, l'appareil décide. Un joueur qui regarde la mauvaise jauge se fait
   surprendre — et c'est le comportement souhaité.

Le paradoxe de la répression tombe alors tout seul dans le dessin : la répression rabote la jauge
visible et gonfle la tension latente. Le cartouche russe affiche donc une rue calme au-dessus d'une
réserve de pression qui monte. **Quatrième instance du même décalage**, et la plus démonstrative.

---

## 4. Les usines civiles — vérification et spécification

**Vérifié : elles n'existent pas.** Le moteur ne connaît que trois traces d'économie civile, et
aucune ne fait ce que l'exemple Wildberries demande.

- `Doctrine.CivilianShare` — une ligne de budget qui augmente `ProductiveCapacityBillions` et le
  moral. Une dépense, pas un actif : rien ne peut la détruire.
- `EnergyGrid.CivilianShareOfDemand` — la part de la demande électrique délestée en premier.
- `Economy.ProductiveCapacityBillions` — la capacité productive soutenable, agrégat de tout, jamais
  ciblable en tant que telle.

`StrikeTarget` ne compte que quatre cibles — réseau, raffinage, industrie d'armement, logistique.
Une frappe sur `Industry` ne touche que `Industry.SetCapacityPerTurn` des ressources militaires.
**Il n'existe aujourd'hui aucun chemin entre la destruction d'un entrepôt et le mécontentement
populaire.** Le mécanisme de l'exemple est absent, et c'est le seul poste du bandeau qui demande du
moteur.

### 4.1 La structure

```
CivilianIndustry
    CapacityBillions      // capacité civile installée, en Md$ de production annuelle
    Integrity             // 0..1 — part de l'appareil civil en état de produire
    ReversibleDamage      // entrepôts, plateformes logistiques : semaines
    PermanentDamage       // lignes d'assemblage, outillage : années
    RepairRatePerTurn     // 0,50 sur le réversible
    RebuildPerTurn        // 0,05 sur le permanent
```

Deux niveaux de dégât, exactement comme le réseau électrique : la distinction sous-stations /
salles des machines se transpose sans rien inventer. Un entrepôt brûlé se rebâtit en un trimestre,
une ligne d'assemblage détruite ne revient pas avant la fin de la partie. **Frapper juste, c'est
viser la ligne, pas la palette.**

### 4.2 Ce qu'elles produisent : le niveau de vie

```
LivingStandard = Integrity × CivilianSupplyRatio(saison) × (ProductiveCapacity / ProductiveCapacité₁)
```

Un indice à 1,00 au T1. Il descend quand on casse les usines, quand on coupe le courant civil, et
quand la mobilisation prend les ouvriers — les trois canaux réels, et les trois sont déjà dans le
moteur sauf le premier.

### 4.3 La chaîne de conséquences, jusqu'au bout

C'est l'exemple de l'utilisateur, écrit comme une suite de causes dont chaque maillon existe déjà
sauf le premier :

```
frappe sur les entrepôts
  → CivilianIndustry.ReversibleDamage ↑           (nouveau)
  → Integrity ↓  →  LivingStandard ↓              (nouveau)
  → Politics.PopularDiscontent ↑                  (existe)
  → Politics.RegimeStress ↑                       (existe, l'élite compte double)
  → Politics.PoliticalCapital ↓ le tour suivant   (existe, AttritionPhase)
  → moins de cartes jouables                      (existe, en V2)
```

Six maillons, un seul à écrire. C'est le rapport qu'on cherche partout dans ce projet.

### 4.4 La règle qui protège les trois issues

Le niveau de vie **remplace** le canal existant, il ne s'y ajoute pas. `EnergyPhase` déplace
aujourd'hui le mécontentement directement (`PopularDiscontent += (1 − civilian) × 8`). Cette ligne
disparaît et son coefficient part dans le terme du niveau de vie, inchangé :

```
PopularDiscontent += (1 − LivingStandard) × 8
```

Avec `Integrity = 1` et une capacité productive à son niveau de référence, `LivingStandard` vaut
exactement `CivilianSupplyRatio` : le terme est **algébriquement identique à l'actuel**. Comme
aucune carte du calendrier ne vise les usines civiles et qu'aucune doctrine ne prend cette cible,
les trois issues du scénario — victoire ukrainienne au T19, front figé, effondrement ukrainien vers
T10 — sont inchangées au bit près. C'est un test, pas une espérance :
`CivilianIndustry_UntouchedRun_ReproducesTheThreeOutcomes_Bitwise`.

Une seule cible nouvelle (`StrikeTarget.CivilianIndustry`) et une seule carte pour l'emprunter
(« Frappe sur les entrepôts »), hors calendrier des trois variantes. La leçon devient jouable sans
que rien de démontré ne bouge.

### 4.5 Ce qu'il ne faut pas modéliser côté civil

Ni secteurs d'activité, ni chômage, ni panier de consommation, ni inflation alimentaire distincte.
Une capacité, une intégrité, un indice. Le joueur décide s'il envoie ses drones sur un entrepôt ou
sur une raffinerie ; tout le reste est du texte de carte.

---

## 5. L'évolution du tour

C'est la demande centrale, et elle se décompose en trois exigences dont aucune n'est décorative.

### 5.1 Le delta est calculé par le moteur, jamais par la vue

`TurnEngine.CaptureOpeningPosition` prend déjà une photo des dépôts et du ratio de génération avant
que les dix phases ne tournent : *une pente demande deux points*. On généralise au vecteur des sept
postes. La vue reçoit `opening` et `closing` dans le même instantané et ne soustrait jamais deux
tours entre eux — sans quoi le premier tour n'a pas de delta, un changement de variante en produit
un faux, et l'origine de la variation est perdue.

### 5.2 Une variation ordinaire et une destruction ne sont pas le même chiffre

Un poste qui perd 4 GW ne dit rien tant qu'on ignore si c'est la demande d'hiver, une réparation en
retard, ou une salle des machines partie pour de bon. Chaque poste porte donc un **delta décomposé
en trois causes au plus**, et jamais plus de trois :

| Cause | Signe | Nature | Dessin |
|---|---|---|---|
| `regeneration` | + | Réparation, croissance, recette, aide reçue | Filet gravé, 2 px, encre tertiaire |
| `consumption` | − | Ponction ordinaire, entretien, érosion, usure | Filet gravé, 2 px, encre tertiaire |
| `destruction` | − | Frappe, carte, perte permanente | **Matière pleine**, hachure `#a8322a` à 0,45, avec l'encoche |

> **Règle de lecture, tenable sur un plateau : le gravé est ordinaire, le plein est une
> destruction.** Aucun pourcentage à comparer, aucune légende à consulter.

La destruction porte **le nom de sa cause** : le titre de la carte, ou la cible de la frappe. Un
chiffre rouge sans nom est un chiffre qu'on ne peut pas contester ; c'est précisément ce qu'un outil
pédagogique doit refuser.

### 5.3 Le ruban de conséquence

Trois deltas côte à côte ne font pas une chaîne. Sur les tours où une destruction a produit une
suite, le bandeau déplie sous le camp concerné un **ruban** de deux à quatre maillons :

```
« Frappe sur les entrepôts »  →  usines civiles −12 %  →  niveau de vie −0,06  →  tenue du pouvoir −4
```

Quatre règles le rendent lisible plutôt que bavard :

1. **Un seul ruban par camp et par tour**, le plus vif. Le moteur sait déjà choisir : les
   `PressureAlert` sont triées du plus tranchant au plus faible dans `TurnEngine.Freeze`.
2. **Le ruban n'apparaît que s'il y a eu destruction.** Un trimestre calme est un bandeau mince,
   et cette minceur est une information.
3. **L'origine est attribuée, jamais devinée.** Une table statique `EffectKind → poste` dit quels
   effets touchent quel poste ; si exactement une carte du tour porte un effet de cette famille,
   elle est nommée. Sinon on regarde la frappe du tour (`InvaderStrike.Target`). Sinon le ruban est
   dessiné sans nom d'origine. Une attribution honnête vaut mieux qu'une attribution jolie.
4. **La dérivation ne touche aucune phase.** Un `CapitalChainBuilder` lit l'ouverture, la clôture,
   les cartes jouées et la frappe. Rien dans la simulation ne bouge.

---

## 6. Le décalage capital / front — « le ciseau »

**Il mérite son propre dessin.** Dilué dans le bandeau, il deviendrait sept petites courbes ; et le
décalage n'est pas un attribut d'un poste, c'est le rapport entre l'ensemble des postes et la
puissance qu'ils nourrissent. C'est la pièce que le visiteur doit emporter.

### 6.1 Les deux courbes

- **Le front** — `SideSnapshot.CombatPower`, la puissance de combat soutenable, en indice base 100 au
  T1. C'est ce que le joueur prend pour son succès. Les trimestres où le camp a gagné du terrain
  portent un petit repère plein sur la courbe : *on avance, et la courbe monte.*
- **Le capital** — l'indice composite des sept postes, base 100 au T1 (§6.2).

### 6.2 Composer l'indice sans mentir

Trois règles, et chacune répond à une erreur tentante.

**Ce n'est pas un minimum.** La règle du minimum ne vaut que pour les flux consommés au front, où
une douve courte plafonne tout. Un capital ne se comporte pas ainsi : une trésorerie vide se
supporte quelques trimestres, un réseau détruit se contourne un temps. Appliquer le minimum au
capital serait la même erreur de catégorie que celle déjà refusée pour les effectifs.

**Ce n'est pas une somme non plus.** Une réserve de 310 Md$ masquerait un réseau mort. La somme
autorise exactement la compensation qu'on veut réfuter.

**C'est une moyenne géométrique**, plancher à 15 points par poste :

```
IndiceCapital = ( ∏ clamp(indice(poste), 15, 150) ) ^ (1/7)
```

La moyenne géométrique punit le déséquilibre sans l'annuler : deux postes à 50 pèsent plus lourd
qu'un poste à 0 et un à 100, ce qui est le comportement juste d'un capital. Le plancher dit
l'essentiel en un chiffre : **aucun poste ne met le capital à zéro à lui seul.** Un camp ne meurt
pas d'avoir perdu son pétrole ; il meurt de l'avoir perdu en même temps que le reste.

Côté ukrainien, le pétrole est une charge et non une recette : son indice s'inverse,
`100 × facture₁ / facture`. Un baril qui monte fait donc baisser le capital ukrainien — c'est
exactement le canal 2 du §7 du modèle, obtenu sans une ligne de règle nouvelle.

### 6.3 Le dessin

`viewBox="0 0 560 230"`, **un ciseau par camp**, les deux côte à côte sur la largeur du bandeau.

- Axe des tours en abscisse, 26 px par trimestre, dix-neuf trimestres de x = 62 à x = 530.
- Ordonnée de 0 à 130, la ligne 100 en pointillé à y = 60, le zéro à y = 210.
- **Front** : trait plein d'encre chaude `#1a1815`, 2,2 px. C'est ce qu'on voit.
- **Capital** : trait de la couleur du camp, 2,2 px. C'est ce qu'on a.
- **La surface entre les deux est le sujet.** Front au-dessus du capital : hachure `#a8322a` à
  0,22 — *on brûle*. Capital au-dessus : aplat crème `--card-2` — *on reconstitue*.
- Le trimestre du croisement porte un filet vertical d'encre et une seule phrase en petites
  capitales : « à partir de T9, le front vit sur le capital ».
- Le dernier point de chaque courbe porte sa valeur en sérif 20 px.

Un joueur qui gagne du terrain en brûlant son capital voit la hachure s'installer six tours avant de
perdre. Un joueur qui recule en préservant son capital voit l'aplat crème et comprend qu'il est en
train de gagner. C'est la demande, littéralement.

---

## 7. Le dessin du bandeau

Même langue que le tonneau et que les cinq parcs : papier `#f2efe7`, carton `#fbf9f4`, filets
`#d9d1be`, encre chaude `#1a1815`, chiffres en sérif, libellés en petites capitales 9,5 px à
`letter-spacing: 0.13em`, graisse 700, couleur `#8b8578`. Fond clair, jamais sombre.

### 7.1 Deux teintes nouvelles, cinq reprises

| Poste | Teinte | Origine |
|---|---|---|
| Réserves monétaires | `#8a6f2b` | **nouvelle** — or bruni, sourd exprès pour ne pas se confondre avec `--gold`, réservé aux événements |
| Centrales électriques | `#c2621a` | `FAMILY_ACCENT["Énergie"]` |
| Pétrole | `#8a5a2b` | la teinte du carburant dans `FLOWS` : c'est la même matière |
| Usines civiles | `#6f8060` | **nouvelle** — vert sourd |
| Usines d'armement | `#4a6070` | `FAMILY_ACCENT["Militaire et technologique"]` |
| Tenue du pouvoir | `#8a4b2a` | `FAMILY_ACCENT["Politique interne"]` |
| Soutien étranger | `#2f8f8f` | seule teinte franche du bandeau, parce que c'est le seul poste qui vient de l'extérieur du camp |

**La teinte porte le poste, la position porte le camp.** Sept postes doivent se distinguer les uns
des autres à l'intérieur d'une rangée ; les deux camps, eux, sont déjà séparés sans ambiguïté par le
haut et le bas. Le camp est rappelé par un filet de 3 px en `--ru` ou `--ua` sur le bord extérieur
de sa rangée, et par son nom une seule fois à l'extrémité gauche.

### 7.2 Géométrie

`viewBox="0 0 1240 318"`, hauteur recalculée quand un ruban se déplie.

**Deux colonnes qui se font face, et non deux rangées empilées.** La Russie tient la moitié gauche
et s'aligne à gauche, l'Ukraine tient la moitié droite et s'aligne à droite ; les huit postes se
lisent l'un sous l'autre, chacun en vis-à-vis de son homologue. C'est ce qui permet de comparer
poste par poste d'un balayage horizontal — un empilement en miroir haut/bas oblige au contraire à
sauter d'un bord à l'autre du bandeau pour rapprocher deux chiffres qui parlent de la même chose.

- **Sept rangées de 36 px**, de y = 54 à y = 306, un filet de séparation entre deux.
- **Gouttière centrale** : x = 528 à 712. C'est le sol commun, celui d'où partent les deux masses.
- **Piste russe** : la masse est ancrée à x = 528 et pousse **vers la gauche**, 2,26 px par point
  d'indice, bornée à 322 px. La bande des chiffres l'attend de x = 22 à 186.
- **Piste ukrainienne** : symétrique, ancrée à x = 712, poussant **vers la droite**.
- Au-dessus de la gouttière, y = 8 à 46 : le cartouche du trimestre — « hiver 2024 », le Brent en
  sérif, et un flocon gravé quand la saison est l'hiver. La saison décide, elle mérite un signe et
  pas un mot. Il coiffe la gouttière parce qu'il n'appartient à aucun des deux camps.
- Le nom du camp est écrit une seule fois, au bord qui lui revient, souligné de son filet de 3 px.

### 7.3 Le nom au centre, les deux camps qui tirent

Le nom du poste est écrit **une seule fois**, dans la gouttière, précédé de son icône. Les deux
masses le tirent de part et d'autre, comme une corde. Cela divise par deux le texte du bandeau
— une donnée répétée à l'identique n'est pas une donnée — et cela produit l'image juste : les deux
camps se disputent le même poste.

**Chaque poste porte son icône, dessinée à la main en SVG dans le trait gravé du plateau** — ni
police d'icônes, ni emoji, ni bibliothèque : le site est servi en statique et rien d'extérieur ne
peut être chargé. Trois lingots empilés pour les réserves, un pylône pour les centrales, un baril
et sa goutte pour le pétrole, un toit en shed pour les usines civiles, un obus sur son convoyeur
pour l'armement, une colonne fendue pour la tenue du pouvoir, une caisse où entre une flèche pour
le soutien extérieur. Chacune tient dans une boîte de 24 et se dessine à l'encre de son poste :
l'icône, le nom et les deux masses se lisent alors comme un seul objet. Les sept s'alignent sur la
même verticale — une colonne de signes se parcourt d'un regard, là où sept signes décalés se
lisent un par un.

Une seule caisse pour le flux donné et le flux acheté : c'est un seul capital, et ce qui change
d'un camp à l'autre est le prix, pas la nature — or le prix se lit dans les chiffres.

Les deux usines demandaient chacune leur silhouette : le shed civil et l'obus militaire sont deux
dessins et non deux variantes du même, parce que c'est précisément l'écart entre ces deux postes —
onze contre un côté russe — qui est la leçon du bandeau.

Un seul poste par tour reçoit une **pastille pleine `#1a1815`** derrière son nom, texte en papier :
celui que le moteur désigne comme la pression la plus vive du trimestre, lu dans les
`PressureAlert` déjà triées. Une seule autorité par information — le dessin ne désigne jamais un
poste que le moteur n'a pas nommé.

### 7.4 Anatomie d'un cartouche

Sur 16 px de haut, depuis la gouttière vers le bord du bandeau :

- **La masse** — un bloc plein de la teinte du poste, longueur = indice, 226 px pour 100, bornée à
  322 px (142 %). Chant supérieur biseauté en blanc à 0,34, comme les douves du tonneau : la matière
  a une épaisseur.
- **Le niveau de février 2022** — un filet pointillé en travers de la piste, à 226 px de la
  gouttière, sans aucune étiquette. C'est le repère qui rend toute masse lisible sans échelle,
  exactement comme le pointillé à 100 % du tonneau.
- **L'encoche de destruction** — la part détruite ce trimestre est **découpée dans le bout de la
  masse par une arête irrégulière**, non par un trait droit, et remplie d'une hachure `#a8322a` à
  0,45 avec un liseré gravé sur la ligne de coupe. On voit le morceau manquant. C'est la convention
  déjà posée pour les cinq parcs (`05` §9.1) et il n'y a aucune raison d'en avoir deux.
- **Le filet de variation ordinaire** — un trait creux de 2 px en travers de la piste, posé là où la
  masse se tenait à l'ouverture du trimestre. Gravé, jamais plein.
- **La valeur** — sérif 19 px en `#1a1815`, au bord du bandeau, en milliards de dollars, dans une
  bande que la masse n'atteint jamais : le chiffre reste lisible quelle que soit la longueur
  atteinte, et c'est ce que poser le chiffre au bout de la masse ne garantissait pas. La variation
  lui fait face en 11 px, adossée à la piste, en pourcentage signé ou en tiret (§11.5), **colorée
  uniquement s'il s'agit d'une destruction**.
- **Le seuil**, sur les trois postes qui en ont un, et seulement ceux-là : les réserves à quatre
  trimestres de ponction restante, les centrales à la demande d'hiver, la tenue du pouvoir à 58.
  Un pointillé rouge de 1 px en travers de la piste. **Les quatre autres postes n'ont pas de seuil
  et n'en dessinent pas.**
- **Le sablier et le cadenas** — repris de `05` §9.1. Un sablier de 16 × 22 quand la perte met des
  tours à mordre, avec un grain par tour ; un cadenas de 14 × 16 en encre pleine quand la perte est
  définitive à l'échelle de la partie. Le cadenas n'apparaît que sur la part permanente des
  centrales et des usines. Il dit « irréparable » mieux que n'importe quel chiffre.

### 7.5 Un poste détruit se voit

Sous **25 % de son indice de départ**, un poste cesse d'être une masse : il est dessiné en filet
creux, et la hachure de destruction remplit tout ce qui manque **jusqu'au pointillé de février
2022**. On voit le fantôme de ce qu'il y avait. Le cartouche devient un trou dans le bandeau, et un
trou se repère avant d'être lu — ce qui est le principe de direction artistique n° 3.

### 7.6 Le cartouche du soutien étranger

Seul cartouche à deux étages, parce que seul poste où la cuve et le robinet peuvent diverger.

- La **cuve** occupe les deux tiers extérieurs : un contenant gravé dont le remplissage est la
  volonté (UA) ou la capacité de payer (RU).
- Le **robinet** occupe le tiers intérieur, contre la bande de partage : un trait plein dont
  l'épaisseur, de 2 à 9 px, est le flux du trimestre.
- Une cuve presque vide sous un robinet encore large est l'image que le poste existe pour produire.
  Elle précède l'arrêt de l'aide de deux à trois trimestres dans la variante « Le soutien s'arrête »,
  et c'est exactement le préavis que le jeu doit donner.
- Côté russe, un troisième repère : la **dépendance**, une petite chaîne gravée dont les maillons se
  ferment à mesure que `Foreign.Dependency` monte. Elle ne coûte rien à dessiner et elle dit ce que
  l'achat coûte vraiment.

### 7.7 Où ça vit

- **Le bandeau** : écran « Résolution », **au-dessus de la carte**, pleine largeur, avant le bloc
  `.field`. C'est là que l'utilisateur le demande et c'est là qu'il est utile : la carte montre le
  thermomètre, le bandeau montre le moteur, et les deux se lisent dans un seul regard.
- **Le ciseau** : immédiatement sous le bandeau, avant la carte.
- Les deux écrans de génération de force ne changent pas. Le tonneau, la chaîne et la main y disent
  déjà le détail d'un camp ; le bandeau dit les deux camps d'un coup.

Cela livre au passage la recommandation 4.4 de la direction artistique — comparer les deux camps
côte à côte — qui attendait une décision de conception. Elle est prise ici.

---

## 8. Chemin d'implémentation

Cinq étapes. **Les quatre premières ne touchent pas la simulation** : elles publient, agrègent et
dessinent des valeurs qui existent déjà. Les tests de non-régression doivent y passer au bit près.
Seule l'étape 5 modifie le moteur, et elle porte sa propre garantie de neutralité (§4.4).

### Étape 1 — publier le capital *(dessin pur, aucun risque)*

| Fichier | Modification |
|---|---|
| `src/TheoryOfVictory.Core/CapitalPost.cs` | **nouveau** — `Code`, `Value`, `IndexBase100`, `Opening`, `Regeneration`, `Consumption`, `Destruction`, `DestructionCause`, `Threshold`, `PermanentLoss` |
| `src/TheoryOfVictory.Core/GameState.cs` | `SideSnapshot.Capital` — la liste des postes ; `CapitalIndex` — la moyenne géométrique du §6.2 |
| `src/TheoryOfVictory.Engine/TurnEngine.cs` | `CaptureOpeningPosition` étendu au vecteur des postes ; `Capture` remplit `Capital` |
| `src/TheoryOfVictory.Engine/Phases/ControlPhase.cs` | `RegimeCollapseStress` passe de privé à `public const` — le bandeau dessine ce seuil |
| `tests/…/ModelRulesTests.cs` | `CapitalPosts_AreCaptured_WithoutChangingAnyOutcome` · `CapitalIndex_NeverZeroesOnASinglePost` |

Six postes sur sept : les usines civiles arrivent à l'étape 5.

### Étape 2 — le bandeau dessiné *(dessin pur, aucun C#)*

`src/TheoryOfVictory.Web/wwwroot/js/board.js` — `renderCapitalBand(t)` appelée en tête de
`renderBattlefield`, avant `.field`. `src/TheoryOfVictory.Web/wwwroot/css/site.css` — la classe
`.capital-band` et les jetons de teinte du §7.1.

### Étape 3 — le ciseau *(dessin pur, aucun risque)*

`board.js` — `renderScissor(game, sideCode)`, deux panneaux côte à côte sous le bandeau. La série
est reconstruite depuis `game.turns[].invader.capitalIndex` et `.combatPower`, déjà publiés à
l'étape 1. Test : `CapitalIndex_FallsBeforeCombatPower_InTheVictoryRun` — le décalage doit exister
dans la variante « L'Occident joue ses cartes », sinon la pièce maîtresse ne démontre rien.

### Étape 4 — le ruban de conséquence *(dérivation, aucune phase touchée)*

`src/TheoryOfVictory.Engine/CapitalChainBuilder.cs` **nouveau** — la table `EffectKind → poste` et
l'attribution du §5.3 ; `GameState.cs` — `SideSnapshot.Chain` ; `board.js` — le ruban. Il fonctionne
dès cette étape sur les campagnes contre le réseau électrique, qui existent déjà au calendrier et
qui déplacent déjà le mécontentement : la chaîne est démontrable **avant** que les usines civiles
n'existent.

### Étape 5 — les usines civiles *(touche la simulation, à recalibrer)*

| Fichier | Modification |
|---|---|
| `src/TheoryOfVictory.Core/CivilianIndustry.cs` | **nouveau** — §4.1 |
| `src/TheoryOfVictory.Core/Belligerent.cs` | propriété `Civilian` |
| `src/TheoryOfVictory.Core/StrikeCampaign.cs` | `StrikeTarget.CivilianIndustry` |
| `src/TheoryOfVictory.Engine/Phases/DeepStrikePhase.cs` | le cas de la nouvelle cible, réversible et permanent |
| `src/TheoryOfVictory.Engine/Phases/EnergyPhase.cs` | la ligne de mécontentement **retirée** — elle passe dans le niveau de vie |
| `src/TheoryOfVictory.Engine/Phases/AttritionPhase.cs` | `LivingStandard` calculé, mécontentement déplacé, réparations civiles |
| `src/TheoryOfVictory.Engine/Scenarios/UkraineScenario.cs` | capacité civile initiale des deux camps |
| `src/TheoryOfVictory.Engine/data/cards.fr.json` | « Frappe sur les entrepôts », hors calendrier |
| `tests/…/ModelRulesTests.cs` | `CivilianIndustry_UntouchedRun_ReproducesTheThreeOutcomes_Bitwise` · `BurningTheWarehouses_MovesTheRegime_ThroughLivingStandardOnly` |

---

## 9. Points de calibration à vérifier

À examiner en implémentant, sans les trancher ici.

1. **La tenue du pouvoir au T1 surprend.** Avec les valeurs du scénario, `RegimeStress` vaut environ
   11 côté russe et 16 côté ukrainien, pour un seuil à 58 : l'Ukraine part **plus près de la
   rupture** que la Russie. Cela vient de `EliteCohesion` (86 contre 78) et du poids double de la
   fracture des élites. C'est peut-être juste — une démocratie en guerre est structurellement plus
   fragile à l'arrière — mais c'est contre-intuitif et le bandeau va le rendre visible pour la
   première fois. À trancher avant de le publier, pas après.
2. **Capacité civile initiale.** Le paramètre qui décide si l'exemple Wildberries est un désagrément
   ou une crise. Le viser de sorte qu'une campagne soutenue de deux à trois trimestres coûte 8 à
   12 points de mécontentement — sensible, jamais décisif à soi seul. Ne pas céder à la tentation de
   le gonfler : la démonstration porte sur le rapport entre l'effet et son prix.
3. **Le plancher de 15 points de la moyenne géométrique.** Trop haut, l'indice ne bouge plus ; trop
   bas, un seul poste effondré emporte tout et l'on retombe dans la règle du minimum qu'on a
   explicitement refusée pour le capital.
4. **Le seuil d'affichage « poste détruit » à 25 %.** À vérifier sur les dix-neuf tours des trois
   variantes : si aucun poste ne le franchit jamais, le dessin le plus fort du bandeau ne sert
   jamais, et il faut remonter le seuil ou admettre que le capital ne s'effondre pas si loin.

---

## 10. Ce qui alourdirait le bandeau sans rien apporter

À écarter explicitement, pour que la question ne se repose pas.

- **Un huitième cartouche pour le soutien international.** Deux cartouches indépendants pour une
  seule causalité ; la cuve et le robinet disent la même chose en mieux (§2).
- **Une sparkline par poste.** Sept petites courbes dans le bandeau tueraient le ciseau, qui est la
  pièce maîtresse. Le bandeau dit le trimestre, le ciseau dit la guerre.
- **Une échelle commune aux deux camps.** Elle affirmerait que la partie est jouée au T1 et
  masquerait la seule question qui compte : qui brûle plus vite.
- **Le mana (`PoliticalCapital`) comme poste à part.** Il est le rendement de la tenue du pouvoir,
  pas un capital ; il est déjà lisible sur les pastilles de coût des cartes.
- **Un score de capital unique en gros chiffre.** L'indice composite n'existe que pour tracer une
  courbe contre celle du front. Affiché seul, il redevient exactement le chiffre agrégé et rassurant
  que ce jeu passe son temps à dénoncer. Le bilan en dollars (§11.3) ne rouvre pas cette porte :
  il publie **deux** totaux par camp — le patrimoine et le flux annuel — et **jamais leur somme**,
  parce qu'additionner un fonds souverain et une année de recette pétrolière est exactement
  l'arithmétique des communiqués de guerre.
- **Une jauge de PIB apparent dans le bandeau.** Le PIB apparent monte quand tout va mal : c'est le
  piège keynésien, il a sa place dans la chaîne économique de chaque camp, pas dans un bandeau qui
  prétend dire ce qu'un camp possède encore.
- **Des seuils inventés sur les postes qui n'en ont pas.** Le pétrole, les usines et le soutien
  n'ont pas de valeur de rupture ; en dessiner une serait affirmer une mécanique qui n'existe pas.

---

## 11. Le bilan en dollars

> **Demande de l'utilisateur** : « Il faut que le capital soit comptabilisé en milliards de
> dollars. » Elle découle de la phrase qui fonde le jeu : *l'idée du jeu, c'est capitaliste — le
> capital produit les éléments pour le front.* Un capital se compte en argent.

Le bandeau parlait cinq langues : un indice base 100 pour les réserves, des gigawatts pour les
centrales, des points de marge pour le régime, un ratio pour le soutien, des milliards par tour
pour l'armement. Cinq langues font une liste, pas un bilan. On ne pouvait ni additionner deux
postes, ni dire lequel pesait le plus, ni répondre à la seule question qu'un bilan pose : *ce
camp vit-il sur ce qu'il possède ou sur ce qu'il produit ?*

### 11.1 Ce qui a été converti, et ce qui ne l'a pas été

**Cinq postes sur sept n'ont demandé aucun coefficient** : le moteur les tenait déjà en argent.
Deux coefficients seulement ont été posés, et ce sont les deux seuls chiffres de ce document qui
ne viennent pas de la simulation.

| Poste | Ce qu'on compte | Conversion | T1 Russie | T1 Ukraine |
|---|---|---|---|---|
| **Réserves monétaires** | Le fonds souverain | aucune — c'est déjà de l'argent | 310 | 26,4 |
| **Centrales électriques** | La valeur de remplacement du parc encore debout | **1,5 Md$ par GW installé** | 367,5 | 39,0 |
| **Pétrole** | La production annuelle au baril du jour | recette (ou facture) du trimestre **× 4** | 148,9 | 7,0 *(facture)* |
| **Usines civiles** | Une année de production civile | aucune — la capacité civile est déjà annuelle | 412,8 | 45,1 |
| **Usines d'armement** | Une année de production d'armes | capacité installée par tour **× 4** | 12,7 | 1,9 |
| **Tenue du pouvoir** | Ce que le régime peut encore consacrer à tenir | **3,5 % (autocratie) / 2,0 % (démocratie)** de la capacité productive soutenable, × la part de marge restante | 47,2 | 2,6 |
| **Soutien extérieur** | Une année du flux obtenu du dehors | flux du trimestre **× 4** | 6,5 *(acheté)* | 16,5 *(reçu)* |

**Production ou outil ?** Les deux usines sont comptées en **production annuelle**, et non en
valeur de l'outil. C'est le seul choix qui les garde comparables entre elles — or leur écart est
la leçon du bandeau — et c'est celui qui suit le moteur, où la capacité civile est déjà une
production annuelle et la capacité d'armement une production par tour. Compter l'outil aurait
demandé un multiple capitalistique par filière, donc deux coefficients inventés de plus pour
répondre à une question que le jeu ne pose pas.

**Pourquoi la tenue du pouvoir se compte ainsi.** La piste évidente — *ce que coûte par an le
maintien de la coalition au pouvoir* — dit l'inverse de ce qui se passe : un régime en difficulté
paie **plus** pour tenir, donc son poste gonflerait à mesure qu'il approche de la rupture. Ce
qu'on met au bilan est donc la **marge**, valorisée au prix courant du maintien :

```
facture annuelle de maintien = part × capacité productive soutenable
tenue du pouvoir            = facture × (58 − RegimeStress) / 58
```

Le poste se vide exactement quand l'appareil se fissure, la masse qui touche la gouttière est
toujours le régime qui tombe, et le seuil à 58 reste le seul de ce poste. La facture se prélève
sur la **capacité productive soutenable** et jamais sur le PIB apparent : le PIB apparent est
gonflé par la guerre elle-même, et un régime qui consomme son économie paraîtrait chaque
trimestre mieux armé pour financer sa propre survie.

### 11.2 Incertitude, et ce que le dépôt promet

Le [`README`](../../README.md) §« Statut des chiffres » vaut ici sans exception : ce sont des
**ordres de grandeur de travail**, posés pour que le bandeau produise un bilan discutable. Ils ne
sont pas sourcés un par un et ne doivent pas être cités comme des faits.

- **1,5 Md$ par GW** — un parc post-soviétique est thermique pour l'essentiel, avec du nucléaire
  et de l'hydraulique dessous. Une tranche prise isolément vaut la moitié ou le double. Le
  coefficient est **le même pour les deux camps** : il déplace le poids du poste dans le
  patrimoine, jamais le rapport entre Moscou et Kyiv.
- **3,5 % et 2,0 % de la capacité soutenable** — le chiffre le plus fragile des deux, et il est
  assumé comme tel. Il vise l'addition sécurité intérieure + clientèle + subventions à la paix
  sociale ; à un facteur deux près, le poste garderait la même forme, puisque c'est la marge qui
  le fait bouger et non la facture.
- Le **× 4** n'est pas un coefficient : c'est un changement de période. Le moteur pense au
  trimestre, un bilan se lit à l'année.

### 11.3 Deux natures, deux totaux, jamais leur somme

Un fonds souverain et une année de recette pétrolière ne s'additionnent pas. Chaque poste porte
donc sa nature, et le bandeau publie **deux totaux par camp** :

| Nature | Postes | T1 Russie | T1 Ukraine |
|---|---|---|---|
| **Patrimoine** — ce qu'on détient | réserves, centrales | **678 Md$** | **65 Md$** |
| **Flux annuel** — ce qu'on produit, reçoit ou doit payer | pétrole, usines civiles, usines d'armement, tenue du pouvoir, soutien extérieur | **628 Md$/an** | **59 Md$/an** |

La facture pétrolière ukrainienne est une **charge** : elle se retranche du flux au lieu de le
gonfler. Le soutien international, lui, n'entre dans aucun des deux : ce n'est pas une
possession mais une position, et il reste lu sur cent, dans l'infobulle du poste qu'il commande.

**Ce que le bilan révèle et que l'indice base 100 cachait.** Les deux camps sont séparés d'un
ordre de grandeur sur *tous* les postes — dix contre un sur le patrimoine, onze sur les réserves,
neuf sur les centrales et l'appareil civil, sept sur l'armement, dix-huit sur la tenue du
pouvoir — **sauf un seul, où le petit dépasse le grand** : le soutien extérieur, 16,5 Md$/an
donnés à l'Ukraine contre 6,5 achetés par la Russie, et jusqu'à 80 contre 4 dans les déroulés où
l'Occident tient. La seule ligne du bandeau qui penche vers la droite est celle qu'on ne fabrique
pas chez soi — et c'est aussi la seule qui peut s'arrêter du jour au lendemain. Un test la
verrouille (`TheBalanceSheet_ShowsRussiaAnOrderOfMagnitudeHeavier_ExceptOnForeignSupport`).

### 11.4 L'échelle des masses

Une règle **par rangée**, partagée par les deux camps, posée sur le plus gros capital que ce
poste atteint dans **tout le déroulé**. Trois conséquences, et chacune répond à une objection.

1. **Partagée par les deux camps** : un milliard vaut la même longueur à gauche et à droite,
   sinon le bilan en dollars ne se comparerait pas. La masse ukrainienne devient courte — dix à
   trente pixels contre trois cents — et c'est une information, pas un défaut. Aucune ne
   descend sous cinq pixels sur les soixante-quatre bandeaux des trois déroulés.
2. **Posée sur le maximum du déroulé, pas sur février 2022** : l'aide occidentale quintuple dans
   le déroulé de la victoire. Une règle calée sur le premier trimestre aurait plafonné soixante
   masses sur la butée de la piste, et une masse plafonnée cache exactement ce que le poste
   existe pour montrer. Une masse ne change donc jamais d'échelle d'un trimestre à l'autre : elle
   se compare toujours à elle-même.
3. **Une règle par rangée, pas une pour les sept** : treize milliards d'usines d'armement contre
   quatre cent treize d'appareil civil, sur une règle commune, feraient de l'armement un trait.
   Ce sont les **chiffres**, au bord du bandeau, qui comparent un poste à un autre.

Ce que la masse a perdu, le pourcentage le reprend : **la longueur dit ce qu'on possède, le
pourcentage dit ce que le trimestre en a fait.** Deux questions, deux réponses, et plus une seule
ligne qui prétendait aux deux. Le pointillé de février 2022 reste sur chaque piste, à la place de
ce camp-là : la distance entre la masse et son propre pointillé reste lisible côté russe et
devient minuscule côté ukrainien, ce qui est précisément pourquoi le pourcentage est imprimé.

**La charge se dessine rayée.** Une facture qui s'allonge n'est pas un capital qui grossit. Le
pétrole ukrainien est donc dessiné en matière rayée et non en aplat plein — une masse pleine dit
« je possède », une masse rayée dit « je paie » — et il échappe au dessin du poste détruit, qui
n'a pas de sens pour une charge.

### 11.5 La forme des variations

> **Demande de l'utilisateur** : « Les variations c'est "-" pour inchangé, et sinon en % + ou
> - x %. »

Une seule forme, partout : le cartouche, l'infobulle et le ruban de conséquence. Plus de
« inchangé » écrit en toutes lettres, plus de « −14,2 Md », plus de « +0,9 pts », plus de
« /100 ».

- La variation est la **part de l'indice du poste**, comptée du point de vue du capital :
  `(indice − indice à l'ouverture) / indice à l'ouverture`. Une facture qui double imprime donc
  −50 %, et non +100 %.
- **Le seuil du tiret est 0,05 %.** En dessous, l'arrondi à la décimale imprimerait « +0,0 % » :
  ce n'est pas une variation, c'est un arrondi, et l'écrire comme une variation ferait chercher
  une cause qui n'existe pas. Sur les soixante-quatre bandeaux, 331 des 896 variations imprimées
  sont des tirets — un tiers d'un bandeau qui ne bouge pas est une information, pas un vide.
- Le pourcentage n'est **coloré que s'il s'agit d'une destruction** : la règle du §5.2 est
  inchangée, seule l'unité a bougé.

### 11.6 Le goulot du trimestre

> **Question de l'utilisateur** : pourquoi ce cercle noir autour de « Réserves » ?

C'était le repère du poste que l'alerte la plus vive du trimestre désigne — la règle centrale du
jeu, *la puissance est la ressource la plus rare* — et **rien à l'écran ne le disait**. Un signe
qu'il faut deviner passe pour une bizarrerie.

La pastille reste, et une **bannière** la précède, juste au-dessus du bandeau : la même pastille
noire portant « goulot du trimestre », le camp concerné, le nom du poste, et la phrase que le
moteur a écrite pour cette alerte. Le lecteur fait le lien de lui-même, sans infobulle et sans
légende. La bannière nomme le camp, ce que la pastille ne pouvait pas faire — le nom du poste est
écrit une seule fois, dans la gouttière, et il appartient aux deux camps. Un trimestre sans
alerte de capital n'affiche aucune bannière : le bandeau commence alors directement, et cette
minceur est elle-même une information.
