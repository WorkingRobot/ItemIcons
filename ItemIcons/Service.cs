using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Storage.Assets;
using ItemIcons.Utils;

namespace ItemIcons;

public sealed class Service
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] public static ICommandManager CommandManager { get; private set; }
    [PluginService] public static IGameGui GameGui { get; private set; }
    [PluginService] public static IDataManager DataManager { get; private set; }
    [PluginService] public static IDalamudAssetManager DalamudAssetManager { get; private set; }
    [PluginService] public static ITextureProvider TextureProvider { get; private set; }
    [PluginService] public static IGameInteropProvider GameInteropProvider { get; private set; }
    [PluginService] public static IPluginLog PluginLog { get; private set; }
    [PluginService] public static IContextMenu ContextMenu { get; private set; }

    public static Plugin Plugin { get; internal set; }
    public static Configuration Configuration { get; internal set; }
    public static WindowSystem WindowSystem => Plugin.WindowSystem;
    public static IconManager IconManager => Plugin.IconManager;
#pragma warning restore CS8618

    internal static void Initialize(Plugin plugin, IDalamudPluginInterface iface)
    {
        Plugin = plugin;
        iface.Create<Service>();
    }
}
