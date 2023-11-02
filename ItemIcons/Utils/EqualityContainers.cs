using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemIcons.Utils;

public class ValueEqualityList<T> : List<T>, IEquatable<IEnumerable<T>>
{
    public override bool Equals(object? other) =>
        other is IEnumerable<T> enumerable && Equals(enumerable);

    public bool Equals(IEnumerable<T>? other) =>
        other != null && this.SequenceEqual(other);

    public override int GetHashCode()
    {
        var hashCode = 0;
        foreach (var item in this)
            hashCode = HashCode.Combine(hashCode, item?.GetHashCode());

        return hashCode;
    }
}

public class ValueEqualityDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IEquatable<ValueEqualityDictionary<TKey, TValue>> where TKey : notnull
{
    public override bool Equals(object? other) =>
        other is IEnumerable<KeyValuePair<TKey, TValue>> dictionary && Count == dictionary.Count() && !this.Except(dictionary).Any();

    public bool Equals(ValueEqualityDictionary<TKey, TValue>? other) =>
        Equals((object?)other);

    public override int GetHashCode()
    {
        var hashCode = 0;
        foreach (var (key, value) in this)
                hashCode = HashCode.Combine(hashCode, key?.GetHashCode(), value?.GetHashCode());
        
        return hashCode;
    }
}

public class ValueEqualityHashSet<T> : HashSet<T>, IEquatable<IEnumerable<T>>
{
    public override bool Equals(object? other) =>
        other is IEnumerable<T> enumerable && Equals(enumerable);

    public bool Equals(IEnumerable<T>? other) =>
         other != null && SetEquals(other);

    public override int GetHashCode()
    {
        var hashCode = 0;
        foreach (var item in this)
            hashCode = HashCode.Combine(hashCode, item?.GetHashCode());

        return hashCode;
    }
}
