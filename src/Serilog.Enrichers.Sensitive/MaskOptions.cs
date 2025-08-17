using System;

namespace Serilog.Enrichers.Sensitive;

public class MaskOptions : IEquatable<MaskOptions>
{
    public static readonly MaskOptions Default = new();
    public const int NotSet = -1;
    public int ShowFirst { get; set; } = NotSet;
    public int ShowLast { get; set; } = NotSet;
    public bool PreserveLength { get; set; } = true;
    public bool WildcardMatch { get; set; }

    public bool Equals(MaskOptions? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other.GetType() != GetType())
        {
            return false;
        }

        return ShowFirst == other.ShowFirst && ShowLast == other.ShowLast && PreserveLength == other.PreserveLength && WildcardMatch == other.WildcardMatch;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((MaskOptions)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = ShowFirst;
            hashCode = (hashCode * 397) ^ ShowLast;
            hashCode = (hashCode * 397) ^ PreserveLength.GetHashCode();
            return hashCode;
        }
    }
}