#if EDITOR

#else
//using BepInEx;
using HarmonyLib;
using Capu_Dimensions.API;
using Capu_Dimensions.Behaviours;
using UnityEngine;
//using Utilla;
using System;
using System.IO;
using System.Reflection;
//using BepInEx.Logging;
//using Capu_Dimensions.Editor.Grabbables;
//using Photon.Pun;
using MelonLoader;
using Il2CppPhoton.Pun;
using UnityEngine.SceneManagement;
using Il2CppLocomotion;
using Il2CppInterop.Runtime.Injection;
using Capu_Dimensions.Browser;
using Capu_Dimensions.Interaction;
using Capu_Dimensions.Models;
using Capu_Dimensions.Helpers;
//using static Il2Cpp.DayNightManager;

[assembly: MelonInfo(typeof(Capu_Dimensions.Main), "Capu Dimensions", "1.0.0", "bitzelcrafpt")]
namespace Capu_Dimensions
{
    internal class Main : MelonMod
    {
        public static AssetBundle assetBundle;
        internal static GameObject StandMD;

        private GameObject Manegerl;
        private DimensionManager DimensionInstance;

        //public static ManualLogSource Logger;

        public static bool inModded = true;

        private const string
            GUID = "bitzelcrafpt.capudimensions",
            NAME = "Capu Dimensions",
            VERSION = "1.0.0";

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            ClassInjector.RegisterTypeInIl2Cpp<DimensionManager>();
            ClassInjector.RegisterTypeInIl2Cpp<TeleportDimension>();
            ClassInjector.RegisterTypeInIl2Cpp<DimensionBrowser>();
            ClassInjector.RegisterTypeInIl2Cpp<Comps>();
            ClassInjector.RegisterTypeInIl2Cpp<AssetLoader>();
            ClassInjector.RegisterTypeInIl2Cpp<Item>();
            ClassInjector.RegisterTypeInIl2Cpp<Button>();
            ClassInjector.RegisterTypeInIl2Cpp<DimensionDescriptor>();
            //CaputillaMelonLoader.CaputillaHub.OnGameInitialized += OnInitialized;
            CaputillaMelonLoader.CaputillaHub.OnModdedJoin += ModdedJoin;
            CaputillaMelonLoader.CaputillaHub.OnModdedLeave += ModdedLeave;
            DimensionEvents.OnDimensionTriggerEvent += (a, b, c, d) => { };

        }
        bool hasDone = false;
        public override void OnUpdate()
        {
            base.OnUpdate();
            if(SceneManager.GetActiveScene().name == "CapuchinCopy" && !hasDone && Player.Instance != null)
            {
                hasDone = true;
                assetBundle = LoadAssetBundle("Capu_Dimensions.Resources.stand");
                UnityEngine.Object.Instantiate(assetBundle.LoadAsset<GameObject>("StandMD"));
                //GameObject.Find("StandMD(Clone)/RealStand").AddComponent<GorillaSurfaceOverride>();
                Comps.SetupComps();
                var dimensionManager = new GameObject("Dimension Manager").AddComponent<DimensionManager>();
                DimensionInstance = dimensionManager.GetComponent<DimensionManager>();
                StandMD = GameObject.Find("StandMD(Clone)");
                new GameObject("Dimension Teleport").AddComponent<TeleportDimension>().transform.SetParent(dimensionManager.gameObject.transform);
                Comps.Confetti = assetBundle.LoadAsset<GameObject>("Confetti");
                StandMD.transform.position = new(-85.811f, 1.67f, 97.895f);
                StandMD.transform.rotation = Quaternion.Euler(0, -203.293f, 0);
                //StandMD.transform.GetChild(0).GetComponent<Renderer>().material = GameObject.Find("Global/Levels/ObjectNotInMaps/Stump/Capuchin Stump 1").GetComponent<MeshRenderer>().materials[0];
                ShaderUtility.ReplaceAllShadersMaps(StandMD);
                UnityEngine.UI.Text creditText = GameObject.Find("StandMD(Clone)/UI/Screen/Main Screen/Credit").GetComponent<UnityEngine.UI.Text>();
                creditText.text = "ORIGINAL BY CHIN0303, PORTED BY BITZELCRAFPT.";
                creditText.horizontalOverflow = HorizontalWrapMode.Overflow;
                creditText.verticalOverflow = VerticalWrapMode.Overflow;
                creditText.transform.localPosition = new Vector3(11.9108f, -67.3294f, -0.0007f);
            }

        }
        void ModdedJoin()
        {
            //string gamemode = e.Gamemode;
            //inModded = gamemode.ToUpper().Contains("MODDED");
            //StandMD.SetActive(inModded);
            StandMD.SetActive(true);
            if (DimensionManager.Instance.inDimension && !inModded)
            {
                DimensionInstance.LoadSelectedDimension(DimensionInstance.dimensionNames[DimensionInstance.currentPage]);
                TeleportDimension.ReturnToSpawn(DimensionInstance.currentDimensionPackage);
            }
        }
        void ModdedLeave()
        {
            StandMD.SetActive(false); inModded = true;
        }
        //internal Main()
        //{
        //    DimensionEvents.OnDimensionEnter += value => { if (!PhotonNetwork.InRoom) return; Manegerl = new GameObject("MeowManager").AddComponent<GrabManager>().gameObject; };
        //    DimensionEvents.OnDimensionLeave += value => { if (Manegerl) { UnityEngine.Object.Destroy(Manegerl); } };
        //}

        public static AssetBundle LoadAssetBundle(string path)
        {
            MemoryStream memoryStream;
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            memoryStream = new MemoryStream((int)stream.Length);
            stream.CopyTo(memoryStream);
            return AssetBundle.LoadFromMemory(memoryStream.ToArray());
        }
    }
}



#endif