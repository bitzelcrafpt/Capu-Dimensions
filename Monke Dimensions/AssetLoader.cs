#if EDITOR

#else
using Capu_Dimensions.Behaviours;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Capu_Dimensions;

internal class AssetLoader : MonoBehaviour
{
    public static AssetBundle assetBundle;
    public static void LoadAssets(string path)
    {
        MemoryStream memoryStream;

        Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        memoryStream = new MemoryStream((int)manifestResourceStream.Length);
        manifestResourceStream.CopyTo(memoryStream);
        assetBundle =  AssetBundle.LoadFromMemory(memoryStream.ToArray());

        Comps.PagePrefab = assetBundle.LoadAsset<GameObject>("PagePanel");
        Comps.ItemPrefab = assetBundle.LoadAsset<GameObject>("Item");

        Instantiate(assetBundle.LoadAsset<GameObject>("StandMD"));
        //GameObject.Find("StandMD(Clone)/RealStand").AddComponent<GorillaSurfaceOverride>();
        
        assetBundle.Unload(false);
    }
}
#endif