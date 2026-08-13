# Theory of Victory

### You have the cards in hand

**→ [Jouer la simulation](https://benoitgaly.github.io/theory-of-victory/)**

Simulation de la guerre en Ukraine comme **compétition de génération de force**, et non comme
succession de batailles. Outil pédagogique inspiré des travaux de **Phillips P. O'Brien**.

> **Le front est un thermomètre, pas un moteur.** Un secteur cède parce que le flux qui l'alimente
> s'est tari, jamais parce qu'un assaut a réussi.

## Ce que la V1 démontre

Le même départ — l'automne 2021, l'armée russe qui se masse aux frontières sans qu'un coup soit
tiré — est rejoué **trois fois**, trimestre par trimestre jusqu'à la fin de la guerre. Ce qui
change, ce sont les cartes que l'Ukraine et ses soutiens jouent.

| Déroulé | Ce que l'Occident fait | Issue |
|---|---|---|
| **L'Occident joue ses cartes** | Embargo sur les composants, campagne sur le raffinage entretenue, avoirs gelés transférés, baril effondré | **L'Ukraine l'emporte** — le régime russe tombe au printemps 2027, armistice à l'automne |
| **Le soutien tient, sans plus** | Le flux ne rompt jamais, sans jamais s'intensifier | **Front figé** — personne ne gagne, six ans plus tard |
| **Le soutien s'arrête** | Le flux gratuit cesse au tour 7 | **L'Ukraine cède** — effondrement au printemps 2024 |

Tout ce qui suit l'été 2026 est une **projection du modèle**, marquée comme telle sur la frise.
Une projection est une conséquence des règles du jeu ; elle se discute, elle ne s'annonce pas.

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
matériel.

La **carte** est une vraie carte d'Ukraine pavée d'hexagones, lue comme un wargame : des pions à
crans qui distinguent les crans alimentés des crans à sec — les hommes présents que rien ne pourvoit
—, une butée là où deux armées se poussent sans bouger, la table de résolution imprimée dans la
légende, et un bandeau d'arrière pour chaque camp, hors du champ de bataille, où se lisent les
frappes profondes. Le terrain suit la **densité d'hommes en ligne de contact par kilomètre** : on
prend du terrain là où il n'y a personne, et on le perd de la même façon.

Sur les vingt trimestres qui vont de l'automne 2021 à l'été 2026, **la carte porte la position
réelle du front**, reconstituée et sourcée trimestre par trimestre : la colonne de Kyiv arrive puis
se retire, Marioupol tombe, la rive droite de Kherson revient à l'Ukraine, Kharkiv perce, le
saillant de Koursk apparaît au nord de la frontière puis disparaît. Au-delà, c'est le modèle qui
place la ligne — et il le dit : **le front passe alors en pointillé**. Un déroulé où une armée
rompt, ce que la guerre réelle n'a pas fait, bascule de la même façon et pour la même raison.
Le détail est dans [la carte historique](docs/design/12-carte-historique.md).

Chaque camp tient une **main de six cartes**, imprimées au format Magic — cadre, coût en capital
politique, illustration propre à chaque carte, ligne de type, boîte de règles et texte d'ambiance.
Celles qu'il a jouées portent leur bandeau ; les autres attendent, parce que la V2 les mettra en
main des joueurs.

**Un camp joue une carte par trimestre, jamais deux.** Le trimestre est l'unité de décision : une
mobilisation, un paquet d'aide, un tour de vis sur les composants. Ce qui se joue occupe donc la
place de tout ce qui ne se joue pas — et c'est pour cela que l'Occident, dans le déroulé de la
victoire, paie son étranglement de dix trimestres d'intercepteurs qu'il ne livre pas. Le détail des
arbitrages est dans le [calendrier](docs/design/06-calendrier-propose.md).

La conception détaillée est répartie en treize documents, du [modèle de jeu](docs/design/01-modele-de-jeu.md)
au [soutien extérieur](docs/design/13-soutien-exterieur-source.md), en passant par la
[calibration des effectifs](docs/design/04-calibration-effectifs.md), le
[front historique](docs/design/11-front-historique.md) — les vingt trimestres sourcés —, la
[carte historique](docs/design/12-carte-historique.md) et l'
[audit de réalisme](docs/design/09-audit-realisme.md), qui nomme les écarts entre le jeu et le réel
sans les excuser. Le dernier confronte le poste d'aide occidentale au tracker de l'Institut de Kiel
et y trouve la bascule que le modèle ne sait pas représenter : entre 2022 et 2026, la part
américaine de l'aide militaire passe de 58 % à presque rien, l'Europe plus que double, et le total
ne bouge pas.

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

**Le C# ne tourne pas en ligne.** Il joue les trois déroulés au démarrage et imprime le résultat
dans la page ; rien n'est calculé à la demande, aucun appel n'est fait au serveur après le
chargement. Une capture de la page rendue **est** donc le jeu complet, et l'hébergement statique
suffit. Le moteur est la presse, pas le kiosque.

Ce qui part en ligne : `index.html` avec les trois parties déjà jouées à l'intérieur, plus les
feuilles de style et les scripts qui les mettent en scène — navigation, carte hexagonale, cartes,
bandeau du capital. Tout le reste du dépôt reste au dépôt.

```powershell
# Produire le site dans .artifacts\site, sans rien publier
scripts\Publish-StaticSite.ps1

# Le produire ET le pousser sur gh-pages, d'où GitHub Pages le sert
scripts\Publish-StaticSite.ps1 -Deploy -Message "Publication — ce qui change"
```

La publication passe par une copie de travail jetable : la copie principale n'est jamais touchée,
et un travail en cours ne peut pas partir en ligne par accident. Le site est à jour une à deux
minutes plus tard sur [benoitgaly.github.io/theory-of-victory](https://benoitgaly.github.io/theory-of-victory/).

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
être citées comme des faits. Cela vaut aussi pour les trois coefficients qui convertissent le
capital de guerre en milliards de dollars — le multiple de capitalisation, la vente annuelle du
gigawatt installé et le prix annuel du maintien au pouvoir — écrits avec leur incertitude au
[§11 du capital de guerre](docs/design/08-capital-de-guerre.md). Font exception les **effectifs**,
calés sur sources ouvertes avec fourchettes d'incertitude assumées dans
[`04-calibration-effectifs.md`](docs/design/04-calibration-effectifs.md).
La conception complète est dans [`docs/design/01-modele-de-jeu.md`](docs/design/01-modele-de-jeu.md).

## Licence

MIT — voir [`LICENSE`](LICENSE).
