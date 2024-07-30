using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Utility.Signatures;
using ItemIcons.Windows;
using System;
using FFXIVClientStructs.FFXIV.Client.UI;
using ItemIcons.Utils;
using System.Reflection;
using ItemIcons.IconProviders;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Utility;
using System.Linq;

namespace ItemIcons;

public sealed class Plugin : IDalamudPlugin
{
    public WindowSystem WindowSystem { get; }

    private Settings SettingsWindow { get; }
    public IconRenderer Renderer { get; }
    public IconManager IconManager { get; }

    [Signature("E8 ?? ?? ?? ?? 8B 8F ?? ?? ?? ?? 8B D1", DetourName = nameof(AddonSetupDetour))]
    private readonly Hook<AddonSetupDelegate> addonSetupHook = null!;
    [Signature("E8 ?? ?? ?? ?? 48 83 EF 01 75 D5", DetourName = nameof(AddonFinalizeDetour))]
    private readonly Hook<AddonFinalizeDelegate> addonFinalizeHook = null!;

    private unsafe delegate void* AddonSetupDelegate(AtkUnitBase* addon);
    private unsafe delegate void AddonFinalizeDelegate(AtkUnitManager* manager, AtkUnitBase** addon);

    // Client::UI::UIModule.Update
    [Signature("48 8B C4 41 56 48 83 EC 60 FF 81", DetourName = nameof(UIModuleUpdateDetour))]
    private readonly Hook<UIModuleUpdateDelegate> uiModuleUpdateHook = null!;
    private unsafe delegate bool UIModuleUpdateDelegate(UIModule* module, float frameDelta);

    [Signature("E8 ?? ?? ?? ?? 40 84 F6 74 2A")]
    public readonly AtkUldManagerUpdateNodeTreeDelegate atkUldManagerUpdateNodeTree = null!;
    public unsafe delegate void AtkUldManagerUpdateNodeTreeDelegate(AtkUldManager* manager, AtkResNode* node, AtkResNode* nodeParent, bool isShallowUpdate);

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        Service.Initialize(this, pluginInterface);

        try
        {
            Service.Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new();
        }
        catch (TargetInvocationException ex)
        {
            Log.Error(ex.InnerException!, "Error while loading config. Using default config.");
            Service.Configuration = new();
        }
        
        WindowSystem = new();

        Renderer = new();
        SettingsWindow = new();
        IconManager = new();

        Service.ContextMenu.OnMenuOpened += OnMenuOpened;

        Service.CommandManager.AddHandler("/ii", new((_, _) => OpenSettingsWindow()) { HelpMessage = "Open the Item Icons settings window." });

        Service.PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        Service.PluginInterface.UiBuilder.OpenConfigUi += OpenSettingsWindow;

        Service.GameInteropProvider.InitializeFromAttributes(this);
        addonSetupHook.Enable();
        addonFinalizeHook.Enable();

        uiModuleUpdateHook.Enable();

        HashSet<uint> rows = [];
        foreach (var item in LuminaSheets.ItemSheet)
        {
            if (item.ClassJobCategory.Value is { } cat)
            {
                if (rows.Add(cat.RowId))
                    Log.Debug($"[{cat.RowId}] = {cat.Name.ToDalamudString().TextValue}");
            }
        }

        var j = (ArmoryJob)Renderer.IconProviders.First(p => p is ArmoryJob);
        var knownCats = j.CategoryIcons.Keys.ToHashSet();
        foreach (var cat in rows)
        {
            if (!knownCats.Contains(cat))
                Log.Debug($"Unknown category: {cat}");
        }
        foreach (var cat in knownCats)
        {
            if (!rows.Contains(cat))
                Log.Debug($"Unused category: {cat}");
        }
    }

    public void Dispose()
    {
        uiModuleUpdateHook?.Dispose();

        addonSetupHook?.Dispose();
        addonFinalizeHook?.Dispose();

        Service.PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;

        Service.CommandManager.RemoveHandler("/ii");

        WindowSystem.RemoveAllWindows();
        
        Renderer.Dispose();
        IconManager.Dispose();
    }

    private unsafe void* AddonSetupDetour(AtkUnitBase* addon)
    {
        var ret = addonSetupHook.Original(addon);
        Renderer.SetupAddon(addon);
        return ret;
    }

    private unsafe void AddonFinalizeDetour(AtkUnitManager* manager, AtkUnitBase** addonPtr)
    {
        Renderer.FinalizeAddon(*addonPtr);
        addonFinalizeHook.Original(manager, addonPtr);
    }

    private unsafe bool UIModuleUpdateDetour(UIModule* module, float frameDelta)
    {
        var ret = uiModuleUpdateHook.Original(module, frameDelta);
        Render();
        return ret;
    }

    private void Render()
    {
        try
        {
            Renderer.Draw();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while drawing icons");
        }
    }

    private void OnMenuOpened(IMenuOpenedArgs args)
    {
        if (!(args.Target is MenuTargetInventory { TargetItem: { } item }))
            return;

        if (!Service.Configuration.IsIconProviderEnabled(typeof(Favorites)))
            return;

        if (item.IsEmpty)
            return;

        args.AddMenuItem(new()
        {
            PrefixChar = 'I',
            PrefixColor = 541,
            Name = "Favorite",
            OnClicked = OpenFavoriteSubmenu,
            IsSubmenu = true,
        });
    }

    private void OpenFavoriteSubmenu(IMenuItemClickedArgs args)
    {
        if (!(args.Target is MenuTargetInventory { TargetItem: { } item }))
            return;

        var items = new List<MenuItem>();

        var hasId = Service.Configuration.FavoritedItems.TryGetValue(item.ItemId, out var existingId);
        BitmapFontIcon? favoritedIcon = hasId ? (existingId < Favorites.IconIds.Length ? Favorites.IconIds[existingId] : null) : null;

        uint itemIdx = 0;
        foreach(var icon in Favorites.IconIds)
        {
            var b = new SeStringBuilder();
            b.AddIcon(icon);
            if (favoritedIcon == icon)
                b.AddUiGlow($" (Remove)", 14);
            var idx = itemIdx;
            items.Add(new()
            {
                Name = b.Build(),
                OnClicked = _ => SetFavoriteForItem(item.ItemId, idx),
            });
            itemIdx++;
        }

        args.OpenSubmenu("Favorite", items);
    }

    private void SetFavoriteForItem(uint itemId, uint itemIdx)
    {
        if (Service.Configuration.FavoritedItems.TryGetValue(itemId, out var existingId) && existingId == itemIdx)
            Service.Configuration.FavoritedItems.Remove(itemId);
        else
            Service.Configuration.FavoritedItems[itemId] = itemIdx;
        Service.Configuration.Save();
    }

    public void OpenSettingsWindow() =>
        SettingsWindow.IsOpen = true;
}
