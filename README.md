# Theory of Victory

### You have the cards in hand

**→ [Jouer la simulation](https://benoitgaly.github.io/theory-of-victory/)**

Simulation de la guerre en Ukraine comme **compétition de génération de force**, et non comme
succession de batailles. Outil pédagogique inspiré des travaux de **Phillips P. O'Brien**.

> **Le front est un thermomètre, pas un moteur.** Un secteur cède parce que le flux qui l'alimente
> s'est tari, jamais parce qu'un assaut a réussi.

## Ce que la V1 démontre

Le même départ — février 2022, dix-neuf tours de trois mois jusqu'à l'été 2026, mêmes forces,
même carte — est rejoué **trois fois**. Ce qui change, ce sont les cartes que l'Occident joue.

| Déroulé | Ce que l'Occident fait | Issue |
|---|---|---|
| **L'Occident joue ses cartes** | Embargo sur les composants, campagne sur le raffinage entretenue, avoirs gelés transférés, baril effondré | **L'Ukraine l'emporte** — chute du régime russe à l'été 2026 |
| **Le soutien tient, sans plus** | Le flux ne rompt jamais, sans jamais s'intensifier | **Front figé** — personne ne gagne, quatre ans plus tard |
| **Le soutien s'arrête** | Le flux gratuit cesse au tour 6 | **L'Ukraine cède** — effondrement au printemps 2024 |

Le déroulé de victoire ne prend pas un hexagone de plus que les autres : il **coupe la caisse**.
Le raffinage est frappé tous les trois trimestres sans relâche, le baril s'effondre, le fonds
souverain se vide pour combler ce que les recettes ne financent plus — et le jour où il ne comble
plus rien, l'appareil lâche. C'est la théorie de la victoire que désigne O'Brien : on ne gagne pas
en prenant du terrain, on gagne en asséchant ce qui permet d'en tenir.

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

Le **tonneau de Liebig** porte la règle centrale : chaque douve est un flux consommé — obus,
carburant, nourriture — sa hauteur est son taux de couverture, et l'eau ne monte jamais au-dessus
de la plus courte. Les hommes, eux, ne sont pas une douve : ils sont la **taille du tonneau**,
puisque c'est l'effectif tenu en ligne qui dimensionne le front et fabrique donc le besoin
matériel. La **carte** est une
vraie carte d'Ukraine pavée d'hexagones, où l'on distingue le territoire occupé avant février 2022
de celui pris depuis. Les cartes événement sont imprimées au format Magic — cadre, coût en capital
politique, illustration, ligne de type, boîte de règles et texte d'ambiance — parce que la V2 les
mettra en main des joueurs.

La conception détaillée est répartie en cinq documents : le [modèle de jeu](docs/design/01-modele-de-jeu.md),
la [direction artistique](docs/design/02-direction-artistique.md), le [gameplay](docs/design/03-gameplay.md),
la [calibration des effectifs](docs/design/04-calibration-effectifs.md) et les
[cinq composantes d'armée](docs/design/05-composantes-armee.md).

## Lancer

```bash
# Le site : rejoue les trois déroulés tour par tour
dotnet run --project src/TheoryOfVictory.Web        # http://localhost:5106

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
base de données : la V1.0 est déterministe, les trois déroulés sont joués au démarrage et servis
depuis la mémoire.

## Publier

Rien n'étant calculé à la demande, une capture de la page rendue **est** le jeu complet. Le site
publié est donc un instantané statique, servi par GitHub Pages depuis la branche `gh-pages` :

```powershell
scripts\Publish-StaticSite.ps1     # écrit .artifacts\site
```

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

Les valeurs sont, sauf mention contraire, des **ordres de grandeur de travail**, posées pour que le
moteur produise des courbes discutables. Elles ne sont pas sourcées une par une et ne doivent pas
être citées comme des faits. Font exception les **effectifs**, calés sur sources ouvertes avec
fourchettes d'incertitude assumées dans [`04-calibration-effectifs.md`](docs/design/04-calibration-effectifs.md).
La conception complète est dans [`docs/design/01-modele-de-jeu.md`](docs/design/01-modele-de-jeu.md).

## Licence

MIT — voir [`LICENSE`](LICENSE).
