# Procedural Stochastic Texturing

![Unity Version Compatibility](https://img.shields.io/badge/Unity-2019.4%20%E2%80%94%202021.1-brightgreen) [![openupm](https://img.shields.io/npm/v/com.needle.procedural-stochastic-texturing?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.needle.procedural-stochastic-texturing/)

## What's this?

This package contains the custom ShaderGraph nodes from https://github.com/UnityLabs/procedural-stochastic-texturing, made compatible with Unity 2019.4–2021.1 and thus URP 7.x-11.x, HDRP 7.x-11.x. Additionally, there are some helpers for the Built-In pipeline to make custom surface shaders.  

From their Readme:
> For a certain class of textures called stochastic textures, it solves the issue of tiling repetition when tileable textures are repeated across a surface. This allows using smaller textures, achieving higher levels of detail and texturing larger surfaces without the need to hide the repetition patterns.

Credit for the method goes to Thomas Deliot and Eric Heitz from Unity Technologies, as well as to mgear/unitycoder for the basic blending implementation.

## Quick Start

All of the below are provided in the package as samples (access through Package Manager).

### Simple Version

- create a new ShaderGraph
- create a Texture2D property as usual
- add a "Sample Procedural Texture Simple" node
- connect UV, texture and output as usual
- press "Save Asset"

### Histogram-Preserving Version

- generate a "Procedural Texture 2D" asset in the Project
- set a texture
- press "Apply" and wait for generation
- create a new ShaderGraph
- add a "Procedural Texture 2D Asset" node, set your asset
- add a "Sample Procedural Texture 2D" node, connect the "Asset" node
- connect UV and output as usual
- press "Save Asset"  
  *Note: when you change the input texture or any settings of it, the Shader needs to be saved again (values are baked into the shader code).*

### Built-In StandardStochastic

- create a new material, choose StandardStochastic as shader
- set your textures
- set generation options at the top (which textures should be stochastically sampled) and press "Apply"

### Built-In Surface Shader

- define a Texture2D property
- include the `ProceduralTexturingSimple.cginc` file
  ```
  sampler2D _BaseMap;
  #include "Packages/com.needle.stochastic-texturing/Editor/TilingAndBlending/ProceduralTexturingSimple.cginc"
  ```
- use `tex2DStochastic(_BaseMap, _Blend, IN.uv_BaseMap)` instead of `tex2D(_BaseMap, IN.uv_BaseMap)` (`_Blend` can be a property or just 1.0)

## Known Issues

- some shader errors are logged on first import (mismatch between Built-In and SRP shader macros). Shouldn't affect functionality. If someone knows how to properly ifdef/macro this, help would be appreciated.

- The histogram-preserving ShaderGraph node doesn't work correctly in Linear color space (looks too bright). The "simple" version works fine.

- Generating a "Procedural Texture 2D" asset doesn't work correctly in Linear color space (totally wrong colors). The "simple" version works fine.

## Node Reference

- **Sample Procedural Texture Simple**
  Uses a custom function node to sample the texture. Uses basic blending; not histogram-preserving.

- **Procedural Texture 2D Asset**  
  Used as an input node to configure which ProceduralTexture to use.  
  This can't currently be set as a material input; the asset has to be "baked" into the shader.

- **Sample Procedural Texture 2D**  
  Takes the output from the "Procedural Texture 2D Asset" node and samples the texture. It's histogram-preserving, so the transitions look very nice.

- **Texture Sampler**  
  Outputs the sampler that goes along with an input texture. This allows the custom function node to use correct anisotropy / filtering settings as specified on the texture asset.

## Notes

### Working with the Histogram Preserving ShaderGraph node

- Make sure a normal texture is imported as a Normal map in its import settings before using it with a ProceduralTexture2D asset.
- Large input textures might take a while to pre-process.
- If you modify a ProceduralTexture2D's parameters and apply them, each ShaderGraph using this ProceduralTexture2D needs to be re-opened and re-saved manually to use the updated parameters.

### Quality Differences between "Tiling and Blending" vs "Histogram Preserving"

The "Sample Procedural Texture Simple" node uses a much simpler blending operation, and does not need a pre-processing step ("just works"). The downside is that the resulting blending quality is lower (it's not histogram preserving). Depending on your use case one or the other might be more suited.

### Supported Render Pipelines

- URP/HDRP through ShaderGraph
- BuiltIn through includes and a StandardStochastic shader

See the Samples in Package Manager for more info, especially for Built-In and custom surface shaders.

## Contact
<b>[needle — tools for unity](https://needle.tools)</b> • 
[@NeedleTools](https://twitter.com/NeedleTools) • 
[@marcel_wiessler](https://twitter.com/marcel_wiessler) • 
[@hybridherbst](https://twitter.com/hybdridherbst)