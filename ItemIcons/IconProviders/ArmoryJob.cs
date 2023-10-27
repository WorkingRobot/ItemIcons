using ItemIcons.IconTypes;
using ItemIcons.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ItemIcons.IconProviders;

internal sealed class ArmoryJob : IconProvider
{
    public override string Name => "Equippable Jobs";

    private uint IdOffset { get; }

    public readonly record struct IconDescriptor(float Scale, short Offset);
    public readonly record struct TextureDescriptor(string Texture, UldRect? Rect);
    public sealed class IconSet
    {
        private readonly BaseIcon?[] icons;

        public IconSet(IEnumerable<BaseIcon?> icons)
        {
            this.icons = icons.ToArray();
        }

        public IconSet(IEnumerable<TextureDescriptor?> textures) : this(textures.Select(t => t == null ? null : (BaseIcon?)new TextureIcon(t.Value.Texture, t.Value.Rect)))
        {

        }

        public IconSet(IEnumerable<uint> icons, IconDescriptor descriptor) : this(icons.Select(icon => (BaseIcon?)new TextureIcon(icon) { Scale = descriptor.Scale, Offset = descriptor.Offset }))
        {
        }

        public IconSet(params IconSet[] sets)
        {
            if (sets.Length == 0)
            {
                icons = Array.Empty<BaseIcon?>();
                return;
            }
            icons = new BaseIcon?[sets[0].Count];
            for (var i = 0; i < icons.Length; ++i)
            {
                foreach(var set in sets)
                {
                    if (set.Count <= i)
                        continue;
                    if (set.icons[i] == null)
                        continue;
                    icons[i] = set.icons[i];
                    break;
                }
            }
        }

        public BaseIcon? TryGet(int index) =>
            icons.Length > index ? icons[index] : null;

        public BaseIcon this[int index] =>
            TryGet(index) ??
                throw new ArgumentOutOfRangeException(nameof(index), index, "No icon at that index");

        public int Count => icons.Length;
    }

    private static readonly IconDescriptor DefaultDescriptor = new(1, 0);
    private static readonly IconDescriptor ColoredDescriptor = new(5 / 3f, -6);

    public IconSet JobSetGold { get; } = new(new uint[]
    {
        62001, 62002, 62003, 62004, 62005, 62006, 62007, 62008, 62009, 62010,
        62011, 62012, 62013, 62014, 62015, 62016, 62017, 62018, 62019, 62020,
        62021, 62022, 62023, 62024, 62025, 62026, 62027, 62028, 62029, 62030,
        62031, 62032, 62033, 62034, 62035, 62036, 62037, 62038, 62039, 62040
    }, DefaultDescriptor);

    public IconSet JobSetFramed { get; } = new(new uint[]
    {
        62101, 62102, 62103, 62104, 62105, 62106, 62107, 62108, 62109, 62110,
        62111, 62112, 62113, 62114, 62115, 62116, 62117, 62118, 62119, 62120,
        62121, 62122, 62123, 62124, 62125, 62126, 62127, 62128, 62129, 62130,
        62131, 62132, 62133, 62134, 62135, 62136, 62137, 62138, 62139, 62140
    }, DefaultDescriptor);

    public IconSet JobSetGlowing { get; } = new(new uint[]
    {
        62301, 62302, 62303, 62304, 62305, 62306, 62307, 62310, 62311, 62312,
        62313, 62314, 62315, 62316, 62317, 62318, 62319, 62320, 62401, 62402,
        62403, 62404, 62405, 62406, 62407, 62308, 62408, 62409, 62309, 62410,
        62411, 62412, 62413, 62414, 62415, 62416, 62417, 62418, 62419, 62420
    }, DefaultDescriptor);

    public IconSet JobSetGray { get; } = new(new uint[]
    {
        91022, 91023, 91024, 91025, 91026, 91028, 91029, 91031, 91032, 91033,
        91034, 91035, 91036, 91037, 91038, 91039, 91040, 91041, 91079, 91080,
        91081, 91082, 91083, 91084, 91085, 91030, 91086, 91087, 91121, 91122,
        91125, 91123, 91124, 91127, 91128, 91129, 91130, 91131, 91132, 91133
    }, ColoredDescriptor);

    public IconSet JobSetBlack { get; } = new(new uint[]
    {
        91522, 91523, 91524, 91525, 91526, 91528, 91529, 91531, 91532, 91533,
        91534, 91535, 91536, 91537, 91538, 91539, 91540, 91541, 91579, 91580,
        91581, 91582, 91583, 91584, 91585, 91530, 91586, 91587, 91621, 91622,
        91625, 91623, 91624, 91627, 91628, 91629, 91630, 91631, 91632, 91633
    }, ColoredDescriptor);

    public IconSet JobSetYellow { get; } = new(new uint[]
    {
        92022, 92023, 92024, 92025, 92026, 92028, 92029, 92031, 92032, 92033,
        92034, 92035, 92036, 92037, 92038, 92039, 92040, 92041, 92079, 92080,
        92081, 92082, 92083, 92084, 92085, 92030, 92086, 92087, 92121, 92122,
        92125, 92123, 92124, 92127, 92128, 92129, 92130, 92131, 92132, 92133
    }, ColoredDescriptor);

    public IconSet JobSetOrange { get; } = new(new uint[]
    {
        92522, 92523, 92524, 92525, 92526, 92528, 92529, 92531, 92532, 92533,
        92534, 92535, 92536, 92537, 92538, 92539, 92540, 92541, 92579, 92580,
        92581, 92582, 92583, 92584, 92585, 92530, 92586, 92587, 92621, 92622,
        92625, 92623, 92624, 92627, 92628, 92629, 92630, 92631, 92632, 92633
    }, ColoredDescriptor);

    public IconSet JobSetRed { get; } = new(new uint[]
    {
        93022, 93023, 93024, 93025, 93026, 93028, 93029, 93031, 93032, 93033,
        93034, 93035, 93036, 93037, 93038, 93039, 93040, 93041, 93079, 93080,
        93081, 93082, 93083, 93084, 93085, 93030, 93086, 93087, 93121, 93122,
        93125, 93123, 93124, 93127, 93128, 93129, 93130, 93131, 93132, 93133
    }, ColoredDescriptor);

    public IconSet JobSetPurple { get; } = new(new uint[]
    {
        93522, 93523, 93524, 93525, 93526, 93528, 93529, 93531, 93532, 93533,
        93534, 93535, 93536, 93537, 93538, 93539, 93540, 93541, 93579, 93580,
        93581, 93582, 93583, 93584, 93585, 93530, 93586, 93587, 93621, 93622,
        93625, 93623, 93624, 93627, 93628, 93629, 93630, 93631, 93632, 93633
    }, ColoredDescriptor);

    public IconSet JobSetBlue { get; } = new(new uint[]
    {
        94022, 94023, 94024, 94025, 94026, 94028, 94029, 94031, 94032, 94033,
        94034, 94035, 94036, 94037, 94038, 94039, 94040, 94041, 94079, 94080,
        94081, 94082, 94083, 94084, 94085, 94030, 94086, 94087, 94121, 94122,
        94125, 94123, 94124, 94127, 94128, 94129, 94130, 94131, 94132, 94133
    }, ColoredDescriptor);

    public IconSet JobSetGreen { get; } = new(new uint[]
    {
        94522, 94523, 94524, 94525, 94526, 94528, 94529, 94531, 94532, 94533,
        94534, 94535, 94536, 94537, 94538, 94539, 94540, 94541, 94579, 94580,
        94581, 94582, 94583, 94584, 94585, 94530, 94586, 94587, 94621, 94622,
        94625, 94623, 94624, 94627, 94628, 94629, 94630, 94631, 94632, 94633
    }, ColoredDescriptor);

    // Icons
    public IconSet RoleSetIcons { get; } = new(new TextureDescriptor?[]
    {
        new("ui/uld/LFG.tex", new(84, 108, 28, 28)), // All
        new("ui/uld/LFG.tex", new(0, 108, 28, 28)), // Tank
        new("ui/uld/LFG.tex", new(28, 108, 28, 28)), // Healer
        new("ui/uld/LFG.tex", new(56, 108, 28, 28)), // DPS
        new("ui/uld/LFG.tex", new(0, 288, 28, 28)), // Crafter
        new("ui/uld/LFG.tex", new(28, 288, 28, 28)), // Gatherer
        null, // Pure Healer
        null, // Shield Healer
        null, // Melee
        null, // Ranged
        null, // Caster
        null, // Tank/Healer
        null, // Tank/DPS
        null, // Healer/DPS
        null, // DoM/DoW
        null, // DoH/DoL
    });

    // Icons with square background
    public IconSet RoleSetSquare { get; } = new(new TextureDescriptor?[]
    {
        new("ui/uld/LFG.tex", new(84, 80, 28, 28)), // All
        new("ui/uld/LFG.tex", new(0, 80, 28, 28)), // Tank
        new("ui/uld/LFG.tex", new(28, 80, 28, 28)), // Healer
        new("ui/uld/LFG.tex", new(56, 80, 28, 28)), // DPS
        new("ui/uld/LFG.tex", new(56, 288, 28, 28)), // Crafter
        new("ui/uld/LFG.tex", new(84, 288, 28, 28)), // Gatherer
        null, // Pure Healer
        null, // Shield Healer
        null, // Melee
        null, // Ranged
        null, // Caster
        null, // Tank/Healer
        null, // Tank/DPS
        null, // Healer/DPS
        null, // DoM/DoW
        null, // DoH/DoL
    });

    // Icons with rounded background
    public IconSet RoleSetRounded { get; } = new(new TextureDescriptor?[]
    {
        null, // All
        new("ui/uld/LFGSelectRole.tex", new(0, 28, 28, 28)), // Tank
        new("ui/uld/LFGSelectRole.tex", new(28, 28, 28, 28)), // Healer
        new("ui/uld/LFGSelectRole.tex", new(56, 28, 28, 28)), // DPS
        null, // Crafter
        null, // Gatherer
        new("ui/uld/LFGSelectRole.tex", new(0, 56, 28, 28)), // Pure Healer
        new("ui/uld/LFGSelectRole.tex", new(28, 56, 28, 28)), // Shield Healer
        new("ui/uld/LFGSelectRole.tex", new(0, 0, 28, 28)), // Melee
        new("ui/uld/LFGSelectRole.tex", new(28, 0, 28, 28)), // Ranged
        new("ui/uld/LFGSelectRole.tex", new(56, 0, 28, 28)), // Caster
        null, // Tank/Healer
        null, // Tank/DPS
        null, // Healer/DPS
        null, // DoM/DoW
        null, // DoH/DoL
    });
    
    // Mini rectangles
    public IconSet RoleSetMini { get; } = new(new TextureDescriptor?[]
    {
        new("ui/uld/LFG2.tex", new(16, 20, 16, 20)), // All
        new("ui/uld/LFG2.tex", new(0, 0, 16, 20)), // Tank
        new("ui/uld/LFG2.tex", new(0, 20, 16, 20)), // Healer
        new("ui/uld/LFG2.tex", new(16, 0, 16, 20)), // DPS
        null, // Crafter
        null, // Gatherer
        null, // Pure Healer
        null, // Shield Healer
        null, // Melee
        null, // Ranged
        null, // Caster
        new("ui/uld/LFG2.tex", new(32, 20, 16, 20)), // Tank/Healer
        new("ui/uld/LFG2.tex", new(30, 0, 16, 20)), // Tank/DPS
        new("ui/uld/LFG2.tex", new(48, 0, 16, 20)), // Healer/DPS
        new("ui/uld/LFG2.tex", new(48, 20, 16, 20)), // DoM/DoW
        new("ui/uld/LFG2.tex", new(0, 40, 16, 20)), // DoH/DoL
    });

    private IconSet JobSetCrafter => JobSetYellow;
    private IconSet JobSetGatherer => JobSetOrange;
    private IconSet JobSetTank => JobSetBlue;
    private IconSet JobSetHealer => JobSetGreen;
    private IconSet JobSetRanged => JobSetRed;
    private IconSet JobSetMelee => JobSetRed;
    private IconSet JobSetCaster => JobSetRed;

    public IconSet RoleSet { get; }
    public Dictionary<uint, BaseIcon> CategoryIcons { get; }

    private readonly ImmutableSortedSet<uint> displayedCategories;

    public ArmoryJob()
    {
        RoleSet = new(RoleSetRounded, RoleSetIcons, RoleSetSquare, RoleSetMini);
        CategoryIcons = new ()
        {
            // 0 - null
            [9] = JobSetCrafter[7], // CRP
            [10] = JobSetCrafter[8], // BSM
            [11] = JobSetCrafter[9], // ARM
            [12] = JobSetCrafter[10], // GSM
            [13] = JobSetCrafter[11], // LTW
            [14] = JobSetCrafter[12], // WVR
            [15] = JobSetCrafter[13], // ALC
            [16] = JobSetCrafter[14], // CUL
            [17] = JobSetGatherer[15], // MIN
            [18] = JobSetGatherer[16], // BTN
            [19] = JobSetGatherer[17], // FSH
            [20] = JobSetTank[18], // PLD
            [21] = JobSetMelee[19], // MNK
            [22] = JobSetTank[20], // WAR
            [23] = JobSetMelee[21], // DRG
            [24] = JobSetRanged[22], // BRD
            [25] = JobSetHealer[23], // WHM
            [26] = JobSetCaster[24], // BLM
            [28] = JobSetCaster[24], // SMN
            [29] = JobSetHealer[27], // SCH
            [38] = JobSetTank[18], // GLA PLD
            [41] = JobSetMelee[19], // PGL MNK
            [44] = JobSetTank[20], // MRD WAR
            [47] = JobSetMelee[21], // LNC DRG
            [50] = JobSetRanged[22], // ARC BRD
            [53] = JobSetHealer[23], // CNJ WHM
            [55] = JobSetCaster[25], // THM BLM
            [69] = JobSetCaster[26], // ACN SMN
            [92] = JobSetMelee[29], // NIN
            [93] = JobSetMelee[29], // ROG NIN
            [96] = JobSetRanged[30], // MCH
            [98] = JobSetTank[31], // DRK
            [99] = JobSetHealer[32], // AST
            [103] = JobSetMelee[29], // ROG NIN
            [111] = JobSetMelee[33], // SAM
            [112] = JobSetCaster[34], // RDM
            [129] = JobSetCaster[35], // BLU
            [149] = JobSetTank[36], // GNB
            [150] = JobSetRanged[37], // DNC
            [180] = JobSetMelee[38], // RPR
            [181] = JobSetHealer[39], // SGE

            [1] = RoleSet[0], // All Classes
            [30] = RoleSet[14], // Disciple of War
            [31] = RoleSet[14], // Disciple of Magic
            [34] = RoleSet[14], // Disciples of War or Magic

            [32] = RoleSet[5], // Disciple of the Land
            [33] = RoleSet[4], // Disciple of the Hand

            [59] = RoleSet[1], // GLA MRD PLD WAR DRK GNB               Tanks (uld)
            [63] = RoleSet[10], // THM ACN BLM SMN RDM BLU              Casters (uld)
            [64] = RoleSet[2], // CNJ WHM SCH AST SGE                   Healers (uld)
            [66] = RoleSet[9], // ARC BRD MCH DNC                      Ranged (uld)
            [65] = RoleSet[8], // PGL MNK SAM                          MNK,SAM
            [76] = RoleSet[8], // LNC DRG RPR                          DRG,RPR
            [84] = RoleSet[8], // PGL LNC MNK DRG SAM RPR              MNK,DRG,SAM,RPR
            [102] = RoleSet[8], // PGL ROG MNK NIN SAM                 MNK,NIN,SAM
            [105] = RoleSet[9], // ARC ROG BRD NIN MCH DNC             Ranged, NIN

            [56] = RoleSet[14], // GLA CNJ THM PLD WHM BLM               PLD,WHM,BLM
            [57] = RoleSet[12], // GLA THM PLD BLM                       PLD,BLM
            [58] = RoleSet[11], // GLA CNJ PLD WHM                       PLD,WHM
            [60] = RoleSet[12], // GLA MRD LNC PLD WAR DRG DRK GNB RPR   Tanks, DRG,RPR
            [68] = JobSetCaster[25], // ACN SMN SCH
        };

        var categories = LuminaSheets.ClassJobCategorySheet;
        var icons = new BaseIcon[categories.RowCount];
        for (uint i = 0; i < categories.RowCount; ++i)
        {
            if (CategoryIcons.TryGetValue(i, out var icon))
                icons[i] = icon;
        }
        displayedCategories = CategoryIcons.Keys.ToImmutableSortedSet();
        IdOffset = RegisterIcons(icons);
        Log.Debug($"Registering {GetType().Name} to {IdOffset}");
    }

    public override uint? GetMatch(Item item)
    {
        var row = item.LuminaRow.ClassJobCategory.Row;
        if (displayedCategories.Contains(row))
            return IdOffset + row;
        return null;
    }
}
