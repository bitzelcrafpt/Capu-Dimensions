﻿#if EDITOR

#else
using System.Collections.Generic;
using UnityEngine;

namespace Capu_Dimensions.Helpers;

public static class AddonExtensions
{
    public static GameObject FindClosestTerminal(this GameObject player, List<GameObject> terminalPoints)
    {
        Vector3 playerPosition = player.transform.position;

        GameObject closestTerminalPoint = null;
        float closestDistance = float.MaxValue;

        foreach (var terminalPoint in terminalPoints)
        {
            float distance = Vector3.Distance(playerPosition, terminalPoint.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTerminalPoint = terminalPoint;
            }
        }

        return closestTerminalPoint;
    }
}
#endif