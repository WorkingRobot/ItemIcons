using ItemIcons.IconTypes;
using ItemIcons.Utils;

namespace ItemIcons.IconProviders;

internal abstract unsafe class SingleIconProvider : IconProvider
{
    protected uint Id { get; }

    public abstract BaseIcon Icon { get; }

    public abstract bool Matches(Item item);

    public override uint? GetMatch(Item item) =>
        Matches(item) ? Id : null;

    protected SingleIconProvider()
    {
        Id = RegisterIcons(new[] { Icon });
        Log.Debug($"Registering {GetType().Name} to {Id}");
    }
}
