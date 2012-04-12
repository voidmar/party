#include "stdafx.h"
#include "aabox_widget.h"
#include "line_renderer.h"

static XMFLOAT3 box_vertices[] = 
{
	XMFLOAT3(-0.5f, -0.5f, -0.5f), // 0 lbf
	XMFLOAT3( 0.5f, -0.5f, -0.5f), // 1 rbf
	XMFLOAT3(-0.5f,  0.5f, -0.5f), // 2 ltf
	XMFLOAT3( 0.5f,  0.5f, -0.5f), // 3 rtf
	XMFLOAT3(-0.5f, -0.5f,  0.5f), // 4 lbB
	XMFLOAT3( 0.5f, -0.5f,  0.5f), // 5 rbB
	XMFLOAT3(-0.5f,  0.5f,  0.5f), // 6 ltB
	XMFLOAT3( 0.5f,  0.5f,  0.5f), // 7 rtB
};

static unsigned short box_indices[] =
{
	0, 1, // lbf to rbf
	0, 2, // lbf to ltf
	0, 4, // lbf to lbB

	1, 3, // rbf to rtf
	1, 5, // rbf to rbB

	2, 3, // ltf to rtf
	2, 6, // ltf to ltB

	3, 7, // rtf to rtB

	4, 5, // lbB to rbB
	4, 6, // lbB to ltB

	5, 7, // rbB to rtB
	6, 7, // ltB to rtB
};

void AABoxWidget::Render(const XMFLOAT3& center, const XMFLOAT3& extents, const XMFLOAT4X4* view_projection, LineRenderer& line_renderer)
{
	XMMATRIX scale_matrix = XMMatrixScalingFromVector(XMLoadFloat3(&extents));
	XMMATRIX translation_matrix = XMMatrixTranslationFromVector(XMLoadFloat3(&center));
	XMMATRIX view_projection_matrix = XMLoadFloat4x4(view_projection);
	XMMATRIX final_matrix = XMMatrixMultiply(XMMatrixMultiply(scale_matrix, translation_matrix), view_projection_matrix);

	XMFLOAT4X4A final;
	XMStoreFloat4x4A(&final, final_matrix);

	line_renderer.DrawIndexedLine(box_vertices, box_indices, sizeof(box_indices) / sizeof(box_indices[0]) / 2, &final, XMFLOAT4(0, 1, 0, 1));
}