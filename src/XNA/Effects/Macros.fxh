//-----------------------------------------------------------------------------
// Macros.fxh
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#ifdef SM4

#define OUTPOSITION SV_POSITION
#define OUTCOLOR0 SV_TARGET
#define OUTCOLOR1 SV_TARGET1
#define OUTCOLOR2 SV_TARGET2
#define OUTCOLOR3 SV_TARGET3

// Macros for targetting shader model 4.0 (DX11)

#define TECHNIQUE(name, vsname, psname ) \
	technique name { pass { VertexShader = compile vs_4_0 vsname (); PixelShader = compile ps_4_0 psname(); } }

#define BEGIN_CONSTANTS     cbuffer Parameters : register(b0) {
#define MATRIX_CONSTANTS
#define END_CONSTANTS       };

#define _vs(r)
#define _ps(r)
#define _cb(r)

#define DECLARE_TEXTURE2D2D(Name, index) \
	Texture2D<float4> t##Name : register(t##index); \
	sampler s##Name : register(s##index)

#define DECLARE_TEXTURE2DCUBE(Name, index) \
	TextureCube<float4> t##Name : register(t##index); \
	sampler s##Name : register(s##index)

// Make sampling macros also available for VS on D3D11
#define Sample2D(tex, uv) t##tex.Sample(s##tex, uv)
#define Sample2DProj(tex, uv) t##tex.Sample(s##tex, uv.xy / uv.w)
#define Sample2DLod0(tex, uv) t##tex.SampleLevel(s##tex, uv, 0.0)
#define SampleCube(tex, uv) t##tex.Sample(s##tex, uv)
#define SampleCubeLOD(tex, uv) t##tex.SampleLevel(s##tex, uv.xyz, uv.w)
#define SampleShadow(tex, uv) t##tex.SampleCmpLevelZero(s##tex, uv.xy, uv.z)


#else

#define OUTPOSITION POSITION
#define OUTCOLOR0 COLOR0
#define OUTCOLOR1 COLOR1
#define OUTCOLOR2 COLOR2
#define OUTCOLOR3 COLOR3

// Macros for targetting shader model 3.0 (mojoshader)

#define TECHNIQUE(name, vsname, psname ) \
	technique name { pass { VertexShader = compile vs_3_0 vsname (); PixelShader = compile ps_3_0 psname(); } }

#define BEGIN_CONSTANTS
#define MATRIX_CONSTANTS
#define END_CONSTANTS

#define _vs(r)  : register(vs, r)
#define _ps(r)  : register(ps, r)
#define _cb(r)

#define DECLARE_TEXTURE2D_LINEAR_CLAMP(Name) \
	texture2D Name; \
	sampler s##Name = sampler_state { Texture = (Name); MipFilter = LINEAR; MinFilter = LINEAR; MagFilter = LINEAR; AddressU = Clamp; AddressV = Clamp; };

#define DECLARE_TEXTURE2D_LINEAR_WRAP(Name) \
	texture2D Name; \
	sampler s##Name = sampler_state { Texture = (Name); MipFilter = LINEAR; MinFilter = LINEAR; MagFilter = LINEAR; AddressU = Wrap; AddressV = Wrap; };

#define DECLARE_TEXTURE2D_POINT_CLAMP(Name) \
	texture2D Name; \
	sampler s##Name = sampler_state { Texture = (Name); MipFilter = POINT; MinFilter = POINT; MagFilter = POINT; AddressU = Clamp; AddressV = Clamp; };

#define DECLARE_TEXTURE2D_POINT_WRAP(Name) \
	texture2D Name; \
	sampler s##Name = sampler_state { Texture = (Name); MipFilter = POINT; MinFilter = POINT; MagFilter = POINT; AddressU = Wrap; AddressV = Wrap; };

#define DECLARE_TEXTURECUBE_LINEAR_CLAMP(Name) \
	textureCUBE Name; \
	sampler s##Name = sampler_state { Texture = (Name); MipFilter = LINEAR; MinFilter = LINEAR; MagFilter = LINEAR; AddressU = Clamp; AddressV = Clamp; };

#define Sample2D(tex, uv) tex2D(s##tex, uv)
#define Sample2DProj(tex, uv) tex2Dproj(s##tex, uv)
#define Sample2DLod0(tex, uv) tex2Dlod(s##tex, float4(uv, 0.0, 0.0))
#define SampleCube(tex, uv) texCUBE(s##tex, uv)
#define SampleCubeLOD(tex, uv) texCUBElod(s##tex, uv)

#endif
