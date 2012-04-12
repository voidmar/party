#include "stdafx.h"
#include <string>
#include <d3d9.h>
#include <d3dx9.h>
#include <unordered_map>
#include "party_api.h"
#include "SPK.h"
#include "missing_texture.h"
#include "sparkrenderer\SPK_DX9QuadRenderer.h"
#include "camera.h"
#include "orientation_widget.h"
#include "grid_widget.h"
#include "line_renderer.h"
#include "aabox_widget.h"
#include "sphere_widget.h"
#include <vector>
#include <intrin.h>


#pragma comment(lib, "winmm.lib")
#pragma comment(lib, "d3d9.lib")
#pragma comment(lib, "d3dx9.lib")

bool							g_Initialized = false;
IDirect3D9*						g_D3D9Interface;
IDirect3DDevice9*				g_D3D9Device;
D3DPRESENT_PARAMETERS			g_D3D9Parameters;
D3DCAPS9						g_D3D9Caps;
LPDIRECT3DTEXTURE9				g_D3D9DefaultTexture;
bool							g_ShouldReset = false;
CRITICAL_SECTION				g_GlobalLock;
std::vector<ParticleGroup*>		g_ParticleGroups;
SPK::System*					g_ParticleSystem;
Camera							g_Camera;
std::unordered_map<std::wstring, LPDIRECT3DTEXTURE9> g_LoadedTextures;

unsigned int					g_FrameCounter;
double							g_FPS;

LPD3DXFONT			g_DebugFont;
LPD3DXLINE			g_LineRenderer;
LineRenderer		_g_LineRenderer;
XMFLOAT4X4A			g_OrthoProjection;
OrientationWidget	g_OrientationWidget;
bool				g_GridEnabled = false;
GridWidget			g_GridWidget;

AABoxWidget g_BoxWidget;
SphereWidget g_SphereWidget;

LARGE_INTEGER g_TimerFrequency;
LARGE_INTEGER g_LastTime;
LARGE_INTEGER g_FPSUpdateFrequency;
LARGE_INTEGER g_FPSTimer;
LARGE_INTEGER g_FrameLimiterFrequency;

D3DCOLOR g_BackgroundColor = 0;

bool g_MotionEnabled = false;
float g_MotionRadius = 2.5f;
float g_MotionRPS = 1.0f / 2.5f;
float g_CurrentMotionAngle = 0;

class Lock
{
	LPCRITICAL_SECTION cs;
public:
	Lock(CRITICAL_SECTION& lock): cs(&lock)
	{
		EnterCriticalSection(cs);
	}

	~Lock()
	{
		LeaveCriticalSection(cs);
	}
};

struct Vector3
{
	float x, y, z;

	operator SPK::Vector3D()
	{
		return SPK::Vector3D(x, y, z);
	}
};

class ParticleGroup
{
public:
	std::wstring				name;
	std::wstring				texture_path;
	SPK::Group*					group;
	SPK::Model*					model;
	SPK::Emitter*				emitter;
	SPK::DX9::DX9QuadRenderer*	renderer;
	std::vector<Burst>			burst_list;

	float flow;
	int tank;

	float age;
	unsigned int next_burst;

	ParticleGroup(): group(nullptr), renderer(nullptr) {};
};


void ResetCameraProjection(int width, int height)
{
	g_Camera.ResetProjection((float)width, (float)height);
	g_Camera.Update();

	XMMATRIX ortho_projection = XMMatrixOrthographicOffCenterRH(0, (float)width, 0, (float)height, -100, 100);
	XMStoreFloat4x4A(&g_OrthoProjection, ortho_projection);

	g_OrientationWidget.SetScreenLocation(40, 40);
	g_OrientationWidget.Update(&g_Camera.view, &g_OrthoProjection);
}

extern "C"
{
	PARTY_API void __stdcall PTInitialize(HWND window)
	{
		if (g_Initialized) return;

		g_D3D9Interface = Direct3DCreate9(D3D_SDK_VERSION);

		RECT client_rect;
		GetClientRect(window, &client_rect);

		int width = client_rect.right - client_rect.left;
		int height = client_rect.bottom - client_rect.top;

		memset(&g_D3D9Parameters, 0, sizeof(g_D3D9Parameters));
		g_D3D9Parameters.BackBufferWidth = width;
		g_D3D9Parameters.BackBufferHeight = height;
		g_D3D9Parameters.BackBufferFormat = D3DFMT_UNKNOWN;
		g_D3D9Parameters.BackBufferCount = 1;
		
		g_D3D9Parameters.MultiSampleType = D3DMULTISAMPLE_NONE;
		g_D3D9Parameters.MultiSampleQuality = 0;

		g_D3D9Parameters.SwapEffect = D3DSWAPEFFECT_DISCARD;
		g_D3D9Parameters.hDeviceWindow = window;
		g_D3D9Parameters.Windowed = TRUE;
		g_D3D9Parameters.EnableAutoDepthStencil = TRUE;
		g_D3D9Parameters.AutoDepthStencilFormat = D3DFMT_D24X8;
		g_D3D9Parameters.Flags = D3DPRESENTFLAG_DISCARD_DEPTHSTENCIL;

		g_D3D9Parameters.FullScreen_RefreshRateInHz = 0;
		g_D3D9Parameters.PresentationInterval = D3DPRESENT_INTERVAL_DEFAULT;

		g_D3D9Interface->GetDeviceCaps(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, &g_D3D9Caps);
		
		DWORD flags = D3DCREATE_HARDWARE_VERTEXPROCESSING | D3DCREATE_MULTITHREADED;
		g_D3D9Interface->CreateDevice(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, window, flags, &g_D3D9Parameters, &g_D3D9Device);

		D3DXCreateTextureFromFileInMemory(g_D3D9Device, missing_texture::texture, missing_texture::texture_size, &g_D3D9DefaultTexture);
		g_LoadedTextures.insert(std::make_pair(L"", g_D3D9DefaultTexture));

		XMStoreFloat4x4(&g_Camera.camera, XMMatrixLookAtRH(XMVectorSet(0, 0, -10, 0), XMVectorSet(0, 0, 0, 0), XMVectorSet(0, 1, 0, 0)));
		g_Camera.fov = XMConvertToRadians(30);
		g_Camera.z_near = 0.01f;
		g_Camera.z_far = 500.0f;
		
		ResetCameraProjection(width, height);

		D3DXFONT_DESCW font_desc = {};
		font_desc.Height = 12;

		D3DXCreateFontIndirectW(g_D3D9Device, &font_desc, &g_DebugFont);
		D3DXCreateLine(g_D3D9Device, &g_LineRenderer);

		_g_LineRenderer.Init(g_D3D9Device);

		SPK::DX9::DX9Info::setDevice(g_D3D9Device);
		SPK::DX9::DX9Info::camera = &g_Camera.camera;
		SPK::DX9::DX9Info::view_projection = &g_Camera.view_projection;
		g_ParticleSystem = SPK::System::create();

		InitializeCriticalSection(&g_GlobalLock);

		QueryPerformanceFrequency(&g_TimerFrequency);
		QueryPerformanceCounter(&g_LastTime);

		g_FPSUpdateFrequency.QuadPart = g_TimerFrequency.QuadPart / 8;
		g_FrameLimiterFrequency.QuadPart = g_TimerFrequency.QuadPart / 30;

		g_Initialized = true;
	}

	PARTY_API void __stdcall PTShutdown()
	{
		if (!g_Initialized) return;

		std::vector<ParticleGroup*> groups_to_remove(std::move(g_ParticleGroups));
		for (auto it = groups_to_remove.begin(); it != groups_to_remove.end(); ++it)
		{
			PTDestroyParticleGroup(*it);
		}

		_g_LineRenderer.Shutdown();

		if (g_DebugFont)
		{
			g_DebugFont->Release();
			g_DebugFont = nullptr;
		}

		if (g_LineRenderer)
		{
			g_LineRenderer->Release();
			g_LineRenderer = nullptr;
		}

		if (g_D3D9Device)
		{
			g_D3D9Device->Release();
			g_D3D9Device = nullptr;
		}

		if (g_D3D9Interface)
		{
			g_D3D9Interface->Release();
			g_D3D9Interface = nullptr;
		}

		DeleteCriticalSection(&g_GlobalLock);
		g_Initialized = false;
	}

	PARTY_API void __stdcall PTResetView()
	{
		g_ShouldReset = true;
	}

	PARTY_API void __stdcall PTRenderLoop()
	{

		LARGE_INTEGER current_time;
		QueryPerformanceCounter(&current_time);
		long long raw_dt = current_time.QuadPart - g_LastTime.QuadPart;

		while (raw_dt < g_FrameLimiterFrequency.QuadPart)
		{
			QueryPerformanceCounter(&current_time);
			raw_dt = current_time.QuadPart - g_LastTime.QuadPart;
			_mm_pause();
		}

		Lock kungfoo_death_grip(g_GlobalLock);

		g_LastTime = current_time;
		double dt = (double)raw_dt / g_TimerFrequency.QuadPart; 

		if (!g_D3D9Device) return;

		if (g_ShouldReset)
		{
			RECT client_rect;
			GetClientRect(g_D3D9Parameters.hDeviceWindow, &client_rect);

			UINT width = client_rect.right - client_rect.left;
			UINT height = client_rect.bottom - client_rect.top;

			if (width == g_D3D9Parameters.BackBufferWidth &&
				height == g_D3D9Parameters.BackBufferHeight)
			{
				g_ShouldReset = false;
			}
			else
			{
				g_D3D9Parameters.BackBufferWidth = width;
				g_D3D9Parameters.BackBufferHeight = height;
				ResetCameraProjection(width, height);

				SPK::DX9::DX9Info::DX9DestroyAllBuffers();
				g_DebugFont->OnLostDevice();
				g_LineRenderer->OnLostDevice();
			}
		}

		HRESULT res = g_D3D9Device->TestCooperativeLevel();
		if (g_ShouldReset || res != S_OK)
		{
			if (res == D3DERR_DEVICELOST)
			{
				SPK::DX9::DX9Info::DX9DestroyAllBuffers();
				g_DebugFont->OnLostDevice();
				g_LineRenderer->OnLostDevice();
			}
			else if (res == D3DERR_DEVICENOTRESET || g_ShouldReset)
			{
				g_D3D9Device->Reset(&g_D3D9Parameters);
				g_DebugFont->OnResetDevice();
				g_LineRenderer->OnResetDevice();
				g_ShouldReset = false;
			}
			else
			{
				wchar_t msg_text[128];
				swprintf_s(msg_text, L"The preview thread has encountered an error. Code %x", res);
				MessageBoxW(NULL, msg_text, L"Preview", MB_OK | MB_ICONERROR);
				PTShutdown();
			}
			return;
		}

		g_D3D9Device->Clear(0, NULL, D3DCLEAR_TARGET | D3DCLEAR_ZBUFFER, g_BackgroundColor, 1.0f, 0);
		g_D3D9Device->BeginScene();

		g_LineRenderer->SetAntialias(TRUE);
		g_LineRenderer->Begin();
		g_OrientationWidget.Render(g_LineRenderer);
		g_LineRenderer->End();

		_g_LineRenderer.Begin();
		g_D3D9Device->SetRenderState(D3DRS_ALPHABLENDENABLE, FALSE);
		if (g_GridEnabled) g_GridWidget.Render(&g_Camera.view_projection, _g_LineRenderer);
		//g_BoxWidget.Render(XMFLOAT3(0.5f, 0, 0), XMFLOAT3(0.5f, 1, 1), &g_Camera.view_projection, _g_LineRenderer);
		//g_SphereWidget.Render(XMFLOAT3(0, 0, 0), 1, &g_Camera.view_projection, _g_LineRenderer);
		_g_LineRenderer.End();


		if (g_MotionEnabled)
		{
			g_CurrentMotionAngle += g_MotionRPS * dt;
			XMMATRIX motion_transform = XMMatrixMultiply(XMMatrixTranslation(0, 0, g_MotionRadius), XMMatrixRotationY(g_CurrentMotionAngle));
			XMFLOAT3 position;
			XMStoreFloat3(&position, motion_transform.r[3]);

			g_ParticleSystem->setTransformPosition(SPK::Vector3D(position.x, position.y, position.z));
			g_ParticleSystem->updateTransform();
		}
		else
		{
			g_ParticleSystem->setTransformPosition(SPK::Vector3D(0, 0, 0));
			g_ParticleSystem->updateTransform();
		}

		for (auto it = g_ParticleGroups.begin(); it != g_ParticleGroups.end(); ++it)
		{
			ParticleGroup* group = *it;
			if (group->next_burst >= group->burst_list.size()) continue;

			group->age += dt;

			Burst& burst = group->burst_list[group->next_burst];
			if (group->age > burst.time)
			{
				group->emitter->setTank(burst.tank);
				++group->next_burst;
			}
		}

		g_ParticleSystem->update(dt);
		g_ParticleSystem->render();

		wchar_t fps_text[128];
		swprintf_s(fps_text, L"FPS: %.2f\nCamera Distance: %.2f\n", g_FPS, g_Camera.GetDistance());

		RECT text_rect = { 10, 10, 10, 10 };

		g_DebugFont->DrawTextW(NULL, fps_text, -1, &text_rect, DT_NOCLIP, D3DCOLOR_XRGB(255, 255, 255));


		g_D3D9Device->EndScene();
		g_D3D9Device->Present(NULL, NULL, NULL, NULL);

		++g_FrameCounter;

		long long fps_duration = current_time.QuadPart - g_FPSTimer.QuadPart;
		if (fps_duration > g_FPSUpdateFrequency.QuadPart)
		{
			g_FPS = g_FrameCounter / ((double)fps_duration / g_TimerFrequency.QuadPart);
			g_FPSTimer = current_time;
			g_FrameCounter = 0;
		}
	}

	PARTY_API void __stdcall PTSetCameraDistance(float distance)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		g_Camera.SetDistance(distance);
	}

	PARTY_API void __stdcall PTSetCameraRotation(float yaw, float pitch)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		g_Camera.SetRotation(yaw, pitch);
		g_OrientationWidget.Update(&g_Camera.view, &g_OrthoProjection);
	}

	PARTY_API void __stdcall PTSetGridEnabled(bool enabled)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		g_GridEnabled = enabled;
	}

	PARTY_API void __stdcall PTSetBackgroundColor(int r, int g, int b)
	{
		g_BackgroundColor = D3DCOLOR_XRGB(r, g, b);
	}

	void RestartGroup(ParticleGroup* group)
	{
		group->group->empty();
		group->emitter->setTank(group->tank);
		group->emitter->setFlow(group->flow);
		group->age = 0;
		group->next_burst = 0;
	}

	PARTY_API void __stdcall PTSetMotion(bool enabled, float radius, float speed)
	{
		g_MotionEnabled = enabled;
		g_MotionRadius = radius;
		g_MotionRPS = speed / radius;

		for(auto it = g_ParticleGroups.begin(); it != g_ParticleGroups.end(); ++it)
		{
			RestartGroup(*it);
		}
		
	}

	PARTY_API ParticleGroup* __stdcall PTCreateParticleGroup()
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		SPK::Group* group = SPK::Group::create();

		SPK::Model* model = SPK::Model::create(0x7FF, 0, 0, 0x7FF);

		SPK::DX9::DX9QuadRenderer* renderer = SPK::DX9::DX9QuadRenderer::create();
		renderer->setTexture(g_D3D9DefaultTexture);
		renderer->setBlending(SPK::BLENDING_ALPHA);

		SPK::Rotator* rotator = SPK::Rotator::create();

		SPK::Emitter* random_emitter = SPK::RandomEmitter::create();
		group->addEmitter(random_emitter);
		group->setRenderer(renderer);
		group->setModel(model);
		group->addModifier(rotator);

		ParticleGroup* particle_group = new ParticleGroup();
		particle_group->group = group;
		particle_group->model = model;
		particle_group->renderer = renderer;
		particle_group->emitter = random_emitter;

		particle_group->flow = 0;
		particle_group->tank = -1;

		g_ParticleSystem->addGroup(group);
		g_ParticleGroups.push_back(particle_group);
		return particle_group;
	}

	PARTY_API void __stdcall PTDestroyParticleGroup(ParticleGroup* group)
	{
		if (!g_Initialized) return;
		Lock kungfoo_death_grip(g_GlobalLock);

		g_ParticleSystem->removeGroup(group->group);
		SPK_Destroy(group->group);

		auto it = std::find(g_ParticleGroups.begin(), g_ParticleGroups.end(), group);
		if (it != g_ParticleGroups.end()) g_ParticleGroups.erase(it);

		delete group;
	}

	PARTY_API void __stdcall PTEnableParticleGroup(ParticleGroup* group, bool enabled)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		if (enabled)
		{
			RestartGroup(group);
			g_ParticleSystem->addGroup(group->group);
		}
		else g_ParticleSystem->removeGroup(group->group);
	}

	PARTY_API void __stdcall PTSetRendererBlendingMode(ParticleGroup* group, int blending_mode)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		group->renderer->setBlending((SPK::BlendingMode)blending_mode);
	}

	PARTY_API void __stdcall PTSetRendererTexture(ParticleGroup* group, const wchar_t* path)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		LPDIRECT3DTEXTURE9 texture = nullptr;

		auto it = g_LoadedTextures.find(path);
		if (it != g_LoadedTextures.end())
		{
			texture = it->second;
		}
		else
		{
			if (FAILED(D3DXCreateTextureFromFile(g_D3D9Device, path, &texture)))
			{
				texture = g_D3D9DefaultTexture;
			}
			g_LoadedTextures.insert(std::make_pair(path, texture));
		}

		group->renderer->setTexture(texture);
	}

	PARTY_API void __stdcall PTSetRendererScale(ParticleGroup* group, float x, float y)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		group->renderer->setScale(x, y);
	}

	PARTY_API void __stdcall PTSetRendererTextureSlices(ParticleGroup* group, int slices_x, int slices_y)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		group->renderer->setAtlasDimensions(slices_x, slices_y);
	}

	PARTY_API void __stdcall PTSetGravity(ParticleGroup* group, Vector3* gravity)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		group->group->setGravity(*gravity);
	}

	PARTY_API void __stdcall PTSetFriction(ParticleGroup* group, float friction)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		group->group->setFriction(friction);
	}

	PARTY_API void __stdcall PTSetModelLifetime(ParticleGroup* group, float min, float max)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		group->model->setLifeTime(min, max);
	}

	PARTY_API void __stdcall PTBeginUpdate()
	{
		EnterCriticalSection(&g_GlobalLock);
	}

	PARTY_API void __stdcall PTEndUpdate()
	{
		LeaveCriticalSection(&g_GlobalLock);
	}

	PARTY_API void __stdcall PTEnableModelParameter(ParticleGroup* group, int parameter, bool value)
	{
		group->model->setInterpolatorEnabled((SPK::ModelParam)parameter, value);
	}

	struct InterpolatorKeyframeStruct
	{
		float x;
		float y0, y1;
	};

	PARTY_API void __stdcall PTSetInterpolatorKeyframes(ParticleGroup* group, int parameter, const InterpolatorKeyframeStruct* keyframes, int count)
	{
		SPK::Interpolator* interpolator = group->model->getInterpolator((SPK::ModelParam)parameter);
		interpolator->clearGraph();

		for (int i = 0; i < count; ++i)
		{
			interpolator->addEntry(keyframes[i].x, keyframes[i].y0, keyframes[i].y1);
		}
	}

	static void ChangeEmitter(ParticleGroup* group, SharedEmitterProperties* properties, SPK::Emitter* emitter)
	{
		emitter->setFlow(properties->flow);
		emitter->setForce(properties->force_min, properties->force_max);
		emitter->setTank(properties->tank);
		if (group->emitter) 
		{
			emitter->setZone(group->emitter->getZone(), group->emitter->isFullZone());
			group->group->removeEmitter(group->emitter);
		}
		emitter->setZone(emitter->getZone(), properties->full_zone);
		group->group->addEmitter(emitter);
		group->emitter = emitter;

		group->flow = properties->flow;
		group->tank = properties->tank;
	}

	PARTY_API void __stdcall PTSetEmitterNormal(ParticleGroup* group, SharedEmitterProperties* properties)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		SPK::NormalEmitter* emitter = SPK::NormalEmitter::create();
		ChangeEmitter(group, properties, emitter);
	}

	PARTY_API void __stdcall PTSetEmitterRandom(ParticleGroup* group, SharedEmitterProperties* properties)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		SPK::RandomEmitter* emitter = SPK::RandomEmitter::create();
		ChangeEmitter(group, properties, emitter);
	}

	PARTY_API void __stdcall PTSetEmitterSpheric(ParticleGroup* group, SharedEmitterProperties* properties, Vector3* direction, float angle_min, float angle_max)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		SPK::SphericEmitter* emitter = SPK::SphericEmitter::create();
		emitter->setDirection(SPK::Vector3D(direction->x, direction->y, direction->z));
		emitter->setAngles(XMConvertToRadians(angle_min), XMConvertToRadians(angle_max));

		ChangeEmitter(group, properties, emitter);
	}

	PARTY_API void __stdcall PTSetEmitterStatic(ParticleGroup* group, SharedEmitterProperties* properties)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		SPK::StaticEmitter* emitter = SPK::StaticEmitter::create();
		ChangeEmitter(group, properties, emitter);
	}

	PARTY_API void __stdcall PTSetEmitterStraight(ParticleGroup* group, SharedEmitterProperties* properties, Vector3* direction)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		SPK::StraightEmitter* emitter = SPK::StraightEmitter::create();
		emitter->setDirection(SPK::Vector3D(direction->x, direction->y, direction->z));

		ChangeEmitter(group, properties, emitter);
	}

	PARTY_API void __stdcall PTSetEmitterBurstList(ParticleGroup* group, Burst* burst_list, int count)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		group->burst_list.clear();
		group->burst_list.insert(group->burst_list.begin(), burst_list, burst_list + count);
		group->age = 0;
		group->next_burst = 0;
	}

	PARTY_API void __stdcall PTRestartEmitter(ParticleGroup* group)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		RestartGroup(group);
	}

	PARTY_API void __stdcall PTSetZoneAABB(ParticleGroup* group, Vector3* position, Vector3* extents)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		SPK::Zone* zone = SPK::AABox::create(SPK::Vector3D(position->x, position->y, position->z), SPK::Vector3D(extents->x, extents->y, extents->z));
		group->emitter->setZone(zone, group->emitter->isFullZone());
	}

	PARTY_API void __stdcall PTSetZoneCylinder(ParticleGroup* group, Vector3* position, Vector3* direction, float radius, float length)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		SPK::Zone* zone = SPK::Cylinder::create(*position, *direction, radius, length);
		group->emitter->setZone(zone, group->emitter->isFullZone());
	}

	PARTY_API void __stdcall PTSetZonePlane(ParticleGroup* group, Vector3* position, Vector3* direction)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		SPK::Zone* zone = SPK::Plane::create(*position, *direction);
		group->emitter->setZone(zone, group->emitter->isFullZone());
	}

	PARTY_API void __stdcall PTSetZonePoint(ParticleGroup* group, Vector3* position)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		SPK::Zone* zone = SPK::Point::create(*position);
		group->emitter->setZone(zone, group->emitter->isFullZone());
	}

	PARTY_API void __stdcall PTSetZoneRing(ParticleGroup* group, Vector3* position, Vector3* direction, float inner_radius, float outer_radius)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		SPK::Zone* zone = SPK::Ring::create(*position, *direction, inner_radius, outer_radius);
		group->emitter->setZone(zone, group->emitter->isFullZone());
	}

	PARTY_API void __stdcall PTSetZoneSphere(ParticleGroup* group, Vector3* position, float radius)
	{
		Lock kungfoo_death_grip(g_GlobalLock);

		SPK::Zone* zone = SPK::Sphere::create(*position, radius);
		group->emitter->setZone(zone, group->emitter->isFullZone());
	}
};