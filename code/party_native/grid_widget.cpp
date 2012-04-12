#include "stdafx.h"
#include "grid_widget.h"
#include <vector>
#include <d3dx9.h>
#include "line_renderer.h"

static std::vector<XMFLOAT3> inner_grid_lines;
static std::vector<XMFLOAT3> outer_grid_lines;

static void GenerateGrid(std::vector<XMFLOAT3>& lines, float spacing, int rows, int columns, bool ignore_center = false)
{
	int bit = 1;
	const float row_start = rows / 2 * -spacing; 
	for (int row = 0; row <= rows; ++row)
	{
		if (ignore_center && row == rows / 2)		
		{
			bit = 0;
			continue;
		}
		if ((row ^ bit) & 1)
		{
			lines.push_back(XMFLOAT3(row_start + row * spacing, 0, -row_start));
			lines.push_back(XMFLOAT3(row_start + row * spacing, 0, row_start));
		}
		else
		{
			lines.push_back(XMFLOAT3(row_start + row * spacing, 0, row_start));
			lines.push_back(XMFLOAT3(row_start + row * spacing, 0, -row_start));
		}
	}

	bit = 1;
	const float col_start = columns / 2 * -spacing; 
	for (int col = columns; col >= 0 ; --col)
	{
		if (ignore_center && col == columns / 2)
		{
			bit = 0;
			continue;
		}
		if ((col ^ bit) & 1)
		{
			lines.push_back(XMFLOAT3(-col_start, 0, col_start + col * spacing));
			lines.push_back(XMFLOAT3(col_start, 0, col_start + col * spacing));
		}
		else
		{
			lines.push_back(XMFLOAT3(col_start, 0, col_start + col * spacing));
			lines.push_back(XMFLOAT3(-col_start, 0, col_start + col * spacing));
		}
	}
}

GridWidget::GridWidget()
{
	inner_grid_lines.clear();
	outer_grid_lines.clear();
	GenerateGrid(inner_grid_lines, 0.1f, 10, 10, true);
	GenerateGrid(outer_grid_lines, 1.0f, 10, 10);
}

void GridWidget::Render(const XMFLOAT4X4* view_projection, LineRenderer& line_renderer)
{
	line_renderer.DrawLineStrip(&*inner_grid_lines.begin(), inner_grid_lines.size() - 1, view_projection, XMFLOAT4(0, 0.25f, 1.0f, 1.0f));
	line_renderer.DrawLineStrip(&*outer_grid_lines.begin(), outer_grid_lines.size() - 1, view_projection, XMFLOAT4(0, 0.125f, 0.25f, 1.0f));
}