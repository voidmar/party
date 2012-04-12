//////////////////////////////////////////////////////////////////////////////////
// SPARK particle engine														//
// Copyright (C) 2008-2009 - Julien Fryer - julienfryer@gmail.com				//
//                           foulon matthieu - stardeath@wanadoo.fr				//
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


#ifndef H_SPK_DX9RENDERER
#define H_SPK_DX9RENDERER

#include "Core/SPK_Renderer.h"
#include "SPK_DX9Info.h"
#include "SPK_DX9BufferHandler.h"

namespace SPK
{
namespace DX9
{
	class DX9BlendModeShader
	{
	public:
		LPDIRECT3DVERTEXSHADER9 vertex_shader;
		LPDIRECT3DPIXELSHADER9 pixel_shader;
		LPD3DXCONSTANTTABLE vertex_shader_constants;
		LPD3DXCONSTANTTABLE pixel_shader_constants;
		D3DXHANDLE view_projection_handle;
		UINT texture_sampler;
		D3DBLEND src_blend;
		D3DBLEND dst_blend;
	};

	class DX9Renderer : public SPK::Renderer, public DX9BufferHandler
	{
	public :

		/////////////////
		// Constructor //
		/////////////////

		/** @brief Constructor of DX9Renderer */
		DX9Renderer();

		////////////////
		// Destructor //
		////////////////

		/** @brief Destructor of DX9Renderer */
		virtual ~DX9Renderer();

		/////////////
		// Setters //
		/////////////

		virtual void setBlending(BlendingMode blendMode)
		{
			blendingMode = blendMode;
		}

		/*virtual */bool DX9DestroyAllBuffers();

	protected :

		BlendingMode blendingMode;
		static DX9BlendModeShader blendModeShaders[MAX_BLENDING_MODE];
		static int activeRenderers;
		static LPDIRECT3DVERTEXDECLARATION9 vertexFormat;
		void initShaders();
		void shutdownShaders();


	};

	inline bool DX9Renderer::DX9DestroyAllBuffers()
	{
		shutdownShaders();

		std::map<std::pair<const Group *, int>, IDirect3DResource9 *>::iterator it = DX9Buffers.begin();
		while( it != DX9Buffers.end() )
		{
			SAFE_RELEASE( it->second );
			it++;
		}
		DX9Buffers.clear();
		return true;
	}
}}

#endif
