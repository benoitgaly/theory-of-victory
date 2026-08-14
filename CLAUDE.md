# Theory of Victory — règles de travail

## L'interface

**Une interface utilisateur, s'il faut l'expliquer, c'est qu'elle n'est pas bonne.**

Une légende, une préface, un paragraphe d'introduction, une bannière qui donne le mode d'emploi
du signe posé juste à côté : ce sont tous le même aveu, que le dessin ne se suffit pas. La
réponse n'est jamais d'ajouter le texte — c'est de refaire le dessin jusqu'à ce que la question
ne se pose plus. Un nom écrit contre la courbe qu'il désigne vaut mieux qu'une légende ; une
matière qui dit ce qu'elle est vaut mieux qu'un signe à décoder ; un avant/après lu dans la
forme vaut mieux qu'un trait de repère qu'il faut situer.

Ce qui reste utile sans être nécessaire descend dans l'infobulle, où on va le chercher quand
on le veut. Ce qui relève de la conception va dans `docs/design/`, jamais à l'écran.

**Ne simplifie pas pour autant.** La densité est le propos d'un plateau de wargame : ce qui ne
tient pas se déplace ou se défile dans son propre cadre, il ne se retire pas.

## Ce qui ne se défait pas

- La carte qui suit l'histoire réelle (`hexmap.js`, `frontline.js`, `geography.js`,
  `data/front-history.json`).
- Les URLs qui portent le déroulé, le trimestre et l'écran.
- **Aucun rang de tour, nulle part** — ni à l'écran, ni dans les URLs. Un trimestre se désigne
  par sa date : « Été 2026 ».
- La règle mobile : la page ne défile jamais latéralement.

## Le code

.NET 10, ASP.NET Core MVC. Front en JavaScript vanilla, **sans framework ni dépendance
externe** : le site est publié en statique et rien ne peut être chargé depuis un CDN.

C# : types explicites, pas de `var`, corps de méthode en bloc, `is null`, commentaires en
anglais qui expliquent le *pourquoi*. Le texte affiché est en français soigné, avec ses accents
et ses articles.

## Vérifier

**Lis le code, n'ouvre pas le navigateur.** Un aller-retour visuel coûte une minute et ne
répond qu'à une question ; le fichier répond à dix. Le navigateur sert **une fois, à la fin**,
quand le lot de changements est écrit — jamais après chaque édition, et jamais pour vérifier ce
qu'un `grep` établit.

- `dotnet test tests/TheoryOfVictory.Engine.UnitTests` vert.
- Passe visuelle finale, **en un seul balayage** : les trois déroulés, 1600 px et 375 px, les
  chevauchements mesurés par intersection de rectangles et non jugés à l'œil, console vide. Le
  balayage se scripte en une évaluation JavaScript qui clique les déroulés et les trimestres —
  pas en une capture d'écran par combinaison.
- **Arrête ton serveur dès la vérification finie** : une application lancée depuis ce dépôt
  verrouille les DLL. Jamais de `Stop-Process -Name dotnet` — par PID, le tien, identifié par
  son port puis par sa ligne de commande.

**Si tu touches au moteur** — production, budget de guerre, aide, dépôts — relis la marge de
l'asphyxie (§12 de `docs/design/06-calendrier-propose.md`) : la puissance russe à la veille de
la chute vaut **48,95 % de son pic pour un seuil à 50**. Un point de marge. « Les tests
passent » ne suffit pas, il faut relire ce chiffre.

**Si tu publies** : `scripts\Publish-StaticSite.ps1 -Deploy -Message "…"`, puis va regarder la
page publique. La preuve, c'est un chiffre qui a changé à l'écran, pas un statut de pipeline —
la mise en ligne prend une à deux minutes.

## Les chiffres

Sauf mention contraire, ce sont des **ordres de grandeur de travail**. Font exception les
effectifs (`04-calibration-effectifs.md`), la position du front (`11-front-historique.md`) et
le soutien extérieur (`13-soutien-exterieur-source.md`), qui sont sourcés.

Ce site affiche une guerre en cours et sera lu comme tel : **une ligne inventée est pire qu'une
ligne absente.** Ce qu'un agent affirme se contrôle à la source.
