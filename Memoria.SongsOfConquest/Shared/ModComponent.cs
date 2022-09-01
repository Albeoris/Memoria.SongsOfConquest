using System;
using BepInEx.Logging;
using Memoria.SongsOfConquest.Configuration;
using Memoria.SongsOfConquest.Core;
using UnityEngine;
using Exception = System.Exception;
using Logger = BepInEx.Logging.Logger;

namespace Memoria.SongsOfConquest;

public sealed class ModComponent : MonoBehaviour
{
    public static ModComponent Instance { get; private set; }
    public static ManualLogSource Log { get; private set; }

    [field: NonSerialized] public ModConfiguration Config { get; private set; }
    [field: NonSerialized] public GameSpeedControl SpeedControl { get; private set; }
    [field: NonSerialized] public ScreenDrawer Drawer { get; private set; }

    private Boolean _isDisabled;

    public void Awake()
    {
        Log = Logger.CreateLogSource("Memoria IL2CPP");
        Log.LogMessage($"[{nameof(ModComponent)}].{nameof(Awake)}(): Begin...");
        try
        {
            Instance = this;

            Config = new ModConfiguration();
            SpeedControl = new GameSpeedControl();

            Drawer = gameObject.AddComponent<ScreenDrawer>();

            Log.LogMessage($"[{nameof(ModComponent)}].{nameof(Awake)}(): Processed successfully.");
        }
        catch (Exception ex)
        {
            _isDisabled = true;
            Log.LogError($"[{nameof(ModComponent)}].{nameof(Awake)}(): {ex}");
            throw;
        }
    }
    
    public void OnDestroy()
    {
        Log.LogInfo($"[{nameof(ModComponent)}].{nameof(OnDestroy)}()");
    }

    private void FixedUpdate()
    {
        try
        {
            if (_isDisabled)
                return;
        }
        catch (Exception ex)
        {
            _isDisabled = true;
            Log.LogError($"[{nameof(ModComponent)}].{nameof(FixedUpdate)}(): {ex}");
        }
    }

    private void Update()
    {
        try
        {
            if (_isDisabled)
                return;
        }
        catch (Exception ex)
        {
            _isDisabled = true;
            Log.LogError($"[{nameof(ModComponent)}].{nameof(Update)}(): {ex}");
        }
    }

    private void LateUpdate()
    {
        try
        {
            if (_isDisabled)
                return;

            SpeedControl.TryUpdate();
        }
        catch (Exception ex)
        {
            _isDisabled = true;
            Log.LogError($"[{nameof(ModComponent)}].{nameof(LateUpdate)}(): {ex}");
        }
    }
}