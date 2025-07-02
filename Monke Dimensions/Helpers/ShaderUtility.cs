#if EDITOR

#else
using Il2CppTMPro;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Capu_Dimensions.Helpers
{
    public class ShaderUtility
    {
        public static void ReplaceAllShaders(GameObject obj)
        {
            foreach (MeshRenderer meshRenderer in obj.GetComponentsInChildren<MeshRenderer>())
            {
                ReplaceShaderForMaterials(meshRenderer.sharedMaterials);
            }

            foreach (SkinnedMeshRenderer skinnedMeshRenderer in obj.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                ReplaceShaderForMaterials(skinnedMeshRenderer.sharedMaterials);
            }

            foreach (TextMeshProUGUI textMeshProUGUI in obj.GetComponentsInChildren<TextMeshProUGUI>())
            {
                ReplaceShaderForMaterial(textMeshProUGUI.fontMaterial);
            }
        }
        public static void ReplaceShaderForMaterials(Material[] materials)
        {
            foreach (Material material in materials)
            {
                ReplaceShaderForMaterial(material);
            }
        }

        public static void ReplaceShaderForMaterial(Material material)
        {
            string shaderName = material.shader.name;
            Shader projectShader = Shader.Find(shaderName);
            material.shader = projectShader;
        }
        public static void ReplaceAllShadersMaps(GameObject obj)
        {
            foreach (MeshRenderer meshRenderer in obj.GetComponentsInChildren<MeshRenderer>())
            {
                ReplaceShaderForMaterialsOther(meshRenderer.sharedMaterials);
            }

            foreach (SkinnedMeshRenderer skinnedMeshRenderer in obj.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                ReplaceShaderForMaterialsOther(skinnedMeshRenderer.sharedMaterials);
            }

            foreach (TextMeshProUGUI textMeshProUGUI in obj.GetComponentsInChildren<TextMeshProUGUI>())
            {
                ReplaceShaderForMaterialsOther(textMeshProUGUI.fontMaterial);
            }
        }
        public static void ReplaceShaderForMaterialsOther(Material[] materials)
        {
            if (materials == null) return;

            foreach (Material material in materials)
            {
                if (material != null)
                    ReplaceShaderForMaterialsOther(material);
            }
        }
        public static void ReplaceShaderForMaterialsOther(Material material)
        {
            if (material == null || material.shader == null)
                return;

            MelonLogger.Msg("replacing shader");

            Shader projectShader = Shader.Find("Shader Graphs/ShadedPiss");
            if (projectShader != null)
            {
                material.shader = projectShader;
            }
            else
            {
                Debug.LogWarning("Shader 'Shader Graphs/Shader' not found.");
            }
        }

    }
}
#endif