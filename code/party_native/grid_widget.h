#pragma once

class LineRenderer;
class GridWidget
{
public:
	GridWidget();

	void Render(const XMFLOAT4X4* view_projection, LineRenderer& line_renderer);
};