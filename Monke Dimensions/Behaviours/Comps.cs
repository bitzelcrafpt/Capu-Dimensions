﻿#if EDITOR

#else
using Capu_Dimensions.Browser;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Capu_Dimensions.Behaviours;

public class Comps : MonoBehaviour
{
    public static GameObject LeftBtn, RightBtn, LoadBtn, BrowserButton, GarfieldButton, MainScreen;
    public static GameObject PagePrefab, ItemPrefab;

    public static Text AuthorText, NameText, DescriptionText, StatusText;

    public static GameObject Confetti, DownloadingText;

    public static void SetupComps()
    {
        NameText = GameObject.Find("UI/Screen/Main Screen/Name").GetComponent<Text>();
        AuthorText = GameObject.Find("UI/Screen/Main Screen/Author").GetComponent<Text>();
        StatusText = GameObject.Find("UI/Screen/Main Screen/Current").GetComponent<Text>();
        DescriptionText = GameObject.Find("UI/Screen/Main Screen/Description").GetComponent<Text>();
        GameObject.Find("UI/Screen/Browser").AddComponent<DimensionBrowser>();
        MainScreen = GameObject.Find("UI/Screen/Main Screen");
        DownloadingText = GameObject.Find("UI/Screen/Main Screen/Downloading");
        DownloadingText.SetActive(false);

        LoadBtn = GameObject.Find("Buttons/Load Btn");

        RightBtn = GameObject.Find("Buttons/Right Btn");
        LeftBtn = GameObject.Find("Buttons/Left Btn");
        BrowserButton = GameObject.Find("Buttons/Browser Btn");
        GarfieldButton = GameObject.Find("Buttons/Garfield Btn");
    }
}
#endif