#if !NO_INTERNALS_ACCESS

using System.Linq;
using UnityEngine;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEditor.ShaderGraph.Internal;

namespace UnityEditor.ShaderGraph
{
    [Title("Input", "Texture", "Texture Sampler")]
    class TextureSamplerNode : AbstractMaterialNode, IGeneratesBodyCode, IMayRequireMeshUV
    {
        public const int TextureInputId = 0;
        public const int SamplerOutputId = 1;

        const string kTextureInputName = "Texture";
        const string kSamplerOutputName = "Sampler";

        public override bool hasPreview { get { return false; } }

        public TextureSamplerNode()
        {
            name = "Texture Sampler";
            UpdateNodeAfterDeserialization();
        }


        [SerializeField]
        private TextureType m_TextureType = TextureType.Default;

        
        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Texture2DInputMaterialSlot(TextureInputId, kTextureInputName, kTextureInputName));
            AddSlot(new SamplerStateMaterialSlot(SamplerOutputId, kSamplerOutputName, kSamplerOutputName, SlotType.Output)); 
            
            RemoveSlotsNameNotMatching(new[] { TextureInputId, SamplerOutputId });
        }

        #if UNITY_2020_2_OR_NEWER
        public override void Setup()
        #else
        public override void ValidateNode()
        #endif
        {
            var textureSlot = FindInputSlot<Texture2DInputMaterialSlot>(TextureInputId);
            textureSlot.defaultType = Texture2DShaderProperty.DefaultType.White;
            #if UNITY_2020_2_OR_NEWER
            base.Setup();
            #else
            base.ValidateNode();
            #endif
        }

        // Node generations
        public virtual void GenerateNodeCode(ShaderStringBuilder sb, GenerationMode generationMode)
        {
            var id = GetSlotValue(TextureInputId, generationMode);
            var result = string.Format("SAMPLER({0}) = {1};", GetVariableNameForSlot(SamplerOutputId), "sampler" + id);

            sb.AppendLine(result);
        }

        public bool RequiresMeshUV(UVChannel channel, ShaderStageCapability stageCapability)
        {
            return false;
        }
    }
}

#endif