//////////////////////////////////////////////////////////////////////////////////
// SPARK particle engine														//
// Copyright (C) 2008-2009 - Julien Fryer - julienfryer@gmail.com				//
//                           matthieu foulon - stardeath@wanadoo.fr				//
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
#include "SPK_DX9QuadRenderer.h"
#include "Core/SPK_Particle.h"
#include "Core/SPK_Group.h"
#include "Core/SPK_ArrayBuffer.h"

namespace SPK
{
namespace DX9
{
	const std::string DX9QuadRenderer::VERTEX_BUFFER_NAME("SPK_DX9QuadRenderer_Vertex");
	const std::string DX9QuadRenderer::COLOR_BUFFER_NAME("SPK_DX9QuadRenderer_Color");
	const std::string DX9QuadRenderer::TEXTURE_BUFFER_NAME("SPK_DX9QuadRenderer_Texture");
	const std::string DX9QuadRenderer::INDEX_BUFFER_NAME("SPK_DX9QuadRenderer_Index");

	XMFLOAT3* DX9QuadRenderer::vertexBuffer = NULL;
	XMFLOAT3* DX9QuadRenderer::vertexIterator = NULL;
	XMFLOAT4* DX9QuadRenderer::colorBuffer = NULL;
	XMFLOAT4* DX9QuadRenderer::colorIterator = NULL;
	float* DX9QuadRenderer::textureBuffer = NULL;
	float* DX9QuadRenderer::textureIterator = NULL;
	short* DX9QuadRenderer::indexBuffer = NULL;
	short* DX9QuadRenderer::indexIterator = NULL;

	LPDIRECT3DVERTEXBUFFER9 DX9QuadRenderer::DX9VertexBuffer = NULL;
	LPDIRECT3DVERTEXBUFFER9 DX9QuadRenderer::DX9ColorBuffer = NULL;
	LPDIRECT3DVERTEXBUFFER9 DX9QuadRenderer::DX9TextureBuffer = NULL;
	LPDIRECT3DINDEXBUFFER9 DX9QuadRenderer::DX9IndexBuffer = NULL;

	short DX9QuadRenderer::offsetIndex = 0;

	void (DX9QuadRenderer::*DX9QuadRenderer::renderParticle)(const Particle&) const = NULL;

	DX9QuadRenderer::DX9QuadRenderer(float scaleX,float scaleY) :
		DX9Renderer(),
		QuadRendererInterface(scaleX,scaleY),
		Oriented3DRendererInterface(),
		textureIndex(0)
	{
		setTexturingMode(TEXTURE_2D);
	}

	DX9QuadRenderer::~DX9QuadRenderer()
	{
		DX9DestroyAllBuffers();
	}

	bool DX9QuadRenderer::checkBuffers(const Group& group)
	{
		ArrayBuffer<XMFLOAT3>* pvbBuffer = NULL;
		ArrayBuffer<XMFLOAT4>* dwColorBuffer = NULL;
		ArrayBuffer<short>* sIndexBuffer = NULL;

		if ((pvbBuffer = dynamic_cast<ArrayBuffer<XMFLOAT3>*>(group.getBuffer(VERTEX_BUFFER_NAME))) == NULL)
			return false;

		if ((dwColorBuffer = dynamic_cast<ArrayBuffer<XMFLOAT4>*>(group.getBuffer(COLOR_BUFFER_NAME))) == NULL)
			return false;

		if ((sIndexBuffer = dynamic_cast<ArrayBuffer<short>*>(group.getBuffer(INDEX_BUFFER_NAME))) == NULL)
			return false;

		if( texturingMode != TEXTURE_NONE )
		{
			FloatBuffer* fTextureBuffer;

			if ((fTextureBuffer = dynamic_cast<FloatBuffer*>(group.getBuffer(TEXTURE_BUFFER_NAME,texturingMode))) == NULL)
				textureBuffer = createTextureBuffer(group);

			textureIterator = textureBuffer = fTextureBuffer->getData();
		}

		vertexIterator = vertexBuffer = pvbBuffer->getData();
		colorIterator = colorBuffer = dwColorBuffer->getData();
		indexIterator = indexBuffer = sIndexBuffer->getData();

		return true;
	}

	void DX9QuadRenderer::createBuffers(const Group& group)
	{
		ArrayBuffer<XMFLOAT3>* vbVertexBuffer = dynamic_cast<ArrayBuffer<XMFLOAT3>*>(group.createBuffer(VERTEX_BUFFER_NAME, ArrayBufferCreator<XMFLOAT3>(4),0,false));
		ArrayBuffer<XMFLOAT4>* vbColorBuffer = dynamic_cast<ArrayBuffer<XMFLOAT4>*>(group.createBuffer(COLOR_BUFFER_NAME, ArrayBufferCreator<XMFLOAT4>(4),0,false));
		ArrayBuffer<short>* ibIndexBuffer  = dynamic_cast<ArrayBuffer<short>*>(group.createBuffer(INDEX_BUFFER_NAME, ArrayBufferCreator<short>(6),0,false));
		vertexIterator = vertexBuffer = vbVertexBuffer->getData();
		colorIterator = colorBuffer = vbColorBuffer->getData();
		indexIterator = indexBuffer = ibIndexBuffer->getData();

		if( texturingMode != TEXTURE_NONE )
			textureIterator = textureBuffer = createTextureBuffer(group);

		offsetIndex = 0;

		// initialisation de l'index buffer
		for(size_t i = 0; i < group.getParticles().getNbReserved(); i++)
		{
			*(indexIterator++) = 0 + offsetIndex;
			*(indexIterator++) = 1 + offsetIndex;
			*(indexIterator++) = 2 + offsetIndex;
			*(indexIterator++) = 0 + offsetIndex;
			*(indexIterator++) = 2 + offsetIndex;
			*(indexIterator++) = 3 + offsetIndex;

			offsetIndex += 4;
		}
		offsetIndex = 0;
	}

	void DX9QuadRenderer::destroyBuffers(const Group& group)
	{
		group.destroyBuffer(VERTEX_BUFFER_NAME);
		group.destroyBuffer(COLOR_BUFFER_NAME);
		group.destroyBuffer(TEXTURE_BUFFER_NAME);
		group.destroyBuffer(INDEX_BUFFER_NAME);
		offsetIndex = 0;
	}

	float* DX9QuadRenderer::createTextureBuffer(const Group& group) const
	{
		FloatBuffer* fbuffer = NULL;

		switch(texturingMode)
		{
			case TEXTURE_2D :
				fbuffer = dynamic_cast<FloatBuffer*>(group.createBuffer(TEXTURE_BUFFER_NAME,FloatBufferCreator(8),TEXTURE_2D,false));
				if (!group.getModel()->isEnabled(PARAM_TEXTURE_INDEX))
				{
					float t[8] = {0.0f,0.0f,1.0f,0.0f,1.0f,1.0f,0.0f,1.0f};
					for (size_t i = 0; i < group.getParticles().getNbReserved() << 3; ++i)
						fbuffer->getData()[i] = t[i & 7];
				}
				break;
		}

		return fbuffer->getData();
	}

	bool DX9QuadRenderer::setTexturingMode(TexturingMode mode)
	{
		texturingMode = mode;
		return true;
	}

	void DX9QuadRenderer::render(const Group& group)
	{
		int nb_part = group.getNbParticles();

		if (!DX9PrepareBuffers(group))
			return;

		if( !prepareBuffers(group) )
			return;

		if (!nb_part) return;

		DX9BlendModeShader& shader = blendModeShaders[blendingMode];
		
		LPDIRECT3DDEVICE9 device = DX9Info::getDevice();
		device->SetVertexShader(shader.vertex_shader);
		device->SetPixelShader(shader.pixel_shader);

		shader.vertex_shader_constants->SetMatrixTranspose(device, shader.view_projection_handle, (D3DXMATRIX*)DX9Info::view_projection);
		device->SetTexture(shader.texture_sampler, textureIndex);
		device->SetSamplerState(shader.texture_sampler, D3DSAMP_MINFILTER, D3DTEXF_LINEAR);
		device->SetSamplerState(shader.texture_sampler, D3DSAMP_MAGFILTER, D3DTEXF_LINEAR);

		device->SetRenderState(D3DRS_ALPHABLENDENABLE, TRUE);
		device->SetRenderState(D3DRS_SRCBLEND, shader.src_blend);
		device->SetRenderState(D3DRS_DESTBLEND, shader.dst_blend);
		device->SetRenderState(D3DRS_ZWRITEENABLE, FALSE);
		device->SetRenderState(D3DRS_CULLMODE, D3DCULL_NONE);

		if (!group.getModel()->isEnabled(PARAM_TEXTURE_INDEX))
		{
			if (!group.getModel()->isEnabled(PARAM_ANGLE))
				renderParticle = &DX9QuadRenderer::render2D;
			else
				renderParticle = &DX9QuadRenderer::render2DRot;
		}
		else
		{
			if (!group.getModel()->isEnabled(PARAM_ANGLE))
				renderParticle = &DX9QuadRenderer::render2DAtlas;
			else
				renderParticle = &DX9QuadRenderer::render2DAtlasRot;
		}

		XMFLOAT4X4* camera = DX9Info::camera;
		bool globalOrientation = precomputeOrientation3D(
			group,
			Vector3D(camera->m[2][0], camera->m[2][1], camera->m[2][2]),
			Vector3D(camera->m[1][0], camera->m[1][1], camera->m[1][2]),
			Vector3D(camera->m[3][0], camera->m[3][1], camera->m[3][2])
		);
		
		if (globalOrientation)
		{
			computeGlobalOrientation3D();

			for (size_t i = 0; i < group.getNbParticles(); ++i)
				(this->*renderParticle)(group.getParticle(i));
		}
		else
		{
			for (size_t i = 0; i < group.getNbParticles(); ++i)
			{
				const Particle& particle = group.getParticle(i);
				computeSingleOrientation3D(particle);
				(this->*renderParticle)(particle);
			}
		}
		
		
		// bind buffers and draw
		{
			void *ptr;
			DX9VertexBuffer->Lock(0, 0, &ptr, 0);
			std::memcpy(ptr, vertexBuffer, 4 * group.getNbParticles() * sizeof(XMFLOAT3));
			DX9VertexBuffer->Unlock();
			device->SetStreamSource(0, DX9VertexBuffer, 0, sizeof(XMFLOAT3));

			DX9ColorBuffer->Lock(0, 0, &ptr, 0);
			std::memcpy(ptr, colorBuffer, 4 * group.getNbParticles() * sizeof(XMFLOAT4));
			DX9ColorBuffer->Unlock();
			device->SetStreamSource(1, DX9ColorBuffer, 0, sizeof(XMFLOAT4));

			DX9TextureBuffer->Lock(0, 0, &ptr, 0);
			std::memcpy(ptr, textureBuffer, 4 * group.getNbParticles() * sizeof(XMFLOAT2));
			DX9TextureBuffer->Unlock();

			device->SetStreamSource(2, DX9TextureBuffer, 0, sizeof(XMFLOAT2));
			device->SetVertexDeclaration(vertexFormat);

			DX9IndexBuffer->Lock(0, 0, &ptr, 0);
			std::memcpy(ptr, indexBuffer, 6 * group.getNbParticles() * sizeof(short));
			DX9IndexBuffer->Unlock();
			device->SetIndices(DX9IndexBuffer);

			this->offsetIndex = 0;
			device->DrawIndexedPrimitive(D3DPT_TRIANGLELIST, 0, 0, nb_part<<2, 0, nb_part<<1);
		}
		//---------------------------------------------------------------------------
	}

	void DX9QuadRenderer::render2D(const Particle& particle) const
	{
		scaleQuadVectors(particle,scaleX,scaleY);
		DX9CallColorAndVertex(particle);
	}

	void DX9QuadRenderer::render2DRot(const Particle& particle) const
	{
		rotateAndScaleQuadVectors(particle,scaleX,scaleY);
		DX9CallColorAndVertex(particle);
	}

	void DX9QuadRenderer::render2DAtlas(const Particle& particle) const
	{
		scaleQuadVectors(particle,scaleX,scaleY);
		DX9CallColorAndVertex(particle);
		DX9CallTexture2DAtlas(particle);
	}

	void DX9QuadRenderer::render2DAtlasRot(const Particle& particle) const
	{
		rotateAndScaleQuadVectors(particle,scaleX,scaleY);
		DX9CallColorAndVertex(particle);
		DX9CallTexture2DAtlas(particle);
	}

	bool DX9QuadRenderer::DX9CheckBuffers(const Group& group)
	{
		if( !DX9Bind(group, DX9_VERTEX_BUFFER_KEY, (void**)&DX9VertexBuffer) )
		{
			DX9VertexBuffer = DX9ColorBuffer = DX9TextureBuffer = NULL;
			DX9IndexBuffer = NULL;
			return false;
		}
		if( !DX9Bind(group, DX9_COLOR_BUFFER_KEY, (void**)&DX9ColorBuffer) )
		{
			DX9VertexBuffer = DX9ColorBuffer = DX9TextureBuffer = NULL;
			DX9IndexBuffer = NULL;
			return false;
		}
		if( !DX9Bind(group, DX9_INDEX_BUFFER_KEY, (void**)&DX9IndexBuffer) )
		{
			DX9VertexBuffer = DX9ColorBuffer = DX9TextureBuffer = NULL;
			DX9IndexBuffer = NULL;
			return false;
		}
		if( texturingMode != TEXTURE_NONE )
		{
			if( !DX9Bind(group, DX9_TEXTURE_BUFFER_KEY, (void**)&DX9TextureBuffer) )
			{
				DX9VertexBuffer = DX9ColorBuffer = DX9TextureBuffer = NULL;
				DX9IndexBuffer = NULL;
				return false;
			}
		}

		return true;
	}

	bool DX9QuadRenderer::DX9CreateBuffers(const Group& group)
	{
		std::cout << "DX9QuadRenderer::DX9CreateBuffers" << std::endl;

		if( DX9Info::getDevice() == NULL ) return false;

		DX9Renderer::initShaders();

		LPDIRECT3DVERTEXBUFFER9 vb;

		// vertex buffer
		if( DX9Info::getDevice()->CreateVertexBuffer(group.getParticles().getNbReserved() * 4 * sizeof(XMFLOAT3), D3DUSAGE_WRITEONLY, 0, D3DPOOL_DEFAULT, &vb, NULL) != S_OK ) return false;
		std::pair<const Group *, int> key(&group, DX9_VERTEX_BUFFER_KEY);
		DX9Buffers[key] = vb;
		DX9VertexBuffer = vb;
		//-----------------------------------------------------------------------------------------------

		// color buffer
		if( DX9Info::getDevice()->CreateVertexBuffer(group.getParticles().getNbReserved() * 4 * sizeof(XMFLOAT4), D3DUSAGE_WRITEONLY, 0, D3DPOOL_DEFAULT, &vb, NULL) != S_OK ) return false;
		key = std::pair<const Group *, int>(&group, DX9_COLOR_BUFFER_KEY);
		DX9Buffers[key] = vb;
		DX9ColorBuffer = vb;
		//-----------------------------------------------------------------------------------------------

		// index buffer
		LPDIRECT3DINDEXBUFFER9 ib;

		if( DX9Info::getDevice()->CreateIndexBuffer(group.getParticles().getNbReserved() * 6 * sizeof(short), D3DUSAGE_WRITEONLY, D3DFMT_INDEX16, D3DPOOL_DEFAULT, &ib, NULL) != S_OK ) return false;
		key = std::pair<const Group *, int>(&group, DX9_INDEX_BUFFER_KEY);
		DX9Buffers[key] = ib;
		DX9IndexBuffer = ib;
		//-----------------------------------------------------------------------------------------------

		// texture buffer
		switch(texturingMode)
		{
		case TEXTURE_2D :
			if( DX9Info::getDevice()->CreateVertexBuffer(group.getParticles().getNbReserved() * 8 * sizeof(float), D3DUSAGE_WRITEONLY, 0, D3DPOOL_DEFAULT, &vb, NULL) != S_OK ) return false;
			key = std::pair<const Group *, int>(&group, DX9_TEXTURE_BUFFER_KEY);
			DX9Buffers[key] = vb;
			DX9TextureBuffer = vb;
			break;
		}
		//-----------------------------------------------------------------------------------------------
		return true;
	}

	bool DX9QuadRenderer::DX9DestroyBuffers(const Group& group)
	{
		DX9Release(group, DX9_VERTEX_BUFFER_KEY);
		DX9Release(group, DX9_COLOR_BUFFER_KEY);
		DX9Release(group, DX9_INDEX_BUFFER_KEY);
		DX9Release(group, DX9_TEXTURE_BUFFER_KEY);

		DX9VertexBuffer = DX9ColorBuffer = DX9TextureBuffer = NULL;
		DX9IndexBuffer = NULL;

		return true;
	}
}}
