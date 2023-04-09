using System;
using System.Collections;
using System.Collections.Generic;

public class AbilityTimer
{
    bool isActive;
    public Action TimeOutCB;
    float duration;

    public void OnUpdate(float deltaTime)
    {
        if (!isActive)
            return;

        duration -= deltaTime;
        if (duration <= 0)
        {
            TimeOutCB?.Invoke();
            isActive = false;
        }
    }

    public void Start(float duration)
    {
        this.duration = duration;
        this.isActive = true;
    }

    public bool IsActive()
    {
        return isActive;
    }
}
