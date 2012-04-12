#pragma once

class LineRenderer;
class AABoxWidget
{
public:
	static void Render(const XMFLOAT3& center, const XMFLOAT3& extents, const XMFLOAT4X4* view_projection, LineRenderer& line_renderer);
};