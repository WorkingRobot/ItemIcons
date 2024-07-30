using ItemIcons.IconTypes;

namespace ItemIcons.IconProviders;

internal abstract unsafe class SingleIconProvider : IconProvider
{
    public abstract BaseIcon Icon { get; }

    public abstract bool Matches(Item item);

    public override uint? GetMatch(Item item) =>
        Matches(item) ? IconOffset : null;

    protected SingleIconProvider()
    {
        Icons = [Icon];
        RegisterIcons();
    }
}
