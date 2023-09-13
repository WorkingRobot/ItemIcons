using Lumina.Excel.GeneratedSheets;
using Lumina.Excel;
using Lumina;
using Lumina.Data;
using LuminaItem = Lumina.Excel.GeneratedSheets.Item;
using Lumina.Text;

namespace ItemIcons;

#nullable disable
[Sheet("SpecialShop", columnHash: 0xd789c459)]
public class SpecialShop : ExcelRow
{
    public class SpecialShopEntry
    {
        public SpecialShopRecieve[] Recieve { get; set; }
        public SpecialShopCost[] Cost { get; set; }
        public LazyRow<Quest> QuestItem { get; set; }
        public int Unknown0 { get; set; }
        public byte Unknown1 { get; set; }
        public byte Unknown2 { get; set; }
        public LazyRow<Achievement> AchievementUnlock { get; set; }
        public byte Unknown3 { get; set; }
        public ushort PatchNumber { get; set; }

        public SpecialShopEntry(int i, RowParser parser, GameData gameData, Language language)
        {
            var n = 1;
            Recieve = new SpecialShopRecieve[2];
            Recieve[0] = new SpecialShopRecieve(n, i, parser, gameData, language); n += 240;
            Recieve[1] = new SpecialShopRecieve(n, i, parser, gameData, language); n += 240;

            Cost = new SpecialShopCost[3];
            Cost[0] = new SpecialShopCost(n, i, parser, gameData, language); n += 240;
            Cost[1] = new SpecialShopCost(n, i, parser, gameData, language); n += 240;
            Cost[2] = new SpecialShopCost(n, i, parser, gameData, language); n += 240;

            QuestItem = new LazyRow<Quest>(gameData, parser.ReadColumn<int>(n + i), language); n += 60;

            Unknown0 = parser.ReadColumn<int>(n + i); n += 60;
            Unknown1 = parser.ReadColumn<byte>(n + i); n += 60;
            Unknown2 = parser.ReadColumn<byte>(n + i); n += 60;

            AchievementUnlock = new LazyRow<Achievement>(gameData, parser.ReadColumn<int>(n + i), language); n += 60;

            Unknown3 = parser.ReadColumn<byte>(n + i); n += 60;

            PatchNumber = parser.ReadColumn<ushort>(n + i);
        }
    }

    public class SpecialShopRecieve
    {
        public LazyRow<LuminaItem> Item { get; set; }
        public uint Count { get; set; }
        public LazyRow<SpecialShopItemCategory> Category { get; set; }
        public bool HQ { get; set; }

        public SpecialShopRecieve(int offset, int i, RowParser parser, GameData gameData, Language language)
        {
            Item = new LazyRow<LuminaItem>(gameData, parser.ReadColumn<int>(offset + i), language);
            Count = parser.ReadColumn<uint>(offset + 60 + i);
            Category = new LazyRow<SpecialShopItemCategory>(gameData, parser.ReadColumn<int>(offset + 120 + i), language);
            HQ = parser.ReadColumn<bool>(offset + 180 + i);
        }
    }

    public class SpecialShopCost
    {
        public LazyRow<LuminaItem> Item { get; set; }
        public uint Count { get; set; }
        public bool HQ { get; set; }
        public ushort CollectabilityRating { get; set; }

        public SpecialShopCost(int offset, int i, RowParser parser, GameData gameData, Language language)
        {
            Item = new LazyRow<LuminaItem>(gameData, parser.ReadColumn<int>(offset + i), language);
            Count = parser.ReadColumn<uint>(offset + 60 + i);
            HQ = parser.ReadColumn<byte>(offset + 120 + i) != 0;
            CollectabilityRating = parser.ReadColumn<ushort>(offset + 180 + i);
        }
    }

    public SeString Name { get; set; }
    public SpecialShopEntry[] Entries { get; set; }
    public byte UseCurrencyType { get; set; }
    public LazyRow<Quest> QuestUnlock { get; set; }
    public LazyRow<DefaultTalk> CompleteText { get; set; }
    public LazyRow<DefaultTalk> NotCompleteText { get; set; }
    public uint Unknown0 { get; set; }
    public bool Unknown1 { get; set; }
    public ushort Unknown2 { get; set; }
    public uint Unknown3 { get; set; }
    public bool Unknown4 { get; set; }

    public override void PopulateData(RowParser parser, GameData gameData, Language language)
    {
        Name = parser.ReadColumn<SeString>(0);
        Entries = new SpecialShopEntry[60];
        for (var i = 0; i < 60; i++)
            Entries[i] = new SpecialShopEntry(i, parser, gameData, language);

        UseCurrencyType = parser.ReadColumn<byte>(1621);
        QuestUnlock = new LazyRow<Quest>(gameData, parser.ReadColumn<uint>(1622), language);
        CompleteText = new LazyRow<DefaultTalk>(gameData, parser.ReadColumn<int>(1623), language);
        NotCompleteText = new LazyRow<DefaultTalk>(gameData, parser.ReadColumn<int>(1624), language);
        Unknown0 = parser.ReadColumn<uint>(1625);
        Unknown1 = parser.ReadColumn<bool>(1626);
        Unknown2 = parser.ReadColumn<ushort>(1627);
        Unknown3 = parser.ReadColumn<uint>(1628);
        Unknown4 = parser.ReadColumn<bool>(1629);
    }
}
#nullable restore
