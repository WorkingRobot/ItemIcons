using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Linq;
using ItemIcons.IconProviders;
using Dalamud.Interface.Utility.Raii;
using ItemIcons.IconTypes;
using Dalamud.Utility;

namespace ItemIcons.Windows;

public class Settings : Window
{
    private static Configuration Config => Service.Configuration;

    private ItemProviderCategory[] ItemProviderTypes { get; }
    private IconProvider[] IconProviders { get; }

    private const bool ShowArmoryJobIcons = false;

    public Settings() : base("Item Icons Settings")
    {
        Service.WindowSystem.AddWindow(this);

        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new Vector2(400, 400),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        Size = SizeConstraints.Value.MinimumSize;
        SizeCondition = ImGuiCond.Appearing;

        ItemProviderTypes = Enum.GetValues<ItemProviderCategory>().OrderBy(t => t.ToName()).ToArray();
        IconProviders = Service.Plugin.Renderer.IconProviders.OrderBy(t => t.Name).ToArray();
    }

    private ItemProviderCategory? selectedItemProvider { get; set; }

    public override unsafe void Draw()
    {
        var isDirty = false;

        using (var table = ImRaii.Table("ItemIconsSettings", 2, ImGuiTableFlags.Resizable))
        {
            if (table)
            {
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 250);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableNextColumn();
                foreach (var provider in ItemProviderTypes)
                {
                    var name = provider.ToName();
                    if (ImGui.Selectable(name, provider == selectedItemProvider))
                    {
                        selectedItemProvider = provider;
                        if (!Config.ItemProviders.ContainsKey(provider))
                            Config.ItemProviders[provider] = new();
                    }
                }
                ImGui.TableNextColumn();

                ImGui.Text($"{selectedItemProvider?.ToName()}");
                if (selectedItemProvider.HasValue)
                {
                    var itemConfig = Config.ItemProviders[selectedItemProvider.Value];

                    var b = itemConfig.Enabled;
                    if (ImGui.Checkbox("Enabled", ref b))
                    {
                        itemConfig.Enabled = b;
                        isDirty = true;
                    }

                    using var disabled = ImRaii.Disabled(!itemConfig.Enabled);

                    b = itemConfig.ShowOnlyOne;
                    if (ImGui.Checkbox("Show Only One Icon", ref b))
                    {
                        itemConfig.ShowOnlyOne = b;
                        isDirty = true;
                    }

                    ImGui.Separator();
                    foreach (var provider in IconProviders)
                    {
                        var typeName = provider.GetType().FullName!;
                        if (!itemConfig.IconProviders.TryGetValue(typeName, out var iconEnabled))
                            iconEnabled = true;
                        if (ImGui.Checkbox(provider.Name, ref iconEnabled))
                        {
                            itemConfig.IconProviders[typeName] = iconEnabled;
                            isDirty = true;
                        }
                    }
                }
            }
        }

        if (isDirty)
            Config.Save();

        if (ShowArmoryJobIcons)
        {
            var a = IconProviders.First(p => p is ArmoryJob) as ArmoryJob ?? throw new InvalidOperationException();
            DrawIconSet("Gold", a.JobSetGold);
            DrawIconSet("Framed", a.JobSetFramed);
            DrawIconSet("Glowing", a.JobSetGlowing);
            DrawIconSet("Gray", a.JobSetGray);
            DrawIconSet("Black", a.JobSetBlack);
            DrawIconSet("Yellow", a.JobSetYellow);
            DrawIconSet("Orange", a.JobSetOrange);
            DrawIconSet("Red", a.JobSetRed);
            DrawIconSet("Purple", a.JobSetPurple);
            DrawIconSet("Blue", a.JobSetBlue);
            DrawIconSet("Green", a.JobSetGreen);
            ImGui.Separator();
            DrawIconSet("Icons", a.RoleSetIcons);
            DrawIconSet("Square", a.RoleSetSquare);
            DrawIconSet("Rounded", a.RoleSetRounded);
            DrawIconSet("Mini", a.RoleSetMini);
            DrawIconSet("Roles", a.RoleSet);
            ImGui.Separator();
            DrawAllIcons(a);
        }
    }

    private static void DrawIconSet(string name, ArmoryJob.IconSet set)
    {
        if (ImGui.CollapsingHeader($"{name}##jobSet"))
        {
            using var t = ImRaii.Table("jobTable", 10);
            if (t)
            {
                for (var i = 0; i < set.Count; ++i)
                {
                    ImGui.TableNextColumn();
                    DrawIcon(set.TryGet(i));
                }
            }
        }
    }

    private static void DrawAllIcons(ArmoryJob job)
    {
        if (ImGui.CollapsingHeader($"categoryIcons##jobSet"))
        {
            using var t = ImRaii.Table("jobTable", 6);
            if (t)
            {
                foreach(var row in LuminaSheets.ClassJobCategorySheet)
                {
                    if (job.CategoryIcons.TryGetValue(row.RowId, out var icon))
                    {
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(row.Name.ToDalamudString().ToString());
                        ImGui.TableNextColumn();
                        DrawIcon(icon);
                    }
                }
            }
        }
    }

    private static void DrawIcon(BaseIcon? icon)
    {
        if (icon is TextureIcon { } texture)
        {
            var tex = Service.IconManager.GetTexture(texture.Texture);
            Vector2 uv0 = new(), uv1 = new(1);
            if (texture.Rect is { } rect)
            {
                uv0 = new(rect.U, rect.V);
                uv1 = new(rect.Width, rect.Height);
                uv1 += uv0;
                uv0 /= tex.Size;
                uv1 /= tex.Size;
            }
            ImGui.Image(tex.ImGuiHandle, new(64), uv0, uv1);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip($"Texture: {texture.Texture}\nRect: {texture.Rect}\nScale: {texture.Scale}");
        }
        else
        {
            ImGui.TextUnformatted($"Unknown\n{icon}");
        }
    }
}
