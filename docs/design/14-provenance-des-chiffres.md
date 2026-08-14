# La provenance des chiffres — spécification, non implémentée

> **Aucune ligne de C# n'accompagne ce document.** Il décrit une table, ce qu'elle porte, comment
> chaque chiffre affiché s'y rattache, et il nomme trois défauts que son absence a laissés passer.
> Il ne l'écrit pas.

---

## 1. Le problème, tel qu'il se constate

Le dépôt tient treize documents de conception, dont trois portent des séries sourcées : les
effectifs ([04](04-calibration-effectifs.md)), la position du front ([11](11-front-historique.md))
et le soutien extérieur ([13](13-soutien-exterieur-source.md)). Partout ailleurs, un chiffre du
moteur est une constante posée dans `UkraineScenario.cs`, sans trace de qui l'a choisie, sur quoi,
ni quand.

La conséquence n'est pas théorique. **Trois défauts ont été trouvés le 14 août 2026 en regardant
l'écran, et aucun des trois n'aurait survécu à une table de provenance :**

| Ce que le moteur porte | Ce que la réalité dit | Où c'est écrit |
|---|---|---|
| ~~Réserves russes **310 Md$ dès l'automne 2021**~~ **corrigé** — voir §5.1 | Les avoirs n'étaient **pas gelés** avant février 2022 : les réserves d'or et de change étaient de l'ordre du double, et c'est l'invasion qui en a immobilisé environ la moitié | `UkraineScenario.cs`, `russia.Economy.ReservesBillions` |
| **Soutien étranger acheté par la Russie : 6,54 Md$/an à l'automne 2021** | Les livraisons iraniennes commencent à l'été 2022, les nord-coréennes fin 2023. Avant la guerre, ce poste vaut à peu près zéro | même fichier |
| **Soutien extérieur ukrainien : 16,49 Md$/an à l'automne 2021** | De l'ordre de 0,35 Md$/an sur 2014-2021 | déjà constaté, [13 §4](13-soutien-exterieur-source.md) |

Les trois disent la même chose : **le premier tour est un trimestre de paix qui tourne avec des
valeurs de guerre.** Le troisième était documenté ; les deux autres ne l'étaient pas, et ils ne
l'étaient pas parce que rien n'oblige un chiffre à déclarer d'où il vient.

---

## 2. Ce que la table doit porter

Deux tables, et le 1:n entre elles.

**`FigureSource`** — une source, une fois, réutilisable par autant de chiffres qu'on veut.
Ce qu'elle porte : un code, le nom de l'organisme, le titre exact du document ou du graphique,
l'URL, le millésime consulté, la date de consultation, et **la nature de ce qu'elle mesure**
(mesure directe, lecture graphique, estimation d'un tiers, ordre de grandeur de travail).

**`HistoricalFigure`** — un chiffre, sa date, son unité, et **la liste de ses sources**. C'est le
1:n : une valeur peut être encadrée par deux sources qui ne disent pas la même chose, et
l'encadrement est l'information — [13 §5.1](13-soutien-exterieur-source.md) tient précisément
parce qu'il compare le modèle à une fourchette, pas à un point.

S'y ajoutent, et ce sont eux qui portent l'honnêteté du dispositif :

- **le calcul** — la formule qui mène de la source à la valeur du moteur, écrite en clair.
  « Md€/mois × 12 × 1,15 » est déjà dans [13 §3](13-soutien-exterieur-source.md) ; il faut qu'elle
  vive avec le chiffre, pas dans un document à côté ;
- **l'incertitude**, en valeur et non en adjectif ;
- **le degré de confiance** : `Sourcé` · `Dérivé` (calculé depuis une ou plusieurs sources)
  · `OrdreDeGrandeur` (posé pour que le moteur produise une courbe discutable) ;
- **la période de validité.** C'est elle qui aurait attrapé les trois défauts du §1 : un chiffre
  qui vaut à partir de février 2022 ne doit pas pouvoir alimenter le tour de l'automne 2021.

---

## 3. Comment un chiffre affiché s'y rattache

La règle : **une constante du scénario ne s'écrit plus en littéral, elle se résout par son code de
figure.** `russia.Economy.ReservesBillions = 310d` devient une lecture du registre, datée par le
tour en cours — ce qui rend le §1 impossible à réintroduire, puisque demander une valeur de 2021 à
une figure qui commence en 2022 devient une erreur et non un silence.

Côté écran, un seul geste : **l'infobulle d'un chiffre finit par sa provenance.** Une ligne, à la
suite de ce qu'elle dit déjà — l'organisme, le millésime, le degré de confiance. Rien de visible
tant qu'on ne le demande pas, conformément à la règle d'interface du dépôt : le plateau ne
s'explique pas, il se lit, et ce qui est utile sans être nécessaire descend dans l'infobulle.

Un chiffre en `OrdreDeGrandeur` le dit franchement. Le site affiche une guerre en cours et sera lu
comme tel — **la seule chose pire qu'un ordre de grandeur, c'est un ordre de grandeur qu'on prend
pour une mesure.**

---

## 4. Le phasage

| Phase | Ce qu'elle fait | Ce qu'elle doit prouver |
|---|---|---|
| **1 — Le registre** | Les deux types, le registre, et **les seules séries déjà sourcées** y entrent : effectifs, front, soutien extérieur. Rien d'autre ne bouge | Les trois déroulés sont rigoureusement identiques, tour par tour |
| **2 — La résolution datée** | Le scénario lit ses constantes dans le registre ; une figure hors de sa période de validité lève | Les trois défauts du §1 deviennent des erreurs de démarrage, pas des chiffres à l'écran |
| **3 — La provenance à l'écran** | La ligne de provenance en fin d'infobulle | Chaque chiffre du bandeau nomme sa source ou se déclare ordre de grandeur |
| **4 — La reprise du reste** | Les constantes restantes migrent, une famille à la fois, chacune avec sa source ou son aveu | Plus une seule constante littérale dans `UkraineScenario.cs` |

---

## 5. La contrainte, et ce qu'elle a coûté

Les trois chiffres du §1 touchent les réserves et l'aide, donc le budget de guerre, donc la
production, donc la courbe de puissance russe. Le [§12 du calendrier](06-calendrier-propose.md)
s'applique intégralement :

> **La marge de l'asphyxie est mince.** La puissance russe à la veille de la chute vaut 48,95 % de
> son pic, pour un seuil que le test fixe à 50. **Un point.**

### 5.1 Les réserves — corrigé, et le gel se joue désormais

Le moteur ouvre maintenant sur **630 Md$**, la valeur d'avant-guerre, et le gel est **joué comme
un effet de la première carte de sanctions**, au tour de l'invasion : `sanctions_package_1` porte
un `ReservesDelta` de −320. La description de cette carte annonçait le gel depuis toujours — « l'Occident gèle
les avoirs » — sans qu'aucun effet ne le produise ; elle le produit.

Trois choses en découlent, et la troisième est la leçon :

1. **Le trimestre de paix est enfin un trimestre de paix.** L'automne 2021 porte 630 Md$, et non
   le bilan que l'invasion n'a pas encore produit.
2. **Le gel devient un coup, pas un décor.** Il coupe à la date où il a coupé.
3. **Le montant du gel n'est pas libre.** Réglé d'abord à −300, il laissait 330 Md$ après gel au
   lieu de 310, et **le test de l'asphyxie est tombé : 50,15 % contre un seuil à 50.** Vingt
   milliards sur une guerre de six ans suffisent à faire tenir le régime russe un tour de trop.
   À −320, le fonds retombe exactement sur les 310 Md$ sur lesquels toute la calibration a été
   bâtie, et **la marge est revenue à 48,95 %, à la deuxième décimale près.**

C'est la démonstration la plus nette de ce que ce document défend : un chiffre dont personne ne
sait sur quoi il est calé ne peut pas être corrigé sans casser autre chose, et il faut deux essais
mesurés pour retrouver ce qu'une ligne de provenance aurait dit d'emblée.

### 5.2 Les deux postes de soutien extérieur — à faire

La valeur d'avant-guerre doit tomber à son niveau réel, et remonter avec la guerre. C'est une
rampe, et [13 §4](13-soutien-exterieur-source.md) nomme déjà l'absence de rampe comme la cause du
défaut. Le geste est le même qu'au §5.1 : ouvrir juste, et jouer la montée.

**Il se relit sur la marge de l'asphyxie**, et « les tests passent » ne suffit pas : il faut relire
le nombre. Il se recalcule dans la console du navigateur sur `window.tovGames[0]` — pic de
`invader.combatPower`, comparé au tour `outcome.turn - 2`.

---

## 6. Ce qu'il reste à sourcer avant d'écrire quoi que ce soit

Les valeurs d'avant-guerre citées au §1 sont ici comme **ordres de grandeur à vérifier**, pas comme
des mesures. Avant qu'une seule entre dans le moteur, il faut la relever à sa source :

- **Réserves d'or et de change de la Fédération de Russie**, série hebdomadaire publiée par la
  Banque centrale de Russie — valeur au dernier point d'avant l'invasion, et part immobilisée
  telle que les États du G7 et l'Union européenne l'ont déclarée.
- **Livraisons militaires iraniennes et nord-coréennes à la Russie** — date de début et volume
  annualisé, à prendre chez un organisme qui publie sa méthode.

Tant que ces relevés ne sont pas faits, les chiffres du §1 restent ce que ce document dit qu'ils
sont : le constat qu'un écart existe, jamais la mesure de cet écart.
