//////////////////////////////////////////////////////////////////////////////////
// SPARK particle engine														//
// Copyright (C) 2009 - foulon matthieu - stardeath@wanadoo.fr					//
//																				//
// This software is provided 'as-is', without any express or implied			//
// warranty.  In no event will the authors be held liable for any damages		//
// arising from the use of this software.										//
//																				//
// Permission is granted to anyone to use this software for any purpose,		//
// including commercial applications, and to alter it and redistribute it		//
// freely, subject to the following restrictions:								//
//																				//
// 1. The origin of this software must not be misrepresented; you must not		//
//    claim that you wrote the original software. If you use this software		//
//    in a product, an acknowledgment in the product documentation would be		//
//    appreciated but is not required.											//
// 2. Altered source versions must be plainly marked as such, and must not be	//
//    misrepresented as being the original software.							//
// 3. This notice may not be removed or altered from any source distribution.	//
//////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "SPK_DX9Renderer.h"
#include "..\shaders\particle_shaders.h"

namespace SPK
{
namespace DX9
{
	int DX9Renderer::activeRenderers = 0;
	DX9BlendModeShader DX9Renderer::blendModeShaders[MAX_BLENDING_MODE];

	LPDIRECT3DVERTEXDECLARATION9 DX9Renderer::vertexFormat = nullptr;

	DX9Renderer::DX9Renderer() :
		Renderer()
	{}

	DX9Renderer::~DX9Renderer() {DX9Info::DX9ReleaseRenderer(this);}

	void DX9Renderer::initShaders()
	{
		if (activeRenderers++ > 0) return;

		LPDIRECT3DDEVICE9 device = DX9Info::getDevice();

		const D3DVERTEXELEMENT9 QuadVertexDecl2D[4] =
		{
			{0, 0, D3DDECLTYPE_FLOAT3, D3DDECLMETHOD_DEFAULT, D3DDECLUSAGE_POSITION, 0},
			{1, 0, D3DDECLTYPE_FLOAT4, D3DDECLMETHOD_DEFAULT, D3DDECLUSAGE_COLOR, 0},
			{2, 0, D3DDECLTYPE_FLOAT2, D3DDECLMETHOD_DEFAULT, D3DDECLUSAGE_TEXCOORD, 0},
			D3DDECL_END()
		};

		device->CreateVertexDeclaration(QuadVertexDecl2D, &vertexFormat);

		LPDIRECT3DVERTEXSHADER9 vertex_shader;
		device->CreateVertexShader((const DWORD*)particle_shaders::vertex, &vertex_shader);

		LPD3DXCONSTANTTABLE vertex_shader_constants;
		D3DXGetShaderConstantTable((const DWORD*)particle_shaders::vertex, &vertex_shader_constants);

		D3DXHANDLE view_projection_handle = vertex_shader_constants->GetConstantByName(NULL, "view_projection");
		
		const unsigned char* source_pixel_shaders[MAX_BLENDING_MODE] =
		{
			nullptr,
			particle_shaders::pixel,		// BLENDING_ADD
			particle_shaders::pixel,		// BLENDING_ALPHA
			particle_shaders::pixel_mask_r,	// BLENDING_ADD_MASK_R
			particle_shaders::pixel_mask_g,	// BLENDING_ADD_MASK_G
			particle_shaders::pixel_mask_b,	// BLENDING_ADD_MASK_B
			particle_shaders::pixel_mask_a,	// BLENDING_ADD_MASK_A
			particle_shaders::pixel_mask_r,	// BLENDING_ALPHA_MASK_R
			particle_shaders::pixel_mask_g,	// BLENDING_ALPHA_MASK_G
			particle_shaders::pixel_mask_b,	// BLENDING_ALPHA_MASK_B
			particle_shaders::pixel_mask_a,	// BLENDING_ALPHA_MASK_A
		};

		for (int i = 1; i < MAX_BLENDING_MODE; ++i)
		{
			DX9BlendModeShader& blend_mode_shader = blendModeShaders[i];

			blend_mode_shader.vertex_shader = vertex_shader;
			blend_mode_shader.vertex_shader_constants = vertex_shader_constants;
			vertex_shader->AddRef();
			vertex_shader_constants->AddRef();

			device->CreatePixelShader((const DWORD*)source_pixel_shaders[i], &blend_mode_shader.pixel_shader);
			D3DXGetShaderConstantTable((const DWORD*)source_pixel_shaders[i], &blend_mode_shader.pixel_shader_constants);

			blend_mode_shader.view_projection_handle = view_projection_handle;

			D3DXHANDLE texture_handle = blend_mode_shader.pixel_shader_constants->GetConstantByName(NULL, "color_texture"); 
			blend_mode_shader.texture_sampler = blend_mode_shader.pixel_shader_constants->GetSamplerIndex(texture_handle);

			switch(i)
			{
			case BLENDING_ADD:
			case BLENDING_ADD_MASK_R:
			case BLENDING_ADD_MASK_G:
			case BLENDING_ADD_MASK_B:
			case BLENDING_ADD_MASK_A:
				blend_mode_shader.src_blend = D3DBLEND_SRCALPHA;
				blend_mode_shader.dst_blend = D3DBLEND_ONE;
				break;
			case BLENDING_ALPHA:
			case BLENDING_ALPHA_MASK_R:
			case BLENDING_ALPHA_MASK_G:
			case BLENDING_ALPHA_MASK_B:
			case BLENDING_ALPHA_MASK_A:
				blend_mode_shader.src_blend = D3DBLEND_SRCALPHA;
				blend_mode_shader.dst_blend = D3DBLEND_INVSRCALPHA;
				break;
			}
		}
	}

	void DX9Renderer::shutdownShaders()
	{
		if (--activeRenderers > 0) return;

		for (int i = 1; i < MAX_BLENDING_MODE; ++i)
		{
			SAFE_RELEASE(blendModeShaders[i].vertex_shader);
			SAFE_RELEASE(blendModeShaders[i].vertex_shader_constants);
			SAFE_RELEASE(blendModeShaders[i].pixel_shader);
			SAFE_RELEASE(blendModeShaders[i].pixel_shader_constants);
		}
	}
}}
