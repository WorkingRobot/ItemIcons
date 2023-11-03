using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.IconTypes;
using ItemIcons.Utils;
using System.Collections.Generic;
using System.Numerics;

namespace ItemIcons.IconProviders;

internal sealed class MateriaText : IconProvider
{
    public override string Name => "Materia Type (Text)";

    private uint IdOffset { get; }

    public readonly Dictionary<uint, int> MateriaToIconId = new();

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

    public readonly List<BaseIcon> Icons = new();

    public MateriaText()
    {
        foreach (var materia in LuminaSheets.MateriaSheet)
        {
            if (!ParamAbbreviations.TryGetValue(materia.BaseParam.Row, out var abbreviation))
                continue;

            var i = -1;
            foreach (var item in materia.Item)
            {
                i++;
                if (item.Row == 0)
                    continue;
                var increase = materia.Value[i];
                if (increase == 0)
                    continue;
                Icons.Add(new TextIcon()
                {
                    Text = $"{abbreviation}\n+{increase}",
                    FontSize = 9,
                    LineSpacing = 9,
                    Alignment = AlignmentType.Center,
                    Flags = TextFlags.Glare | TextFlags.WordWrap | TextFlags.MultiLine | TextFlags.Edge,
                    Offset = 9,
                });
                MateriaToIconId.TryAdd(item.Row, Icons.Count - 1);
            }
        }

        IdOffset = RegisterIcons(Icons);
        Log.Debug($"Registering {GetType().Name} to {IdOffset}");
    }

    public override uint? GetMatch(Item item)
    {
        if (MateriaToIconId.TryGetValue(item.ItemId, out var iconId))
            return ResolveIconId(iconId);

        return null;
    }

    private uint ResolveIconId(int iconId) =>
        (uint)(IdOffset + iconId);
}
