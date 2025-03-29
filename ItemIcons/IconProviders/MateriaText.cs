using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.IconTypes;
using System.Collections.Generic;

namespace ItemIcons.IconProviders;

internal sealed class MateriaText : IconProvider
{
    public override string Name => "Materia Type (Text)";

    public override string Description => "Shows the specific stat increase that a materia will provide.";

    public readonly Dictionary<uint, int> MateriaToIconId = [];

    public readonly Dictionary<uint, string> ParamAbbreviations = new()
    {
        [1] = "STR",
        [2] = "DEX",
        [3] = "VIT",
        [4] = "INT",
        [5] = "MND",
        [6] = "PIE",
        [7] = "HP",
        [8] = "MP",
        [9] = "TP",
        [12] = "DMG",
        [13] = "DMG",
        [19] = "TEN",
        [21] = "DEF",
        [22] = "DH",
        [24] = "MDEF",
        [27] = "CRT",
        [44] = "DET",
        [45] = "SKS",
        [46] = "SPS",
        [10] = "GP",
        [11] = "CP",
        [70] = "CRFT",
        [71] = "CNTL",
        [72] = "GATH",
        [73] = "PERC"
    };

    public MateriaText()
    {
        var icons = new List<BaseIcon>();
        foreach (var materia in LuminaSheets.MateriaSheet)
        {
            if (!ParamAbbreviations.TryGetValue(materia.BaseParam.RowId, out var abbreviation))
                continue;

            var i = -1;
            foreach (var item in materia.Item)
            {
                i++;
                if (item.RowId == 0 || !item.IsValid)
                    continue;
                var increase = materia.Value[i];
                if (increase == 0)
                    continue;
                icons.Add(new TextIcon()
                {
                    Text = $"{abbreviation}\n+{increase}",
                    FontSize = 9,
                    LineSpacing = 9,
                    Alignment = AlignmentType.Center,
                    Flags = TextFlags.Glare | TextFlags.WordWrap | TextFlags.MultiLine | TextFlags.Edge,
                    Offset = 9,
                });
                MateriaToIconId.TryAdd(item.RowId, icons.Count - 1);
            }
        }

        Icons = icons;
        RegisterIcons();
    }

    public override uint? GetMatch(Item item)
    {
        if (MateriaToIconId.TryGetValue(item.ItemId, out var iconId))
            return ResolveIconId(iconId);

        return null;
    }

    private uint ResolveIconId(int iconId) =>
        (uint)(IconOffset + iconId);
}
