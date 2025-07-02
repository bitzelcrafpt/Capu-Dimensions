using UnityEngine;
namespace Capu_Dimensions.Models;
#if !EDITOR
using MelonLoader;

[RegisterTypeInIl2Cpp]
#endif
public class DimensionDescriptor : MonoBehaviour
{
#if !EDITOR
    public DimensionDescriptor(IntPtr ptr) : base(ptr) { }
#endif
    public string Name;
    public string Author;
    public string Description;

    public GameObject SpawnPosition;
    public GameObject TerminalPosition;

    public Texture2D Photo;
    public bool UsePostProcessing = true;
    public bool UseNewShader = true;
    public bool ExperimentalBaking = true;
}