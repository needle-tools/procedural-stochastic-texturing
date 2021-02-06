#if !NO_INTERNALS_ACCESS

using System;
using UnityEngine;

namespace UnityEditor.ShaderGraph
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Procedural Texture 2D", menuName = "", order = 1)]
    public class ProceduralTexture2D : ScriptableObject, ISerializationCallbackReceiver
    {
        public enum TextureType
        {
            Color,
            Normal,
            Other
        };

        public enum CompressionLevel
        {
            None = -1,
            LowQuality = 0,
            NormalQuality = 50,
            HighQuality = 100
        };

        public Texture2D input = null;
        public TextureType type = TextureType.Color;
        public bool includeAlpha = false;
        public bool generateMipMaps = true;
        public FilterMode filterMode = FilterMode.Trilinear;
        public int anisoLevel = 16;
        public CompressionLevel compressionQuality = ProceduralTexture2D.CompressionLevel.HighQuality;

        public Texture2D Tinput;
        public Texture2D invT;
        public Vector3 colorSpaceOrigin = Vector3.zero;
        public Vector3 colorSpaceVector1 = Vector3.zero;
        public Vector3 colorSpaceVector2 = Vector3.zero;
        public Vector3 colorSpaceVector3 = Vector3.zero;
        public Vector4 compressionScalers = Vector4.zero;

        public long memoryUsageBytes = 0;

        // Currently applied parameters
        public Texture2D currentInput;
        public TextureType currentType;
        public bool currentIncludeAlpha;
        public bool currentGenerateMipMaps;
        public FilterMode currentFilterMode;
        public int currentAnisoLevel;
        public CompressionLevel currentCompressionQuality;
        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            Debug.Log("was deserialized");
        }

        [ContextMenu("Log Custom Importer")]
        private void LogCustom()
        {
            Debug.Log("custom importer was: " + AssetDatabase.GetImporterOverride(AssetDatabase.GetAssetPath(this)));
        }

        private void OnValidate()
        {
            Debug.Log("OnValidate");
        }

        [ContextMenu("Set Custom Importer")]
        void SetCustom()
        {
            var path = AssetDatabase.GetAssetPath(this);
            AssetDatabase.SetImporterOverride<Proc2DImporter>(path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        [ContextMenu("Clear Custom Importer")]
        void ClearCustom()
        {
            var path = AssetDatabase.GetAssetPath(this);
            AssetDatabase.ClearImporterOverride(path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
    }
}

#endif