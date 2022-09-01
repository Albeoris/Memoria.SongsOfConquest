using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Memoria.SongsOfConquest.BeepInEx;
using Memoria.SongsOfConquest.Configuration;
using SongsOfConquest;
using SongsOfConquest.Common;
using SongsOfConquest.Common.Gamestate;
using SongsOfConquest.Common.Skills;

namespace Memoria.SongsOfConquest.HarmonyHooks;

// ReSharper disable InconsistentNaming
[HarmonyPatch(typeof(SkillLookup), nameof(SkillLookup.GetChoosableSkills))]
public static class SkillLookup_GetChoosableSkills
{
    public static Boolean Prefix(ICommanderState commander, Int32 instanceRandomSeed, Boolean ignoreSkillInterval,
        SkillLookup __instance, ref List<SkillReference> __result,
        IWielderLookup ____wielderLookup, ISkillPoolManifest ____skillPoolManifest, IGameConfig ____config
    )
    {
        try
        {
            CommandersLevelUpConfiguration config = ModComponent.Instance.Config.CommandersLevelUp;
            if (!config.EnableOverrideSelectableSkillsCount)
            {
                // Call native method
                return true;
            }

            var wrapper = new Wrapper(__instance, ____wielderLookup, ____skillPoolManifest, ____config, commander);
            __result = wrapper.GetChoosableSkills(instanceRandomSeed,  ignoreSkillInterval);

            // Skip native method
            return false;
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
            // Call native method
            return true;
        }
    }
	
    private sealed class Wrapper
    {
        private readonly SkillLookup _self;
        private readonly IWielderLookup _wielderLookup;
        private readonly ISkillPoolManifest _skillPoolManifest;
        private readonly IGameConfig _gameConfig;
		
        private readonly ICommanderState _commander;
        private readonly ISkillPoolDefinition _poolDefinition;
        private readonly CommandersLevelUpConfiguration _modConfig;

        private readonly MethodInfo _getCommandSkillPoolMethod;
        private readonly MethodInfo _getUpgradeSkillPoolMethod;


        public Wrapper(SkillLookup self, IWielderLookup wielderLookup, ISkillPoolManifest skillPoolManifest, IGameConfig gameConfig, ICommanderState commander)
        {
            _self = self;
            _wielderLookup = wielderLookup;
            _skillPoolManifest = skillPoolManifest;
            _gameConfig = gameConfig;
            _commander = commander;
			
            SkillPoolTypes poolTypes = _wielderLookup.Get(_commander.Reference).SkillPool;
            _poolDefinition = _skillPoolManifest.GetDefinition((UInt16)poolTypes);
            _modConfig = ModComponent.Instance.Config.CommandersLevelUp;

            _getCommandSkillPoolMethod = AccessTools.Method(typeof(SkillLookup), "GetCommandSkillPool");
            _getUpgradeSkillPoolMethod = AccessTools.Method(typeof(SkillLookup), "GetUpgradeSkillPool");
        }

        public List<SkillReference> GetChoosableSkills(Int32 instanceRandomSeed, Boolean ignoreSkillInterval = false)
        {
            List<SkillReference> result = new List<SkillReference>();
			
            UnityEngine.Random.InitState(instanceRandomSeed + _commander.Id + _commander.Skills.Sum(skill => skill.Level));
            Int32 commanderLevel = _commander.GetLevel() - (_commander.UnspentSkillPoints - 1);

            GetNewSkills(result, commanderLevel, exclusive: true, ignoreSkillInterval);
            GetUpgradeSkills(result, commanderLevel, exclusive: true, ignoreSkillInterval);
            if (result.Count > 0)
                return result;

            GetNewSkills(result, commanderLevel, exclusive: false, ignoreSkillInterval);
            GetUpgradeSkills(result, commanderLevel, exclusive: false, ignoreSkillInterval);
			
            List<SkillReference> commandSkillPool = GetCommandSkillPool(_commander);
            if (commandSkillPool.Count > 0)
            {
                if (result.Count > 2)
                    result.Insert(2, commandSkillPool[0]);
                else
                    result.Add(commandSkillPool[0]);
            }

            return result;
        }

        private Int32 GetMaximumSkillCount(bool exclusive)
        {
            return exclusive
                ? _gameConfig.GetValue("wielders.maxNumberOfPowers", 2)
                : _gameConfig.GetValue("wielders.maxNumberOfSkills", 10);
        }

        private Int32 GetObtainedSkillCount(bool exclusive)
        {
            return _commander.Skills.Count(r => _poolDefinition.IsExclusive(r.Skill) == exclusive);
        }

        private Int32 GetDesiredNewSkillCount(bool exclusive)
        {
            return exclusive
                ? _modConfig.NewSpecialSkillCount
                : _modConfig.NewCommonSkillCount;
        }

        private Int32 GetDesiredUpgradeSkillCount(bool exclusive)
        {
            return exclusive
                ? _modConfig.UpgradeSpecialSkillCount
                : _modConfig.UpgradeCommonSkillCount;
        }

        private void GetNewSkills(List<SkillReference> result, int commanderLevel, bool exclusive, bool ignoreLevelIntervals)
        {
            Int32 maximumSkillCount = GetMaximumSkillCount(exclusive);
            Int32 obtainedSkillCount = GetObtainedSkillCount(exclusive);

            if (obtainedSkillCount >= maximumSkillCount)
                return;

            Int32 desiredSkillCount = GetDesiredNewSkillCount(exclusive);
            List<SkillReference> availableSkills = _poolDefinition.GetAvailableNonObtainedSkills(_commander.Skills, commanderLevel, exclusive, ignoreLevelIntervals);
            if (exclusive && availableSkills.Count == 0)
                return;

            List<SkillReference> extraAvailableSkills = _poolDefinition.GetAvailableNonObtainedSkills(_commander.Skills, commanderLevel, exclusive, true);
            ExtractAndAddRandomSkillFromList(ref desiredSkillCount, result, availableSkills, extraAvailableSkills);
        }

        private void GetUpgradeSkills(List<SkillReference> result, Int32 commanderLevel, Boolean exclusive, Boolean ignoreLevelIntervals)
        {
            Int32 desiredSkillCount = GetDesiredUpgradeSkillCount(exclusive);
				
            List<SkillReference> availableSkills = GetUpgradeSkillPool(_commander, commanderLevel, _poolDefinition, exclusive, ignoreLevelIntervals);
            if (exclusive && availableSkills.Count == 0)
                return;
			
            List<SkillReference> extraAvailableSkills = GetUpgradeSkillPool(_commander, commanderLevel, _poolDefinition, exclusive, ignoreLevelIntervals: true);
            ExtractAndAddRandomSkillFromList(ref desiredSkillCount, result, availableSkills, extraAvailableSkills);
        }

        private void ExtractAndAddRandomSkillFromList(ref Int32 count, List<SkillReference> listToAddTo, params List<SkillReference>[] listsToExtractFrom)
        {
            HashSet<SkillReference> extracted = new();

            foreach (List<SkillReference> listToExtractFrom in listsToExtractFrom)
            {
                while (count > 0 && listToExtractFrom.Count > 0)
                {
                    Int32 index = UnityEngine.Random.Range(0, listToExtractFrom.Count);
                    SkillReference skill = listToExtractFrom[index];
                    listToExtractFrom.RemoveAt(index);
                    if (extracted.Add(skill))
                    {
                        listToAddTo.Add(skill);
                        if (--count < 1)
                            return;
                    }	
                }
            }
        }

        private List<SkillReference> GetCommandSkillPool(ICommanderState commander)
        {
            return (List<SkillReference>)_getCommandSkillPoolMethod.Invoke(_self, new Object[] { commander });
        }

        private List<SkillReference> GetUpgradeSkillPool(ICommanderState commander, Int32 currentLevel, ISkillPoolDefinition skillPool, Boolean exclusive, Boolean ignoreLevelIntervals)
        {
            return (List<SkillReference>)_getUpgradeSkillPoolMethod.Invoke(_self, new Object[] { commander, currentLevel, skillPool, exclusive, ignoreLevelIntervals });
        }
    }
}