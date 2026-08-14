namespace TheoryOfVictory.Core.Localization;

/// <summary>
/// La mise en phrase. Le moteur énonce un fait — un code et ses paramètres — et c'est ici, à la
/// LECTURE, que ce fait devient une phrase, dans la langue du lecteur.
///
/// Tout ce que le moteur disait en français est descendu ici, et une seule fois : la phrase
/// française est la clé, la traduction vit dans les catalogues, et l'ordre des mots appartient
/// à chaque langue — c'est tout l'intérêt de %1 plutôt que d'une concaténation. « Les dépôts de
/// la Russie couvrent encore 1,4 trimestre » et son équivalent anglais n'ont ni le même ordre
/// ni le même nombre de mots, et aucun des deux n'a à le savoir.
///
/// Un code inconnu lève. Il ne produit surtout pas une phrase vide : une phrase qui manque se
/// verrait au premier déroulé joué, et les trois le sont à chaque test.
/// </summary>
public static class Phrasebook
{
    public static string Say(LocalizedText text)
    {
        object?[] arguments = [.. text.Arguments.Select(Render)];
        return Labels(text.Code, arguments);
    }

    /// <summary>Un argument peut être lui-même un fait — le nom d'un camp, celui d'une ressource.</summary>
    private static object? Render(object? argument)
    {
        return argument is LocalizedText nested ? Say(nested) : argument;
    }

    private static string Labels(string code, object?[] a)
    {
        return code switch
        {
            // Le texte venu des données traverse sans être touché.
            TextCodes.Verbatim => Localizer.Loc("%1", a),

            TextCodes.Side.Invader => Localizer.Loc("Russie"),
            TextCodes.Side.Defender => Localizer.Loc("Ukraine"),
            TextCodes.Side.InvaderInProse => Localizer.Loc("la Russie"),
            TextCodes.Side.DefenderInProse => Localizer.Loc("l'Ukraine"),
            TextCodes.Side.InvaderOpening => Localizer.Loc("La Russie"),
            TextCodes.Side.DefenderOpening => Localizer.Loc("L'Ukraine"),
            TextCodes.Side.EitherSide => Localizer.Loc("chaque camp"),

            TextCodes.Resource.Weapons => Localizer.Loc("Armes"),
            TextCodes.Resource.Fuel => Localizer.Loc("Carburant"),
            TextCodes.Resource.Food => Localizer.Loc("Nourriture"),
            TextCodes.Resource.StrikeDrones => Localizer.Loc("Drones d'attaque"),
            TextCodes.Resource.Missiles => Localizer.Loc("Missiles"),
            TextCodes.Resource.CheapInterceptors => Localizer.Loc("Défense bas coût"),
            TextCodes.Resource.HeavyInterceptors => Localizer.Loc("Intercepteurs lourds"),

            TextCodes.Resource.WeaponsInline => Localizer.Loc("armes"),
            TextCodes.Resource.FuelInline => Localizer.Loc("carburant"),
            TextCodes.Resource.FoodInline => Localizer.Loc("nourriture"),
            TextCodes.Resource.StrikeDronesInline => Localizer.Loc("drones d'attaque"),
            TextCodes.Resource.MissilesInline => Localizer.Loc("missiles"),
            TextCodes.Resource.CheapInterceptorsInline => Localizer.Loc("défense bas coût"),
            TextCodes.Resource.HeavyInterceptorsInline => Localizer.Loc("intercepteurs lourds"),
            TextCodes.Resource.UnitsInline => Localizer.Loc("unités"),

            TextCodes.Season.Winter => Localizer.Loc("Hiver"),
            TextCodes.Season.Spring => Localizer.Loc("Printemps"),
            TextCodes.Season.Summer => Localizer.Loc("Été"),
            TextCodes.Season.Autumn => Localizer.Loc("Automne"),

            _ => Capital(code, a),
        };
    }

    private static string Capital(string code, object?[] a)
    {
        return code switch
        {
            TextCodes.Capital.Reserves => Localizer.Loc("Réserves monétaires"),
            TextCodes.Capital.Grid => Localizer.Loc("Centrales électriques"),
            TextCodes.Capital.OilRevenue => Localizer.Loc("Pétrole — recette"),
            TextCodes.Capital.OilBill => Localizer.Loc("Pétrole — facture"),
            TextCodes.Capital.Civilian => Localizer.Loc("Usines civiles"),
            TextCodes.Capital.Arms => Localizer.Loc("Usines d'armement"),
            TextCodes.Capital.Regime => Localizer.Loc("Tenue du pouvoir"),
            TextCodes.Capital.ForeignReceived => Localizer.Loc("Soutien étranger — reçu"),
            TextCodes.Capital.ForeignBought => Localizer.Loc("Soutien étranger — acheté"),
            TextCodes.Capital.InternationalLatitude => Localizer.Loc("Soutien international — latitude"),
            TextCodes.Capital.InternationalPressure => Localizer.Loc("Soutien international — pression obtenue"),
            TextCodes.Capital.LivingStandardLink => Localizer.Loc("Niveau de vie"),

            TextCodes.Capital.ReserveThreshold => Localizer.Loc("quatre trimestres de ponction"),
            TextCodes.Capital.WinterDemand => Localizer.Loc("la demande d'hiver, %1 GW", a),

            TextCodes.Capital.QuarterDraw => Localizer.Loc("ponction du trimestre"),
            TextCodes.Capital.StandingPlant => Localizer.Loc("parc debout"),
            TextCodes.Capital.YearRevenue => Localizer.Loc("recette de l'année"),
            TextCodes.Capital.YearBill => Localizer.Loc("facture de l'année"),
            TextCodes.Capital.YearProduction => Localizer.Loc("production de l'année"),
            TextCodes.Capital.LivingStandard => Localizer.Loc("niveau de vie"),
            TextCodes.Capital.PoliticalCapital => Localizer.Loc("capital politique produit"),
            TextCodes.Capital.BackersWill => Localizer.Loc("volonté des soutiens"),
            TextCodes.Capital.AbilityToPay => Localizer.Loc("capacité à payer"),
            TextCodes.Capital.SupplierDependency => Localizer.Loc("dépendance aux fournisseurs"),
            TextCodes.Capital.ComponentLock => Localizer.Loc("verrou sur les composants"),

            TextCodes.Capital.UnitBillions => Localizer.Loc("Md$"),
            TextCodes.Capital.UnitOutOfHundred => Localizer.Loc("sur 100"),
            TextCodes.Capital.UnitGigawatts => Localizer.Loc("GW"),
            TextCodes.Capital.UnitPoints => Localizer.Loc("pts"),
            TextCodes.Capital.UnitPercent => Localizer.Loc("%"),
            TextCodes.Capital.UnitPreWarShare => Localizer.Loc("% de l'avant-guerre"),

            TextCodes.Capital.GridStrike => Localizer.Loc("frappe sur le réseau électrique"),
            TextCodes.Capital.RefiningStrike => Localizer.Loc("frappe sur le raffinage"),
            TextCodes.Capital.ArmsStrike => Localizer.Loc("frappe sur les usines d'armement"),
            TextCodes.Capital.CivilianStrike => Localizer.Loc("frappe sur les entrepôts civils"),
            TextCodes.Capital.DeepStrike => Localizer.Loc("frappe en profondeur"),

            _ => Alerts(code, a),
        };
    }

    private static string Alerts(string code, object?[] a)
    {
        return code switch
        {
            TextCodes.Alert.DepotTitle =>
                Localizer.Loc("%1 : %2 trimestre(s) de couverture, et ça descend", a),
            TextCodes.Alert.DepotDetail =>
                Localizer.Loc(
                    "Les dépôts de %1 couvrent encore %2 trimestre(s) de consommation, mais ils ne se "
                    + "remplissent plus. Le front ne verra rien passer jusqu'au trimestre où il ne verra "
                    + "plus rien du tout.", a),

            TextCodes.Alert.SustainmentTitle => Localizer.Loc("Ravitaillement impayé à %1 %", a),
            TextCodes.Alert.SustainmentDetail =>
                Localizer.Loc(
                    "%1 n'a plus de quoi acheter les rations et le carburant de ses propres troupes. On "
                    + "nourrit avant de choisir : quand cette ligne-là casse, aucune allocation ne "
                    + "rattrape plus rien.", a),

            TextCodes.Alert.ReservesTitle => Localizer.Loc("Fonds souverain : %1 trimestre(s)", a),
            TextCodes.Alert.ReservesDetail =>
                Localizer.Loc(
                    "%1 ponctionne %2 Md par trimestre pour tenir son effort de guerre. Il reste %3 Md. "
                    + "Après quoi la guerre ne coûte plus que ce qu'elle rapporte.", a),

            TextCodes.Alert.FundingGapTitle => Localizer.Loc("Effort de guerre bridé à %1 %", a),
            TextCodes.Alert.FundingGapDetail =>
                Localizer.Loc(
                    "Les recettes du trimestre ne financent que %1 Md sur les %2 Md que %3 voudrait "
                    + "dépenser. L'appareil le voit avant le front.", a),

            TextCodes.Alert.CollapseNow => Localizer.Loc("Le front cède"),
            TextCodes.Alert.CollapseIn => Localizer.Loc("Effondrement dans %1 tour(s)", a),
            TextCodes.Alert.CollapseDetail =>
                Localizer.Loc(
                    "%1 régénère %2 de ce qu'elle consomme, sous le seuil de %3 depuis %4 tour(s). Trois "
                    + "tours sous le seuil et le front ne cède pas sous un assaut : il cède faute de flux.", a),

            TextCodes.Alert.GenerationTitle => Localizer.Loc("Régénération à %1, seuil à %2", a),
            TextCodes.Alert.GenerationDetail =>
                Localizer.Loc(
                    "%1 remplace tout juste ce qu'elle perd. Un mauvais trimestre et le compte à rebours "
                    + "démarre.", a),

            TextCodes.Alert.ApparatusStressTitle => Localizer.Loc("Appareil sous tension : %1 / %2", a),
            TextCodes.Alert.ApparatusStressDetail =>
                Localizer.Loc(
                    "Cohésion des élites à %1, tension latente à %2. Un régime ne tombe pas quand la rue "
                    + "crie, il tombe quand la guerre cesse de payer ceux qui comptent.", a),
            TextCodes.Alert.WillStressTitle => Localizer.Loc("Volonté sous tension : %1 / %2", a),
            TextCodes.Alert.NegotiationDetail =>
                Localizer.Loc("Moral à %1, mécontentement à %2. Ce camp-là ne se renverse pas : il négocie.", a),

            TextCodes.Alert.WinterSheddingTitle =>
                Localizer.Loc("Hiver : délestage de %1 % au prochain tour", a),
            TextCodes.Alert.WinterSheddingIndustrial =>
                Localizer.Loc(
                    "La demande hivernale dépasse ce que le réseau de %1 peut fournir, et le tampon civil "
                    + "est épuisé : %2 % de la production d'armes s'éteindra avec les usines.", a),
            TextCodes.Alert.WinterSheddingCivilian =>
                Localizer.Loc(
                    "La demande hivernale dépasse la génération disponible. Le civil sera délesté le "
                    + "premier — cela coûte du moral, pas encore des obus."),

            TextCodes.Alert.EdgeDecayTitle => Localizer.Loc("Avance tactique périmée en 2 tours (%1 → %2)", a),
            TextCodes.Alert.EdgeDecayDetail =>
                Localizer.Loc(
                    "L'adversaire s'adapte de %1 % par tour. Sans réinvestissement, %2 retombe à sa "
                    + "consommation d'obus d'avant — et le goulot revient se placer là où il était.", a),

            TextCodes.Alert.ExternalWillTitle => Localizer.Loc("Volonté des soutiens : %1 / 100", a),
            TextCodes.Alert.ExternalWillDetail =>
                Localizer.Loc(
                    "%1 vit d'un flux qu'elle ne paie pas et ne contrôle pas. Couper ce flux prend une "
                    + "journée et une élection ; le reconstituer prend des années.", a),

            _ => Narrative(code, a),
        };
    }

    private static string Narrative(string code, object?[] a)
    {
        return code switch
        {
            TextCodes.Narrative.MobilisationEmpty =>
                Localizer.Loc("%1 : mobilisation décrétée, mais le réservoir est vide.", a),
            TextCodes.Narrative.Mobilised =>
                Localizer.Loc("%1 : %2 hommes mobilisés, %3 Md de capacité productive perdus.", a),
            TextCodes.Narrative.WarBudgetCapped =>
                Localizer.Loc("%1 : effort de guerre bridé par les recettes, %2 Md finançables seulement.", a),
            TextCodes.Narrative.ForeignPurchases =>
                Localizer.Loc("%1 : %2 Md d'armes achetées à l'étranger, hors capacité nationale.", a),
            TextCodes.Narrative.UnspentMoney =>
                Localizer.Loc("%1 : %2 Md non convertis — l'argent existe, la capacité non.", a),
            TextCodes.Narrative.PayNotCovered =>
                Localizer.Loc("%1 : la solde n'est plus couverte — %2 hommes finançables sur %3 au front.", a),
            TextCodes.Narrative.SustainmentUnpaid =>
                Localizer.Loc("%1 : %2 % du ravitaillement impayé — la trésorerie ne suit plus.", a),
            TextCodes.Narrative.Recruited =>
                Localizer.Loc("%1 : %2 recrues, %3 Md de capacité productive en moins.", a),
            TextCodes.Narrative.IndustrialShedding =>
                Localizer.Loc("%1 : délestage industriel, %2 % de la production d'armes perdue.", a),
            TextCodes.Narrative.CivilianCuts =>
                Localizer.Loc("%1 : coupures civiles en plein hiver, le moral encaisse.", a),
            TextCodes.Narrative.NewCapacity =>
                Localizer.Loc("%1 : nouvelle capacité en service, +%2 k unités par tour.", a),
            TextCodes.Narrative.ReserveDraw =>
                Localizer.Loc("%1 : %2 Md ponctionnés sur les réserves.", a),
            TextCodes.Narrative.DelayedEffect => Localizer.Loc("Effet différé : %1.", a),
            TextCodes.Narrative.CardCountered =>
                Localizer.Loc("« %1 » est contrée : la carte est jouée, elle ne produit rien.", a),
            TextCodes.Narrative.CardOverdraft =>
                Localizer.Loc(
                    "« %1 » coûte %2 de capital politique — il en manquait %3. En V2, cette carte reste "
                    + "en main.", a),
            TextCodes.Narrative.StrikeSaturated =>
                Localizer.Loc("%1 sature %2 — %3 % interceptés, mais %4 € dépensés par € détruit.", a),
            TextCodes.Narrative.Strike => Localizer.Loc("%1 frappe %2 — %3 % interceptés.", a),
            TextCodes.Narrative.Rupture =>
                Localizer.Loc("RUPTURE — %1 ne régénère plus assez de force : le front cède.", a),
            TextCodes.Narrative.QuietQuarter =>
                Localizer.Loc("Les deux camps remplacent ce qu'ils consomment. Rien ne bouge, et c'est le sujet."),

            TextCodes.Narrative.NoOnePaysInvader =>
                Localizer.Loc(
                    "Plus personne ne paie l'armée russe : %1 hommes ont quitté la ligne ce trimestre. "
                    + "Aucun assaut ne les en a délogés.", a),
            TextCodes.Narrative.NoOnePaysDefender =>
                Localizer.Loc(
                    "Plus personne ne paie l'armée ukrainienne : %1 hommes ont quitté la ligne ce "
                    + "trimestre. Aucun assaut ne les en a délogés.", a),
            TextCodes.Narrative.ArmisticeInvader =>
                Localizer.Loc(
                    "ARMISTICE — l'armée russe n'existe plus comme force organisée. Le terrain a changé "
                    + "de mains sans bataille."),
            TextCodes.Narrative.ArmisticeDefender =>
                Localizer.Loc(
                    "ARMISTICE — l'armée ukrainienne n'existe plus comme force organisée. Le terrain a "
                    + "changé de mains sans bataille."),

            _ => Targets(code, a),
        };
    }

    private static string Targets(string code, object?[] a)
    {
        return code switch
        {
            TextCodes.Target.GridInvader => Localizer.Loc("le réseau électrique de l'arrière russe"),
            TextCodes.Target.GridDefender => Localizer.Loc("le réseau électrique de l'arrière ukrainien"),
            TextCodes.Target.RefiningInvader => Localizer.Loc("le raffinage et les terminaux russes"),
            TextCodes.Target.RefiningDefender => Localizer.Loc("le raffinage et les terminaux ukrainiens"),
            TextCodes.Target.IndustryInvader => Localizer.Loc("les usines d'armement russes"),
            TextCodes.Target.IndustryDefender => Localizer.Loc("les usines d'armement ukrainiennes"),
            TextCodes.Target.LogisticsInvader => Localizer.Loc("les nœuds logistiques russes"),
            TextCodes.Target.LogisticsDefender => Localizer.Loc("les nœuds logistiques ukrainiens"),
            TextCodes.Target.CivilianInvader => Localizer.Loc("les entrepôts et les usines civiles russes"),
            TextCodes.Target.CivilianDefender => Localizer.Loc("les entrepôts et les usines civiles ukrainiens"),
            TextCodes.Target.Rear => Localizer.Loc("l'arrière adverse"),

            TextCodes.Sector.HolderCollapsed => Localizer.Loc("Effondrement de %1 — avance libre", a),
            TextCodes.Sector.NoMovement => Localizer.Loc("Aucun mouvement, usure réciproque"),
            TextCodes.Sector.Nibbling => Localizer.Loc("Grignotage par %1", a),
            TextCodes.Sector.Advance => Localizer.Loc("Avance de %1", a),
            TextCodes.Sector.Breakthrough => Localizer.Loc("Percée de %1", a),

            TextCodes.Sector.Kharkiv => Localizer.Loc("Kharkiv"),
            TextCodes.Sector.Kupiansk => Localizer.Loc("Koupiansk"),
            TextCodes.Sector.Lyman => Localizer.Loc("Lyman"),
            TextCodes.Sector.Bakhmut => Localizer.Loc("Bakhmout — Tchassiv Iar"),
            TextCodes.Sector.Pokrovsk => Localizer.Loc("Pokrovsk"),
            TextCodes.Sector.Vuhledar => Localizer.Loc("Vouhledar"),
            TextCodes.Sector.Zaporizhzhia => Localizer.Loc("Zaporijjia"),
            TextCodes.Sector.Kherson => Localizer.Loc("Kherson — Dniepr"),

            _ => Outcomes(code, a),
        };
    }

    private static string Outcomes(string code, object?[] a)
    {
        return code switch
        {
            TextCodes.Outcome.MilitaryCollapseTitle => Localizer.Loc("Effondrement militaire — %1", a),
            TextCodes.Outcome.MilitaryCollapseExplanation =>
                Localizer.Loc(
                    "Le ratio de génération de force est resté sous le seuil trois tours consécutifs. Le "
                    + "front n'a pas cédé sous un assaut : il a cédé parce que le flux s'est tari."),

            TextCodes.Outcome.RegimeCollapseTitle => Localizer.Loc("Chute du régime — %1", a),
            TextCodes.Outcome.RegimeCollapseExplanation =>
                Localizer.Loc(
                    "L'appareil s'est fracturé. Ce n'est pas la rue qui a renversé le régime : c'est la "
                    + "guerre qui a cessé de payer ceux qui comptaient."),

            TextCodes.Outcome.NegotiatedTitle => Localizer.Loc("Capitulation négociée — %1", a),
            TextCodes.Outcome.NegotiatedExplanation =>
                Localizer.Loc(
                    "La volonté de continuer s'est épuisée et le pays a négocié. On perd aussi par "
                    + "l'arrière."),

            TextCodes.Outcome.MutualExhaustionTitle => Localizer.Loc("Épuisement mutuel"),
            TextCodes.Outcome.MutualExhaustionExplanation =>
                Localizer.Loc(
                    "Les deux camps sont passés sous leur seuil de régénération. Armistice sur la ligne "
                    + "atteinte, faute de pouvoir continuer."),

            TextCodes.Outcome.FrozenFrontTitle => Localizer.Loc("Front figé"),
            TextCodes.Outcome.FrozenFrontExplanation =>
                Localizer.Loc(
                    "Les deux camps régénèrent autant qu'ils consomment. Le front tient, personne ne "
                    + "gagne : l'égalité industrielle produit l'enlisement, pas la paix."),

            TextCodes.Outcome.ArmisticeTitle => Localizer.Loc("Armistice — %1 se retire", a),
            TextCodes.Outcome.ArmisticeExplanation =>
                Localizer.Loc(
                    "%1 En %2 trimestres, elle est tombée sous %3 % de son effectif théorique sans qu'une "
                    + "seule attaque l'ait emportée : les secteurs sont revenus parce qu'il n'y avait plus "
                    + "personne pour les tenir. Le front n'a jamais été le moteur de cette guerre — il en "
                    + "était le thermomètre, et il vient de le montrer une dernière fois.", a),
            TextCodes.Outcome.ArmisticeCauseRegime =>
                Localizer.Loc("Le régime est tombé, et l'armée a cessé d'être payée."),
            TextCodes.Outcome.ArmisticeCauseWill =>
                Localizer.Loc("La volonté a cédé, et l'armée a cessé d'être tenue."),
            TextCodes.Outcome.ArmisticeCauseGeneration =>
                Localizer.Loc("La génération de force s'est tarie, et l'armée a cessé d'être alimentée."),

            TextCodes.Outcome.Unresolved => Localizer.Loc("Partie non conclue"),

            TextCodes.Outcome.ReasonGeneration =>
                Localizer.Loc("génération de force sous le seuil trois tours de suite"),
            TextCodes.Outcome.ReasonRegime => Localizer.Loc("chute du régime"),
            TextCodes.Outcome.ReasonWill => Localizer.Loc("épuisement de la volonté"),

            _ => Cards(code, a),
        };
    }

    private static string Cards(string code, object?[] a)
    {
        return code switch
        {
            TextCodes.Card.TypeLine => Localizer.Loc("%1 — %2", a),
            TextCodes.Card.Delayed => Localizer.Loc("%1 (dans %2 tours)", a),
            TextCodes.Card.Permanent => Localizer.Loc("Permanent"),
            TextCodes.Card.Instant => Localizer.Loc("Éphémère"),
            TextCodes.Card.SlowRitual => Localizer.Loc("Rituel lent"),
            TextCodes.Card.Counter => Localizer.Loc("Contre-carte"),
            TextCodes.Card.Plain => Localizer.Loc("Carte"),

            TextCodes.Card.FamilyEconomic => Localizer.Loc("Économique"),
            TextCodes.Card.FamilyWesternPolitics => Localizer.Loc("Politique occidentale"),
            TextCodes.Card.FamilyDomesticPolitics => Localizer.Loc("Politique interne"),
            TextCodes.Card.FamilyEnergy => Localizer.Loc("Énergie"),
            TextCodes.Card.FamilyMilitary => Localizer.Loc("Militaire et technologique"),
            TextCodes.Card.FamilyExternal => Localizer.Loc("Externe"),

            TextCodes.Card.OilPrice => Localizer.Loc("Prix du baril %1 $", a),
            TextCodes.Card.AidPledge => Localizer.Loc("%1 : aide promise %2 Md par tour", a),
            TextCodes.Card.AidDisbursement => Localizer.Loc("%1 : versement de l'aide %2 %", a),
            TextCodes.Card.ForeignCeiling => Localizer.Loc("%1 : plafond d'achat étranger %2 Md", a),
            TextCodes.Card.BarrelDiscount => Localizer.Loc("%1 : décote sur le baril %2 pts", a),
            TextCodes.Card.CustomsFriction => Localizer.Loc("%1 : friction douanière %2 pts", a),
            TextCodes.Card.ComponentAccess => Localizer.Loc("%1 : accès aux composants %2 pts", a),
            TextCodes.Card.Mobilisation => Localizer.Loc("%1 : mobilise %2 k hommes", a),
            TextCodes.Card.RecruitmentCost => Localizer.Loc("%1 : coût de recrutement ×%2", a),
            TextCodes.Card.Morale => Localizer.Loc("%1 : moral %2", a),
            TextCodes.Card.Discontent => Localizer.Loc("%1 : mécontentement %2", a),
            TextCodes.Card.EliteCohesion => Localizer.Loc("%1 : cohésion des élites %2", a),
            TextCodes.Card.BackersWill => Localizer.Loc("%1 : volonté des soutiens %2", a),
            TextCodes.Card.Corruption => Localizer.Loc("%1 : corruption %2", a),
            TextCodes.Card.PoliticalCapital => Localizer.Loc("%1 : capital politique %2", a),
            TextCodes.Card.TacticalDroneEdge => Localizer.Loc("%1 : avance drones tactiques %2", a),
            TextCodes.Card.DeepStrikeEdge => Localizer.Loc("%1 : avance frappe profonde %2", a),
            TextCodes.Card.CounterDroneEdge => Localizer.Loc("%1 : avance contre-drone %2", a),
            TextCodes.Card.IndustrialCapacity => Localizer.Loc("%1 : capacité industrielle ×%2", a),
            TextCodes.Card.GridDestroyed => Localizer.Loc("%1 : %2 GW détruits définitivement", a),
            TextCodes.Card.CivilianDestroyed => Localizer.Loc("%1 : %2 Md d'appareil civil détruits", a),
            TextCodes.Card.Refining => Localizer.Loc("%1 : raffinage %2 %", a),
            TextCodes.Card.Logistics => Localizer.Loc("%1 : logistique %2 %", a),
            TextCodes.Card.Treasury => Localizer.Loc("%1 : trésorerie %2 Md", a),
            TextCodes.Card.StockDelta => Localizer.Loc("%1 : stock %2 %3", a),
            TextCodes.Card.AidConditionality => Localizer.Loc("%1 : conditionnalité de l'aide %2 pts", a),

            // Un effet que personne n'a encore mis en mots : son propre nom technique vaut mieux
            // qu'une ligne vide sur une carte.
            TextCodes.Card.Unnamed => Localizer.Loc("%1", a),

            _ => Verdicts(code, a),
        };
    }

    private static string Verdicts(string code, object?[] a)
    {
        return code switch
        {
            TextCodes.Verdict.Decisive => Localizer.Loc("Décisive — sans elle, la guerre finit en « %1 ».", a),
            TextCodes.Verdict.BringsForward =>
                Localizer.Loc("Elle fait tomber la décision %1 trimestre(s) plus tôt, %2.", a),
            TextCodes.Verdict.Lengthens =>
                Localizer.Loc("Elle prolonge la guerre de %1 trimestre(s) sans en changer l'issue.", a),
            TextCodes.Verdict.NoMeasurableEffect =>
                Localizer.Loc("Sans effet mesurable sur l'issue : elle rend le chemin plus confortable, rien de plus."),
            TextCodes.Verdict.WithoutSeparating => Localizer.Loc("sans départager les camps"),
            TextCodes.Verdict.InFavourOf => Localizer.Loc("au profit de : %1", a),

            TextCodes.Verdict.ReadableFrozenFront => Localizer.Loc("front figé"),
            TextCodes.Verdict.ReadableMutualExhaustion => Localizer.Loc("épuisement mutuel"),
            TextCodes.Verdict.ReadableMilitaryCollapse => Localizer.Loc("effondrement militaire"),
            TextCodes.Verdict.ReadableRegimeCollapse => Localizer.Loc("chute du régime"),
            TextCodes.Verdict.ReadableNegotiated => Localizer.Loc("capitulation négociée"),
            TextCodes.Verdict.ReadableUnresolved => Localizer.Loc("partie non conclue"),

            TextCodes.Deck.DeepStrike => Localizer.Loc("Frappe profonde"),
            TextCodes.Deck.FrontalAttrition => Localizer.Loc("Attrition frontale"),
            TextCodes.Deck.Political => Localizer.Loc("Épuisement politique"),

            _ => Scenarios(code, a),
        };
    }

    private static string Scenarios(string code, object?[] a)
    {
        return code switch
        {
            TextCodes.Scenario.ResolveTitle => Localizer.Loc("L'Occident joue ses cartes"),
            TextCodes.Scenario.ResolveSubtitle => Localizer.Loc("L'Ukraine l'emporte"),
            TextCodes.Scenario.ResolveDescription =>
                Localizer.Loc(
                    "On ne prend pas de terrain : on coupe la caisse. Embargo sur les composants, campagne "
                    + "trimestrielle sur le raffinage, baril effondré, aide rendue prévisible. La guerre "
                    + "cesse de payer ceux qui la tiennent, et c'est l'arrière russe qui cède avant le "
                    + "front ukrainien."),

            TextCodes.Scenario.HoldsTitle => Localizer.Loc("Le soutien tient, sans plus"),
            TextCodes.Scenario.HoldsSubtitle => Localizer.Loc("Front figé, personne ne gagne"),
            TextCodes.Scenario.HoldsDescription =>
                Localizer.Loc(
                    "Le soutien extérieur vacille mais ne rompt jamais, sans jamais non plus s'intensifier. "
                    + "Les deux camps remplacent ce qu'ils consomment et le front se fige. L'égalité "
                    + "industrielle produit l'enlisement, pas la paix."),

            TextCodes.Scenario.CollapseTitle => Localizer.Loc("Le soutien s'arrête"),
            TextCodes.Scenario.CollapseSubtitle => Localizer.Loc("L'Ukraine cède"),
            TextCodes.Scenario.CollapseDescription =>
                Localizer.Loc(
                    "Même départ, mêmes cartes, même calendrier pétrolier. Seule différence : au tour 6, le "
                    + "flux gratuit s'arrête. Rien ne bouge pendant deux tours, puis tout cède d'un bloc."),

            _ => throw new InvalidOperationException(
                $"Le code de texte '{code}' n'a pas de phrase : le moteur énoncerait un fait que personne ne sait lire."),
        };
    }
}
