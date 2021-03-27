#if !NO_INTERNALS_ACCESS

using System.Linq;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.ShaderGraph
{
    [Title("Input", "Texture", "Sample Procedural Texture 2D")]
    class SampleProceduralTexture2DNode : AbstractMaterialNode,
	    IGeneratesBodyCode,
	    #if !UNITY_2020_2_OR_NEWER
	    IHasSettings,
	    #endif
	    IMayRequireMeshUV
    {
        public const int OutputSlotRGBAId = 0;
        public const int OutputSlotRId = 1;
        public const int OutputSlotGId = 2;
        public const int OutputSlotBId = 3;
        public const int OutputSlotAId = 4;
        public const int ProceduralTexture2DId = 5;
        // public const int SamplerInput = 6;
        public const int UVInput = 7;
        public const int BlendId = 8;
        public const int TinputId = 9;
        public const int InvTinputId = 10;
        public const int CompressionScalersId = 11;
        public const int ColorSpaceOriginId = 12;
        public const int ColorSpaceVector1Id = 13;
        public const int ColorSpaceVector2Id = 14;
        public const int ColorSpaceVector3Id = 15;
        public const int InputSizeId = 16;

        const string kOutputSlotRGBAName = "RGBA";
        const string kOutputSlotRName = "R";
        const string kOutputSlotGName = "G";
        const string kOutputSlotBName = "B";
        const string kOutputSlotAName = "A";
        const string kProceduralTexture2DName = "ProceduralTex2D";
        // const string kSamplerInputName = "Sampler";
        const string kUVInputName = "UV";
        const string kBlendIdName = "Blend";
        
        internal const string kTinputName = "Tinput";
        internal const string kInvTinputName = "invT";
        internal const string kCompressionScalersId = "compressionScalers";
        internal const string kColorSpaceOriginName = "colorSpaceOrigin";
        internal const string kColorSpaceVector1Name = "colorSpaceVector1";
        internal const string kColorSpaceVector2Name = "colorSpaceVector2";
        internal const string kColorSpaceVector3Name = "colorSpaceVector3";
        internal const string kInputSizeName = "inputSize";

        public override bool hasPreview { get { return true; } }

        public SampleProceduralTexture2DNode()
        {
            name = "Sample Procedural Texture 2D";
            UpdateNodeAfterDeserialization();
        }

        public override string documentationURL
        {
            get { return "https://github.com/Unity-Technologies/ShaderGraph/wiki/Sample-Texture-2D-Node"; }
        }

        public sealed override void UpdateNodeAfterDeserialization()
        {
            // Output slots
            AddSlot(new Vector4MaterialSlot(OutputSlotRGBAId, kOutputSlotRGBAName, kOutputSlotRGBAName, SlotType.Output, Vector4.zero, ShaderStageCapability.Fragment));
            AddSlot(new Vector1MaterialSlot(OutputSlotRId, kOutputSlotRName, kOutputSlotRName, SlotType.Output, 0, ShaderStageCapability.Fragment));
            AddSlot(new Vector1MaterialSlot(OutputSlotGId, kOutputSlotGName, kOutputSlotGName, SlotType.Output, 0, ShaderStageCapability.Fragment));
            AddSlot(new Vector1MaterialSlot(OutputSlotBId, kOutputSlotBName, kOutputSlotBName, SlotType.Output, 0, ShaderStageCapability.Fragment));
            AddSlot(new Vector1MaterialSlot(OutputSlotAId, kOutputSlotAName, kOutputSlotAName, SlotType.Output, 0, ShaderStageCapability.Fragment));

            // Input slots
            AddSlot(new ProceduralTexture2DInputMaterialSlot(ProceduralTexture2DId, kProceduralTexture2DName, kProceduralTexture2DName, ShaderStageCapability.Fragment, false));
            AddSlot(new UVMaterialSlot(UVInput, kUVInputName, kUVInputName, UVChannel.UV0));
            AddSlot(new Vector1MaterialSlot(BlendId, kBlendIdName, kBlendIdName, SlotType.Input, 0, ShaderStageCapability.Fragment));

            RemoveSlotsNameNotMatching(new[] { OutputSlotRGBAId, OutputSlotRId, OutputSlotGId, OutputSlotBId, OutputSlotAId,
                ProceduralTexture2DId, UVInput, BlendId, TinputId,
                InvTinputId, CompressionScalersId, ColorSpaceOriginId, ColorSpaceVector1Id, ColorSpaceVector2Id, ColorSpaceVector3Id, InputSizeId });
        }

        [SerializeField]
        private TextureType m_TextureType = TextureType.Default;

        [EnumControl("Type")]
        public TextureType textureType
        {
	        get { return m_TextureType; }
	        set
	        {
		        if (m_TextureType == value)
			        return;

		        m_TextureType = value;
		        Dirty(ModificationScope.Graph);

		        ValidateNode();
	        }
        }

        public void GenerateNodeCode(ShaderStringBuilder sb, GenerationMode generationMode)
        {
	        ProceduralTexture2DInputMaterialSlot slot = FindInputSlot<ProceduralTexture2DInputMaterialSlot>(ProceduralTexture2DId);
            
            var slotValue = GetSlotValue(ProceduralTexture2DId, generationMode);
            
            var edges = owner.GetEdges(slot.slotReference).ToArray();
            string referenceName = null;
            if (edges.Any())
            {
                var fromSocketRef = edges[0].outputSlot;
                #if UNITY_2020_2_OR_NEWER
                var fromNode = owner.GetNodeFromId<PropertyNode>(fromSocketRef.node.objectId);
				#else
	            var fromNode = owner.GetNodeFromGuid<PropertyNode>(fromSocketRef.nodeGuid);
				#endif
	            if (fromNode != null) {
                    // proceduralTexture2D = fromNode.proceduralTexture2D;
                    #if UNITY_2020_2_OR_NEWER
					var property = (fromNode.property as ProceduralTexture2DProperty);
					#else
                    var property = owner.properties.FirstOrDefault(x => x.guid == fromNode.propertyGuid);
                    #endif
                    referenceName = property?.referenceName;
	            }
            }
            
            #if !UNITY_2020_2_OR_NEWER
	        referenceName = "ProceduralTexture_" + GuidEncoder.Encode(guid);
	        #endif
            
            var precision = concretePrecision.ToShaderString();

            string code = null;
            if (referenceName == null)
            {
	            code = $"float4 {GetVariableNameForSlot(OutputSlotRGBAId)} = float4(1,1,1,1);";
	            sb.AppendLine(code);
	            return;
            }
            
            code =
            @"
				float4 {9} = float4(0, 0, 0, 0);
				{
					float2 uvScaled = {0} * 3.464; // 2 * sqrt(3)

					const float2x2 gridToSkewedGrid = float2x2(1.0, 0.0, -0.57735027, 1.15470054);
					float2 skewedCoord = mul(gridToSkewedGrid, uvScaled);

					int2 baseId = int2(floor(skewedCoord));
					float3 temp = float3(frac(skewedCoord), 0);
					temp.z = 1.0 - temp.x - temp.y;

					float w1, w2, w3;
					int2 vertex1, vertex2, vertex3;
					if (temp.z > 0.0)
					{
						w1 = temp.z;
						w2 = temp.y;
						w3 = temp.x;
						vertex1 = baseId;
						vertex2 = baseId + int2(0, 1);
						vertex3 = baseId + int2(1, 0);
					}
					else
					{
						w1 = -temp.z;
						w2 = 1.0 - temp.y;
						w3 = 1.0 - temp.x;
						vertex1 = baseId + int2(1, 1);
						vertex2 = baseId + int2(1, 0);
						vertex3 = baseId + int2(0, 1);
					}

					float2 uv1 = {0} + frac(sin(mul(float2x2(127.1, 311.7, 269.5, 183.3), (float2)vertex1)) * 43758.5453);
					float2 uv2 = {0} + frac(sin(mul(float2x2(127.1, 311.7, 269.5, 183.3), (float2)vertex2)) * 43758.5453);
					float2 uv3 = {0} + frac(sin(mul(float2x2(127.1, 311.7, 269.5, 183.3), (float2)vertex3)) * 43758.5453);

					float2 duvdx = ddx({0});
					float2 duvdy = ddy({0});

					float4 G1 = {1}.SampleGrad({10}, uv1, duvdx, duvdy);
					float4 G2 = {1}.SampleGrad({10}, uv2, duvdx, duvdy);
					float4 G3 = {1}.SampleGrad({10}, uv3, duvdx, duvdy);

					float exponent = 1.0 + {11} * 15.0;
					w1 = pow(w1, exponent);
					w2 = pow(w2, exponent);
					w3 = pow(w3, exponent);
					float sum = w1 + w2 + w3;
					w1 = w1 / sum;
					w2 = w2 / sum;
					w3 = w3 / sum;

					float4 G = w1 * G1 + w2 * G2 + w3 * G3;
					G = G - 0.5;
					G = G * rsqrt(w1 * w1 + w2 * w2 + w3 * w3);
					G = G * {3};
					G = G + 0.5;

					duvdx *= {8}.xy;
					duvdy *= {8}.xy;
					float delta_max_sqr = max(dot(duvdx, duvdx), dot(duvdy, duvdy));
					float mml = 0.5 * log2(delta_max_sqr);
					float LOD = max(0, mml) / {8}.z;

					{9}.r = {2}.SampleLevel(sampler{2}, float2(G.r, LOD), 0).r;
					{9}.g = {2}.SampleLevel(sampler{2}, float2(G.g, LOD), 0).g;
					{9}.b = {2}.SampleLevel(sampler{2}, float2(G.b, LOD), 0).b;
					{9}.a = {2}.SampleLevel(sampler{2}, float2(G.a, LOD), 0).a;
				}
			";
            
            if(textureType == TextureType.Default)
                code += "{9}.rgb = {4} + {5} * {9}.r + {6} * {9}.g + {7} * {9}.b;";
            else if(textureType == TextureType.Normal)
                code += "{9}.rgb = UnpackNormalmapRGorAG({9});";

            code = code.Replace("{0}", GetSlotValue(UVInput, generationMode));
            code = code.Replace("{1}", referenceName);
            code = code.Replace("{2}", referenceName + "_" + kInvTinputName);
            code = code.Replace("{3}", referenceName + "_" + kCompressionScalersId);
            code = code.Replace("{4}", referenceName + "_" + kColorSpaceOriginName);
            code = code.Replace("{5}", referenceName + "_" + kColorSpaceVector1Name);
            code = code.Replace("{6}", referenceName + "_" + kColorSpaceVector2Name);
            code = code.Replace("{7}", referenceName + "_" + kColorSpaceVector3Name);
            code = code.Replace("{8}", referenceName + "_" + kInputSizeName);
            code = code.Replace("{9}", GetVariableNameForSlot(OutputSlotRGBAId));

            code = code.Replace("{10}", "sampler" + referenceName);

            code = code.Replace("{11}", GetSlotValue(BlendId, generationMode));

            sb.AppendLine(code);

            sb.AppendLine("{0} {1} = {2}.r;", precision, GetVariableNameForSlot(OutputSlotRId), GetVariableNameForSlot(OutputSlotRGBAId));
            sb.AppendLine("{0} {1} = {2}.g;", precision, GetVariableNameForSlot(OutputSlotGId), GetVariableNameForSlot(OutputSlotRGBAId));
            sb.AppendLine("{0} {1} = {2}.b;", precision, GetVariableNameForSlot(OutputSlotBId), GetVariableNameForSlot(OutputSlotRGBAId));
            sb.AppendLine("{0} {1} = {2}.a;", precision, GetVariableNameForSlot(OutputSlotAId), GetVariableNameForSlot(OutputSlotRGBAId));
        }

        public bool RequiresMeshUV(UVChannel channel, ShaderStageCapability stageCapability = ShaderStageCapability.All)
        {
	        return true;
        }

        #if !UNITY_2020_2_OR_NEWER
	    [SerializeField] private string customDisplayName = null;
	    
        public override void CollectShaderProperties(PropertyCollector properties, GenerationMode generationMode)
        {
	        var newProperty = new ProceduralTexture2DProperty()
	        {
		        overrideReferenceName = "ProceduralTexture_" + GuidEncoder.Encode(guid)
	        };
	        if (!string.IsNullOrEmpty(customDisplayName))
		        newProperty.displayName = customDisplayName;
	        properties.AddShaderProperty(newProperty);
	        base.CollectShaderProperties(properties, generationMode);
        }
        
        public VisualElement CreateSettingsElement()
        {
	        var v = new VisualElement();
	        var tf = new TextField("Display Name");
	        tf.value = customDisplayName;
	        tf.RegisterValueChangedCallback(evt =>
	        {
		        customDisplayName = evt.newValue;
	        });
	        v.Add(tf);
	        return v;
        }
        
        #endif
    }
}

#endif