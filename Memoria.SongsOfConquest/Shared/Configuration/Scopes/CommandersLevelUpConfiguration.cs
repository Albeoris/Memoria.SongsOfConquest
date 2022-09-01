using System;

namespace Memoria.SongsOfConquest.Configuration;

[ConfigScope("Commanders.LevelUp")]
public abstract partial class CommandersLevelUpConfiguration
{
    [ConfigEntry($"Allows to override the number of skills available when leveling up.")]
    public virtual Boolean EnableOverrideSelectableSkillsCount { get; set; } = false;
    
    [ConfigEntry($"Change the number of new special skills the hero can learn. Level restrictions will be ignored if skills are not enough.")]
    public virtual Int32 NewSpecialSkillCount { get; set; } = 1;
    
    [ConfigEntry($"Change the number of upgradeable special skills the hero can learn. Level restrictions will be ignored if skills are not enough.")]
    public virtual Int32 UpgradeSpecialSkillCount { get; set; } = 1;
    
    [ConfigEntry($"Change the number of new common skills the hero can learn. Level restrictions will be ignored if skills are not enough.")]
    public virtual Int32 NewCommonSkillCount { get; set; } = 1;
    
    [ConfigEntry($"Change the number of upgradeable common skills the hero can learn. Level restrictions will be ignored if skills are not enough.")]
    public virtual Int32 UpgradeCommonSkillCount { get; set; } = 1;
    
    public abstract void CopyFrom(CommandersLevelUpConfiguration configuration);
}