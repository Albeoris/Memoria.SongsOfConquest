using System;
using Memoria.SongsOfConquest.Core;
using UnityEngine;

namespace Memoria.SongsOfConquest;

public sealed class ScreenDrawer : MonoBehaviour
{
    private GUIStyle _guiStyle;

    private OrderedSet<String> _entries = new OrderedSet<String>();
    private String _textToDraw = String.Empty;
    private Boolean _changed;

    public void Add(String text)
    {
        _changed |= _entries.TryAdd(text);
    }
    
    public void Remove(String text)
    {
        _changed |= _entries.TryRemove(text);
    }

    private void Awake()
    {
        // GuiIndicatorsConfiguration config = ModComponent.Instance.Config.GuiIndicators;
        // if (!config.Enabled.Value)
        {
            ModComponent.Log.LogInfo($"[GuiIndicators] Indicators are disabled in the settings.");
            Destroy(this);
            return;
        }

        _guiStyle = new GUIStyle
        {
            // fontSize = config.TextSize.Value,
            // normal = { textColor = config.TextColor.Value },
            // alignment = config.TextAlignment.Value
        };
    }

    private void Update()
    {
        if (_changed)
        {
            _textToDraw = String.Join(" ", _entries);
            _changed = false;
        }
    }
    
    private void OnGUI()
    {
        Int32 offset = _guiStyle.fontSize + 8;

        Rect rect = new Rect(offset, offset, Screen.width - offset * 2, Screen.height - offset * 2);

        // GUI.Box(rect, GUIContent.none);
        GUI.Label(rect, _textToDraw, _guiStyle);
    }
}