using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

[Serializable]
[BlackboardInputInfo(55)]
public class ProceduralTexture2DProperty : AbstractShaderProperty<LazyLoadReference<ProceduralTexture2D>>
{
    internal override bool isExposable => true;
    internal override bool isRenamable => true;
    internal override ShaderInput Copy()
    {
        return new ProceduralTexture2DProperty()
        {
            displayName = displayName,
            hidden = hidden,
            value = value,
            precision = precision,
            overrideHLSLDeclaration = overrideHLSLDeclaration,
            hlslDeclarationOverride = hlslDeclarationOverride
        };
    }
    
    internal override string GetPropertyBlockString()
    {
        return 
        $@"[ProceduralTexture2D]{referenceName}(""{displayName}"", Float) = {0}
        [HideInInspector]{referenceName}_{SampleProceduralTexture2DNode.kTinputName}(""{displayName}-Data"", 2D) = ""white""
        [HideInInspector]{referenceName}_{SampleProceduralTexture2DNode.kInvTinputName}(""{displayName}-Data"", 2D) = ""white""
        [HideInInspector]{referenceName}_{SampleProceduralTexture2DNode.kCompressionScalersId}(""{displayName}-Data"", Vector) = (0,0,0,0)
        [HideInInspector]{referenceName}_{SampleProceduralTexture2DNode.kColorSpaceOriginName}(""{displayName}-Data"", Vector) = (0,0,0,0)
        [HideInInspector]{referenceName}_{SampleProceduralTexture2DNode.kColorSpaceVector1Name }(""{displayName}-Data"", Vector) = (0,0,0,0)
        [HideInInspector]{referenceName}_{SampleProceduralTexture2DNode.kColorSpaceVector2Name }(""{displayName}-Data"", Vector) = (0,0,0,0)
        [HideInInspector]{referenceName}_{SampleProceduralTexture2DNode.kColorSpaceVector3Name }(""{displayName}-Data"", Vector) = (0,0,0,0)
        [HideInInspector]{referenceName}_{SampleProceduralTexture2DNode.kInputSizeName}(""{displayName}-Data"", Vector) = (0,0,0,0)";
    }

    public override PropertyType propertyType => PropertyType.Texture2D;
    internal override void ForeachHLSLProperty(Action<HLSLProperty> action)
    {
        HLSLDeclaration decl = GetDefaultHLSLDeclaration();
        
        action(new HLSLProperty(HLSLType._float, referenceName, decl));
        action(new HLSLProperty(HLSLType._Texture2D, $"{referenceName}_{SampleProceduralTexture2DNode.kTinputName}", HLSLDeclaration.Global));
        action(new HLSLProperty(HLSLType._Texture2D, $"{referenceName}_{SampleProceduralTexture2DNode.kInvTinputName}", HLSLDeclaration.Global));
        action(new HLSLProperty(HLSLType._SamplerState, "sampler" + $"{referenceName}_{SampleProceduralTexture2DNode.kTinputName}", HLSLDeclaration.Global));
        action(new HLSLProperty(HLSLType._SamplerState, "sampler" + $"{referenceName}_{SampleProceduralTexture2DNode.kInvTinputName}", HLSLDeclaration.Global));
        action(new HLSLProperty(HLSLType._float4, $"{referenceName}_{SampleProceduralTexture2DNode.kCompressionScalersId}", HLSLDeclaration.Global));
        action(new HLSLProperty(HLSLType._float3, $"{referenceName}_{SampleProceduralTexture2DNode.kColorSpaceOriginName}", HLSLDeclaration.Global));
        action(new HLSLProperty(HLSLType._float3, $"{referenceName}_{SampleProceduralTexture2DNode.kColorSpaceVector1Name }", HLSLDeclaration.Global));
        action(new HLSLProperty(HLSLType._float3, $"{referenceName}_{SampleProceduralTexture2DNode.kColorSpaceVector2Name }", HLSLDeclaration.Global));
        action(new HLSLProperty(HLSLType._float3, $"{referenceName}_{SampleProceduralTexture2DNode.kColorSpaceVector3Name }", HLSLDeclaration.Global));
        action(new HLSLProperty(HLSLType._float3, $"{referenceName}_{SampleProceduralTexture2DNode.kInputSizeName}", HLSLDeclaration.Global));
    }

    public override string GetDefaultReferenceName() => $"ProceduralTexture2D_{objectId}";

    internal override string GetPropertyAsArgumentString()
    {
        return $"float {referenceName}";
    }

    internal override AbstractMaterialNode ToConcreteNode()
    {
        var node = new ProceduralTexture2DNode();
        node.proceduralTexture2D = value.asset;
        return node;
    }

    internal override PreviewProperty GetPreviewMaterialProperty()
    {
        return new PreviewProperty(propertyType)
        {
            name = referenceName,
            textureValue = Texture2D.whiteTexture
        };
    }
}