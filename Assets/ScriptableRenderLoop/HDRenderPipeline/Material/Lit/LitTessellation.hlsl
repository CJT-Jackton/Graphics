float4 TessellationEdge(float3 p0, float3 p1, float3 p2, float3 n0, float3 n1, float3 n2)
{
  //  if (_TessellationFactorFixed >= 0.0f)
    {
    //    return  _TessellationFactorFixed.xxxx;
    }
 
    return DistanceBasedTess(p0, p1, p2, 0.0, _TessellationFactorMaxDistance, _WorldSpaceCameraPos) * _TessellationFactorFixed.xxxx;
}

float3 GetDisplacement(VaryingsDS input)
{
    // This call will work for both LayeredLit and Lit shader
    LayerTexCoord layerTexCoord;
    GetLayerTexCoord(
#ifdef VARYING_DS_WANT_TEXCOORD0
        input.texCoord0,
#else
        float2(0.0, 0.0),
#endif
#ifdef VARYING_DS_WANT_TEXCOORD1
        input.texCoord1,
#else
        float2(0.0, 0.0),
#endif
#ifdef VARYING_DS_WANT_TEXCOORD2
        input.texCoord2,
#else
        float2(0.0, 0.0),
#endif
#ifdef VARYING_DS_WANT_TEXCOORD3
        input.texCoord3,
#else
        float2(0.0, 0.0),
#endif
        input.positionWS, 
        input.normalWS,
        layerTexCoord);

    // TODO: For now just use Layer0, but we are suppose to apply the same heightmap blending than in the pixel shader
#ifdef _HEIGHTMAP
    float height = (SAMPLE_LAYER_TEXTURE2D_LOD(ADD_ZERO_IDX(_HeightMap), ADD_ZERO_IDX(sampler_HeightMap), ADD_ZERO_IDX(layerTexCoord.base), 0).r - ADD_ZERO_IDX(_HeightCenter)) * ADD_ZERO_IDX(_HeightAmplitude);
#else
    float height = 0.0;
#endif

    return input.positionWS + height * input.normalWS;
}
