#pragma once
#include <vector>

class LineRenderer;
class SphereWidget
{
public:

	SphereWidget();

	void Render(const XMFLOAT3& center, float radius, const XMFLOAT4X4* view_projection, LineRenderer& line_renderer);

private:
	std::vector<XMFLOAT3> vertices;
	std::vector<unsigned short> indices;
};