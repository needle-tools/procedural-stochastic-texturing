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
            
            var kTinputName = MaterialEditor.GetMaterialProperty(editor.targets, prop.name + "_" + SampleProceduralTexture2DNode.kTinputName);
            var kInvTinputName = MaterialEditor.GetMaterialProperty(editor.targets, prop.name + "_" + SampleProceduralTexture2DNode.kInvTinputName);
            var kCompressionScalersId = MaterialEditor.GetMaterialProperty(editor.targets, prop.name + "_" + SampleProceduralTexture2DNode.kCompressionScalersId);
            var kColorSpaceOriginName = MaterialEditor.GetMaterialProperty(editor.targets, prop.name + "_" + SampleProceduralTexture2DNode.kColorSpaceOriginName);
            var kColorSpaceVector1Name = MaterialEditor.GetMaterialProperty(editor.targets, prop.name + "_" + SampleProceduralTexture2DNode.kColorSpaceVector1Name);
            var kColorSpaceVector2Name = MaterialEditor.GetMaterialProperty(editor.targets, prop.name + "_" + SampleProceduralTexture2DNode.kColorSpaceVector2Name);
            var kColorSpaceVector3Name = MaterialEditor.GetMaterialProperty(editor.targets, prop.name + "_" + SampleProceduralTexture2DNode.kColorSpaceVector3Name);
            var kInputSizeName = MaterialEditor.GetMaterialProperty(editor.targets, prop.name + "_" + SampleProceduralTexture2DNode.kInputSizeName);

            var tex = (Texture2D) kTinputName.textureValue;
            ProceduralTexture2D asset = null;
            if (tex && availableAssets.ContainsKey(tex))
                asset = availableAssets[tex];

            EditorGUI.BeginChangeCheck();
            asset = (ProceduralTexture2D) EditorGUI.ObjectField(position, label, asset, typeof(ProceduralTexture2D), false);
            if (EditorGUI.EndChangeCheck())
            {
                kTinputName.textureValue = asset.Tinput;
                kInvTinputName.textureValue = asset.invT;
                kCompressionScalersId.vectorValue = asset.compressionScalers;
                kColorSpaceOriginName.vectorValue = asset.colorSpaceOrigin;
                kColorSpaceVector1Name.vectorValue = asset.colorSpaceVector1;
                kColorSpaceVector2Name.vectorValue = asset.colorSpaceVector2;
                kColorSpaceVector3Name.vectorValue = asset.colorSpaceVector3;
                kInputSizeName.vectorValue = new Vector3(asset.Tinput.width, asset.Tinput.height, asset.invT.height);

                if (availableAssets.ContainsKey(asset.Tinput))
                    availableAssets[asset.Tinput] = asset;
                else
                    availableAssets.Add(asset.Tinput, asset);
            }
        }
    }
}