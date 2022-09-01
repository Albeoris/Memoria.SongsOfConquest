using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Memoria.SongsOfConquest.BeepInEx;
using Memoria.SongsOfConquest.Configuration;
using SongsOfConquest;
using SongsOfConquest.Client.Adventure;
using SongsOfConquest.Client.UI;
using SongsOfConquest.Common.Gamestate;
using SongsOfConquest.Common.Skills;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Memoria.SongsOfConquest.HarmonyHooks;

// ReSharper disable InconsistentNaming
[HarmonyPatch(typeof(CommanderLevelUpMenu), nameof(CommanderLevelUpMenu.Open))]
public static class CommanderLevelUpMenu_Open
{
    private static CommanderLevelUpMenu.Settings _lastSettings;
    private static readonly Dictionary<Int32, CommanderLevelUpSkillComponent> _skillComponents = new();

    public static void Prefix(CommanderLevelUpMenu __instance, ITeamState team, ICommanderState commander, List<SkillReference> skills,
        CommanderLevelUpMenu.Settings ____settings)
    {
        try
        {
            CommandersLevelUpConfiguration config = ModComponent.Instance.Config.CommandersLevelUp;
            if (!config.EnableOverrideSelectableSkillsCount)
                return;

            CommanderLevelUpMenu.Settings settings = ____settings;
            if (ReferenceEquals(settings, _lastSettings))
                return;

            _lastSettings = settings;
            _skillComponents.Clear();

            Int32 additionalCommon = config.NewCommonSkillCount + config.UpgradeCommonSkillCount - 2;
            Int32 additionalSpecial = config.NewSpecialSkillCount + config.UpgradeSpecialSkillCount - 2;
            Int32 additionalCount = Math.Max(additionalCommon, additionalSpecial);
            if (additionalCount < 1)
                return;

            {
                UITextMesh[] headerObjects = settings.SkillChoiceHeaders;
                Int32 index = headerObjects.Length - 1;
                Array.Resize(ref headerObjects, headerObjects.Length + additionalCount);
                headerObjects[headerObjects.Length - 1] = headerObjects[index];
                //GameObject firstObject = headerObjects[0].MonoTransform.gameObject;
                //Transform parent = firstObject.transform.parent;
                for (; index < headerObjects.Length - 1; index++)
                    headerObjects[index] = headerObjects[0];//GameObject.Instantiate(firstObject, parent, worldPositionStays: true).GetComponent<UITextMesh>();
                settings.SkillChoiceHeaders = headerObjects;
            }

            for (int i = 0; i < additionalCount; i++)
            {
                CommanderLevelUpSkillComponent prototype = settings.LeftSkill;
                Transform parent = prototype.gameObject.transform.parent;
			
                CommanderLevelUpSkillComponent component = Object.Instantiate(prototype, parent, true);
                component.transform.localScale = Vector3.one;
                _skillComponents.Add(i + 2, component);
            }
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }

    public static void Postfix(CommanderLevelUpMenu __instance, ITeamState team, ICommanderState commander, List<SkillReference> skills,
        CommanderLevelUpMenu.Settings ____settings)
    {
        try
        {
            CommandersLevelUpConfiguration config = ModComponent.Instance.Config.CommandersLevelUp;
            if (!config.EnableOverrideSelectableSkillsCount)
                return;
		
            Int32 notCommandSkills = skills.Count(s => s.Skill != SkillTypes.Command);
            foreach (UITextMesh header in ____settings.SkillChoiceHeaders)
                header.MonoTransform.gameObject.SetActive(false);
		
            UITextMesh[] headers = ____settings.SkillChoiceHeaders;
            Int32 commandSkillIndex = headers.Length - 1;
            for (Int32 i = notCommandSkills; i < commandSkillIndex; i++)
            {
                CommanderLevelUpSkillComponent skillComponent = GetSkillComponent(i);
                skillComponent.gameObject.SetActive(true);
                skillComponent.Setup(String.Empty, null, String.Empty, String.Empty, false);
                skillComponent.SetAsInactive(false);
                skillComponent.gameObject.SetActive(false);
            }
		
            _lastSettings.AdventureMenuBackground.gameObject.transform.parent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }

    public static CommanderLevelUpSkillComponent GetSkillComponent(Int32 index)
    {
        return _skillComponents[index];
    }
}