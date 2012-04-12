#include "stdafx.h"
#include "camera.h"

Camera::Camera(): distance(10), yaw(XMConvertToRadians(-45)), pitch(XMConvertToRadians(-45))
{
}

void Camera::ResetProjection(float width, float height)
{
	XMMATRIX proj = XMMatrixPerspectiveFovRH(fov, width / height, z_near, z_far);
	XMStoreFloat4x4(&projection, proj);
}

void Camera::Update()
{
	XMMATRIX camera_matrix = XMMatrixRotationRollPitchYaw(pitch, XM_PI - yaw, 0);
	camera_matrix = XMMatrixMultiply(XMMatrixTranslation(0, 0, distance), camera_matrix);
	XMStoreFloat4x4(&camera, camera_matrix);

	XMMATRIX view_matrix = camera_matrix;
	XMVECTOR view_translation = view_matrix.r[3];
	view_matrix.r[3] = g_XMIdentityR3;
	view_matrix = XMMatrixTranspose(view_matrix);
	view_matrix.r[3] = XMVectorSetW(XMVectorNegate(XMVector3TransformCoord(view_translation, view_matrix)), 1);

	XMMATRIX projection_matrix = XMLoadFloat4x4(&projection);
	XMMATRIX view_projection_matrix = XMMatrixMultiply(view_matrix, projection_matrix);

	XMStoreFloat4x4(&view, view_matrix);
	XMStoreFloat4x4(&view_projection, view_projection_matrix);
}

void Camera::SetDistance(float new_distance)
{
	distance = new_distance;
	Update();
}

void Camera::SetRotation(float new_yaw, float new_pitch)
{
	yaw = new_yaw;
	pitch = new_pitch;
	Update();
}
