# Theory of Victory

### You have the cards in hand

Simulation de la guerre en Ukraine comme **compétition de génération de force**, et non comme
succession de batailles. Outil pédagogique inspiré des travaux de **Phillips P. O'Brien**.

> **Le front est un thermomètre, pas un moteur.** Un secteur cède parce que le flux qui l'alimente
> s'est tari, jamais parce qu'un assaut a réussi.

## Ce que la V1 démontre

Le même départ — février 2022, seize tours de trois mois, mêmes forces, même carte — est rejoué
**trois fois**. Ce qui change, ce sont les cartes que l'Occident décide de jouer.

| Déroulé | Ce que l'Occident fait | Issue |
|---|---|---|
| **L'Occident joue ses cartes** | Embargo sur les composants, campagne trimestrielle sur le raffinage, baril effondré, aide rendue prévisible | **L'Ukraine l'emporte** — chute du régime russe au tour 12 |
| **Le soutien tient, sans plus** | Le flux ne rompt jamais, sans jamais s'intensifier | **Front figé** — 2 300 km² en quatre ans, personne ne gagne |
| **Le soutien s'arrête** | Le flux gratuit cesse au tour 6 | **L'Ukraine cède** — effondrement au tour 9, 30 000 km² |

Le déroulé de victoire ne prend pas un hexagone de plus que les autres : il **coupe la caisse**.
Le baril tombe à 48 $, les recettes ne financent plus l'effort de guerre, la puissance russe passe
de 450 à 283, et l'appareil lâche. C'est la théorie de la victoire que désigne O'Brien — on ne gagne
pas en prenant du terrain, on gagne en asséchant ce qui permet d'en tenir.

Dans le déroulé d'abandon, le tour de la coupure ne produit *rien* : les dépôts couvrent encore le
besoin. C'est deux tours plus tard que tout cède. **L'effondrement est un seuil, pas une pente.**

Le modèle retrouve par ailleurs seul, sans y être forcé, la crise des munitions de fin 2023 : le
blocage budgétaire du déroulé « tient, sans plus » fait chuter la puissance ukrainienne au tour
suivant, et le déblocage la restaure. C'est le test de validation historique du modèle.

## L'interface

Trois écrans par tour, comme les trois temps d'un tour de jeu de plateau :

1. **Génération de force — Russie** — la chaîne complète, du PIB au front, et le tonneau de Liebig
2. **Génération de force — Ukraine** — la même chaîne, avec une économie de flux opposée
3. **Résolution — champ de bataille** — la carte, les rapports de force, les cartes du tour

Le **tonneau de Liebig** porte la règle centrale : chaque douve est une ressource, sa hauteur est
son taux de couverture, et l'eau ne monte jamais au-dessus de la plus courte. Les cartes événement
sont imprimées au format Magic — cadre, coût en capital politique, illustration, ligne de type,
boîte de règles et texte d'ambiance — parce que la V2 les mettra en main des joueurs.

## Lancer

```bash
# Le site : rejoue les deux parties tour par tour
dotnet run --project src/TheoryOfVictory.Web        # http://localhost:5241

# La trace console, utile pour calibrer
dotnet run --project src/TheoryOfVictory.Simulator

# Les tests, qui verrouillent les règles du modèle
dotnet test tests/TheoryOfVictory.Engine.UnitTests
```

## Structure

| Projet | Rôle |
|---|---|
| `TheoryOfVictory.Core` | Le modèle : ressources, économie, énergie, front, cartes. Aucune dépendance. |
| `TheoryOfVictory.Engine` | Les dix phases du tour, le scénario, le deck (`data/cards.fr.json`) |
| `TheoryOfVictory.Simulator` | Runner console |
| `TheoryOfVictory.Web` | ASP.NET Core MVC — navigation tour par tour, carte hexagonale, cubes |

Même pile que Green Acres (.NET 10, ASP.NET Core MVC, mêmes conventions C#), sans back-office ni
base de données : la V1.0 est déterministe, les deux parties sont jouées au démarrage et servies
depuis la mémoire.

## Les règles qui portent le modèle

1. **La puissance est la ressource la plus rare, jamais la somme.** Cinq cent mille hommes sans obus
   ne percent rien. Le goulot est affiché en permanence pour chaque camp.
2. **Attaquer coûte trois à cinq fois plus que tenir**, et avancer dégrade sa propre logistique.
3. **L'électricité est l'intrant des intrants** : elle ne va jamais au front mais conditionne les
   usines, le raffinage, le rail et le moral. Le délestage est un seuil, la saison décide.
4. **Le pétrole a quatre canaux**, tous dans le même sens : il paie la Russie, coûte à l'Ukraine,
   lasse l'Occident et achète la paix sociale à Moscou.
5. **Donner contre vendre** : l'Ukraine reçoit un flux gratuit qui peut s'arrêter du jour au
   lendemain ; la Russie achète un flux payant qui ne s'arrête jamais.
6. **Les sanctions frappent trois canaux** — prix, friction, composants — et le PIB suit. Le canal
   lent, les composants, est le seul décisif. Toutes s'érodent si on ne les resserre pas.
7. **Innover ne multiplie pas la puissance, ça déplace le goulot** : obtenir le même effet avec la
   ressource qu'on a. Et toute avance se périme.
8. **Mobiliser au mauvais moment est suicidaire** : si le goulot est l'obus, trois cent mille hommes
   de plus n'apportent rien au front et amputent le PIB qui les aurait armés.
9. **La saturation précède la pénétration** : les drones bon marché vident les magasins que les
   missiles ne rencontreront plus. Le rapport de coût décide, pas le taux d'interception.
10. **Un régime tombe par ses élites**, pas par sa rue — et la répression échange une échéance
    proche contre une rupture plus violente.

## Suite

- **V1.1** — réactiver les tirages probabilistes, montrer la dispersion des trajectoires
- **V2** — deux joueurs, pioche et cartes façon Magic : le capital politique comme seconde monnaie,
  la pioche indexée sur la santé des flux, et le deck comme théorie de la victoire explicite
- **V3** — IA de doctrine

## Statut des chiffres

Les valeurs sont des **ordres de grandeur de travail**, posées pour que le moteur produise des
courbes discutables. Elles ne sont pas sourcées une par une et ne doivent pas être citées comme
des faits. La conception complète est dans [`docs/design/01-modele-de-jeu.md`](docs/design/01-modele-de-jeu.md).
