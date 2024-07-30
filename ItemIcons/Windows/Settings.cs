using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Linq;
using ItemIcons.IconProviders;
using Dalamud.Interface.Utility.Raii;
using ItemIcons.IconTypes;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;

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

        ItemProviderTypes = [.. Enum.GetValues<ItemProviderCategory>().OrderBy(t => t.ToName())];
        IconProviders = [.. Service.Plugin.Renderer.IconProviders.OrderBy(t => t.Name)];
    }

    private ItemProviderCategory? SelectedItemProvider { get; set; }
    private IconProvider? SelectedIconProvider { get; set; }
    private string? SelectedTab { get; set; }

    public override unsafe void Draw()
    {
        var oldCode = Config.GetHashCode();

        using (var tabs = ImRaii.TabBar("tabs"))
        {
            if (tabs)
            {
                DrawTabItemProviders();
                DrawTabIconProviders();
            }
        }

        if (Config.GetHashCode() != oldCode)
            Config.Save();

        //if (ShowArmoryJobIcons)
        //{
        //    var a = IconProviders.First(p => p is ArmoryJob) as ArmoryJob ?? throw new InvalidOperationException();
        //    DrawIconSet("Gold", a.JobSetGold);
        //    DrawIconSet("Framed", a.JobSetFramed);
        //    DrawIconSet("Glowing", a.JobSetGlowing);
        //    DrawIconSet("Gray", a.JobSetGray);
        //    DrawIconSet("Black", a.JobSetBlack);
        //    DrawIconSet("Yellow", a.JobSetYellow);
        //    DrawIconSet("Orange", a.JobSetOrange);
        //    DrawIconSet("Red", a.JobSetRed);
        //    DrawIconSet("Purple", a.JobSetPurple);
        //    DrawIconSet("Blue", a.JobSetBlue);
        //    DrawIconSet("Green", a.JobSetGreen);
        //    ImGui.Separator();
        //    DrawIconSet("Icons", a.RoleSetIcons);
        //    DrawIconSet("Square", a.RoleSetSquare);
        //    DrawIconSet("Rounded", a.RoleSetRounded);
        //    DrawIconSet("Mini", a.RoleSetMini);
        //    DrawIconSet("Roles", a.RoleSet);
        //    ImGui.Separator();
        //    DrawAllIcons(a);
        //}
    }
    
    private ImRaii.IEndObject TabItem(string label)
    {
        var isSelected = string.Equals(SelectedTab, label, StringComparison.Ordinal);
        if (isSelected)
        {
            SelectedTab = null;
            var open = true;
            return ImRaii.TabItem(label, ref open, ImGuiTabItemFlags.SetSelected);
        }
        return ImRaii.TabItem(label);
    }

    private void DrawTabItemProviders()
    {
        using var tab = TabItem("Item Providers");
        if (!tab)
            return;

        using (var table = ImRaii.Table("providers", 2, ImGuiTableFlags.Resizable))
        {
            if (table)
            {
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 250);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableNextColumn();
                foreach (var provider in ItemProviderTypes)
                {
                    var isEnabled = true;
                    if (Config.ItemProviders.TryGetValue(provider, out var providerConfig))
                        isEnabled = providerConfig.Enabled;

                    using var color = ImRaii.PushColor(ImGuiCol.Text, isEnabled ? new Vector4(0, .8f, 0, 1) : new Vector4(.8f, 0, 0, 1));

                    var name = provider.ToName();
                    if (ImGui.Selectable(name, provider == SelectedItemProvider))
                    {
                        SelectedItemProvider = provider;
                        if (!Config.ItemProviders.ContainsKey(provider))
                            Config.ItemProviders[provider] = new();
                    }
                }
                ImGui.Dummy(new(0, ImGui.GetContentRegionAvail().Y - ImGui.GetStyle().CellPadding.Y));
                ImGui.TableNextColumn();

                ImGui.Text($"{SelectedItemProvider?.ToName()}");
                if (SelectedItemProvider.HasValue)
                {
                    var itemConfig = Config.ItemProviders[SelectedItemProvider.Value];

                    var b = itemConfig.Enabled;
                    if (ImGui.Checkbox("Enabled", ref b))
                        itemConfig.Enabled = b;

                    using var disabled = ImRaii.Disabled(!itemConfig.Enabled);

                    b = itemConfig.ShowOnlyOne;
                    if (ImGui.Checkbox("Show Only One Icon", ref b))
                        itemConfig.ShowOnlyOne = b;

                    ImGui.Separator();
                    foreach (var provider in IconProviders)
                    {
                        var typeName = provider.GetType().FullName!;

                        var iconEnabled = Config.IconProviders.GetValueOrDefault(typeName, true);
                        using var _disabled = ImRaii.Disabled(!iconEnabled);

                        iconEnabled = iconEnabled && itemConfig.IconProviders.GetValueOrDefault(typeName, true);
                        if (ImGui.Checkbox(provider.Name, ref iconEnabled))
                            itemConfig.IconProviders[typeName] = iconEnabled;
                    }
                }
            }
        }
    }

    private void DrawTabIconProviders()
    {
        using var tab = TabItem("Icon Providers");
        if (!tab)
            return;

        using (var table = ImRaii.Table("providers", 2, ImGuiTableFlags.Resizable))
        {
            if (table)
            {
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 250);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableNextColumn();
                foreach (var iconProvider in IconProviders)
                {
                    if (!Config.IconProviders.TryGetValue(iconProvider.GetType().FullName!, out var isEnabled))
                        isEnabled = true;
                    using var color = ImRaii.PushColor(ImGuiCol.Text, isEnabled ? new Vector4(0, .8f, 0, 1) : new Vector4(.8f, 0, 0, 1));
                    if (ImGui.Selectable(iconProvider.Name, iconProvider == SelectedIconProvider))
                    {
                        Config.IconProviders.TryAdd(iconProvider.GetType().FullName!, true);
                        SelectedIconProvider = iconProvider;
                    }
                }
                ImGui.Dummy(new(0, ImGui.GetContentRegionAvail().Y - ImGui.GetStyle().CellPadding.Y));
                ImGui.TableNextColumn();

                ImGui.Text($"{SelectedIconProvider?.Name}");
                if (SelectedIconProvider is { } provider)
                {
                    var name = provider.GetType().FullName!;
                    var b = Config.IconProviders[name];
                    if (ImGui.Checkbox("Enabled", ref b))
                        Config.IconProviders[name] = b;

                    ImGui.Separator();
                    
                    ImGui.TextWrapped(provider.Description);

                    ImGui.Separator();

                    ImGui.Text("Icons");

                    using var c = ImRaii.Child("icons", new(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y - ImGui.GetStyle().CellPadding.Y));
                    if (c)
                    {
                        //var gap = ImGui.GetCursorPosX() - ImGui.GetContentRegionAvail().X;
                        var i = 0;
                        foreach(var icon in provider.Icons.Distinct())
                        {
                            if (icon is null)
                                continue;
                            using (var child = ImRaii.Child($"icon{i}{icon?.IconId}", new(32), false, ImGuiWindowFlags.NoScrollbar))
                                DrawIcon(icon, new(32));
                            ImGui.SameLine(0, 0);
                            if (ImGui.GetContentRegionAvail().X < 32)
                                ImGui.Dummy(default);
                            i++;
                        }
                    }
                }
            }
        }
    }

    //private static void DrawIconSet(string name, ArmoryJob.IconSet set)
    //{
    //    if (ImGui.CollapsingHeader($"{name}##jobSet"))
    //    {
    //        using var t = ImRaii.Table("jobTable", 10);
    //        if (t)
    //        {
    //            for (var i = 0; i < set.Count; ++i)
    //            {
    //                ImGui.TableNextColumn();
    //                DrawIcon(set.TryGet(i));
    //            }
    //        }
    //    }
    //}

    //private static void DrawAllIcons(ArmoryJob job)
    //{
    //    if (ImGui.CollapsingHeader($"categoryIcons##jobSet"))
    //    {
    //        using var t = ImRaii.Table("jobTable", 6);
    //        if (t)
    //        {
    //            foreach(var row in LuminaSheets.ClassJobCategorySheet)
    //            {
    //                if (job.CategoryIcons.TryGetValue(row.RowId, out var icon))
    //                {
    //                    ImGui.TableNextColumn();
    //                    ImGui.TextUnformatted(row.Name.ToDalamudString().ToString());
    //                    ImGui.TableNextColumn();
    //                    DrawIcon(icon);
    //                }
    //            }
    //        }
    //    }
    //}

    private static Dictionary<GameFontStyle, IFontHandle> CachedFonts { get; } = [];
    private static void DrawIcon(BaseIcon? icon, Vector2 size)
    {
        var pos = ImGui.GetCursorScreenPos();
        var isHovered = ImGui.IsMouseHoveringRect(pos, pos + size);

        if (icon is TextureIcon { } texture)
        {
            var tex = Service.IconManager.GetTextureCached(texture.Texture);
            Vector2 uv0 = new(), uv1 = new(1);
            if (texture.Rect is { } rect)
            {
                uv0 = new(rect.U, rect.V);
                uv1 = new(rect.Width, rect.Height);
                uv1 += uv0;
                if (tex.Dimensions is { } dim)
                {
                    uv0 /= dim;
                    uv1 /= dim;
                }
            }
            ImGui.Image(tex.ImGuiHandle, size, uv0, uv1, new Vector4(texture.MultiplyRGB, 1));
            if (isHovered)
                ImGui.SetTooltip($"Id: {texture.IconId}\nScale: {texture.Scale}\nAddRGB: {texture.AddRGB}\nMultiplyRGB: {texture.MultiplyRGB}\nOffset: {texture.Offset}\nTexture: {texture.Texture}\nRect: {texture.Rect}");
        }
        else if (icon is TextIcon { } text)
        {
            var style = new GameFontStyle(text.FontType switch
            {
                FontType.Axis => GameFontFamily.Axis,
                FontType.MiedingerMed => GameFontFamily.MiedingerMid,
                FontType.Miedinger => GameFontFamily.Meidinger,
                FontType.TrumpGothic => GameFontFamily.TrumpGothic,
                FontType.Jupiter => GameFontFamily.Jupiter,
                FontType.JupiterLarge => GameFontFamily.JupiterNumeric,
                _ => GameFontFamily.Axis
            }, text.FontSize);
            if (!CachedFonts.TryGetValue(style, out var font))
                CachedFonts[style] = font = Service.PluginInterface.UiBuilder.FontAtlas.NewGameFontHandle(style);
            {
                using var _ = font.Push();
                using var color = ImRaii.PushColor(ImGuiCol.Text, text.TextColor);
                ImGui.TextUnformatted(text.Text.TextValue);
            }
            if (isHovered)
                ImGui.SetTooltip($"Id: {text.IconId}\nScale: {text.Scale}\nAddRGB: {text.AddRGB}\nMultiplyRGB: {text.MultiplyRGB}\nOffset: {text.Offset}\nText: {text.Text}\nFont: {text.FontType} {text.FontSize}px\nLine Spacing: {text.LineSpacing}\nColors: ({text.TextColor}, {text.EdgeColor}, {text.BackgroundColor})\nAlignment: {text.Alignment}\nFlags: {text.Flags}; {text.Flags2}");
        }
        else
        {
            ImGui.TextUnformatted($"???");
            if (isHovered)
                ImGui.SetTooltip($"Id: {icon?.IconId}\n{icon}");
        }
    }
}
