#pragma once

class OrientationWidget
{
public:
	OrientationWidget();

	void SetScreenLocation(float x, float y);
	void Update(const XMFLOAT4X4* camera, const XMFLOAT4X4* ortho_projection);
	void Render(LPD3DXLINE line_renderer);

private:
	float screen_x, screen_y;
	XMFLOAT4X4 projection;
	XMFLOAT4X4 view_projection;
};