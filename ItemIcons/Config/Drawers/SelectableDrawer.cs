using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ItemIcons.Config.Drawers;

public abstract class SelectableDrawer<K, V> : ConfigDrawer<Dictionary<K, V>> where K : notnull
{
    public abstract IReadOnlyList<(string Name, K Key, ConfigDrawState<V> State)> Keys { get; }
    private bool IntializedKeys { get; set; }

    public int? SelectedKeyIdx { get; set; }

    public override bool Draw(ref Dictionary<K, V> value, ConfigAttribute config, Stack<object?> parents)
    {
        if (!IntializedKeys)
        {
            if (config.ValueAttribute is { } attribute)
            {
                foreach (var (_, _, state) in Keys)
                    state.InjectValueAttribute(attribute);
            }
            IntializedKeys = true;
        }
        using var id = ImRaii.PushId(config.Name);
        using var push = parents.PushRaii(value);
        using var table = ImRaii.Table("selectable", 2, ImGuiTableFlags.Resizable);
        if (!table)
            return false;

        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 250);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableNextColumn();
        for (var i = 0; i < Keys.Count; i++)
        {
            if (ImGui.Selectable(Keys[i].Name, i == SelectedKeyIdx))
                SelectedKeyIdx = i;
        }
        ImGui.Dummy(new(0, ImGui.GetContentRegionAvail().Y - ImGui.GetStyle().CellPadding.Y));
        ImGui.TableNextColumn();

        if (SelectedKeyIdx is { } idx)
        {
            var (keyName, key, state) = Keys[idx];
            // TODO: center text
            ImGui.Text($"{keyName}");

            if (!value.ContainsKey(key))
            {
                var val = default(V) ?? Activator.CreateInstance<V>();
                if (config.ValueAttribute?.ConstructorInstance is INewConstructor c)
                    val = c.New<V>();
                value.Add(key, val);
            }
            using var pushKey = parents.PushRaii(key);
            return state.Draw(ref CollectionsMarshal.GetValueRefOrAddDefault(value, key, out _)!, parents);
        }

        return false;
    }
}
