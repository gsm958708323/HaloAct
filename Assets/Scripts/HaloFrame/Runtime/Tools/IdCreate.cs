using System;
using System.Collections;
using System.Collections.Generic;
using Ability;

public class IdCreate
{
    private Dictionary<EntityType, int> typeCount = new();

    public int Get(EntityType entityType)
    {
        if (!typeCount.ContainsKey(entityType))
        {
            var value = (int)entityType * 10000;
            typeCount.Add(entityType, value);
        }

        return ++typeCount[entityType];
    }

}