# Procedural Stochastic Texturing

![Unity Version Compatibility](https://img.shields.io/badge/Unity-2019.4%20%E2%80%94%202021.1-brightgreen) [![openupm](https://img.shields.io/npm/v/com.needle.procedural-stochastic-texturing?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.needle.procedural-stochastic-texturing/)

## What's this?

This package contains the custom ShaderGraph nodes from https://github.com/UnityLabs/procedural-stochastic-texturing, made compatible with Unity 2019.4–2021.1 and thus URP 7.x-11.x, HDRP 7.x-11.x. Additionally, there are some helpers for the Built-In pipeline to make custom surface shaders.  

From their Readme:
> For a certain class of textures called stochastic textures, it solves the issue of tiling repetition when tileable textures are repeated across a surface. This allows using smaller textures, achieving higher levels of detail and texturing larger surfaces without the need to hide the repetition patterns.

Credit for the method goes to Thomas Deliot and Eric Heitz from Unity Technologies, as well as to mgear/unitycoder for the basic blending implementation.

## Quick Start

## Node Reference

- Procedural Texture 2D
- Sample Procedural Texture 2D
- Sample Procedural Texture Simple

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

## Contact
<b>[needle — tools for unity](https://needle.tools)</b> • 
[@NeedleTools](https://twitter.com/NeedleTools) • 
[@marcel_wiessler](https://twitter.com/marcel_wiessler) • 
[@hybridherbst](https://twitter.com/hybdridherbst)