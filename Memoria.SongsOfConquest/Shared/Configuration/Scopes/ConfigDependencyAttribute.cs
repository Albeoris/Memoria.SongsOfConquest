using System;

namespace Memoria.SongsOfConquest.Configuration;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfigDependencyAttribute : Attribute
{
    public String PropertyName { get; }
    public String DefaultValue { get; }
    
    public ConfigDependencyAttribute(String propertyName, String defaultValue)
    {
        PropertyName = propertyName;
        DefaultValue = defaultValue;
    }
}