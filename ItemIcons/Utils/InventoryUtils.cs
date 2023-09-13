using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.Interop;
using FFXIVClientStructs.STD;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ItemIcons.Utils;

internal static unsafe class InventoryUtils
{
    public sealed class ContainerWrapper : IReadOnlyList<InventoryItem?>
    {
        public int Count => (int)sorter->Items.Size();

        private readonly ItemOrderModuleSorter* sorter;

        public ContainerWrapper(ItemOrderModuleSorter* sorter)
        {
            this.sorter = sorter;
        }

        public InventoryItem? GetItem(int idx)
        {
            var orderData = sorter->Items.Span[idx].Value;
            var container = (sorter->InventoryType + orderData->Page).GetContainerRaw();
            var ptr = container->GetInventorySlot(orderData->Slot);
            return ptr == null ? null : *ptr;
        }

        public IEnumerator<InventoryItem?> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
                yield return GetItem(i);
        }

        public InventoryItem? this[int idx] => GetItem(idx);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public sealed class PagedContainerWrapper : IReadOnlyList<InventoryItem?>
    {
        public int Page { get; }
        public int Count { get; }

        private readonly ItemOrderModuleSorter* sorter;

        public PagedContainerWrapper(ItemOrderModuleSorter* sorter, int page, int? size)
        {
            this.sorter = sorter;
            Page = page;
            Count = size ?? sorter->ItemsPerPage;
        }

        public InventoryItem* GetItemPtr(int idx)
        {
            var orderData = sorter->Items.Span[(Page * Count) + idx].Value;
            var container = (sorter->InventoryType + orderData->Page).GetContainerRaw();
            var ptr = container->GetInventorySlot(orderData->Slot);
            return ptr;
        }

        public InventoryItem? GetItem(int idx)
        {
            var orderData = sorter->Items.Span[(Page * Count) + idx].Value;
            var container = (sorter->InventoryType + orderData->Page).GetContainerRaw();
            var ptr = container->GetInventorySlot(orderData->Slot);
            return ptr == null ? null : *ptr;
        }

        public IEnumerator<InventoryItem?> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
                yield return GetItem(i);
        }

        public InventoryItem? this[int idx] => GetItem(idx);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public sealed class InventoryWrapper : IReadOnlyList<InventoryItem?>
    {
        private readonly InventoryContainer* container;

        public int Count => (int)container->Size;

        public InventoryWrapper(InventoryContainer* container)
        {
            this.container = container;
        }

        public InventoryItem? GetItem(int idx)
        {
            if (idx < 0 || idx >= Count)
                return null;
            return container->Items[idx];
        }

        public IEnumerator<InventoryItem?> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
                yield return GetItem(i);
        }

        public InventoryItem? this[int idx] => GetItem(idx);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public static IReadOnlyList<InventoryItem?> GetContainer(this ContainerType me)
    {
        var sorter = me.GetSorter();
        if (sorter != null)
            return new ContainerWrapper(me.GetSorter());
        return ((InventoryType)me).GetContainer();
    }

    public static PagedContainerWrapper GetContainerPage(this ContainerType me, int page, int? slotCount = null)
    {
        var sorter = me.GetSorter();
        if (sorter == null)
            throw new InvalidOperationException("Container does not have a sorter to page");
        return new PagedContainerWrapper(me.GetSorter(), page, slotCount);
    }

    public static InventoryWrapper GetContainer(this InventoryType me) =>
        new(me.GetContainerRaw());

    public static InventoryContainer* GetContainerRaw(this InventoryType me) =>
        InventoryManager.Instance()->GetInventoryContainer(me);

    private static ItemOrderModuleSorter* GetSorter(this ContainerType me)
    {
        var m = ItemOrderModule.Instance();
        var sorter = me switch
        {
            ContainerType.ArmoryMainHand => m->ArmouryMainHandSorter,
            ContainerType.ArmoryHead => m->ArmouryHeadSorter,
            ContainerType.ArmoryBody => m->ArmouryBodySorter,
            ContainerType.ArmoryHands => m->ArmouryHandsSorter,
            // ContainerType.ArmoryWaist => m->ArmouryWaistSorter,
            ContainerType.ArmoryLegs => m->ArmouryLegsSorter,
            ContainerType.ArmoryFeets => m->ArmouryFeetSorter,
            ContainerType.ArmoryOffHand => m->ArmouryOffHandSorter,
            ContainerType.ArmoryEar => m->ArmouryEarsSorter,
            ContainerType.ArmoryNeck => m->ArmouryNeckSorter,
            ContainerType.ArmoryWrist => m->ArmouryWristsSorter,
            ContainerType.ArmoryRings => m->ArmouryRingsSorter,
            ContainerType.ArmorySoulCrystal => m->ArmourySoulCrystalSorter,
            ContainerType.SaddleBag => m->SaddleBagSorter,
            ContainerType.PremiumSaddleBag => m->PremiumSaddleBagSorter,
            ContainerType.Inventory => m->InventorySorter,
            ContainerType.Retainer => GetRetainerSorter(m),
            _ => null
        };
        return sorter;
    }

    private static ItemOrderModuleSorter* GetRetainerSorter(ItemOrderModule* m)
    {
        var sorter = (StdMap<ulong, Pointer<ItemOrderModuleSorter>>*)&m->RetainerSorter;
        foreach(var pair in *sorter)
        {
            if (pair.Item1 == m->ActiveRetainerId)
                return pair.Item2;
        }
        return null;
    }
}

public enum ContainerType : uint
{
    Inventory = InventoryType.Inventory1,
    SaddleBag = InventoryType.SaddleBag1,
    PremiumSaddleBag = InventoryType.PremiumSaddleBag1,
    Retainer = InventoryType.RetainerPage1,
    ArmoryMainHand = InventoryType.ArmoryMainHand,
    ArmoryHead = InventoryType.ArmoryHead,
    ArmoryBody = InventoryType.ArmoryBody,
    ArmoryHands = InventoryType.ArmoryHands,
    // ArmoryWaist = InventoryType.ArmoryWaist,
    ArmoryLegs = InventoryType.ArmoryLegs,
    ArmoryFeets = InventoryType.ArmoryFeets,
    ArmoryOffHand = InventoryType.ArmoryOffHand,
    ArmoryEar = InventoryType.ArmoryEar,
    ArmoryNeck = InventoryType.ArmoryNeck,
    ArmoryWrist = InventoryType.ArmoryWrist,
    ArmoryRings = InventoryType.ArmoryRings,
    ArmorySoulCrystal = InventoryType.ArmorySoulCrystal,
}
