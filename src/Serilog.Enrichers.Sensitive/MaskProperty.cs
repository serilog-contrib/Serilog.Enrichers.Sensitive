using System;
using System.Collections.Generic;

namespace Serilog.Enrichers.Sensitive;

public class MaskProperty
{
    private MaskOptions _options = new();
    // Use StringComparer.OrdinalIgnoreCase to ensure we'd match on SomePROP and SomeProp
    private readonly HashSet<string> _matchingProperties = new(StringComparer.OrdinalIgnoreCase);
    private string? _match;
    private int? _minLength;
    private MatchMode _matchMode = MatchMode.Unknown;
    public string Name { get; set; }

    public MaskOptions Options
    {
        get => _options;
        set
        {
            _options = value;

            if (_options.WildcardMatch)
            {
                if (Name[0] == '*')
                {
                    if (Name[Name.Length - 1] == '*')
                    {
                        _match = Name.Substring(1, Name.Length - 2);
                        _minLength = _match.Length + 2;
                        _matchMode = MatchMode.Middle;
                    }
                    else
                    {
                        _match = Name.Substring(1);
                        _minLength = _match.Length + 1;
                        _matchMode = MatchMode.End;
                    }
                }
                else if (Name[Name.Length - 1] == '*')
                {
                    _match = Name.Substring(0, Name.Length - 2);
                    _minLength = _match.Length + 1;
                    _matchMode = MatchMode.Start;
                }
                else
                {
                    // If someone sets WildcardMatch to true but there
                    // is no wildcard character in the property name then
                    // set WildcardMatch to false so we short-circuit
                    // property name matching.
                    //
                    // Note: consider throwing an exception in this scenario.
                    _options.WildcardMatch = false;
                }
            }
        }
    }

    public static MaskProperty WithDefaults(string propertyName)
    {
        return new MaskProperty { Name = propertyName };
    }

    public bool IsMatch(string propertyName)
    {
        if (Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return Options.WildcardMatch && PropertyNameMatchesWildcard(propertyName);
    }
    
    protected bool PropertyNameMatchesWildcard(string propertyName)
    {
        // If the property name is shorter than the match + at least one char
        // then it won't match so exit early.
        if (propertyName.Length < _minLength)
        {
            return false;
        }
        
        if (_matchingProperties.Contains(propertyName))
        {
            return true;
        }
        
        bool isMatch;
        
        switch (_matchMode)
        {
            case MatchMode.Start:
                isMatch = propertyName.StartsWith(_match!, StringComparison.OrdinalIgnoreCase);
                break;
            case MatchMode.End:
                isMatch = propertyName.EndsWith(_match!, StringComparison.OrdinalIgnoreCase);
                break;
            case MatchMode.Middle:
                isMatch = propertyName.IndexOf(_match!, StringComparison.OrdinalIgnoreCase) > 0;
                break;
            default:
                return false;
        }
        
        if (isMatch)
        {
            _matchingProperties.Add(propertyName);
        }

        return isMatch;
    }

    private enum MatchMode
    {
        Unknown,
        Start,
        Middle,
        End
    }
}

