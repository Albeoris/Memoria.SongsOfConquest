using System;

namespace Memoria.SongsOfConquest.Core;

public abstract class SafeComponent
{
    private Boolean _isDisabled;

    protected virtual void Update()
    {
    }

    public void TryUpdate()
    {
        try
        {
            if (_isDisabled)
                return;

            Update();
        }
        catch (Exception ex)
        {
            _isDisabled = true;
            ModComponent.Log.LogError($"[{GetType().Name}].{nameof(Update)}(): {ex}");
        }   
    }
}