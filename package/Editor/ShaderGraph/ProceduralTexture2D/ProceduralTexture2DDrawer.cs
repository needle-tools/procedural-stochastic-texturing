using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEngine;

namespace UnityEditor.ShaderGraph
{
    public class ProceduralTexture2DDrawer : MaterialPropertyDrawer
    {
        // public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor) => 0;

        private Dictionary<Texture2D, ProceduralTexture2D> availableAssets;

        public static void AssignTexture(ProceduralTexture2D proceduralTexture2D, Material material)
        {
            
        }
        
        public override void OnGUI (Rect position, MaterialProperty prop, String label, MaterialEditor editor)
        {
            if (availableAssets == null)
            {
                var assets = AssetDatabase
                    .FindAssets("t:ProceduralTexture2D")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<ProceduralTexture2D>)
                    .ToList();
                availableAssets = assets.ToDictionary(x => x.Tinput, x => x);
            }
            
            var kTinputName =            MaterialEditor.GetMaterialProperty(editor.targets, prop.name + "_" + SampleProceduralTexture2DNode.kTinputName);

            var tex = (Texture2D) kTinputName.textureValue;
            ProceduralTexture2D asset = null;
            if (tex && availableAssets.ContainsKey(tex))
                asset = availableAssets[tex];

            EditorGUI.BeginChangeCheck();
            asset = (ProceduralTexture2D) EditorGUI.ObjectField(position, label, asset, typeof(ProceduralTexture2D), false);
            if (EditorGUI.EndChangeCheck())
            {
                ApplySettings(prop.name, asset, editor.targets);

                if(asset) {
                    if (availableAssets.ContainsKey(asset.Tinput))
                        availableAssets[asset.Tinput] = asset;
                    else
                        availableAssets.Add(asset.Tinput, asset);
                }
                
                // can we add this asset as a dependency to the material somehow?
                // can we create a custom thing which has only the job of updating the material?
            }
        }

        public static void ApplySettings(string propertyName, ProceduralTexture2D asset, params UnityEngine.Object[] materials)
        {
            if (materials == null || string.IsNullOrEmpty(propertyName)) return;
            
            var kTinputName2 =           MaterialEditor.GetMaterialProperty(materials, propertyName + "_" + SampleProceduralTexture2DNode.kTinputName);
            var kInvTinputName =         MaterialEditor.GetMaterialProperty(materials, propertyName + "_" + SampleProceduralTexture2DNode.kInvTinputName);
            var kCompressionScalersId =  MaterialEditor.GetMaterialProperty(materials, propertyName + "_" + SampleProceduralTexture2DNode.kCompressionScalersId);
            var kColorSpaceOriginName =  MaterialEditor.GetMaterialProperty(materials, propertyName + "_" + SampleProceduralTexture2DNode.kColorSpaceOriginName);
            var kColorSpaceVector1Name = MaterialEditor.GetMaterialProperty(materials, propertyName + "_" + SampleProceduralTexture2DNode.kColorSpaceVector1Name);
            var kColorSpaceVector2Name = MaterialEditor.GetMaterialProperty(materials, propertyName + "_" + SampleProceduralTexture2DNode.kColorSpaceVector2Name);
            var kColorSpaceVector3Name = MaterialEditor.GetMaterialProperty(materials, propertyName + "_" + SampleProceduralTexture2DNode.kColorSpaceVector3Name);
            var kInputSizeName =         MaterialEditor.GetMaterialProperty(materials, propertyName + "_" + SampleProceduralTexture2DNode.kInputSizeName);

            kTinputName2.textureValue = asset ? asset.Tinput : null;
            kInvTinputName.textureValue = asset ? asset.invT : null;
            kCompressionScalersId.vectorValue = asset ? asset.compressionScalers : Vector4.zero;
            kColorSpaceOriginName.vectorValue = asset ? asset.colorSpaceOrigin : Vector3.zero;
            kColorSpaceVector1Name.vectorValue = asset ? asset.colorSpaceVector1 : Vector3.zero;
            kColorSpaceVector2Name.vectorValue = asset ? asset.colorSpaceVector2 : Vector3.zero;
            kColorSpaceVector3Name.vectorValue = asset ? asset.colorSpaceVector3 : Vector3.zero;
            kInputSizeName.vectorValue = asset ? new Vector3(asset.Tinput.width, asset.Tinput.height, asset.invT.height) : Vector3.one;
        }
        
        public static void ApplySettings(Material mat, string texturePropertyName, ProceduralTexture2D procTex)
        {
            var basePropertyName = texturePropertyName.Substring(0, texturePropertyName.Length - SampleProceduralTexture2DNode.kTinputName.Length - 1);
            // Debug.Log(basePropertyName);
            
            ApplySettings(basePropertyName, procTex, mat);
            AssetDatabase.SaveAssets();
        }
    }
}