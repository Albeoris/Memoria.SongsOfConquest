using System;
using BepInEx.Logging;

namespace Memoria.SongsOfConquest.Configuration;

public sealed partial class ModConfiguration
{
    public SpeedConfiguration Speed { get; }
    public FontConfiguration Font { get; }
    public CommandersLevelUpConfiguration CommandersLevelUp { get; }

    public ModConfiguration()
    {
        using (var log = Logger.CreateLogSource("Memoria Config"))
        {
            try
            {
                log.LogInfo($"Initializing {nameof(ModConfiguration)}");

                ConfigFileProvider provider = new();
                Speed = SpeedConfiguration.Create(provider);
                Font = FontConfiguration.Create(provider);
                CommandersLevelUp = CommandersLevelUpConfiguration.Create(provider);

                log.LogInfo($"{nameof(ModConfiguration)} initialized successfully.");
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to initialize {nameof(ModConfiguration)}: {ex}");
                throw;
            }
        }
    }
}