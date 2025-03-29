using ItemIcons.Utils;
using Lumina;
using LuminaSupplemental.Excel.Model;
using LuminaSupplemental.Excel.Services;
using System.Collections.Generic;

namespace ItemIcons;

public static class Supplemental
{
    private static GameData GameData { get; }

    public static List<DungeonBoss> DungeonBosses { get; }
    public static List<DungeonBossDrop> DungeonBossDrops { get; }
    public static List<DungeonBossChest> DungeonBossChests { get; }
    public static List<ItemSupplement> ItemSupplements { get; }
    public static List<SubmarineDrop> SubmarineDrops { get; }
    public static List<AirshipDrop> AirshipDrops { get; }
    public static List<DungeonChestItem> DungeonChestItems { get; }
    public static List<DungeonDrop> DungeonDrops { get; }
    public static List<DungeonChest> DungeonChests { get; }
    public static List<MobSpawnPosition> MobSpawns { get; }
    public static List<ENpcPlace> ENpcPlaces { get; }
    public static List<ENpcShop> ENpcShops { get; }
    public static List<ShopName> ShopNames { get; }
    public static List<AirshipUnlock> AirshipUnlocks { get; }
    public static List<SubmarineUnlock> SubmarineUnlocks { get; }
    public static List<ItemPatch> ItemPatches { get; }
    public static List<RetainerVentureItem> RetainerVentureItems { get; }
    public static List<StoreItem> StoreItems { get; }
    public static List<MobDrop> MobDrops { get; }
    public static List<HouseVendor> HouseVendors { get; }

    static Supplemental()
    {
        GameData = Service.DataManager.GameData;

        DungeonBosses = LoadCsv<DungeonBoss>(CsvLoader.DungeonBossResourceName); // Dungeon Boss
        DungeonBossChests = LoadCsv<DungeonBossChest>(CsvLoader.DungeonBossChestResourceName); // Dungeon Boss Chests
        DungeonBossDrops = LoadCsv<DungeonBossDrop>(CsvLoader.DungeonBossDropResourceName); // Dungeon Boss Drops
        DungeonChestItems = LoadCsv<DungeonChestItem>(CsvLoader.DungeonChestItemResourceName); // Dungeon Chest Items
        DungeonChests = LoadCsv<DungeonChest>(CsvLoader.DungeonChestResourceName); // Dungeon Chests
        DungeonDrops = LoadCsv<DungeonDrop>(CsvLoader.DungeonDropItemResourceName); // Dungeon Chest Items
        ItemSupplements = LoadCsv<ItemSupplement>(CsvLoader.ItemSupplementResourceName); // Item Supplement
        MobDrops = LoadCsv<MobDrop>(CsvLoader.MobDropResourceName); // Mob Drops
        SubmarineDrops = LoadCsv<SubmarineDrop>(CsvLoader.SubmarineDropResourceName); // Submarine Drops
        AirshipDrops = LoadCsv<AirshipDrop>(CsvLoader.AirshipDropResourceName); // Airship Drops
        MobSpawns = LoadCsv<MobSpawnPosition>(CsvLoader.MobSpawnResourceName); // Mob Spawns
        ENpcPlaces = LoadCsv<ENpcPlace>(CsvLoader.ENpcPlaceResourceName); // ENpc Places
        ENpcShops = LoadCsv<ENpcShop>(CsvLoader.ENpcShopResourceName); // ENpc Shops
        ShopNames = LoadCsv<ShopName>(CsvLoader.ShopNameResourceName); // Shop Names
        AirshipUnlocks = LoadCsv<AirshipUnlock>(CsvLoader.AirshipUnlockResourceName); // Airship Unlocks
        SubmarineUnlocks = LoadCsv<SubmarineUnlock>(CsvLoader.SubmarineUnlockResourceName); // Submarine Unlocks
        ItemPatches = LoadCsv<ItemPatch>(CsvLoader.ItemPatchResourceName); // Item Patches
        RetainerVentureItems = LoadCsv<RetainerVentureItem>(CsvLoader.RetainerVentureItemResourceName); // Retainer Ventures
        StoreItems = LoadCsv<StoreItem>(CsvLoader.StoreItemResourceName); // SQ Store Items
        HouseVendors = LoadCsv<HouseVendor>(CsvLoader.HouseVendorResourceName); // House Vendors
    }

    private static List<T> LoadCsv<T>(string name) where T : ICsv, new()
    {
        var ret = CsvLoader.LoadResource<T>(name, true, out _, out var exceptions, GameData, GameData.Options.DefaultExcelLanguage);
        foreach (var e in  exceptions)
            Log.Error(e, $"Error loading csv {name}");
        return ret!;
    }
}
