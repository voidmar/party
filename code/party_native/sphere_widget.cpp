#include "stdafx.h"
#include <vector>
#include "sphere_widget.h"
#include "line_renderer.h"

#include <math.h>

SphereWidget::SphereWidget()
{
	const int ring_segments = 25;
	const int rings = 8;

	vertices.resize(ring_segments * rings * 2);

	const float ring_delta_pi = XM_PI / ring_segments * 2;
	const float ring_rotation = XM_PI / rings * 2;

	XMFLOAT3 ring[ring_segments];
	for (int i = 0; i < ring_segments; ++i)
	{
		ring[i] = XMFLOAT3(sinf(ring_delta_pi * i), cosf(ring_delta_pi * i), 0);
	}


	for (int i = 0; i < rings; ++i)
	{
		XMMATRIX rotation = XMMatrixRotationY(i * ring_rotation);
		XMVector3TransformCoordStream(&*vertices.begin() + i * ring_segments, sizeof(XMFLOAT3), ring, sizeof(XMFLOAT3), ring_segments, rotation);

		for (int j = 0; j < ring_segments - 1; ++j)
		{
			indices.push_back(unsigned short(i * ring_segments + j));
			indices.push_back(unsigned short(i * ring_segments + j + 1));
		}
		indices.push_back(unsigned short(i * ring_segments + ring_segments - 1));
		indices.push_back(unsigned short(i * ring_segments));
	}

	const int second_sphere_start = ring_segments * rings;
	for (int i = 0; i < rings; ++i)
	{
		XMMATRIX rotation = XMMatrixRotationX(i * ring_rotation);
		XMVector3TransformCoordStream(&*vertices.begin() + second_sphere_start + i * ring_segments, sizeof(XMFLOAT3), ring, sizeof(XMFLOAT3), ring_segments, rotation);

		for (int j = 0; j < ring_segments - 1; ++j)
		{
			indices.push_back(unsigned short(second_sphere_start + i * ring_segments + j));
			indices.push_back(unsigned short(second_sphere_start + i * ring_segments + j + 1));
		}
		indices.push_back(unsigned short(second_sphere_start + i * ring_segments +  ring_segments - 1));
		indices.push_back(unsigned short(second_sphere_start + i * ring_segments));
	}
}

void SphereWidget::Render(const XMFLOAT3& center, float radius, const XMFLOAT4X4* view_projection, LineRenderer& line_renderer)
{
	XMMATRIX scale_matrix = XMMatrixScalingFromVector(XMVectorReplicate(radius));
	XMMATRIX translation_matrix = XMMatrixTranslationFromVector(XMLoadFloat3(&center));
	XMMATRIX view_projection_matrix = XMLoadFloat4x4(view_projection);
	XMMATRIX final_matrix = XMMatrixMultiply(XMMatrixMultiply(scale_matrix, translation_matrix), view_projection_matrix);

	XMFLOAT4X4A final;
	XMStoreFloat4x4A(&final, final_matrix);

	line_renderer.DrawIndexedLine(&*vertices.begin(), &*indices.begin(), indices.size() / 2, &final, XMFLOAT4(0, 1, 0, 1));
}