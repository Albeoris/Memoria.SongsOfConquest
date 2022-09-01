using System;

namespace Memoria.SongsOfConquest.Configuration;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ConfigScopeAttribute : Attribute
{
    public String SectionName { get; }
    
    public ConfigScopeAttribute(String sectionName)
    {
        SectionName = sectionName;
    }
}