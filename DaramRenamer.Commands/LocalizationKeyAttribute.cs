using System;

namespace DaramRenamer;

public class LocalizationKeyAttribute : Attribute
{
    public LocalizationKeyAttribute(string key)
    {
        LocalizationKey = key;
    }

    public string LocalizationKey { get; }
}