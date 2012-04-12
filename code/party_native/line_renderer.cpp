#include "stdafx.h"
#include "line_renderer.h"
#include "shaders/line_shaders.h"

LineRenderer::LineRenderer():
	device(nullptr),
	initial_state(nullptr),
	vertex_format(nullptr),
	vertex_shader(nullptr),
	pixel_shader(nullptr),
	vertex_shader_constants(nullptr),
	pixel_shader_constants(nullptr),
	view_projection_handle(nullptr),
	color_handle(nullptr)
{
}

void LineRenderer::Init(LPDIRECT3DDEVICE9 _device)
{
	device = _device;
	device->AddRef();

	const D3DVERTEXELEMENT9 line_vertex_decl[] =
	{
		{0, 0, D3DDECLTYPE_FLOAT3, D3DDECLMETHOD_DEFAULT, D3DDECLUSAGE_POSITION, 0},
		D3DDECL_END()
	};

	device->CreateVertexDeclaration(line_vertex_decl, &vertex_format);

	device->CreateVertexShader((const DWORD*)line_shaders::vertex, &vertex_shader);
	device->CreatePixelShader((const DWORD*)line_shaders::pixel, &pixel_shader);

	D3DXGetShaderConstantTable((const DWORD*)line_shaders::vertex, &vertex_shader_constants);
	view_projection_handle = vertex_shader_constants->GetConstantByName(NULL, "view_projection");

	D3DXGetShaderConstantTable((const DWORD*)line_shaders::pixel, &pixel_shader_constants);
	color_handle = pixel_shader_constants->GetConstantByName(NULL, "color");
}

void LineRenderer::Shutdown()
{
	SAFE_RELEASE(pixel_shader_constants);
	SAFE_RELEASE(vertex_shader_constants);
	SAFE_RELEASE(pixel_shader);
	SAFE_RELEASE(vertex_shader);
	SAFE_RELEASE(vertex_format);
	SAFE_RELEASE(initial_state);
	SAFE_RELEASE(device);
}

void LineRenderer::Begin()
{
	device->CreateStateBlock(D3DSBT_ALL, &initial_state);

	device->SetVertexDeclaration(vertex_format);
	device->SetVertexShader(vertex_shader);
	device->SetPixelShader(pixel_shader);
}

void LineRenderer::DrawLineStrip(const XMFLOAT3* line_strip, unsigned int line_count, const XMFLOAT4X4* transform, XMFLOAT4 color)
{
	vertex_shader_constants->SetMatrixTranspose(device, view_projection_handle, (D3DXMATRIX*)transform);
	pixel_shader_constants->SetVector(device, color_handle, (D3DXVECTOR4*)&color);
	device->DrawPrimitiveUP(D3DPT_LINESTRIP, line_count, line_strip, sizeof(XMFLOAT3));
}

void LineRenderer::DrawIndexedLine(const XMFLOAT3* line_vertices, const unsigned short* line_indices, unsigned int line_count, const XMFLOAT4X4* transform, XMFLOAT4 color)
{
	vertex_shader_constants->SetMatrixTranspose(device, view_projection_handle, (D3DXMATRIX*)transform);
	pixel_shader_constants->SetVector(device, color_handle, (D3DXVECTOR4*)&color);
	device->DrawIndexedPrimitiveUP(D3DPT_LINELIST, 0, line_count * 2, line_count, line_indices, D3DFMT_INDEX16, line_vertices, sizeof(XMFLOAT3));
}

void LineRenderer::End()
{
	initial_state->Apply();
	SAFE_RELEASE(initial_state);
}
