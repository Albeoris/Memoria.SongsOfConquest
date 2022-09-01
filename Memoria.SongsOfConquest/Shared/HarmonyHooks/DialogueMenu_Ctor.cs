using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Memoria.SongsOfConquest.BeepInEx;
using SongsOfConquest.Client.Menu;

namespace Memoria.SongsOfConquest.HarmonyHooks;

// ReSharper disable InconsistentNaming
[HarmonyPatch]
public static class DialogueMenu_Ctor
{
    [HarmonyTargetMethod]
    static MethodBase GetConstructor()
    {
        return typeof(DialogueMenu).GetConstructors().Single();
    }
    
    public static void Postfix(DialogueMenu.Settings settings)
    {
        try
        {
            Int32 minSize = ModComponent.Instance.Config.Font.MinFontSize;
            if (minSize == 0)
                return;
            
            settings.DialogueText.Autosize = false;
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }
}