# Direction artistique

Ce document fixe la langue visuelle de *Theory of Victory* et consigne les
recommandations qui n'ont pas été mises en œuvre, avec leur raison.

Le site est un site de soutien à l'Ukraine. Le ton est celui d'un jeu de
plateau sérieux : un dossier d'état-major posé sur une table, jamais un
tableau de bord d'entreprise, jamais un divertissement.

---

## 1. Principes

**Le papier d'abord.** Le fond est un papier clair (`#f2efe7`), jamais un thème
sombre. Un grain fractal en SVG inline (`body::before`, opacité 5 %, mélange
`multiply`) donne la matière. Aucune ressource distante : le site doit
fonctionner hors-ligne, donc pas de CDN, pas de police téléchargée, pas
d'image externe. Toute illustration est un SVG construit en JavaScript.

**Ce n'est pas un tableur.** Les chiffres sont mis en scène, pas alignés. Un
écran a un seul chiffre-héros ; tout le reste recule. Une donnée répétée à
l'identique sur sept lignes n'est pas une donnée, c'est du bruit : elle est
factorisée en en-tête et les lignes gardent leur seule information
distinctive.

**Le goulot d'étranglement se voit avant d'être lu.** C'est la thèse du jeu :
la puissance est celle de la ressource la plus rare. Sur chaque écran, ce qui
bloque est en rouge, nommé, et désigné au même endroit par le dessin et par le
texte.

**Une seule autorité par information.** Quand le moteur nomme un goulot, le
dessin désigne cette ressource-là — même si plusieurs sont à égalité. Deux
surfaces qui se contredisent coûtent plus cher que l'une des deux en moins.

---

## 2. Jetons de style

### Couleurs

| Rôle | Jeton | Valeur |
|---|---|---|
| Papier | `--paper` | `#f2efe7` |
| Papier creusé | `--paper-deep` | `#e6e0d0` |
| Carton (panneaux) | `--card` | `#fbf9f4` |
| Carton secondaire | `--card-2` | `#f5f1e6` |
| Encre | `--ink` | `#1a1815` |
| Encre secondaire | `--ink-2` | `#4e4a42` |
| Encre tertiaire | `--ink-3` | `#8b8578` |
| Filet | `--rule` / `--rule-2` | `#d9d1be` / `#eae4d5` |
| Russie | `--ru` | `#a8322a` |
| Ukraine | `--ua` | `#1e5fa8` |
| Or (événement, issue) | `--gold` | `#b8860b` |

Le noir est chaud (`#1a1815`), jamais `#000` : sur du papier, un noir froid
sonne écran.

### Typographie

Deux familles, aucune téléchargée :

- **Titrage** — `--font-display` : pile sérif système
  (`Iowan Old Style`, `Palatino Linotype`, `Book Antiqua`, `Palatino`,
  `Georgia`). Elle porte les titres, tous les chiffres importants, les noms de
  secteur, les titres de carte et le texte d'ambiance en italique.
- **Interface** — `--font-ui` : `Segoe UI` et suivants. Elle porte le corps de
  texte, les libellés et les étiquettes.

Les libellés secondaires sont en petites capitales : 9,5 à 10,5 px,
`letter-spacing: 0.13em`, graisse 700, couleur `--ink-3`. Tous les chiffres
comparables utilisent `font-variant-numeric: tabular-nums`.

Échelle des chiffres, par ordre d'importance : chiffre-héros 68 px, maillon de
chaîne 34 px, déplacement de secteur 20 px, valeur de stock 19 px.

### Matière

Les panneaux sont du carton, pas des cartes Material : fond `--card`, filet
`--rule`, rayon 8 px, et une ombre qui commence par un liseré blanc interne
(`inset 0 1px 0`) — c'est ce liseré qui donne l'épaisseur.

---

## 3. Les pièces

### Le tonneau de Liebig

Pièce maîtresse. Quatre douves, une par ressource ; la hauteur est le taux de
couverture ; l'eau ne monte jamais au-dessus de la plus courte.

Ce qui le fait lire comme un tonneau et non comme un histogramme :

- un dégradé horizontal par douve (sombre sur les chants, clair au centre),
  renforcé sur les douves de bord pour que l'ensemble se lise comme un
  cylindre ;
- des cerclages métalliques **continus** : là où une douve manque, le fer est
  dessiné en retrait (opacité 0,3) — on voit l'autre côté du tonneau à travers
  la brèche. C'est ce détail qui rend l'image immédiatement canonique ;
- un voile d'eau léger (opacité 0,19) posé par-dessus le bois : la teinte de
  chaque ressource doit rester lisible sous l'eau ;
- un filet de débordement à la douve courte, avec ses gouttes ;
- la ligne d'eau prolongée dans la marge, avec son étiquette « NIVEAU RÉEL » ;
- un repère pointillé à 100 %, pour que l'écart au besoin se mesure à l'œil.

### Les cartes

Format Magic non négociable : cadre, ligne de titre avec coût, illustration,
ligne de type, boîte de règles, texte d'ambiance, pied de carte.

La **coque porte la famille** (six teintes, `FAMILY_ACCENT` dans `board.js`),
le **parchemin porte le camp** (rouge Russie, bleu Ukraine, beige neutre). La
teinte de famille se retrouve sur trois surfaces : le dégradé de la coque, le
filet autour de l'illustration, la pastille de la ligne de type et du pied.

Les illustrations suivent une grammaire commune : ciel dégradé, une source de
lumière unique, un plan intermédiaire, une silhouette au premier plan, un voile
sombre en bas de vignette. C'est la profondeur qui fait l'illustration ; le
pictogramme fait le panneau de signalisation.

### Le rapport de force

Toutes les jauges de secteur partagent la même échelle (0 à 3) et le même
repère de seuil à 1,1, ce qui rend les secteurs comparables d'un coup d'œil :
on voit lequel est le plus près de céder. Un secteur qui bouge est traité en
événement (cerné de sa couleur de camp, déplacement en 20 px) ; les secteurs
figés passent en liste compacte sous un en-tête qui porte, une seule fois, la
phrase qu'ils répétaient tous.

---

## 4. Recommandations non mises en œuvre

### 4.1 Embarquer une police de titrage locale

L'identité repose aujourd'hui sur une pile sérif système. Elle rend très bien
sur Windows (Palatino Linotype) et sur macOS (Iowan Old Style), mais Linux
tombera sur un substitut quelconque et la personnalité disparaîtra.

*Recommandation* : embarquer une sérif à licence libre en `woff2` dans
`wwwroot/fonts/` (EB Garamond, Spectral ou Zilla Slab conviennent au registre),
déclarée en `@font-face` avec `font-display: swap`.

*Pourquoi ce n'est pas fait* : ajouter un binaire de police au dépôt engage le
projet sur un poids et une licence. C'est une décision de projet, pas un
arbitrage de mise en forme.

### 4.2 Animer le passage d'un tour à l'autre

La frise est une piste de jeu, mais passer de T9 à T10 redessine tout d'un
bloc. Faire **monter et descendre l'eau** du tonneau, glisser les douves et
déplacer la ligne de front sur 400 ms transformerait une consultation en
démonstration : on verrait la contrainte se resserrer.

*Pourquoi ce n'est pas fait* : `render()` reconstruit intégralement le DOM à
chaque changement d'état. Animer suppose de conserver l'état précédent et de
séparer la construction du DOM de sa mise à jour — c'est une refonte du cycle
de rendu de `board.js`, hors du périmètre d'une passe de direction artistique.

### 4.3 Montrer la main du joueur

Le sous-titre du site est « You have the cards in hand ». On ne voit jamais de
main. Afficher, à côté des cartes jouées, les cartes **disponibles et non
jouées** du tour — en dos de carte, ou en éventail atténué — matérialiserait le
choix, et donnerait tout son sens à la promesse du sous-titre.

*Pourquoi ce n'est pas fait* : le modèle n'expose que `turn.cardsPlayed`. Les
cartes disponibles non jouées ne sortent pas du moteur ; il faut les publier
côté C# avant de pouvoir les dessiner.

### 4.4 Comparer les deux camps côte à côte

Russie et Ukraine occupent deux écrans successifs. Or le jeu se comprend dans
la comparaison : deux tonneaux, deux chiffres-héros, deux goulots différents.
Un mode « face à face » rendrait l'asymétrie du conflit immédiate — la Russie
achète son soutien et bute sur ses usines, l'Ukraine le reçoit et bute sur ce
qu'on lui livre.

*Pourquoi ce n'est pas fait* : c'est un ajout à la structure de navigation (une
quatrième phase, ou un basculement dans les deux existantes), donc une décision
de conception du jeu, pas d'esthétique.

### 4.5 Donner une trajectoire à la frise des tours

Les dix-huit tours sont dix-huit boutons de même poids. La frise pourrait
porter, en fond très pâle, la courbe des kilomètres carrés pris depuis février
2022 : on lirait la trajectoire de la guerre en même temps qu'on navigue
dedans, et les tours où une carte a été jouée prendraient un sens causal.

*Pourquoi ce n'est pas fait* : la barre de navigation est déjà dense (18 tours,
saisons, marqueurs de carte). Ajouter une courbe derrière risque de la rendre
illisible ; l'arbitrage se fait sur écran, avec le concepteur du jeu.

### 4.6 Une trame de plateau en fond

Une grille hexagonale très pâle, ou un réseau de filets, derrière le contenu —
comme le carton imprimé d'un jeu de plateau — renforcerait la sensation de
plateau.

*Pourquoi ce n'est pas fait* : le fond est déjà grainé. Superposer une trame
géométrique risque le bruit visuel et une baisse de contraste sur les textes
secondaires. À tester sur écran réel avant de l'adopter.
