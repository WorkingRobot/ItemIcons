using Lumina.Excel;
using Lumina.Excel.Sheets;
using LuminaItem = Lumina.Excel.Sheets.Item;

namespace ItemIcons;

public static class LuminaSheets
{
    public static readonly ExcelSheet<LuminaItem> ItemSheet = Service.DataManager.GetExcelSheet<LuminaItem>()!;
    public static readonly ExcelSheet<Cabinet> CabinetSheet = Service.DataManager.GetExcelSheet<Cabinet>()!;
    public static readonly ExcelSheet<CabinetCategory> CabinetCategorySheet = Service.DataManager.GetExcelSheet<CabinetCategory>()!;
    public static readonly ExcelSheet<ClassJobCategory> ClassJobCategorySheet = Service.DataManager.GetExcelSheet<ClassJobCategory>()!;
    public static readonly ExcelSheet<Recipe> RecipeSheet = Service.DataManager.GetExcelSheet<Recipe>()!;
    public static readonly ExcelSheet<SpecialShop> SpecialShopSheet = Service.DataManager.GetExcelSheet<SpecialShop>()!;
    public static readonly ExcelSheet<HousingFurniture> HousingFurnitureSheet = Service.DataManager.GetExcelSheet<HousingFurniture>()!;
    public static readonly ExcelSheet<Materia> MateriaSheet = Service.DataManager.GetExcelSheet<Materia>()!;
    public static readonly ExcelSheet<ItemLevel> ItemLevelSheet = Service.DataManager.GetExcelSheet<ItemLevel>()!;
}
