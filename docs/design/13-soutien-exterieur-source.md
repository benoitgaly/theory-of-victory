# Le soutien extérieur — la source, et ce qu'elle dit du modèle

> Le poste `Foreign.EffectiveGrantBillions` porte, dans le moteur, tout ce que l'Ukraine reçoit sans
> le payer : munitions livrées en nature, intercepteurs, et l'argent qui finance le reste du budget
> de guerre. C'est un poste central — il commande la moitié de la démonstration du jeu — et il
> n'avait jusqu'ici aucune source. Ce document lui en donne une, dit ce qu'elle vaut, et confronte
> le modèle à ce qu'elle mesure.
>
> **Ce document ne modifie aucune règle du moteur.** Aucune valeur, aucun seuil, aucune trajectoire
> n'a bougé. Il constate, il chiffre, et il spécifie une suite qui n'est pas écrite.

---

## 1. La source

**Ukraine Support Tracker**, Institut de Kiel pour l'économie mondiale (Kiel Institute for the
World Economy), Trebesch *et al.* — <https://www.kielinstitut.de/ukrainetracker>.

| | |
|---|---|
| Millésime consulté | **Release 30**, publiée le **13 août 2026** |
| Période couverte | 24 janvier 2022 → **juin 2026** |
| Auteurs | Trebesch, Antezza, Bushnell, Dyussimbinov, Frank (A.), Frank (P.), Franz, Kharitonov, Kumar, Nishikawa, Rebinskaya, Schramm, Weiser, Schade |
| Unité du graphique lu | milliards d'euros, **moyenne mensuelle par année**, **corrigée de l'inflation** |
| Graphique | « Aid allocations by donor group », deux panneaux (aide militaire ; aide financière et humanitaire), trois groupes empilés |

**Ce qu'est un « groupe de donateurs », tel que la source le déclare.** *Europe* désigne l'ensemble
des États membres de l'Union européenne, plus l'Islande, la Norvège, la Suisse et le Royaume-Uni,
institutions européennes comprises. *Autres* désigne l'Australie, le Canada, la Chine, l'Inde, le
Japon, la Nouvelle-Zélande, la Corée du Sud, Taïwan et la Turquie. Les *États-Unis* sont seuls dans
leur colonne — et c'est tout le sujet de ce document.

**Ce qu'est une « allocation », et pourquoi ce mot compte.** Le tracker distingue trois grandeurs
et n'en publie que deux :

- les **engagements** (*commitments*) : ce qu'un gouvernement a annoncé, promis ou voté ;
- les **allocations** : la part de ces engagements effectivement affectée à un paquet concret, en
  cours de mise en œuvre ou de livraison. C'est l'indicateur principal du tracker, et le plus
  conservateur des deux ;
- les **décaissements** et **livraisons** : ce qui est réellement arrivé en Ukraine. L'équipe de
  Kiel les collecte quand elle le peut, mais ne les publie que pour un sous-ensemble — quelques
  catégories d'armes lourdes et le soutien budgétaire.

L'écart entre engagement et allocation est important, et la source le documente elle-même — c'est
la raison pour laquelle elle publie l'allocation comme indicateur principal. **Nous n'avons pas
retenu de chiffre pour cet écart** : la page publique du tracker n'affiche pas de total
engagé / alloué consultable, et un ordre de grandeur cité de mémoire n'a pas sa place dans un
document qui prétend être une source. L'écart entre allocation et décaissement, lui, n'est pas
publiable du tout — et c'est exactement l'écart dont le modèle aurait besoin. **Le graphique lu
ici mesure donc une grandeur voisine de celle du modèle, jamais la même.** La section 5 en tire
toutes les conséquences.

---

## 2. Ce que le graphique dit, en Md€ par mois

Valeurs **relevées sur le graphique publié**, à ±0,05 Md€ près. Ce n'est pas un export de données,
c'est une lecture d'image, et la section 4 dit ce que cela coûte.

### Aide militaire

| Année | Europe | États-Unis | Autres | **Total** | Part des États-Unis |
|---|---:|---:|---:|---:|---:|
| 2022 | 0,92 | 1,44 | 0,14 | **2,50** | **58 %** |
| 2023 | 1,44 | 1,57 | 0,08 | **3,09** | 51 % |
| 2024 | 1,46 | 1,33 | 0,06 | **2,85** | 47 % |
| 2025 | 2,36 | 0,06 | 0,14 | **2,56** | 2 % |
| 2026 (jan.–juin) | 2,23 | ~0,02 | 0,07 | **2,32** | **1 %** |

### Aide financière et humanitaire

| Année | Europe | États-Unis | Autres | **Total** | Part des États-Unis |
|---|---:|---:|---:|---:|---:|
| 2022 | 1,65 | 0,98 | 0,25 | **2,88** | 34 % |
| 2023 | 1,76 | 0,64 | 0,44 | **2,84** | 23 % |
| 2024 | 1,63 | 1,67 | 0,39 | **3,69** | 45 % |
| 2025 | 2,71 | ~0,00 | 0,30 | **3,01** | 0 % |
| 2026 (jan.–juin) | 1,54 | ~0,00 | 0,23 | **1,77** | 0 % |

**Deux recoupements indépendants confirment la lecture.** Le communiqué de Kiel du 4 juin 2026
(*Military aid holds steady as focus shifts to drones*, arrêté à avril 2026) chiffre l'aide
militaire européenne à **2,4 Md€ par mois en 2025** — la lecture donne 2,36 — et à **2 Md€ par
mois sur janvier-avril 2026**, contre 2,23 sur le semestre complet : l'écart est celui qu'attendent
les deux mois de mai et juin, où le communiqué de Release 30 signale une hausse européenne portée
par le *Ukraine Support Loan*, près de **11 Md€ sur deux mois**. Le même communiqué de juin qualifie
l'aide financière et humanitaire européenne de janvier-avril 2026 de « moins d'un cinquième de la
moyenne 2025 », soit environ 0,5 Md€ par mois contre 2,71 — un rapport de 0,18, cohérent au dixième
près. **La lecture graphique tient sur les deux points que la source publie en clair.**

---

## 3. Le même tableau en milliards de dollars par an

**Taux retenu : 1 EUR = 1,15 USD.** Le cours de référence de la Banque centrale européenne tenait
1,1535 au 7 août 2026, dernier point que nous ayons vérifié nous-mêmes ; l'arrondi à 1,15 est en
deçà de la précision que le reste du document revendique, et rien ici ne dépend de la troisième
décimale. Ce n'est donc pas une hypothèse de travail mais un cours daté ; il est néanmoins appliqué uniformément à
une série corrigée de l'inflation, ce qui est une convention et non une conversion exacte — les
euros de 2022 et ceux de 2026 ne se convertissent pas au même cours dans la réalité. La conversion
est donc valable pour comparer des ordres de grandeur, jamais pour recalculer une année isolée à
la décimale.

Passage : Md€/mois **× 12 × 1,15 = × 13,8** → Md$/an.

### Aide militaire, Md$ par an

| Année | Europe | États-Unis | Autres | **Total** |
|---|---:|---:|---:|---:|
| 2022 | 12,7 | 19,9 | 1,9 | **34,5** |
| 2023 | 19,9 | 21,7 | 1,1 | **42,6** |
| 2024 | 20,1 | 18,4 | 0,8 | **39,3** |
| 2025 | 32,6 | 0,8 | 1,9 | **35,3** |
| 2026 (annualisé sur six mois) | 30,8 | 0,3 | 1,0 | **32,0** |

### Aide financière et humanitaire, Md$ par an

| Année | Europe | États-Unis | Autres | **Total** |
|---|---:|---:|---:|---:|
| 2022 | 22,8 | 13,5 | 3,5 | **39,7** |
| 2023 | 24,3 | 8,8 | 6,1 | **39,2** |
| 2024 | 22,5 | 23,0 | 5,4 | **50,9** |
| 2025 | 37,4 | 0,0 | 4,1 | **41,5** |
| 2026 (annualisé sur six mois) | 21,3 | 0,0 | 3,2 | **24,4** |

### Les deux panneaux additionnés, Md$ par an

| Année | 2022 | 2023 | 2024 | 2025 | 2026 |
|---|---:|---:|---:|---:|---:|
| **Total du soutien alloué** | **74,2** | **81,8** | **90,3** | **76,9** | **56,4** |

Les colonnes de détail peuvent ne pas sommer exactement au total : effet d'arrondi à la première
décimale, jamais supérieur à 0,1 Md$.

---

## 4. Le degré de confiance, assumé

**C'est une lecture graphique, pas un export.** Les vingt-quatre valeurs des deux tableaux de la
section 2 ont été relevées à l'œil sur un graphique empilé, à ±0,05 Md€ près, soit ±0,7 Md$ par an
une fois annualisées. Sur les postes européens et américains, où les barres font plusieurs
centimètres, la précision est bonne. Sur le poste « Autres », qui vaut 0,06 Md€ en 2024, l'incertitude
de lecture est du même ordre que la valeur : **ce poste ne doit pas être cité au-delà de son ordre de
grandeur.** Le tracker publie ses données en clair ; les reprendre depuis le jeu de données mettrait
fin à cette réserve, et c'est le premier geste à faire si l'on veut durcir ce document.

**« Allocation » n'est pas « décaissement », et l'écart n'est pas un détail.** Le modèle porte
explicitement la distinction — `ForeignSupport.DisbursementRate` est *la part de l'engagement
effectivement versée*, et l'audit de réalisme salue ce mécanisme comme « exactement la bonne
forme ». Or la série lue mesure l'échelon du dessus. Une année où beaucoup est alloué et peu livré
apparaît haute dans la source et basse dans le modèle, **et les deux ont raison**. La section 5.2
montre que c'est précisément ce qui s'est produit en 2024, et que le désaccord entre les deux
courbes est un point en faveur du modèle plutôt que contre lui.

**Le point 2026 ne couvre que six mois.** Il est annualisé par simple multiplication, ce qui suppose
un second semestre semblable au premier. Cette hypothèse est déjà démentie à l'intérieur du
semestre : les deux derniers mois lus, mai et juin, portent à eux seuls près de 11 Md€ de prêt
européen. Le chiffre de 56,4 Md$ pour 2026 est donc **le moins solide des cinq**, et il doit être
lu comme un plancher de tendance, pas comme une prévision.

**Le seul point de la simulation entièrement hors du domaine de la source.** Le tracker commence le
24 janvier 2022. Le premier tour du jeu, l'automne 2021, n'y est donc pas — et le modèle y porte
4,12 Md$ de soutien extérieur pour le trimestre, soit **16,5 Md$ par an avant le premier coup de
feu**. La réalité était d'un autre ordre : l'aide de sécurité américaine à l'Ukraine a totalisé
**environ 2,7 à 2,8 Md$ sur les huit années 2014-2021**, soit de l'ordre de 0,35 Md$ par an. Même
en y ajoutant l'assistance macro-financière européenne, le total d'avant-guerre reste sous quelques
milliards par an. **Le modèle ouvre donc sur une valeur de guerre appliquée à un trimestre de paix**,
un ordre de grandeur au-dessus du réel. C'est le prix de n'avoir pas de rampe : le scénario veut être
en régime de guerre dès le troisième tour et n'a pas de mécanisme pour y monter. L'écart ne fausse
aucune des trois issues — il porte sur un seul tour, avant que quoi que ce soit ne se consomme — mais
il est réel et il est ici plutôt que masqué.

---

## 5. La confrontation au modèle

### 5.1 Le volume tient, et il tient bien

Ce que le moteur produit sur le poste `Foreign.EffectiveGrantBillions`, en Md$ par an, face à ce
que la source mesure :

| Année | Militaire seule (source) | Total alloué (source) | Modèle — « Le soutien tient, sans plus » | Modèle — « L'Occident joue ses cartes » |
|---|---:|---:|---:|---:|
| 2022 | 34,5 | 74,2 | 52,7 | 52,7 |
| 2023 | 42,6 | 81,8 | 64,8 | 64,8 |
| 2024 | 39,3 | 90,3 | 48,5 | 54,7 |
| 2025 | 35,3 | 76,9 | 70,7 | 79,7 |
| 2026 | 32,0 | 56,4 | 48,6 | 71,6 |

**Le premier résultat est net : sur les cinq années, et pour les deux déroulés, la valeur du modèle
tombe à l'intérieur de l'encadrement [aide militaire seule ; aide totale allouée].** Une seule
exception, l'année 2026 du déroulé de victoire, à 71,6 contre un plafond de 56,4 — mais c'est une
contrefactuelle assumée, où l'Occident a joué les avoirs gelés, et son écart au réel est le propos
de la variante, pas son défaut.

Cet encadrement n'est pas un intervalle de confort : c'est exactement la fourchette dans laquelle le
poste doit tomber. Le grant du modèle finance deux choses — le matériel livré en nature (44 % du
grant, une fois appliqués `InKindShare = 0,54` et la ventilation 52/12/18 de `RevenuePhase`) et
l'argent qui alimente le budget de guerre pour le reste. Il correspond donc à **l'aide militaire
plus l'aide financière, humanitaire exclue** : une grandeur qui, par construction, se situe entre
les deux bornes du tableau. Le modèle y est, chaque année.

Deux repères pour finir de cadrer :

- **La valeur de croisière du moteur, 16,2 Md$ par trimestre soit 64,8 Md$ par an, vaut 85 % de la
  moyenne quinquennale du total alloué** (75,9 Md$/an) et 1,77 fois la moyenne de l'aide militaire
  seule (36,7 Md$/an). C'est le bon étage.
- **Le maximum jamais atteint sur toute la simulation, 19,95 Md$ par trimestre soit environ
  80 Md$ par an, reste sous le pic réel de 2024** (90,3 Md$ alloués). Le modèle ne sait donc pas
  produire l'année la plus généreuse de la guerre — mais il n'en a pas besoin, puisque cette
  générosité-là était une allocation et non une livraison. Voir immédiatement ci-dessous.

*Note de version.* L'[audit de réalisme](09-audit-realisme.md) §6 chiffrait l'aide à « 11,3 Md$ par
trimestre, soit environ 45 Md$ par an » : c'était la mesure sur l'instantané publié le 13 août 2026.
Le moteur a évolué depuis, et le régime établi vaut aujourd'hui 16,2 Md$ par trimestre. Les chiffres
de ce document portent sur cette mesure-là. Le jugement de l'audit — « l'aide occidentale est bien
dimensionnée » — n'en est pas affaibli : il l'est mieux qu'avant.

### 5.2 La forme décroche en 2024, et c'est le modèle qui a raison

Les deux courbes n'ont pas le même profil, et l'écart se concentre sur une année.

| | 2022 | 2023 | 2024 | 2025 | 2026 |
|---|---:|---:|---:|---:|---:|
| Total alloué (source) | 74,2 | 81,8 | **90,3** | 76,9 | 56,4 |
| Modèle, « le soutien tient » | 52,7 | 64,8 | **48,5** | **70,7** | 48,6 |
| Rapport modèle / source | 0,71 | 0,79 | **0,54** | 0,92 | 0,86 |

La source culmine en 2024. Le modèle y creuse son minimum, puis culmine en 2025. **Le rapport tombe
à 0,54 sur cette seule année, contre 0,71 à 0,92 partout ailleurs** : c'est là, et nulle part
ailleurs, que le modèle décroche de la source.

Le creux de 2024 du modèle n'est pas un accident : c'est la carte `aid_blocked` jouée au T9 et
`aid_unblocked` au T11, calquées sur le blocage budgétaire américain d'octobre 2023 à avril 2024.
Le modèle représente donc **la disette du front**. La source, elle, enregistre le paquet américain
voté le 24 avril 2024 — 23 Md$ d'aide financière américaine allouée sur l'année, contre 8,8 l'année
précédente — au moment où il est *alloué*, non au moment où les obus arrivent.

**C'est la démonstration la plus claire de la section 4 : la source ne peut pas montrer la crise des
munitions de 2024, elle montre son contraire.** Le front ukrainien tirait 2 000 coups par jour
contre 6 000 côté russe pendant que Kiel enregistrait la meilleure année d'allocation de la guerre.
La divergence entre les deux courbes n'est pas un écart de calibration à corriger : c'est la mesure
de la distance entre allouer et livrer, et le modèle est du bon côté de cette distance. Toute
tentative de « recaler » le poste 2024 sur les 90,3 Md$ de la source ferait disparaître du moteur
l'événement même que le modèle s'est donné pour critère de validation à l'[§18 du modèle de
jeu](01-modele-de-jeu.md).

### 5.3 Verdict

**Le volume du poste est juste** — encadré correctement sur les cinq années, valeur de croisière au
bon étage, plafond cohérent avec ce qu'un flux réellement livré peut valoir.

**La forme est juste sauf en 2024, où le désaccord est un mérite** et non un défaut, parce que la
source mesure l'allocation et le modèle le versement.

**Un seul écart franc subsiste : le trimestre d'avant-guerre**, dix fois trop haut, hors du domaine
de la source et documenté au §4.

**Et une lacune de nature, qui n'est pas un écart de valeur** — le modèle ne sait pas représenter ce
que la source montre le plus nettement. C'est l'objet de la section suivante, et elle est instruite
en propre au [§6 de l'audit de réalisme](09-audit-realisme.md).

---

## 6. La bascule que le modèle ne sait pas dire

C'est le fait le plus visible du graphique, et il n'a pas d'équivalent dans le moteur.

Entre 2022 et 2026, sur l'aide militaire : **la part américaine passe de 58 % à 1 %, l'Europe passe
de 0,92 à 2,23 Md€ par mois — deux fois et demie —, et le total ne bouge presque pas**, de 2,50 à
2,32 Md€ par mois, soit −7 % en cinq ans.

Le premier soutien de l'Ukraine s'est retiré, **et le flux militaire a tenu.**

**La substitution ne vaut que là, et il faut le dire tout de suite.** Côté financier et
humanitaire, le remplacement est engagé — les États-Unis sortent complètement après 2024 — mais il
échoue : le total tombe de 2,88 à 1,77 Md€ par mois, **−39 %**, et l'Europe elle-même, après avoir
culminé à 2,71 en 2025, redescend à 1,54 en 2026, sous son propre niveau de 2022. Les deux
panneaux mis bout à bout, l'aide totale passe de 5,38 à 4,09 Md€ par mois : **−24 %**. La source
titre elle-même, sur une livraison antérieure, *« Europe fails to offset US aid drop »*, et le
présent document ne prétend pas la contredire.

Ce que le graphique montre n'est donc pas une substitution réussie : c'est **une substitution
réussie sur les armes et manquée sur l'argent**. Pour le modèle, la nuance est meilleure que le
fait simple — elle donne deux issues à la même mécanique au lieu d'une, et c'est bien une
mécanique manquante qu'on cherche à nommer, pas une bonne nouvelle à enregistrer.

Le jeu ne connaît que trois avenirs pour ce flux : il tient, il s'intensifie, il s'arrête. La réalité
en a pris un quatrième — **le donateur a changé et le flux a tenu** — et c'est une substitution entre
soutiens, pas une variation du soutien. Le moteur ne porte qu'une `Politics.ExternalWill` unique, de
0 à 100, la volonté des soutiens vue comme un bloc ; et un seul `Foreign.DisbursementRate`, également
scalaire. **Un bloc unique ne peut pas se recomposer.** Il ne sait que monter ou descendre.

La lacune est instruite, chiffrée et sourcée au **[§6 de l'audit de réalisme](09-audit-realisme.md)**.
Elle n'est pas répétée ici.

---

## 7. Ce qu'il faudrait changer dans le moteur — spécification, non implémentée

> **Aucune ligne de C# n'accompagne cette section.** Elle décrit ce qu'il faudrait écrire, dans quel
> ordre, et ce que cela coûterait. Elle ne l'écrit pas.

### 7.1 Le principe : un bloc qui se décompose sans cesser d'exister

Le geste central tient en une phrase : **`ExternalWill` reste, mais devient un résultat.**

`ForeignSupport` porte aujourd'hui un `PledgedPerTurnBillions` unique, un `DisbursementRate` unique
et un `InKindShare` unique. Il porterait à la place une petite liste de **blocs de donateurs**, deux
ou trois au plus — *Europe*, *États-Unis*, *Autres* —, chacun avec son propre engagement, son propre
taux de versement, sa propre volonté politique et **sa propre part livrée en nature**. Le grant
effectif devient la somme des blocs.

`Politics.ExternalWill` deviendrait alors une moyenne pondérée par les engagements, calculée et non
posée. C'est ce qui rend le changement supportable : **les quatre consommateurs actuels de cette
valeur continuent de fonctionner sans être touchés** — l'alerte de `PressureAnalyser` au seuil de 45,
le « réservoir » de `CapitalReader`, la génération de capital politique ukrainien d'`AttritionPhase`,
et l'érosion par le prix du pétrole de `RevenuePhase`. L'agrégat reste à l'écran ; le détail apparaît
en dessous.

### 7.2 Le vocabulaire d'effets, et le seul verbe qui manque

Les trois effets de cartes qui touchent l'aide — `AidPledgeDelta`, `AidDisbursementRate`,
`ExternalWillDelta` — gagneraient un champ `donorCode` **facultatif** dans `cards.fr.json`. Absent,
l'effet s'applique à tous les blocs au prorata : **aucune des trente et quelques cartes existantes
n'a besoin d'être touchée**, ce qui est la condition pour que la phase 1 soit inerte.

Il manque ensuite **un seul verbe, et c'est celui que le deck ne sait pas prononcer**. Le jeu porte
déjà deux cartes qui nomment les donateurs séparément — `us_election_swing` et
`european_election_swing`. Elles sont toutes deux au bénéfice de l'envahisseur, toutes deux
purement soustractives, et toutes deux poussent le même curseur unique. Le vocabulaire actuel ne
permet d'écrire que « l'aide baisse ». Il faudrait un `AidSubstitution` : **déplacer un engagement
d'un bloc vers un autre**, avec trois paramètres.

- **La part transférée.** Combien du bloc sortant l'autre reprend.
- **Le délai, en tours.** C'est le paramètre décisif : une substitution instantanée est un
  non-événement, elle ne produit aucune courbe. La réalité a mis de deux à quatre trimestres à
  recomposer, et c'est ce délai qui creuse le trou par lequel un front peut céder. Sans lui, la
  carte ne mérite pas d'exister.
- **La composition.** Le bloc qui reprend ne livre pas le même panier. Les États-Unis livraient du
  matériel de haut de gamme — défense antiaérienne, frappe longue portée ; l'Europe reprend
  davantage en argent et moins en intercepteurs lourds. Chaque bloc portant son propre
  `InKindShare`, le moteur devient capable de dire ce qu'aucune de ses trois issues ne sait dire
  aujourd'hui : **le flux a tenu en euros et s'est dégradé en capacité.** C'est, très
  vraisemblablement, ce qu'a été l'année 2025.

### 7.3 Le phasage, et ce qu'il coûte

| Phase | Ce qu'elle fait | Fichiers touchés | Ce qu'elle doit prouver |
|---|---|---|---|
| **1 — La décomposition inerte** | Les blocs existent, la somme vaut exactement la valeur d'aujourd'hui, `ExternalWill` devient une moyenne pondérée qui reproduit la valeur actuelle | `Core/ForeignSupport.cs` (+ un type `DonorBloc`), `Core/GameState.cs`, `Engine/Scenarios/UkraineScenario.cs` | **Les trois déroulés sont rigoureusement identiques**, tour par tour. Aucun test ne bouge. C'est la seule preuve acceptable qu'un refactor de ce poste est sans effet |
| **2 — Le verbe manquant** | `AidSubstitution` avec part, délai et cible ; `donorCode` facultatif sur les trois effets existants ; deux cartes ; un quatrième déroulé | `Core/EventCard.cs`, `Engine/CardEffectApplier.cs`, `Engine/CardPrinter.cs`, `Engine/data/cards.fr.json`, `Engine/Scenarios/` | Le quatrième déroulé existe et se distingue des trois autres : le donateur change, le total tient, et le front encaisse le délai |
| **3 — La composition** | `InKindShare` par bloc, donc un panier livré qui change quand le donateur change | `Engine/Phases/RevenuePhase.cs` | Une substitution à volume constant dégrade quand même la couverture matérielle — le résultat que le moteur ne sait pas produire aujourd'hui |

Côté écran, le seul geste nécessaire est de faire de la jauge « volonté des soutiens » une barre
empilée de deux ou trois blocs, l'agrégat restant affiché comme il l'est. La bande du capital de
guerre garde son nombre unique ; la composition se lit en dessous, en seconde lecture, comme le
prévoit déjà `CapitalReader`.

### 7.4 La contrainte à ne pas oublier

Le [§12 du calendrier proposé](06-calendrier-propose.md) porte un avertissement qui s'applique
intégralement ici :

> **La marge de l'asphyxie est mince.** La puissance russe à la veille de la chute vaut
> quarante-neuf pour cent de son pic — 48,95 % à la mesure — contre les cinquante pour cent que le
> test exige au maximum. **Un point.**

Une modification du poste de soutien extérieur touche l'aide, donc le budget de guerre, donc la
production, donc la courbe de puissance russe. **Les trois phases ci-dessus, y compris la première
qui se veut inerte, doivent chacune être suivies d'une relecture de ce chiffre — et non du seul
« les tests passent ».** C'est précisément parce que la phase 1 se prétend sans effet qu'elle doit
le prouver sur cette valeur-là : un refactor qui déplace la marge de 48,95 % à 50,1 % passerait tous
les tests unitaires et casserait la démonstration centrale du jeu.

---

## 8. Sources

**Principale.**

- **Kiel Institute for the World Economy — Ukraine Support Tracker**, Trebesch, C., Antezza, A.,
  Bushnell, K., Dyussimbinov, Y., Frank, A., Frank, P., Franz, L., Kharitonov, I., Kumar, B.,
  Nishikawa, T., Rebinskaya, E., Schramm, S., Weiser, L., Schade, C. — Release 30, mise à jour du
  13 août 2026, données arrêtées à juin 2026. <https://www.kielinstitut.de/ukrainetracker>
  Graphique lu : « Aid allocations by donor group », moyennes mensuelles annuelles corrigées de
  l'inflation, en milliards d'euros.

**Recoupements et méthode.**

- [Ukraine Support Tracker : l'aide militaire tient pendant que l'attention se déplace vers les
  drones — Institut de Kiel, 4 juin 2026](https://www.kielinstitut.de/publications/news/ukraine-support-tracker-military-aid-holds-steady-as-focus-shifts-to-drones/)
  — moyennes mensuelles européennes 2025 et janvier-avril 2026, utilisées pour recouper la lecture.
- [Ukraine Support Tracker — note de recherche méthodologique, Institut de Kiel](https://www.kielinstitut.de/fileadmin/Dateiverwaltung/Subject_Dossiers_Topics/Ukraine/Ukraine_Support_Tracker/Ukraine_Support_Tracker_-_Research_Note.pdf)
  et [documentation du jeu de données](https://www.kielinstitut.de/fileadmin/Dateiverwaltung/Subject_Dossiers_Topics/Ukraine/Ukraine_Support_Tracker/Dataset_Documentation.pdf)
  — définitions d'*engagement*, d'*allocation* et de *décaissement*.
- [L'Europe ne compense pas le retrait américain — Institut de Kiel](https://www.kielinstitut.de/publications/news/ukraine-support-tracker-europe-fails-to-offset-us-aid-drop/)
  — déjà cité par l'[audit de réalisme](09-audit-realisme.md) §9.

**Pour le trimestre d'avant-guerre, hors du domaine du tracker.**

- [U.S. Security Assistance to Ukraine — Congressional Research Service](https://www.congress.gov/crs-product/IF12040)
  et [Ukraine to Set Record for U.S. Security Assistance — Stimson Center](https://www.stimson.org/2022/ukraine-to-set-record-for-u-s-security-assistance/)
  — environ 2,7 à 2,8 Md$ d'aide de sécurité américaine cumulés sur 2014-2021.

**Taux de change.**

- Cours EUR/USD de référence, 1,1535 au 7 août 2026 — [Banque centrale européenne, taux de change de référence](https://www.ecb.europa.eu/stats/policy_and_exchange_rates/euro_reference_exchange_rates/html/eurofxref-graph-usd.en.html). Arrondi à 1,15 pour tout le document.
