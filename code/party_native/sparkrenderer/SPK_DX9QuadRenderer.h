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


#ifndef H_SPK_DX9QUADRENDERER
#define H_SPK_DX9QUADRENDERER

#include "SPK_DX9Renderer.h"
#include "Extensions/Renderers/SPK_QuadRendererInterface.h"
#include "Extensions/Renderers/SPK_Oriented3DRendererInterface.h"
#include "Core/SPK_Vector3D.h"
#include "Core/SPK_Particle.h"
#include "Core/SPK_Model.h"

namespace SPK
{
namespace DX9
{

	class DX9QuadRenderer : public DX9Renderer, public QuadRendererInterface, public Oriented3DRendererInterface
	{
		SPK_IMPLEMENT_REGISTERABLE(DX9QuadRenderer)

	public :

		//////////////////
		// Constructors //
		//////////////////

		DX9QuadRenderer(float scaleX = 1.0f, float scaleY = 1.0f);

		virtual ~DX9QuadRenderer();

		static DX9QuadRenderer* create(float scaleX = 1.0f,float scaleY = 1.0f);

		/////////////
		// Setters //
		/////////////

		virtual bool setTexturingMode(TexturingMode mode);

		void setTexture(LPDIRECT3DTEXTURE9 textureIndex);


		/////////////
		// Getters //
		/////////////

		LPDIRECT3DTEXTURE9 getTexture() const;

		///////////////
		// Interface //
		///////////////

		virtual void createBuffers(const Group& group);
		virtual void destroyBuffers(const Group& group);

		virtual bool DX9CreateBuffers(const Group& group);
		virtual bool DX9DestroyBuffers(const Group& group);

		virtual void render(const Group& group);

	protected:

		virtual bool checkBuffers(const Group& group);

		virtual bool DX9CheckBuffers(const Group& group);

	private :

		LPDIRECT3DTEXTURE9 textureIndex;

		// vertex buffers and iterators
		static XMFLOAT3* vertexBuffer;
		static XMFLOAT3* vertexIterator;
		static XMFLOAT4* colorBuffer;
		static XMFLOAT4* colorIterator;
		static float* textureBuffer;
		static float* textureIterator;
		static short* indexBuffer;
		static short* indexIterator;

		static LPDIRECT3DVERTEXBUFFER9 DX9VertexBuffer;
		static LPDIRECT3DVERTEXBUFFER9 DX9ColorBuffer;
		static LPDIRECT3DVERTEXBUFFER9 DX9TextureBuffer;
		static LPDIRECT3DINDEXBUFFER9 DX9IndexBuffer;

		static short offsetIndex;

		// buffers names
		static const std::string VERTEX_BUFFER_NAME;
		static const std::string COLOR_BUFFER_NAME;
		static const std::string TEXTURE_BUFFER_NAME;
		static const std::string INDEX_BUFFER_NAME;

		
		float* createTextureBuffer(const Group& group) const;


		void DX9CallColorAndVertex(const Particle& particle) const;	// DX9 calls for color and position
		void DX9CallTexture2DAtlas(const Particle& particle) const;	// DX9 calls for 2D atlastexturing 

		static void (DX9QuadRenderer::*renderParticle)(const Particle&)  const;	// pointer to the right render method

		void render2D(const Particle& particle) const;			// Rendering for particles with texture 2D or no texture
		void render2DRot(const Particle& particle) const;		// Rendering for particles with texture 2D or no texture and rotation
		void render2DAtlas(const Particle& particle) const;		// Rendering for particles with texture 2D atlas
		void render2DAtlasRot(const Particle& particle) const;	// Rendering for particles with texture 2D atlas and rotation
	};


	inline DX9QuadRenderer* DX9QuadRenderer::create(float scaleX,float scaleY)
	{
		DX9QuadRenderer* obj = new DX9QuadRenderer(scaleX,scaleY);
		registerObject(obj);
		DX9Info::DX9RegisterRenderer(obj);
		return obj;
	}

	inline void DX9QuadRenderer::setTexture(LPDIRECT3DTEXTURE9 textureIndex)
	{
		if (textureIndex) textureIndex->AddRef();
		SAFE_RELEASE(this->textureIndex);
		this->textureIndex = textureIndex;
	}

	inline LPDIRECT3DTEXTURE9 DX9QuadRenderer::getTexture() const
	{
		return textureIndex;
	}

	inline void DX9QuadRenderer::DX9CallColorAndVertex(const Particle& particle) const
	{
		float x = particle.position().x;
		float y = particle.position().y;
		float z = particle.position().z;

		// top left vertex
		(vertexIterator)->x = x + quadSide().x + quadUp().x;
		(vertexIterator)->y = y + quadSide().y + quadUp().y;
		(vertexIterator++)->z = z + quadSide().z + quadUp().z;

		// top right vertex
		(vertexIterator)->x = x - quadSide().x + quadUp().x;
		(vertexIterator)->y = y - quadSide().y + quadUp().y;
		(vertexIterator++)->z = z - quadSide().z + quadUp().z;

		// bottom right vertex
		(vertexIterator)->x = x - quadSide().x - quadUp().x;
		(vertexIterator)->y = y - quadSide().y - quadUp().y;
		(vertexIterator++)->z = z - quadSide().z - quadUp().z;

		// bottom left vertex
		(vertexIterator)->x = x + quadSide().x - quadUp().x;
		(vertexIterator)->y = y + quadSide().y - quadUp().y;
		(vertexIterator++)->z = z + quadSide().z - quadUp().z;

		// TODO : éviter la duplication de l'information couleur
		//DWORD color = D3DCOLOR_COLORVALUE(particle.getR(), particle.getG(), particle.getB(), particle.getParamCurrentValue(PARAM_ALPHA));
		XMFLOAT4 color;
		color.x = particle.getR();
		color.y = particle.getG();
		color.z = particle.getB();
		color.w = particle.getParamCurrentValue(PARAM_ALPHA);

		*(colorIterator++) = color;
		*(colorIterator++) = color;
		*(colorIterator++) = color;
		*(colorIterator++) = color;
	}

	inline void DX9QuadRenderer::DX9CallTexture2DAtlas(const Particle& particle) const
	{
		computeAtlasCoordinates(particle);

		*(textureIterator++) = textureAtlasU0();
		*(textureIterator++) = textureAtlasV0();

		*(textureIterator++) = textureAtlasU1();
		*(textureIterator++) = textureAtlasV0();

		*(textureIterator++) = textureAtlasU1();
		*(textureIterator++) = textureAtlasV1();

		*(textureIterator++) = textureAtlasU0();
		*(textureIterator++) = textureAtlasV1();
	}
}}

#endif
