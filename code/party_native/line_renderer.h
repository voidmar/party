#pragma once

class LineRenderer
{
public:
	LineRenderer();

	void Init(LPDIRECT3DDEVICE9 _device);
	void Shutdown();

	void Begin();
	void DrawLineStrip(const XMFLOAT3* line_strip, unsigned int line_count, const XMFLOAT4X4* transform, XMFLOAT4 color);
	void DrawIndexedLine(const XMFLOAT3* line_vertices, const unsigned short* line_indices, unsigned int line_count, const XMFLOAT4X4* transform, XMFLOAT4 color);
	void End();

private:

	LPDIRECT3DDEVICE9 device;
	LPDIRECT3DSTATEBLOCK9 initial_state;
	LPDIRECT3DVERTEXDECLARATION9 vertex_format;
	LPDIRECT3DVERTEXSHADER9 vertex_shader;
	LPDIRECT3DPIXELSHADER9 pixel_shader;
	LPD3DXCONSTANTTABLE vertex_shader_constants;
	LPD3DXCONSTANTTABLE pixel_shader_constants;
	D3DXHANDLE view_projection_handle;
	D3DXHANDLE color_handle;
};