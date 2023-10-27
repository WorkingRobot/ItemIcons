using Dalamud.Interface.Internal;
using System;
using System.Collections.Generic;

namespace ItemIcons.Utils;

public sealed class IconManager : IDisposable
{
    private readonly Dictionary<uint, IDalamudTextureWrap> iconCache = new();
    private readonly Dictionary<string, IDalamudTextureWrap> textureCache = new();

    public IDalamudTextureWrap GetIcon(uint id)
    {
        if (!iconCache.TryGetValue(id, out var ret))
            iconCache.Add(id, ret = Service.TextureProvider.GetIcon(id) ??
                throw new ArgumentException($"Invalid icon id {id}", nameof(id)));
        return ret;
    }

    public IDalamudTextureWrap GetTexture(string path)
    {
        if (!textureCache.TryGetValue(path, out var ret))
            textureCache.Add(path, ret = Service.TextureProvider.GetTextureFromGame(path) ??
            throw new ArgumentException($"Invalid texture {path}", nameof(path)));
        return ret;
    }

    public void Dispose()
    {
        foreach (var image in iconCache.Values)
            image.Dispose();
        iconCache.Clear();

        foreach (var image in textureCache.Values)
            image.Dispose();
        textureCache.Clear();
    }
}
