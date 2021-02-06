using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEditorInternal;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEngine;

namespace UnityEditor.ShaderGraph
{
    [ScriptedImporter(1, "stochastic-texture", 0)]
    public class Proc2DImporter : ScriptedImporter
    {
        public Texture2D texture;
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            Debug.Log("hello import from " + ctx.assetPath);
            
            // let's find all materials that used the previous version of this import
            
            // let's make a simple main object that's a copy of the input texture
            // if(texture != null)
            // {
            //     var asset = ScriptableObject.CreateInstance<ProceduralTexture2D>();
            //     ctx.AddObjectToAsset("procedural-texture-2d", asset);
            //     ctx.SetMainObject(asset);
            //     ctx.DependsOnArtifact(AssetDatabase.GetAssetPath(texture));
            //
            //     asset.input = texture;
            //
            //     ProceduralTexture2DEditor.PreprocessData(asset, ctx);
            // }
            
            // InternalEditorUtility.SaveToSerializedFileAndForget(new[] {s_Instance}, filePath, saveAsText);
            
            // check file size - is this an empty file?
            // var fileIsEmpty = new FileInfo(ctx.assetPath).Length == 0;
            //
            // void CreateInitialContent()
            // {
            //     var asset = ScriptableObject.CreateInstance<ProceduralTexture2D>();
            //     asset.name = "Procedural Texture 2D";
            //     asset.input = texture;
            //     ctx.DependsOnArtifact(AssetDatabase.GetAssetPath(texture));
            //     ctx.AddObjectToAsset("procedural-texture-2d", asset);
            //     ProceduralTexture2DEditor.PreprocessData(asset, ctx);
            //     ctx.SetMainObject(asset);
            //     var objs = new List<Object>();
            //     ctx.GetObjects(objs);
            //     InternalEditorUtility.SaveToSerializedFileAndForget(objs.ToArray(), ctx.assetPath, true);
            //     AssetDatabase.ImportAsset(ctx.assetPath, ImportAssetOptions.Default);
            // }
            //
            // if (fileIsEmpty)
            // {
            //     CreateInitialContent();
            // }
            // else {
            //     var objects = InternalEditorUtility.LoadSerializedFileAndForget(ctx.assetPath);
            //     if(objects.Any())
            //     {
            //         foreach (var obj in objects)
            //             ctx.AddObjectToAsset(obj.name, obj);
            //         ctx.SetMainObject(objects.FirstOrDefault());
            //     }
            //     else
            //     {
            //         CreateInitialContent();
            //     }
            // }
            
            var objects = InternalEditorUtility.LoadSerializedFileAndForget(ctx.assetPath);
            if(objects.Any())
            {
                foreach (var obj in objects)
                    ctx.AddObjectToAsset(obj.name, obj);
                ctx.SetMainObject(objects.FirstOrDefault());
            }
            Debug.Log("have loaded assets");
        }
    }
}