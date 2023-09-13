using Dalamud.Logging;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ItemIcons.AtkIcons;
using ItemIcons.Utils;
using System;

namespace ItemIcons.IconTypes;

internal sealed record TextureIcon : BaseIcon
{
    public string Texture { get; }
    public UldRect? Rect { get; }

    private nint AssetPtr { get; }
    private nint PartPtr { get; }
    private nint PartsListPtr { get; }

    private byte NodeFlags { get; }
    private float NodeScale { get; init; }

    private unsafe AtkUldAsset* Asset => (AtkUldAsset*)AssetPtr;
    private unsafe AtkUldPart* Part => (AtkUldPart*)PartPtr;
    private unsafe AtkUldPartsList* PartsList => (AtkUldPartsList*)PartsListPtr;

    public override unsafe uint IconId { set { if (AssetPtr != nint.Zero) Asset->Id = value; } }

    public override unsafe float Scale {
        get => base.Scale;
        init
        {
            base.Scale = value;
            NodeScale = Rect is { } uldRect ? IconSize / MathF.Max(uldRect.Width, uldRect.Height) * Scale : Scale;
        }
    }

    private unsafe TextureIcon(string texture, uint? iconId, UldRect? rect)
    {
        if (texture.EndsWith("_hr1.tex"))
        {
            texture = texture.Replace("_hr1.tex", ".tex");
            if (rect != null)
                rect = new UldRect((ushort)(rect.Value.U / 2), (ushort)(rect.Value.V / 2), (ushort)(rect.Value.Width / 2), (ushort)(rect.Value.Height / 2));
        }

        Texture = texture;
        Rect = rect;

        AssetPtr = (nint)NodeManager.Malloc<AtkUldAsset>();
        PartPtr = (nint)NodeManager.Malloc<AtkUldPart>();
        PartsListPtr = (nint)NodeManager.Malloc<AtkUldPartsList>();

        Asset->AtkTexture.Ctor();
        if (iconId != null)
        {
            if (Asset->AtkTexture.LoadIconTexture((int)iconId.Value, 0) != 0)
                throw new ArgumentException($"Icon id {iconId.Value} (Texture: {Texture}) not found", nameof(iconId));
        }
        else
        {
            if (Asset->AtkTexture.LoadTexture(Texture, 2) != 0) // returns 0 on success, -1 on failure
                throw new ArgumentException($"Texture {Texture} not found", nameof(texture));
        }

        if (Rect is { } uldRect)
        {
            *Part = new()
            {
                UldAsset = Asset,
                U = uldRect.U,
                V = uldRect.V,
                Width = uldRect.Width,
                Height = uldRect.Height
            };
            NodeFlags = 0;
            NodeScale = IconSize / MathF.Max(uldRect.Width, uldRect.Height) * Scale;
        }
        else
        {
            *Part = new()
            {
                UldAsset = Asset,
                U = 0,
                V = 0,
                Width = IconSize,
                Height = IconSize,
            };
            NodeFlags = (byte)ImageNodeFlags.AutoFit;
            NodeScale = Scale;
        }

        *PartsList = new()
        {
            Id = 0,
            PartCount = 1,
            Parts = Part
        };
    }

    public TextureIcon(string texture, UldRect? rect = null) : this(texture, null, rect)
    {
        
    }

    public TextureIcon(uint icon, UldRect? rect = null) : this(LookupIcon(icon), icon, rect)
    {

    }

    private static string LookupIcon(uint icon) =>
        Service.TextureProvider.GetIconPath(icon, flags: ITextureProvider.IconFlags.None) ??
        throw new ArgumentOutOfRangeException(nameof(icon), icon, "Icon not found");

    private const int IconSize = 18;

    public const bool SetPartsList = true;
    private unsafe bool ApplyTextures(AtkImageNode* node)
    {
        if (SetPartsList)
        {
            if (node->PartsList != PartsList)
            {
                node->PartsList = PartsList;
                node->PartId = 0;
                return true;
            }
            return false;
        }
        else
        {
            if (node->PartsList->Id != Asset->Id)
            {
                PluginLog.Debug($"Unloading tex");
                //node->UnloadTexture();
                node->PartsList->Id = Asset->Id;
                PluginLog.Debug($"Loading tex");
                //node->LoadTexture(Texture, 2);
                node->PartsList->Parts[node->PartId].UldAsset = Asset;

                return true;
            }
            return false;
        }
    }

    public override unsafe void Apply(AtkItemIcon icon, bool usePrimary, byte alpha)
    {
        //if (AssetPtr == nint.Zero)
        //    PluginLog.Debug($"AssetPtr is null");
        //EnsureTexLoaded();
        //if (Asset->AtkTexture.Resource == null)
        //    PluginLog.Debug($"Resource is null {Asset->AtkTexture.TextureType}");
        //else if (Asset->AtkTexture.Resource->TexFileResourceHandle == null)
        //    PluginLog.Debug($"TexFileResourceHandle is null");
        //else
        //    PluginLog.Debug($"Nothing is null");
        //EnsureKernelLoaded();
        //PluginLog.Debug($"Applying {Texture} -> {KernelLoaded}");

        var node = usePrimary ? icon.ImageNode1 : icon.ImageNode2;

        node->AtkResNode.Color.A = alpha;
        NodeUtils.SetVisibility(&node->AtkResNode, true);

        if (ApplyTextures(node))
        {
            node->AtkResNode.X = (short)(BaseOffset + Offset);
            node->AtkResNode.Y = (short)(BaseOffset + Offset);
            node->AtkResNode.Width = Part->Width;
            node->AtkResNode.Height = Part->Height;
            node->AtkResNode.ScaleX = NodeScale;
            node->AtkResNode.ScaleY = NodeScale;
            node->Flags = NodeFlags;

            node->AtkResNode.AddRed = unchecked((ushort)(short)AddRGB.X);
            node->AtkResNode.AddGreen = unchecked((ushort)(short)AddRGB.Y);
            node->AtkResNode.AddBlue = unchecked((ushort)(short)AddRGB.Z);
            node->AtkResNode.MultiplyRed = (byte)MultiplyRGB.X;
            node->AtkResNode.MultiplyGreen = (byte)MultiplyRGB.Y;
            node->AtkResNode.MultiplyBlue = (byte)MultiplyRGB.Z;
        }

        icon.UpdateDirtyNode(&node->AtkResNode);
    }

    public override unsafe void Dispose()
    {
        if (AssetPtr != nint.Zero)
            IMemorySpace.Free(Asset);
        if (PartPtr != nint.Zero)
            IMemorySpace.Free(Part);
        if (PartsListPtr != nint.Zero)
            IMemorySpace.Free(PartsList);
    }

    //private unsafe void EnsureTexLoaded()
    //{
    //    if (TexLoaded || AssetPtr == nint.Zero)
    //        return;
    //    TexLoaded = Asset->AtkTexture.LoadTexture(Texture, 1) == 0;
    //    PluginLog.Debug($"Loaded {Texture} into resource -> {(nint)Asset->AtkTexture.Resource:X8}");
    //}

    //private unsafe void EnsureKernelLoaded()
    //{
    //    if (KernelLoaded || AssetPtr == nint.Zero || Asset->AtkTexture.Resource == null)
    //        return;
    //    KernelLoaded = LoadIntoKernel(&Asset->AtkTexture.Resource->TexFileResourceHandle->ResourceHandle);
    //    PluginLog.Debug($"Loaded {Texture} into kernel -> {KernelLoaded}");
    //}

    //[StructLayout(LayoutKind.Explicit)]
    //private unsafe partial struct ResourceHandleVTable
    //{
    //    [FieldOffset(48)] public delegate* unmanaged[Stdcall]<ResourceHandle*, byte> GetUserData;
    //    [FieldOffset(136)] public delegate* unmanaged[Stdcall]<ResourceHandle*, ulong> GetLength;
    //    [FieldOffset(184)] public delegate* unmanaged[Stdcall]<ResourceHandle*, byte*> GetData;
    //    [FieldOffset(248)] public delegate* unmanaged[Stdcall]<ResourceHandle*, bool> LoadIntoKernel;
    //    [FieldOffset(264)] public delegate* unmanaged[Stdcall]<ResourceHandle*, void*, bool, bool> Load;
    //}

    //private static unsafe bool LoadIntoKernel(ResourceHandle* handle)
    //{
    //    return ((ResourceHandleVTable*)handle->vtbl)->LoadIntoKernel(handle);
    //}
}