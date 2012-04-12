#pragma once

class Camera
{
public:

	float fov;
	float z_near;
	float z_far;

	XMFLOAT4X4 camera;
	XMFLOAT4X4 view;
	XMFLOAT4X4 projection;
	XMFLOAT4X4 view_projection;

	Camera();
	void ResetProjection(float width, float height);
	void Update();

	void SetDistance(float distance);
	void SetRotation(float yaw, float pitch);

	float GetDistance() const { return distance; }

private:
	float distance;
	float yaw;
	float pitch;
};