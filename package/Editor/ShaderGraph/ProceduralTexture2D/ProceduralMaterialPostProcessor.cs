using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
#if UNITY_2020_1_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEditor.Experimental;
using UnityEditor.ShaderGraph;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Object = UnityEngine.Object;

public class ProceduralMaterialPostProcessor : AssetPostprocessor
{
    private const string Tinput = "_" + SampleProceduralTexture2DNode.kTinputName;
    private const string invT = "_" + SampleProceduralTexture2DNode.kInvTinputName;

    [InitializeOnLoadMethod]
    static void SetUpPostProcessors()
    {
        trackers = new List<DependencyTracker>();
        trackers.Add(new DependencyTracker());
    }

    private static List<DependencyTracker> trackers;
    
    class DependencyTracker
    {
        public static string ExtensionFilter => ".mat";
        
        public bool ShouldAddDependencies(string assetPath, out List<object> references)
        {
            references = null;
            if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) != typeof(Material)) return false;
            var mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);

            if (HasProceduralTexture(mat, out var refs))
            {
                references = refs.Cast<object>().ToList();
                return true;
            }

            return false;
        }
    
        public void OnPostprocessAfterAddingDependencies(string assetPath, List<object> references)
        {
            
        }
    
        public void AddDependenciesInPreprocess(string assetPath, List<object> references)
        {
            
        }
    }

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        for(int i = 0; i < importedAssets.Length; i++)
        {
            var type = AssetDatabase.GetMainAssetTypeAtPath(importedAssets[i]);
            if (type == typeof(Material))
            {
                var path = importedAssets[i];
                
                // guess we need to reimport the material so we can modify the import dependencies on OnPreprocessAssets.
                // why is there no OnPostprocessMaterial for things outside models?
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (HasProceduralTexture(mat, out var refs))
                {
                    // TODO can we get the actual, direct artifact dependencies here?
                    var importer = AssetImporter.GetAtPath(path);
                    
                    // first import
                    if(!lastImport.ContainsKey(importer))
                        Debug.LogError("weird import order - OnPostProcessAllAssets called before OnPreprocessAsset??");
                    if(importer.assetTimeStamp == lastImport[importer] && !references.ContainsKey(path))
                    {
                        // set up custom references that we want to inject in OnPreprocessAsset
                        references.Add(path, refs.Cast<object>().ToList());
                        // force a reimport of this asset right now - so we can inject the dependencies in OnPreprocessAsset
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    }
                    // second import
                    else
                    {
                        // clean up
                        references.Remove(path);
                        
                        // apply post-import settings here (basically the actual AssetPostprocessor)
                        // Apply procedural texture settings here
                        foreach (var tex in refs)
                        {
                            var procTex = AssetDatabase.LoadAssetAtPath<ProceduralTexture2D>(AssetDatabase.GetAssetPath(tex.texture));
                            Debug.Log("Applying procedural texture "+ procTex.name + "settings to material " + mat.name, mat);
                            ProceduralTexture2DDrawer.ApplySettings(mat, tex.name, procTex);
                        }
                    }
                }
            }
            else if (type == typeof(ProceduralTexture2D))
            {
                var path = importedAssets[i];
                var proc = AssetDatabase.LoadAssetAtPath<ProceduralTexture2D>(path);
                if (proc.input)
                {
                    var importer = AssetImporter.GetAtPath(path);

                    if (!lastImport.ContainsKey(importer))
                        Debug.LogError("weird import order - OnPostProcessAllAssets called before OnPreprocessAsset??");
                    if (importer.assetTimeStamp == lastImport[importer] && !references.ContainsKey(path))
                    {
                        references.Add(path, new List<object>() { proc.input });
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    }
                    else
                    {
                        references.Remove(path);
                        
                        // regenerate since the source texture has changed
                        ProceduralTexture2DEditor.PreprocessData(proc, null);
                    }
                }
            }
        }
    }

    private static Dictionary<AssetImporter, ulong> lastImport = new Dictionary<AssetImporter, ulong>();
    // private static Dictionary<string, AssetImportContext> contexts = new Dictionary<string, AssetImportContext>();
    private void OnPreprocessAsset()
    {
        var path = context.assetPath;
        
        if (path.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
        {
            var importer = AssetImporter.GetAtPath(path);
            var lastTime = importer.assetTimeStamp;
            
            // TODO best order to clean this up?
            // if (lastImport.ContainsKey(importer))
            //     Debug.LogWarning("this shouldn't happen, weird import order?");
            
            lastImport[importer] = lastTime;
            Debug.Log("Last: " + importer.assetTimeStamp);
            
            if (references.TryGetValue(path, out var refs))
            {
                foreach(var ref2 in refs)
                {
                    if (ref2 is TextureReference tex)
                    {
                        var proceduralTexture2D =
                            AssetDatabase.LoadAssetAtPath<ProceduralTexture2D>(AssetDatabase.GetAssetPath(tex.texture));

                        // set up the actual dependencies that we want to track
                        context.DependsOnArtifact(AssetDatabase.GetAssetPath(tex.texture));
                        context.DependsOnSourceAsset(AssetDatabase.GetAssetPath(tex.texture));
                    }
                }
            }
        }

        if (path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
        {
            if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(ProceduralTexture2D))
            {
                var importer = AssetImporter.GetAtPath(path);
                lastImport[importer] = importer.assetTimeStamp;

                if (references.TryGetValue(path, out var refs))
                {
                    foreach(var ref2 in refs)
                    {
                        // this should be a texture reference
                        var tex = ref2 as Texture2D;
                        if (tex)
                        {
                            var assetPath2 = AssetDatabase.GetAssetPath(tex);
                            context.DependsOnArtifact(assetPath2);
                            context.DependsOnSourceAsset(assetPath2);
                        }
                    }
                }
            }
        }
    }

    struct TextureReference
    {
        public string name;
        public Texture2D texture;
    }
    
    static bool HasProceduralTexture(Material material, out List<TextureReference> references)
    {
        references = null;
        var shader = material.shader;
        if (!shader || !shader.isSupported) return false;

        bool has_Tinput = false;
        bool has_invT = false;
        var propertyCount = ShaderUtil.GetPropertyCount(shader);
        for (int i = 0; i < propertyCount; i++)
        {
            if (ShaderUtil.GetPropertyType(shader, i) != ShaderUtil.ShaderPropertyType.TexEnv)
                continue;

            Debug.Log("searching for properties");

            // this is a texture, get it's name
            var texturePropertyName = ShaderUtil.GetPropertyName(shader, i);
            if (!texturePropertyName.EndsWith(Tinput, StringComparison.Ordinal)) continue;

            // check if there's also the other texture property we're looking for, just to be sure
            var propertyName = texturePropertyName.Substring(0, texturePropertyName.Length - Tinput.Length) + invT;
            // Debug.Log("1st prop: " + texturePropertyName + ", " + "2nd prop: " + propertyName);
            if (shader.FindPropertyIndex(propertyName) < 0) continue;

            // ok, so we found a procedural texture property.
            // let's find the right asset for it
            var texture = (Texture2D) material.GetTexture(texturePropertyName);
            if (!texture) continue;

            if (references == null) references = new List<TextureReference>();
            references.Add(new TextureReference() { name = texturePropertyName, texture = texture});
        }

        if (references?.Count > 0) return true;
        return false;
    }

    private static Dictionary<string, List<object>> references = new Dictionary<string, List<object>>();
    
    void OnPostprocessMaterial(Material material)
    {
        // check if this material's shader has ProceduralTexture2D properties ---
        // we're doing that here by checking for the right property name ends. Not the best way but
        var shader = material.shader;
        if (!shader || !shader.isSupported) return;

        bool has_Tinput = false;
        bool has_invT = false;
        var propertyCount = ShaderUtil.GetPropertyCount(shader);
        for (int i = 0; i < propertyCount; i++)
        {
            if (ShaderUtil.GetPropertyType(shader, i) != ShaderUtil.ShaderPropertyType.TexEnv)
                continue;
            
            Debug.Log("searching for properties");
            
            // this is a texture, get it's name
            var texturePropertyName = ShaderUtil.GetPropertyName(shader, i);
            if (!texturePropertyName.EndsWith(Tinput, StringComparison.Ordinal)) continue;
            
            // check if there's also the other texture property we're looking for, just to be sure
            var propertyName = texturePropertyName.Substring(0, texturePropertyName.Length - Tinput.Length) + invT;
            if (shader.FindPropertyIndex(propertyName) < 0) continue;
            
            // ok, so we found a procedural texture property.
            // let's find the right asset for it
            var texture = (Texture2D) material.GetTexture(texturePropertyName);
            if (!texture) continue;
            
            var assets = AssetDatabase
                .FindAssets("t:ProceduralTexture2D")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ProceduralTexture2D>)
                .ToList();
            
            var availableAssets = assets.ToDictionary(x => x.Tinput, x => x);
            if(availableAssets.TryGetValue(texture, out ProceduralTexture2D proceduralTexture2D))
            {
                if (!AssetDatabase.Contains(proceduralTexture2D))
                {
                    Debug.LogWarning("Dynamic ProceduralTexture2D assets won't properly track dependencies. Make sure to update materials yourself.");
                }
                
                Debug.Log("Added dependency from " + material + " to " + proceduralTexture2D, material);
                context.DependsOnArtifact(AssetDatabase.GetAssetPath(proceduralTexture2D));
            }
        }
    }
    
    
    [MenuItem("internal:ProceduralTexture2D/OutputLibraryPathsForAsset")]
    public static void OutputLibraryPathsForAsset()
    {
        var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
 
        StringBuilder assetPathInfo = new StringBuilder();
        
        var guidString = AssetDatabase.AssetPathToGUID(assetPath);
        //The ArtifactKey is needed here as there are plans to 
        //allow importing for different platforms without switching
        //platform, thus ArtifactKeys will be parametrized in the future
        var artifactKey = new ArtifactKey(new GUID(guidString));
        var artifactID = AssetDatabaseExperimental.LookupArtifact(artifactKey);
        
        //Its possible for an Asset to have multiple import results,
        //if, for example, Sub-assets are present, so we need to iterate
        //over all the artifacts paths
        AssetDatabaseExperimental.GetArtifactPaths(artifactID, out var paths);
 
        assetPathInfo.Append($"Files associated with {assetPath}");
        assetPathInfo.AppendLine();
        
        foreach (var curVirtualPath in paths)
        {
            //The virtual path redirects somewhere, so we get the 
            //actual path on disk (or on the in memory database, accordingly)
            var curPath = Path.GetFullPath(curVirtualPath);
            assetPathInfo.Append("\t" + curPath);
            assetPathInfo.AppendLine();
        }
 
        Debug.Log("Path info for asset:\n"+assetPathInfo.ToString());
    }
    
    [MenuItem("internal:ProceduralTexture2D/GetImporters")]
    static void GetImporters()
    {
        Debug.Log(string.Join("\n", (object[]) AssetDatabase.GetAvailableImporterTypes(AssetDatabase.GetAssetPath(Selection.activeObject))));
    }
}