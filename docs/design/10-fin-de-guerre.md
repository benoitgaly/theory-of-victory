# La fin de la guerre

> Comment le scénario va jusqu'au bout, et pourquoi ce bout-là. Le modèle de référence reste
> [`01-modele-de-jeu.md`](01-modele-de-jeu.md) ; l'audit de réalisme est dans
> [`09-audit-realisme.md`](09-audit-realisme.md).

État : mécanisme implémenté, calibration provisoire. Le calendrier appartient au scénario.

---

## 1. Ce qui manquait

Jusqu'ici la partie s'arrêtait au trimestre où un camp cassait. Le régime russe tombait, et
l'écran s'éteignait — sur l'événement le plus intéressant du jeu, une demi-seconde avant qu'il ne
produise quoi que ce soit.

C'est une perte sèche, parce que **l'après-chute est la démonstration finale de la thèse**. Un
régime qui tombe ne rend pas le terrain le jour même. Il cesse de payer son armée ; l'armée cesse
d'être une armée ; et le front se dénoue tout seul, sans qu'un assaut l'ait emporté. Le jeu
affirme depuis le premier document que *le front est un thermomètre, pas un moteur*. C'est le seul
moment où il peut le montrer au lieu de l'écrire.

---

## 2. Le mécanisme, et ce qu'il fait faire au moteur

La phase `AftermathPhase` ne déplace jamais la ligne de front. Elle dissout l'armée du camp brisé,
et le front se dénoue tout seul dans les phases qui existaient déjà.

```
part restante   = (1 − 0,55) ^ (trimestres écoulés depuis la rupture)
plafond         = effectif théorique × part restante
hommes en ligne = min(hommes en ligne, plafond)
```

Les hommes retirés **retournent au réservoir mobilisable** et ne sont **jamais comptés en pertes** :
ils sont rentrés chez eux. Les compter en morts serait un mensonge pur sur une page qui affiche
les pertes cumulées dans la colonne d'à côté.

**Ce qui se dissout n'est pas l'effectif, c'est l'armée comme organisation.** Trois choses cèdent
ensemble, au même rythme, et il a fallu les trois pour que le délitement se voie se produire :

| Ce qui cède | Pourquoi | Ce que ça corrige |
|---|---|---|
| Les hommes tenus en ligne | Personne ne les paie plus | La puissance de combat suit la taille du tonneau |
| L'intégrité logistique | Une armée qu'on ne paie plus n'a plus de chaîne d'approvisionnement | **Sans elle, la dissolution produisait l'inverse de son objet** : les dépôts restaient pleins pendant que les bouches disparaissaient, la couverture par homme passait de 0,12 à 0,84 en un trimestre, et la puissance d'une armée réduite de moitié **triplait** |
| La formation | Un État effondré ne forme plus et n'achemine plus de recrues | Sans elle, le recrutement regarnissait la ligne entre deux trimestres et la fonte n'était pas monotone |
| Les fortifications | Une tranchée tenue par un dixième des hommes qui l'ont creusée vaut un dixième | La résistance décroît régulièrement au lieu de tomber d'un coup |

Aucune de ces quatre lignes ne touche au terrain. C'est toujours `FrontPhase`, inchangée, qui
décide où passe la ligne — et qui nomme elle-même ce qui se passe : *avance libre*, jamais
*percée*.

### 2.1 La chaîne, maillon par maillon

Chaque étape du déroulé est produite par un mécanisme du moteur qui existait avant cette phase.
Aucune n'est écrite à la main.

Une correction est allée dans `ControlPhase`, hors de la phase elle-même, et elle mérite d'être
signalée : le ratio de génération d'un camp effondré affichait **1,00** pendant qu'il se
dissolvait. Le calcul prend le maximum entre « hommes remplacés sur hommes perdus » et le taux
d'encadrement — une courtoisie faite à une armée dont les trimestres sont calmes parce qu'elle est
au complet. Pour une armée effondrée, la courtoisie s'inverse : ses trimestres sont calmes parce
qu'il n'y a plus personne pour se battre. Le maximum est donc désactivé une fois le camp brisé, et
l'indicateur descend enfin — de l'ordre de 0,4 puis 0,2 au lieu de 1,00.

| Étape | Ce qui la produit | Où |
|---|---|---|
| Le baril s'effondre, les recettes ne financent plus l'effort de guerre | Le fonds souverain comble le déficit et s'épuise en le faisant | `RevenuePhase` |
| Le recrutement se tarit | Une armée recrute pour combler sa cible, et la trésorerie ne suit plus les primes | `AllocationPhase` |
| La cohésion des élites cède, le régime tombe | `FundingGap` ronge `EliteCohesion` jusqu'au seuil de `RegimeStress` | `AttritionPhase`, `ControlPhase` |
| **L'armée se dissout** | **Les quatre lignes de cette phase** | `AftermathPhase` |
| La puissance de combat s'écroule | Le tonneau rétrécit, la couverture ne se refait pas, la cohésion baisse avec le taux d'encadrement | `FrontPhase`, `Manpower.CohesionFactor` |
| Les secteurs reviennent les uns après les autres | Le ratio franchit 1,1 secteur par secteur, dans l'ordre que le terrain autorise, et le multiplicateur d'effondrement s'applique au camp brisé | `FrontPhase` |
| Le terrain revient sans assaut | La résolution de secteur le nomme elle-même : *avance libre*, jamais *percée* | `FrontPhase.DescribeOutcome` |
| La partie s'arrête | Seuil de dénouement, ci-dessous | `AftermathPhase`, `GameRunner` |

C'est le point important pour qui voudrait contester le scénario : **il n'y a pas de script de
fin.** Il y a un plafond sur les effectifs, et dix-neuf mécanismes déjà en place qui en tirent les
conséquences.

### 2.2 Ce qui n'a pas d'état caché

La phase ne retient rien entre deux tours. Elle relit ce que le moteur publie déjà : l'issue qui a
enregistré la rupture, le tour où elle s'est produite, et l'effectif théorique visé. Le nombre de
trimestres écoulés est une soustraction, la part restante une puissance. Une partie est donc
**rejouable à partir des seuls instantanés**, ce qui est aussi ce qui permettra à la page
d'expliquer la fin au lieu de simplement l'afficher.

---

## 3. Combien de temps dure l'après-chute, et ce qui l'arrête

**Réponse courte, pour le calendrier : trois trimestres après la rupture, dont deux sont joués.**
Le troisième prononce l'armistice et n'est pas joué. Un scénario qui veut que la guerre se termine
à un trimestre donné doit donc faire tomber le régime **deux trimestres plus tôt** — chute au
printemps 2027, dénouement à l'été et à l'automne, armistice prononcé sur l'hiver 2028.

Ce nombre n'est pas écrit en dur : il se déduit de deux paramètres, et il se lit sur le scénario
avant qu'un seul tour ne soit joué, par `scenario.Aftermath.QuartersToArmistice`.

### 3.1 Les paramètres appartiennent au scénario

`AftermathRules`, porté par `Scenario.Aftermath`, expose trois réglages et une lecture dérivée.
La phase ne connaît aucune constante et aucun numéro de tour : elle lit ces valeurs et compte des
écarts.

| Réglage | Défaut | Ce qu'il fait |
|---|---|---|
| `DissolutionPerTurn` | 0,55 | Part de l'effectif théorique que le camp brisé perd chaque trimestre |
| `ArmisticeManningRatio` | 0,12 | Sous cette part, il ne tient plus de front : la guerre s'arrête |
| `MaxTurns` | 6 | Garde-fou dur, pour que la frise ne se remplisse jamais de trimestres vides |
| `QuartersToArmistice` | **3** | *Lecture dérivée* — trimestres entre la rupture et l'armistice |

La trajectoire que ces valeurs produisent, en part de l'effectif théorique encore en ligne :

```
rupture      T+1      T+2      T+3
 100 %      45 %     20 %      9 %  ← sous le seuil, armistice
            joué     joué    non joué
```

Un camp déjà sous son effectif théorique au moment où il casse — c'est le cas ordinaire, un
groupement usé tourne autour de 90 % — arrive au seuil **plus tôt, jamais plus tard**. Trois
trimestres est donc un plafond, et `QuartersToArmistice` le majorant que le calendrier doit
prévoir.

**Le seuil n'a pas été choisi au hasard : il borne aussi le dernier trimestre.** Descendu à 0,06,
il laissait jouer un trimestre de plus, celui où le rapport de forces dépasse 3 et où la table de
mouvement sature — la ligne partait alors de vingt-huit à cent dix kilomètres au-delà de son point
de départ en un seul trimestre. À 0,12, la guerre s'arrête pendant que le dénouement est encore
lisible.

### 3.2 Pourquoi ces valeurs-là

**La vitesse de dissolution** est encadrée par deux repères historiques très écartés : l'armée
allemande de l'automne 1918 s'est défaite en quelques semaines une fois l'arrière cédé, l'armée
russe de 1917 a mis près d'un an par désertion progressive. Trois à quatre trimestres se situent
entre les deux, et c'est tout ce qu'on peut en dire honnêtement. C'est un choix de scénarisation,
pas une découverte.

**L'arrêt est un seuil, pas un compte de tours**, pour deux raisons : c'est la signature du modèle
énoncée au §1 de `01` — *partout où c'est possible, un effet est un seuil, jamais une pente* — et
un compte de tours casserait le jour où le calendrier change de longueur, ce qui s'est produit
deux fois pendant l'écriture de cette phase.

### 3.3 Le trimestre qui prononce l'armistice n'est pas joué

Quand le seuil est franchi, la partie s'arrête **avant** de résoudre ce trimestre-là. Il n'y a plus
de front à résoudre, et le résoudre quand même enverrait la ligne courir sur un rapport de forces
qui n'a plus de sens — le moteur produisait dans ce cas une avance de plus de cent kilomètres en un
trimestre. La guerre s'arrête donc sur le dernier trimestre qui avait encore quelque chose à
décider, et la phrase d'armistice est rattachée à celui-là.

---

## 4. La symétrie

Il n'y a qu'un mécanisme, et il ne connaît pas les camps. Il lit `WinnerSideCode` sur l'issue —
que toute rupture renseigne — et en déduit le camp brisé. Les trois codes d'effondrement
(`military_collapse`, `regime_collapse`, `negotiated_capitulation`) l'activent ; le front figé et
l'épuisement mutuel ne l'activent pas, parce qu'il n'y a rien à dénouer.

| Déroulé | Camp brisé | Ce que produit le dénouement |
|---|---|---|
| L'Occident joue ses cartes | Russie | Le groupement russe perd plus de quatre cinquièmes de ses effectifs et la ligne revient vers son point de départ |
| Le soutien s'arrête | Ukraine | Le miroir exact : l'armée ukrainienne fond, et la ligne s'enfonce dans l'autre sens |
| Le soutien tient | aucun | Le calendrier va au bout, front figé, aucun dénouement |

Le déroulé où l'Ukraine cède se comporte donc comme le miroir de l'autre, sans une ligne de code
de plus : l'armée fond, et la ligne se déplace dans l'autre sens.

Les valeurs chiffrées ne sont pas reproduites ici volontairement. Le calendrier et la calibration
appartiennent à d'autres chantiers et bougent d'une heure à l'autre ; ce document décrit le
mécanisme, qui ne bouge pas. Le simulateur donne les chiffres du jour.

**Le coût de la fin est asymétrique, et c'est le moteur qui le dit.** Pendant le dénouement, le
camp qui se dissout perd environ deux fois plus d'hommes que celui qui avance — un test le
verrouille, parce que l'inverse signifierait que le vainqueur a dû assaillir. C'est une
conséquence, pas un réglage — une première version de cette phase
faisait passer le vainqueur en posture de poursuite, et le moteur lui a immédiatement facturé
99 000 hommes pour 61 km². La leçon a été retenue : **le dénouement ne vient pas d'une attaque, et
le modèle refuse qu'on le lui fasse dire.** Le levier a été retiré.

---

## 5. Ce que ceci est, et ce que ceci n'est pas

Tout ce qui suit le trimestre où nous vivons est une **projection** : le moteur tire les
conséquences d'hypothèses, il ne prédit rien. La frise le marque déjà d'un trait discontinu, et ce
document est là pour que personne n'ait à le deviner.

Ce qui est **défendable comme conséquence du modèle** : qu'un État qui ne finance plus son effort
de guerre cesse de payer son armée ; qu'une armée qui n'est plus payée se dissolve ; qu'un front
tenu par une armée dissoute cède sans être attaqué. Ces trois propositions sont la thèse du jeu,
et elles sont ici mécaniques.

Ce qui relève d'un **choix de scénarisation**, et qui se conteste légitimement : la vitesse de
dissolution, le seuil d'armistice, et surtout **le fait que ce soit la Russie qui casse**. Ce
dernier point n'est pas produit par cette phase : il vient du calendrier de cartes du déroulé
« L'Occident joue ses cartes », c'est-à-dire d'une hypothèse politique sur ce que l'Occident
déciderait de faire. Les deux autres déroulés montrent ce qui se passe sans cette hypothèse, et
c'est précisément pour cela qu'il y en a trois. **Une partie ne prouve rien ; une comparaison
prouve tout.**

Ce qui n'est **pas modélisé du tout** : la négociation, le sort des territoires occupés, les
garanties de sécurité, la reconstruction, le retour des prisonniers. Le jeu s'arrête à l'armistice
parce qu'il ne sait parler que de génération de force. Une paix n'est pas une compétition de
génération de force, et prétendre la simuler avec ces règles serait malhonnête.

---

## 6. Limites connues

**La ligne d'arrêt manque, et le dénouement la rend visible.** Le §13 de `01` prévoit qu'un
effondrement produit une *avance non bornée jusqu'à la ligne d'arrêt* — la borne n'existe pas dans
le code. Tant que la partie s'arrêtait à la rupture, cela ne se voyait pas. Maintenant que le
front se dénoue, la dernière résolution peut déplacer la ligne de plusieurs dizaines de kilomètres
d'un coup, et dans le déroulé où l'Ukraine cède, l'avance russe finit au-delà de ce qu'aucune
géographie ne borne. Ne pas jouer le trimestre de l'armistice (§3.3) borne le dépassement à une
seule résolution, pas à zéro.

*Correction recommandée* — elle appartient à `FrontPhase` / `FrontSector` : une borne de secteur au
delà de laquelle une avance libre s'arrête, faute de quoi la table de mouvement n'a pas de fin.
C'est le même diagnostic que celui posé au §4.1 de l'audit : le rythme d'avance est juste, c'est
la borne qui manque.

**Quelques tests du modèle lisent le dernier tour** et verrouillent ainsi l'ancienne troncature :
tant que la partie s'arrêtait à la rupture, « le dernier tour » et « le tour où la guerre s'est
décidée » étaient le même objet. Ils ne le sont plus. Ces tests vivent dans `ModelRulesTests.cs`,
qu'un autre chantier détient ; le correctif est le même pour tous et tient en une ligne :

> lire `game.Turns[game.Decision.Turn - 1]` — le trimestre de la rupture — au lieu de
> `game.Turns[^1]`. C'est ce que ces tests veulent dire, et ce qu'ils disaient déjà avant que
> l'après-chute n'existe.

Au moment de la remise, deux tests tombent pour cette raison :
`PlayingTheCards_BreaksTheInvader_WithoutTakingGround` (le ratio de génération du vainqueur au
dernier tour) et `TheInversion_TheMenSetTheMaterialRequirement_NeverTheReverse` (le déroulé y est
manipulé, finit par un effondrement, donc par une armée fondue au dernier tour). La liste exacte
bouge d'une heure à l'autre : `TheAsphyxiationIsVisible_InvaderPowerFalls_WithoutLosingGround`
relevait de la même catégorie il y a une heure et est aujourd'hui rouge pour une autre raison.

Six autres tests étaient déjà rouges avant ce chantier, du fait des travaux en cours sur le
scénario et sur la calibration ; ils ne relèvent pas de cette phase. Les onze tests de
`WarTerminationTests.cs` verrouillent le mécanisme lui-même et passent.

**Rien ici ne dépend d'un numéro de tour.** Le calendrier a changé deux fois pendant l'écriture de
cette phase, et il changera encore — le prologue de l'automne 2021 décalera tout d'un cran. Ni la
phase, ni ses tests, ni ce document ne nomment un tour absolu : la phase compte des écarts, les
tests portent sur des propriétés, et aucun d'eux ne parie sur le camp qui casse ni sur le déroulé
où il casse. C'est verrouillé par `NoRun_EverOutlivesItsHardBound`,
`ARunNobodyBreaks_GoesTheWholeCalendar_AndNotAQuarterMore` et
`WhicheverSideBreaks_TheOtherOneWins_AndTheBrokenOneIsTheOneThatMelted`.

---

## 7. Ce que la page peut annoncer

L'issue finale porte un code (`armistice`), un titre qui nomme le camp qui se retire, et une
explication qui dit pourquoi. À côté d'elle, `PlayedGame.Decision` conserve la **rupture** — la
chute du régime, l'effondrement militaire — avec son propre trimestre.

Deux événements, donc, et il faut les deux : *la guerre s'est décidée au trimestre où le flux s'est
tari ; elle s'est arrêtée trois trimestres plus tard, quand il n'y a plus eu personne pour tenir la
ligne.* L'issue publiée est datée sur le premier, pas sur le second, parce que c'est le premier qui
porte la démonstration.

Le récit de chaque trimestre de dénouement porte enfin la phrase qui résume tout le jeu :

> Plus personne ne paie l'armée russe : 303 000 hommes ont quitté la ligne ce trimestre. Aucun
> assaut ne les en a délogés.
