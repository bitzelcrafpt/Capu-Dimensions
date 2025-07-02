#if EDITOR
#else
using Capu_Dimensions.Models;
//using Capu_Dimensions.Patches;
using UnityEngine;

using Capu_Dimensions.API;
using Capu_Dimensions.Helpers;
using Il2CppLocomotion;
using Il2Cpp;
using MelonLoader;

namespace Capu_Dimensions.Behaviours;

internal class TeleportDimension : MonoBehaviour
{

    public static void OnTeleport(DimensionPackage dimensionPackage)
    {
        MelonLogger.Msg("ONTELEPORT 0");
        GameObject spawnPointObject = FindObjectOfType<DimensionDescriptor>().SpawnPosition;
        MelonLogger.Msg("ONTELEPORT 1");
        GameObject terminalPointObject = FindObjectOfType<DimensionDescriptor>().TerminalPosition;
        MelonLogger.Msg("ONTELEPORT 2");

        MelonLogger.Msg("ONTELEPORT 3");
        Vector3 spawnPoint = spawnPointObject.transform.position;
        MelonLogger.Msg("ONTELEPORT 4");
        Vector3 terminalPoint = terminalPointObject.transform.position;
        MelonLogger.Msg("ONTELEPORT 5");

        MelonLogger.Msg("ONTELEPORT 6");
        Player.Instance.transform.position = spawnPoint;
        MelonLogger.Msg("ONTELEPORT 7");
        Player.Instance.playerRigidbody.velocity = new Vector3(0, 0, 0);
        MelonLogger.Msg("ONTELEPORT 8");

        MelonLogger.Msg("ONTELEPORT 9");
        GameObject standObject = GameObject.Find("StandMD(Clone)");
        MelonLogger.Msg("ONTELEPORT 10");
        standObject.transform.position = terminalPoint;
        MelonLogger.Msg("ONTELEPORT 11");
        MelonLogger.Msg(dimensionPackage.Name);
        MelonLogger.Msg("DIMENSIONPACKAGENAME");
        MelonLogger.Msg(dimensionPackage.Author);
        MelonLogger.Msg("DIMENSIONPACKAGEAUTHOR");
        DimensionEvents.OnDimensionEnter?.Invoke($"{dimensionPackage.Name}, {dimensionPackage.Author}");
        MelonLogger.Msg("ONTELEPORT 12");
    }

    public static void ReturnToSpawn(DimensionPackage packg)
    {
        Vector3 SpawnStump = new Vector3(-84.86f, 2.75f, 95.199f);

        Player.Instance.transform.position = SpawnStump;
        Player.Instance.playerRigidbody.velocity = new Vector3(0, 0, 0);
        GameObject.Find("StandMD(Clone)").transform.position = new Vector3(-85.811f, 1.67f, 97.895f);
        DimensionEvents.OnDimensionLeave?.Invoke($"{packg.Name}, {packg.Author}");
    }
}
#endif