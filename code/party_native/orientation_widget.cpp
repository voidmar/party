#include "stdafx.h"
#include "orientation_widget.h"

static float widget_size = 30.0f;
static D3DXVECTOR3 widget_lines[] =
{
	D3DXVECTOR3(0, 0, 0),
	D3DXVECTOR3(widget_size, 0, 0),
	D3DXVECTOR3(0, 0, 0),
	D3DXVECTOR3(0, widget_size, 0),
	D3DXVECTOR3(0, 0, 0),
	D3DXVECTOR3(0, 0, widget_size)
};

OrientationWidget::OrientationWidget(): screen_x(0), screen_y(0)
{
}

void OrientationWidget::SetScreenLocation(float x, float y)
{
	screen_x = x;
	screen_y = y;
}

void OrientationWidget::Update(const XMFLOAT4X4* camera, const XMFLOAT4X4* ortho_projection)
{
	XMMATRIX camera_matrix = XMLoadFloat4x4(camera);
	camera_matrix.r[3] = XMVectorSet(screen_x, screen_y, 0, 1);
	XMStoreFloat4x4(&view_projection, XMMatrixMultiply(camera_matrix, XMLoadFloat4x4(ortho_projection)));
}

void OrientationWidget::Render(LPD3DXLINE line_renderer)
{
	line_renderer->DrawTransform(widget_lines + 0, 2, (D3DXMATRIX*)&view_projection, D3DCOLOR_XRGB(255, 0, 0));
	line_renderer->DrawTransform(widget_lines + 2, 2, (D3DXMATRIX*)&view_projection, D3DCOLOR_XRGB(0, 255, 0));
	line_renderer->DrawTransform(widget_lines + 4, 2, (D3DXMATRIX*)&view_projection, D3DCOLOR_XRGB(0, 0, 255));
}