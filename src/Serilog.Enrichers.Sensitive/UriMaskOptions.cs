using System;

namespace Serilog.Enrichers.Sensitive;

public sealed class UriMaskOptions : MaskOptions, IEquatable<UriMaskOptions>
{
    public bool ShowScheme { get; set; } = true;
    public bool ShowHost { get; set; } = true;
    public bool ShowPath { get; set; } = false;
    public bool ShowQueryString { get; set; } = false;

    public bool Equals(UriMaskOptions? other)
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

        return base.Equals(other) && ShowScheme == other.ShowScheme && ShowHost == other.ShowHost && ShowPath == other.ShowPath && ShowQueryString == other.ShowQueryString;
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

        return Equals((UriMaskOptions)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = base.GetHashCode();
            hashCode = (hashCode * 397) ^ ShowScheme.GetHashCode();
            hashCode = (hashCode * 397) ^ ShowHost.GetHashCode();
            hashCode = (hashCode * 397) ^ ShowPath.GetHashCode();
            hashCode = (hashCode * 397) ^ ShowQueryString.GetHashCode();
            return hashCode;
        }
    }
}