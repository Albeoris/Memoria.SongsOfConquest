using System;
using System.Threading;
using HarmonyLib;
using Lavapotion.Networking;
using Memoria.SongsOfConquest.BeepInEx;
using Memoria.SongsOfConquest.Configuration;
using SongsOfConquest.Client.Adventure;
using SongsOfConquest.Client.Adventure.Menu;

namespace Memoria.SongsOfConquest.HarmonyHooks;

// ReSharper disable InconsistentNaming
[HarmonyPatch(typeof(CommanderLevelUpMenu), "GetSkillComponent")]
public static class CommanderLevelUpMenu_GetSkillComponent
{
	public static Boolean Prefix(CommanderLevelUpMenu __instance, Int32 index, ref CommanderLevelUpSkillComponent __result)
	{
		try
		{
			if (index < 2)
			{
				// Call native method
				return true;
			}
		
			CommandersLevelUpConfiguration config = ModComponent.Instance.Config.CommandersLevelUp;
			if (!config.EnableOverrideSelectableSkillsCount)
			{
				// Call native method
				return true;
			}

			__result = CommanderLevelUpMenu_Open.GetSkillComponent(index);
			return false;
		}
		catch (Exception ex)
		{
			ModComponent.Log.LogException(ex);
			// Call native method
			return true;
		}
	}
}