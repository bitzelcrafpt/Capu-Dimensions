#if EDITOR

#else
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using Capu_Dimensions.Interaction;
using Capu_Dimensions.Models;
using System.Threading.Tasks;
using Capu_Dimensions.Browser;
//using Capu_Dimensions.Editor;
using MelonLoader;
using Il2CppTMPro;
using Il2CppLocomotion;
//using BepInEx;
using Capu_Dimensions.Helpers;
using Il2Cpp;
namespace Capu_Dimensions.Behaviours;
[RegisterTypeInIl2Cpp]
internal class DimensionManager : MonoBehaviour
{
    public DimensionManager(IntPtr ptr) : base(ptr) { }

    public static DimensionManager Instance { get; private set; }

    public DimensionPackage currentViewingPackage;
    public DimensionPackage currentDimensionPackage;

    internal GameObject loadedDimensionObj;

    private Dictionary<string, DimensionPackage> dimensions = new Dictionary<string, DimensionPackage>();
    private Dictionary<string, AssetBundle> loadedAssetBundles = new Dictionary<string, AssetBundle>();

    public GameObject[] currentLoadedDimensionObjects;

    public int currentPage = 1;
    public List<string> dimensionNames;

    public bool inDimension;

    public GameObject Garfield;


    public GameObject LocalObjectsGameObject;
    void Start()
    {
        Instance = this;

        if (IncompatibleModInstalled())
        {
            Comps.AuthorText.text = "Please delete your mod menu(s) or cosmetx";
            Comps.NameText.text = "If you believe this is a mistake";
            Comps.DescriptionText.text = "Please say 'i got error 404' in the discord server";
            return;
        }

        if (LatestVersion.NeedToUpdate()) 
        {
            Comps.AuthorText.text = "There is a new update for Capu Dimensions.";
            Comps.NameText.text = "Please update to the latest version";
            Comps.DescriptionText.text = "to experience the latest features and fixes.";
        }

        loadedDimensionObj = new GameObject("LoadedDimension");
        ButtonSetup();
        LoadDimensions();
    }

    public void LoadDimensions()
    {
        LocalObjectsGameObject = GameObject.Find("Global/Levels");
        Garfield = GameObject.Find("Garfield");
        Garfield.SetActive(false);
#if DEBUG
        MelonLogger.Msg("-> Found Dimension(s): <-");
#endif
        string path = Path.Combine(Path.GetDirectoryName(typeof(Main).Assembly.Location), "Dimensions");
        string[] dimensionFiles = Directory.GetFiles(path, "*.dimension");

        dimensionNames = new List<string>();

        foreach (string dimensionFile in dimensionFiles)
        {
            string dimensionName = Path.GetFileNameWithoutExtension(dimensionFile);

            dimensionNames.Add(dimensionName);
            LoadDimension(dimensionFile);
        }
    }
    float scaleInterval = 0.02f;
    Vector3 scaleVector;
    private void ButtonSetup()
    {
        Comps.LeftBtn.AddComponent<Button>().BtnType = ButtonType.Left;
        Comps.RightBtn.AddComponent<Button>().BtnType = ButtonType.Right;
        Comps.LoadBtn.AddComponent<Button>().BtnType = ButtonType.Load;
        Comps.BrowserButton.AddComponent<Button>().BtnType = ButtonType.Browser;
        Comps.GarfieldButton.AddComponent<Button>().BtnType = ButtonType.Garfield;
        scaleVector = new Vector3(scaleInterval, scaleInterval, scaleInterval);
        Comps.LeftBtn.GetComponent<BoxCollider>().size = scaleVector;
        Comps.RightBtn.GetComponent<BoxCollider>().size = scaleVector;
        Comps.LoadBtn.GetComponent<BoxCollider>().size = scaleVector;
        Comps.BrowserButton.GetComponent<BoxCollider>().size = scaleVector;
        Comps.GarfieldButton.GetComponent<BoxCollider>().size = scaleVector;

    }

    string name;
    string author;
    string description;
    string spawnpospath;
    string terminalpospath;
    bool usePost;
    bool useShader;

    private void LoadDimension(string dimensionFile)
    {
        string currentPath = Path.GetFullPath(dimensionFile);

        using (var zip = ZipFile.OpenRead(currentPath))
        {
            ZipArchiveEntry packageEntry = zip.GetEntry("Package.json");

            if (packageEntry == null)
            {
                MelonLogger.Error("Invalid dimension: " + currentPath);
                return;
            }

            using (StreamReader packageReader = new StreamReader(packageEntry.Open()))
            {
                DimensionPackage package = Newtonsoft.Json.JsonConvert.DeserializeObject<DimensionPackage>(packageReader.ReadToEnd());
                string jsonContent = packageReader.ReadToEnd();
                name = package.Name;
                author = package.Author;
                description = package.Description;
                spawnpospath = package.spawnPosPath;
                terminalpospath = package.terminalPosPath;
                usePost = package.UsePostProcessing;
                useShader = package.UseNewShader;
                MelonLogger.Msg($"[Capu Dimensions Map] -> Name: {package.Name}, Author: {package.Author}");
                dimensions.Add(dimensionFile, package);
            }

        }
    }
    object routine = null;
    private System.Collections.IEnumerator LoadBundleCoroutine(byte[] bundleBytes, string assetBundleName)
    {
        AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(bundleBytes);
        yield return assetBundleCreateRequest;

        AssetBundle bundle = assetBundleCreateRequest.assetBundle;
        if (bundle == null)
        {
            MelonLogger.Error("Failed to load asset bundle.");
            yield break;
        }

        MelonLogger.Msg("bundle loaded!!.");

        if (!loadedAssetBundles.ContainsKey(assetBundleName))
        {
            loadedAssetBundles[assetBundleName] = bundle;
        }
        else
        {
            //assetBundle.Unload(true);
            bundle = loadedAssetBundles[assetBundleName];
        }
        routine = null;
    }
    //for some reason this only works on the second click????
    private async void LoadAssets(string zipFilePath, string assetBundleName)
    {
        if(routine != null)
        {
            MelonCoroutines.Stop(routine);
        }
        if (loadedAssetBundles.TryGetValue(assetBundleName, out AssetBundle cachedAssetBundle))
        {
            if(loadedAssetBundles.Count == 2)
            {
                loadedAssetBundles.Clear();
                MelonLogger.Msg("UNLOADING");
                cachedAssetBundle.Unload(true);
                return;
            }
            InstantiateDimensions(cachedAssetBundle);
            return;
        }

        Comps.DownloadingText.SetActive(true);
        Comps.DownloadingText.GetComponent<UnityEngine.UI.Text>().text = "Loading...";

        byte[] zipBytes = File.ReadAllBytes(zipFilePath);
        byte[] bundleBytes;

        using (MemoryStream zipStream = new MemoryStream(zipBytes))
        using (ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read))

        {

            // Debug: List all ZIP entries
            foreach (var entry in zipArchive.Entries)
            {
                MelonLogger.Msg($"ZIP Entry: {entry.FullName} ({entry.Length} bytes)");
                if (!entry.FullName.Contains('.'))
                {
                    // This is the bundle file, load it into bundleBytes
                    using (var stream = entry.Open())
                    using (var ms = new MemoryStream())
                    {
                        await stream.CopyToAsync(ms);
                        bundleBytes = ms.ToArray();
                        MelonLogger.Msg($"Loaded asset bundle bytes from {entry.FullName}");
                    }
                }
                else if (entry.FullName == "Package.json")
                {
                    using (Stream packageStream = entry.Open())
                    using (StreamReader reader = new StreamReader(packageStream))
                    {
                        string packageContent = await reader.ReadToEndAsync();
                        MelonLogger.Msg($"Package.json content:\n{packageContent}");

                        // Deserialize from packageContent string
                        DimensionPackage package = Newtonsoft.Json.JsonConvert.DeserializeObject<DimensionPackage>(packageContent);

                        MelonLogger.Msg("package");
                        name = package.Name;
                        MelonLogger.Msg("package3");
                        author = package.Author;
                        MelonLogger.Msg("package4");
                        description = package.Description;
                        MelonLogger.Msg("package5");
                        spawnpospath = package.spawnPosPath;
                        MelonLogger.Msg("package6");
                        terminalpospath = package.terminalPosPath;
                        MelonLogger.Msg("package7");
                        usePost = package.UsePostProcessing;
                        MelonLogger.Msg("package8");
                        useShader = package.UseNewShader;
                        MelonLogger.Msg($"[Capu Dimensions Map FROMPACKAGEJSON] -> Name: {package.Name}, Author: {package.Author}");
                    }

                }
            }

            // Pick the first file that has no extension (i.e., no dot after the last slash)
            ZipArchiveEntry selectedEntry = zipArchive.Entries
                .Where(entry => !string.IsNullOrEmpty(entry.Name) && !entry.Name.Contains('.'))
                .FirstOrDefault();

            if (selectedEntry == null)
            {
                MelonLogger.Error("No valid entries found in ZIP archive.");
                return;
            }

            MelonLogger.Msg($"Selected entry '{selectedEntry.FullName}'");

            using (Stream entryStream = selectedEntry.Open())
            using (MemoryStream bundleStream = new MemoryStream())
            {
                await entryStream.CopyToAsync(bundleStream);
                bundleBytes = bundleStream.ToArray();
                MelonLogger.Msg($"bundleBytes size: {bundleBytes.Length}");
            }
        }
        routine = MelonCoroutines.Start(LoadBundleCoroutine(bundleBytes, assetBundleName));


        //AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(bundleBytes);

        //while (!assetBundleCreateRequest.isDone)
        //    await Task.Yield();

        //AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;

        //if (!loadedAssetBundles.ContainsKey(assetBundleName))
        //{
        //    loadedAssetBundles[assetBundleName] = assetBundle;
        //}
        //else
        //{
        //    //assetBundle.Unload(true);
        //    assetBundle = loadedAssetBundles[assetBundleName];
        //}
        //MelonLogger.Msg("LOG 41");
        //Il2CppAssetBundle thing = null;
        //MelonLogger.Msg(bundleBytes);
        //MelonLogger.Msg(zipFilePath);
        //MelonLogger.Msg(assetBundleName);
        //for (int attempt = 0; attempt < 50; attempt++)
        //{
        //    thing = Il2CppAssetBundleManager.LoadFromMemory(bundleBytes);
        //    if (thing != null)
        //        break;

        //    MelonLogger.Warning($"Attempt {attempt + 1}: Failed to load asset bundle, retrying...");
        //    await Task.Delay(100); // wait a bit
        //}

        //if (thing == null)
        //{
        //    await Task.Delay(5000);
        //    MelonLogger.Error("Failed to load AssetBundle using Il2CppAssetBundleManager after retries.");
        //    if (loadedAssetBundles.TryGetValue(assetBundleName, out AssetBundle aaaa))
        //    {
        //        MelonLogger.Msg("LOG 2");
        //        await Task.Delay(5000);
        //        InstantiateDimensions(aaaa);
        //        MelonLogger.Msg("LOG 3");
        //        return;
        //    }
        //    return;
        //}


        //MelonLogger.Msg("LOG 46");
        //string[] assetNames = thing.GetAllAssetNames();
        //foreach (string assetName in assetNames)
        //{
        //    GameObject asset = thing.LoadAsset<GameObject>(assetName);
        //    if (asset != null)
        //    {
        //        MelonLogger.Msg($"Loaded GameObject: {asset.name}");
        //        UnityEngine.Object.Instantiate(asset);
        //    }
        //    else
        //    {
        //        MelonLogger.Msg($"Asset '{assetName}' is not a GameObject or failed to load.");
        //    }
        //}

        //MelonLogger.Msg("LOG 47");
    }
    DimensionDescriptor? dimDesc;
    private void InstantiateDimensions(AssetBundle assetBundle)
    {
        if (!Main.inModded)
        return;

        string[] assetNames;
        try
        {
            assetNames = assetBundle.GetAllAssetNames();
            MelonLogger.Msg($"Found {assetNames.Length} assets in bundle.");
        }
        catch (Exception e)
        {
            MelonLogger.Msg("Exception calling GetAllAssetNames: " + e);
            return;
        }
        MelonLogger.Msg("assetNames");
        MelonLogger.Msg(assetNames[0]);

        List<GameObject> gameObjects = new List<GameObject>();
        MelonLogger.Msg("gameObjects");
        foreach (string assetName in assetNames)
        {
            MelonLogger.Msg("foreach");
            try
            {
                MelonLogger.Msg("loadasset");
                var asset = GameObject.Instantiate(assetBundle.LoadAsset<GameObject>(assetName)) as GameObject;

                if (asset != null)
                {
                    MelonLogger.Msg($"Loaded GameObject: {asset.name}");
                    //gameObjects.Add(asset);
                    gameObjects.Add(asset);
                }
                else
                {
                    MelonLogger.Msg($"Asset '{assetName}' is not a GameObject or failed to load.");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Msg($"Exception loading asset '{assetName}': {ex}");
                // continue trying to load others
            }
        }

        currentLoadedDimensionObjects = gameObjects.ToArray();

        if (currentLoadedDimensionObjects.Length == 0)
        {
            MelonLogger.Msg("No GameObjects found in asset bundle!");
            return;
        }

        if (loadedDimensionObj == null)
        {
            MelonLogger.Msg("loadedDimensionObj is null!");
            return;
        }
        foreach (GameObject loadedObject in currentLoadedDimensionObjects)
        {
            GameObject instantiatedObject = Instantiate(loadedObject, loadedDimensionObj.transform);
            instantiatedObject.transform.SetParent(loadedDimensionObj.transform);
            //SetupSurface(instantiatedObject);
        }
        GameObject.Find(currentLoadedDimensionObjects.First().gameObject.name + "(Clone)").transform.position = new Vector3(0f, 0, 0f);

        dimDesc = currentLoadedDimensionObjects.First().GetComponent<DimensionDescriptor>();
        if (dimDesc == null)
        {
            MelonLogger.Msg($"Adding DimensionDescriptor to {currentLoadedDimensionObjects.First().name} at runtime");
            dimDesc = currentLoadedDimensionObjects.First().AddComponent<DimensionDescriptor>();
            dimDesc.Name = name;
            MelonLogger.Msg(name);
            dimDesc.Author = author;
            MelonLogger.Msg(author);
            dimDesc.Description = description;
            MelonLogger.Msg(description);
            int slashIndex = spawnpospath.IndexOf('/');
            string resultSpawn;
            string resultTerminal;
            if (slashIndex != -1)
            {
                string suffix = spawnpospath.Substring(slashIndex);  // includes '/'
                resultSpawn = name + "(Clone)" + suffix;
                MelonLogger.Msg(resultSpawn);
                dimDesc.SpawnPosition = GameObject.Find(resultSpawn);
                MelonLogger.Msg("set spawnpospath!");
                MelonLogger.Msg(dimDesc.SpawnPosition.gameObject.name);
                resultTerminal = name + "(Clone)" + terminalpospath.Substring(slashIndex);
                dimDesc.TerminalPosition = GameObject.Find(resultTerminal);
                MelonLogger.Msg("set terminalpospath!");
                MelonLogger.Msg(dimDesc.TerminalPosition.gameObject.name);
            }
            else
            {
                MelonLogger.Msg("how did we get here");
            }
        }
        if(useShader)
        {
            ShaderUtility.ReplaceAllShadersMaps(loadedDimensionObj);

        }
        else
        {
            ShaderUtility.ReplaceAllShaders(loadedDimensionObj);
        }
        if(!usePost)
        {
            GameObject.Find("Gloabl/Managment/CapuchinLighting").SetActive(false);
        }
        loadedDimensionObj.transform.position = new Vector3(650f, 200f, 0f);
        Player.Instance.playerCam.farClipPlane += 7500;
        Transform parentTransform = loadedDimensionObj.transform;
        for (int i = parentTransform.childCount - 1; i >= 0; i--)
        {
            Transform child = parentTransform.GetChild(i);
            UnityEngine.Object.Destroy(child.gameObject);
        }
        TeleportDimension.OnTeleport(currentDimensionPackage);
        inDimension = true;
        LocalObjectsGameObject.SetActive(false);
        Comps.DownloadingText.SetActive(false);
        Comps.DownloadingText.GetComponent<UnityEngine.UI.Text>().text = "Downloading...";
    }


    private void SetupSurface(GameObject obj)
    {
        //if (obj.GetComponent<GorillaSurfaceOverride>() == null) obj.AddComponent<GorillaSurfaceOverride>();

        //if (obj.GetComponent<Animation>() != null && obj.GetComponent<MovingPlatform>() == null) obj.AddComponent<MovingPlatform>();

        if (obj.transform.childCount > 0)
        {
            foreach (Transform child in obj.transform)
            {
                SetupSurface(child.gameObject);
            }
        }
    }

    //public async void LoadSelectedDimension(string dimensionName)
    //{
    //    if (inDimension)
    //    {
    //        MelonLogger.Msg("indimension");
    //        TeleportDimension.ReturnToSpawn(currentDimensionPackage);
    //        UnloadCurrentDimension();

    //        return;
    //    }
    //    else
    //    {

    //        //string dimensionFilePath = Path.Combine(Paths.PluginPath, "Dimensions", $"{dimensionName}.dimension");
    //        //if (!File.Exists(dimensionFilePath))
    //        //{
    //        string dimensionFilePath = Path.Combine(Path.GetDirectoryName(typeof(DimensionManager).Assembly.Location), "Dimensions", $"{dimensionName}.dimension");
    //        //}
    //        MelonLogger.Msg("dimensionfilepath");
    //        MelonLogger.Msg(dimensionFilePath);
    //        currentDimensionPackage = currentViewingPackage;
    //        MelonLogger.Msg("currentDimensionPackage");
    //        MelonLogger.Msg(currentDimensionPackage);

    //        LoadAssets(dimensionFilePath, currentDimensionPackage.Name);

    //        LoadAssets(dimensionFilePath, currentDimensionPackage.Name);
    //    }
    //}
    

    public void LoadSelectedDimension(string dimensionName)
    {
        MelonCoroutines.Start(LoadSelectedDimensionCoroutine(dimensionName));
    }
    private System.Collections.IEnumerator LoadSelectedDimensionCoroutine(string dimensionName)
    {
        //if(firstTime)
        //{
            if (inDimension)
            {
                MelonLogger.Msg("indimension");
                TeleportDimension.ReturnToSpawn(currentDimensionPackage);
                UnloadCurrentDimension();
                yield break;
            }

            string dimensionFilePath = Path.Combine(
                Path.GetDirectoryName(typeof(DimensionManager).Assembly.Location),
                "Dimensions",
                $"{dimensionName}.dimension");

            MelonLogger.Msg("dimensionfilepath");
            MelonLogger.Msg(dimensionFilePath);
            currentDimensionPackage = currentViewingPackage;
            MelonLogger.Msg("currentDimensionPackage");
            MelonLogger.Msg(currentDimensionPackage);

            LoadAssets(dimensionFilePath, currentDimensionPackage.Name);

            yield return new WaitForSeconds(10f); // horrendous and scary fix to the code!

            LoadAssets(dimensionFilePath, currentDimensionPackage.Name);
        /*   firstTime = false;
        }
        else
        {
            if (inDimension)
            {
                MelonLogger.Msg("indimension");
                TeleportDimension.ReturnToSpawn(currentDimensionPackage);
                UnloadCurrentDimension();
                yield break;
            }

            string dimensionFilePath = Path.Combine(
                Path.GetDirectoryName(typeof(DimensionManager).Assembly.Location),
                "Dimensions",
                $"{dimensionName}.dimension");

            MelonLogger.Msg("dimensionfilepath");
            MelonLogger.Msg(dimensionFilePath);
            currentDimensionPackage = currentViewingPackage;
            MelonLogger.Msg("currentDimensionPackage");
            MelonLogger.Msg(currentDimensionPackage);

            LoadAssets(dimensionFilePath, currentDimensionPackage.Name);
        }*/
    }


    private bool IncompatibleModInstalled()
    {
        //foreach (PluginInfo pluginInfo in BepInEx.Bootstrap.Chainloader.PluginInfos.Values)
        //{
        //    if (pluginInfo.Metadata.GUID.ToLower().Contains("iidk") ||
        //        pluginInfo.Metadata.GUID.ToLower().Contains("menu") ||
        //        pluginInfo.Metadata.GUID.ToLower().Contains("cosmetx") ||
        //        pluginInfo.Metadata.GUID.ToLower().Contains("shibagt") ||
        //        pluginInfo.Metadata.GUID.ToLower().Contains("displyy"))
        //    {
        //        if (pluginInfo.Metadata.GUID == "com.wryser.gorillatag.gorillamenu")
        //            return false;

        //        return true;
        //    }
        //}
        return false;
    }

    public void UnloadCurrentDimension()
    {
        //foreach(WaterObject water in FindObjectsOfType<WaterObject>())
        //    Destroy(water.ogWater);
        MelonLogger.Msg("UNLOADINGGGGGGGGGGGGGGGGGGGGGGGG");
        Transform parentTransform2 = dimDesc.transform;
        if(dimDesc != null)
        {
            for (int i = parentTransform2.childCount - 1; i >= 0; i--)
            {
                Transform child2 = parentTransform2.GetChild(i);
                UnityEngine.Object.Destroy(child2.gameObject);
            }
            UnityEngine.Object.Destroy(parentTransform2.gameObject);
        }
        else
        {
            MelonLogger.Msg("dimdesc null!");
        }
        currentLoadedDimensionObjects = null;
        inDimension = false;
        loadedDimensionObj.transform.position = new Vector3(0f, 0f, 0f);
        LocalObjectsGameObject.SetActive(true);
        Player.Instance.playerCam.farClipPlane -= 7500;
        foreach(KeyValuePair<string, AssetBundle> thing in loadedAssetBundles)
        {
            thing.Value.Unload(true);
        }
        loadedAssetBundles.Clear();
    }

    public void SwitchPage(int direction)
    {
        MelonLogger.Msg("SwitchPage");
        int totalPages = dimensionNames.Count;

        currentPage = (currentPage + direction + totalPages) % totalPages;

        if (currentPage >= 0 && currentPage < dimensions.Count)
        {
            DimensionPackage selectedDimension = dimensions.Values.ElementAt(currentPage);

            Comps.AuthorText.text = $"AUTHOR: {selectedDimension.Author}".ToUpper();
            Comps.NameText.text = $"DIMENSION: {selectedDimension.Name}".ToUpper();
            Comps.DescriptionText.text = selectedDimension.Description.ToUpper();
            Comps.StatusText.text = $"DIMENSIONS FOUND: ({currentPage + 1} / {totalPages})";
            currentViewingPackage = selectedDimension;
        }
    }

    public void LoadDownloadedDimension()
    {
        string path = Path.Combine(Path.GetDirectoryName(typeof(DimensionManager).Assembly.Location), "Dimensions");
        var dimensionFiles = Directory.GetFiles(path, "*.dimension");

        foreach (string dimensionFile in dimensionFiles)
        {
            if (!dimensions.ContainsKey(dimensionFile))
            {
                string dimensionName = Path.GetFileNameWithoutExtension(dimensionFile);
                dimensionNames.Add(dimensionName);
                LoadDimension(dimensionFile);
            }
        }
    }

#if DEBUG
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 200, 500));

        GUILayout.Label("Available Dimensions:");
        if(dimensionNames != null)
        {
            foreach (string dimensionName in dimensionNames)
            {
                if (GUILayout.Button(dimensionName))
                {
                    LoadSelectedDimension(dimensionName);
                }
            }
        }
        
        //GUILayout.Space(20);
        GUILayout.Label("Page Navigation:");
        if (GUILayout.Button("Previous Page"))
        {
            SwitchPage(-1);
        }
        if (GUILayout.Button("Next Page"))
        {
            SwitchPage(1);
        }

        //GUILayout.Space(20);

        GUILayout.Label($"Current Page: {currentPage + 1}");

        GUILayout.Label("Load Current Dimension:");

        if (GUILayout.Button("Load Current Dimension"))
        {
            if(dimensionNames != null)
            {
                LoadSelectedDimension(dimensionNames[currentPage]);
            }
        }
        //GUILayout.Space(20);
        if (GUILayout.Button("Join Crafterbot"))
        {
            //Utilla.Utils.RoomUtils.JoinPrivateLobby("CRAFTERBOT");
        }
        //GUILayout.Space(30);
        GUILayout.Label("Browser Stuff");
        //GUILayout.Space(10);

        if (GUILayout.Button("Open Browser"))
        {
            DimensionBrowser.inBrowser = true;
            DimensionBrowser.instance.OnBrowserEnabled();
        }
        if (GUILayout.Button("Next Page"))
        {
            DimensionBrowser.instance.NextPage();
        }
        if (GUILayout.Button("Previous Page"))
        {
            DimensionBrowser.instance.PreviousPage();
        }
        GUILayout.EndArea();
    }
#endif
}
#endif