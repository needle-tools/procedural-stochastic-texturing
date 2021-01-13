using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

namespace UnityEditor.ShaderGraph.Internal
{
    [Serializable]
    [BlackboardInputInfo(50)]
    public sealed class ProceduralTexture2DProperty : AbstractShaderProperty<SerializableProceduralTexture2D> // SerializableTexture
    {
        public enum DefaultType { White, Black, Grey, Bump }

        internal ProceduralTexture2DProperty()
        {
            displayName = "ProceduralTexture2D";
            value = null;
        }

        public override PropertyType propertyType => PropertyType.Texture2D;

        #if !UNITY_2020_1_OR_NEWER
        internal override bool isBatchable => false;
        #endif
        internal override bool isExposable => true;
        internal override bool isRenamable => true;

        internal string modifiableTagString => modifiable ? "" : "[NonModifiableTextureData]";

        internal override string GetPropertyBlockString()
        {
            return $"{hideTagString}{modifiableTagString}[NoScaleOffset]{referenceName}(\"{displayName}\", 2D) = \"{defaultType.ToString().ToLower()}\" {{}}";
        }

        #if !UNITY_2020_1_OR_NEWER
        internal override string GetPropertyDeclarationString(string delimiter = ";")
        {
            return $"TEXTURE2D({referenceName}){delimiter} SAMPLER(sampler{referenceName}){delimiter} {concretePrecision.ToShaderString()}4 {referenceName}_TexelSize{delimiter}";
        }
        #endif

        internal override void ForeachHLSLProperty(Action<HLSLProperty> action)
        {
            HLSLDeclaration decl = (generatePropertyBlock ? HLSLDeclaration.UnityPerMaterial : HLSLDeclaration.Global);

            action(new HLSLProperty(HLSLType._Texture2D, referenceName, HLSLDeclaration.Global));
            action(new HLSLProperty(HLSLType._SamplerState, "sampler" + referenceName, HLSLDeclaration.Global));
            action(new HLSLProperty(HLSLType._float4, referenceName + "_TexelSize", decl));
        }

        internal override string GetPropertyAsArgumentString()
        {
            return $"TEXTURE2D_PARAM({referenceName}, sampler{referenceName}), {concretePrecision.ToShaderString()}4 {referenceName}_TexelSize";
        }

        [SerializeField]
        bool m_Modifiable = true;

        internal bool modifiable
        {
            get => m_Modifiable;
            set => m_Modifiable = value;
        }

        [SerializeField]
        DefaultType m_DefaultType = DefaultType.White;

        public DefaultType defaultType
        {
            get { return m_DefaultType; }
            set { m_DefaultType = value; }
        }

        internal override AbstractMaterialNode ToConcreteNode()
        {
            return new ProceduralTexture2DNode() { proceduralTexture2D = value.texture };
        }

        internal override PreviewProperty GetPreviewMaterialProperty()
        {
            return new PreviewProperty(propertyType)
            {
                name = referenceName,
                textureValue = value?.texture ? value?.texture.Tinput : null
            };
        }

        internal override ShaderInput Copy()
        {
            return new ProceduralTexture2DProperty()
            {
                displayName = displayName,
                hidden = hidden,
                value = value
            };
        }
    }
    
    [Serializable]
    public sealed class SerializableProceduralTexture2D : ISerializationCallbackReceiver
    {
        [SerializeField]
        string m_SerializedTexture;

        [SerializeField]
        string m_Guid;

        [NonSerialized]
        ProceduralTexture2D m_Texture;

        public ProceduralTexture2D texture
        {
            get
            {
                if (!string.IsNullOrEmpty(m_SerializedTexture))
                {
                    var textureHelper = new ProceduralTexture2DNode.ProceduralTexture2DSerializer();
                    EditorJsonUtility.FromJsonOverwrite(m_SerializedTexture, textureHelper);
                    m_SerializedTexture = null;
                    m_Guid = null;
                    m_Texture = textureHelper.proceduralTexture2DAsset;
                }
                else if (!string.IsNullOrEmpty(m_Guid) && m_Texture == null)
                {
                    m_Texture = AssetDatabase.LoadAssetAtPath<ProceduralTexture2D>(AssetDatabase.GUIDToAssetPath(m_Guid));
                    m_Guid = null;
                }

                return m_Texture;
            }
            set
            {
                m_Texture = value;
                m_Guid = null;
                m_SerializedTexture = null;
            }
        }

        public void OnBeforeSerialize()
        {
            m_SerializedTexture = EditorJsonUtility.ToJson(new ProceduralTexture2DNode.ProceduralTexture2DSerializer { proceduralTexture2DAsset = texture }, false);
        }

        public void OnAfterDeserialize()
        {
        }
    }
}