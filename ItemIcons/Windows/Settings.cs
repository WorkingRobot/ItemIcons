using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Linq;
using ItemIcons.IconProviders;
using Dalamud.Interface.Utility.Raii;

namespace ItemIcons.Windows;

public class Settings : Window
{
    private static Configuration Config => Service.Configuration;

    private ItemProviderCategory[] ItemProviderTypes { get; }
    private IconProvider[] IconProviders { get; }

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

        using var table = ImRaii.Table("ItemIconsSettings", 2, ImGuiTableFlags.Resizable);
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

        if (isDirty)
            Config.Save();
    }
}
