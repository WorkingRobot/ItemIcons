using Dalamud.Logging;
using ItemIcons.IconTypes;
using System.Collections.Generic;
using System.Numerics;

namespace ItemIcons.IconProviders;

internal sealed class Materia : IconProvider
{
    public override string Name => "Materia Type (Icons)";

    private uint IdOffset { get; }

    private readonly record struct ColorType(bool UseRedIcons, bool UseWhite, Vector3? Add, Vector3? Multiply);

    private static readonly ColorType[] Colors = new ColorType[]
    {
        // new(true, false, new(-408, 61, 33), new(255, 82, 91)), // White from Red (looks like shit, don't use)
        new(false, false, new(109, -84, -396), new(61, 133, 255)), // White - 0
        new(true, false, null, null), // Red - 10
        new(false, true, null, new(1.2f, 0.8f, 0.8f)), // Orange - 20
        new(false, true, null, new(0.8f, 1f, 0.7f)), // Yellow - 30
        new(false, true, new(-80, 0, 0), new(.3f, 1f, 0.3f)), // Green - 40
        new(false, false, null, null), // Blue - 50
        new(false, true, null, new(1f, 0.6f, 1f)), // Purple - 60
        new(false, true, new(0, 70, 100), new(1.2f, 0.5f, 0.8f)), // Pink - 70
    };

    public static readonly ArmoryJob.TextureDescriptor[] IconsRed = new ArmoryJob.TextureDescriptor[]
    {
        new("ui/uld/ItemDetail.tex", new(96, 36, 24, 24)),
        new("ui/uld/ItemDetail.tex", new(72, 36, 24, 24)),
        new("ui/uld/ItemDetail.tex", new(48, 36, 24, 24)),
        new("ui/uld/ItemDetail.tex", new(24, 36, 24, 24)),
        new("ui/uld/ItemDetail.tex", new(156, 24, 24, 24)),
        new("ui/uld/ItemDetail.tex", new(156, 72, 24, 24)),
        new("ui/uld/ItemDetail.tex", new(180, 24, 24, 24)),
        new("ui/uld/ItemDetail.tex", new(180, 72, 24, 24)),
        new("ui/uld/ItemDetail.tex", new(204, 24, 24, 24)),
        new("ui/uld/ItemDetail.tex", new(204, 72, 24, 24)),
    };

    public static readonly ArmoryJob.TextureDescriptor[] IconsBlue = new ArmoryJob.TextureDescriptor[]
    {
        new("ui/uld/ItemDetail.tex", new(96, 12, 24, 24)),
        new("ui/uld/ItemDetail.tex", new(72, 12, 24, 24)),
        new("ui/uld/ItemDetail.tex", new(48, 12, 24, 24)),
        new("ui/uld/ItemDetail.tex", new(24, 12, 24, 24)),
        new("ui/uld/ItemDetail.tex", new(156, 0, 24, 24)),
        new("ui/uld/ItemDetail.tex", new(156, 48, 24, 24)),
        new("ui/uld/ItemDetail.tex", new(180, 0, 24, 24)),
        new("ui/uld/ItemDetail.tex", new(180, 48, 24, 24)),
        new("ui/uld/ItemDetail.tex", new(204, 0, 24, 24)),
        new("ui/uld/ItemDetail.tex", new(204, 48, 24, 24))
    };

    public static readonly Dictionary<uint, int> BaseParamToIconId = new()
    {
        // Red
        [44] = 60 + 10,  // Determination
        [27] = 30 + 8,   // Critical Hit
        [22] = 40 + 6,   // Direct Hit Rate

        // Purple
        [45] = 10 + 10,  // Skill Speed
        [46] = 50 + 7,   // Spell Speed

        // Yellow
        [6] = 70 + 6,   // Piety
        [19] = 20 + 7,   // Tenacity

        // Green
        [10] = 70 + 10,  // GP
        [72] = 50 + 8,   // Gathering
        [73] = 30 + 6,   // Perception

        // Blue
        [11] = 40 + 10,  // CP
        [71] = 60 + 8,   // Control
        [70] = 20 + 6,   // Craftsmanship
    };

    public readonly Dictionary<uint, int> MateriaToIconId = new();

    public readonly List<BaseIcon> Icons = new();

    public Materia()
    {
        foreach(var materia in LuminaSheets.MateriaSheet)
        {
            if (!BaseParamToIconId.TryGetValue(materia.BaseParam.Row, out var iconId))
                continue;
            foreach(var item in materia.Item)
            {
                if (item.Row == 0)
                    continue;
                MateriaToIconId.TryAdd(item.Row, iconId);
            }
        }

        foreach(var color in Colors)
        {
            var iconSet = color.UseRedIcons ? IconsRed : IconsBlue;
            var add = color.Add ?? Vector3.Zero;
            var multiply = color.Multiply ?? Vector3.One;
            if (color.UseWhite) {
                add += Colors[0].Add!.Value;
                multiply *= Colors[0].Multiply!.Value;
            }
            else
                multiply *= new Vector3(100);

            foreach (var icon in iconSet)
                Icons.Add(new TextureIcon(icon.Texture, icon.Rect) { Scale = 4 / 3f, Offset = -3, AddRGB = add, MultiplyRGB = multiply });
        }

        IdOffset = RegisterIcons(Icons);
        PluginLog.Debug($"Registering {GetType().Name} to {IdOffset}");
    }

    public override uint? GetMatch(Item item)
    {
        if (MateriaToIconId.TryGetValue(item.ItemId, out var iconId))
            return ResolveIconId(iconId);

        return null;
    }

    private uint ResolveIconId(int iconId) =>
        (uint)(IdOffset + iconId - 1);
}
