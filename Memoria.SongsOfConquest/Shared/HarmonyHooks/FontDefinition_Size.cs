using System;
using HarmonyLib;
using Memoria.SongsOfConquest.BeepInEx;
using SongsOfConquest.Client.UI;

namespace Memoria.SongsOfConquest.HarmonyHooks;

// ReSharper disable InconsistentNaming
[HarmonyPatch(typeof(FontDefinition), nameof(FontDefinition.Size), MethodType.Getter)]
public static class FontDefinition_Size
{
    public static void Postfix(ref Int32 __result)
    {
        try
        {
            Int32 minSize = ModComponent.Instance.Config.Font.MinFontSize;
            if (minSize == 0)
                return;

            if (__result < minSize)
                __result = minSize;
        }
        catch (Exception ex)
        {
            ModComponent.Log.LogException(ex);
        }
    }
}