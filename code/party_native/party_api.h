#pragma once

#ifdef PARTY_EXPORTS
#define PARTY_API __declspec(dllexport)
#else
#define PARTY_API __declspec(dllimport)
#endif

class ParticleGroup;

struct Burst
{
	float time;
	int tank;
};

struct SharedEmitterProperties
{
	float flow;
	int tank;
	float force_min;
	float force_max;
	bool full_zone;
};

extern "C"
{
	PARTY_API void __stdcall PTInitialize(HWND window);
	PARTY_API void __stdcall PTShutdown();
	PARTY_API void __stdcall PTResetView();
	PARTY_API void __stdcall PTRenderLoop();


	PARTY_API ParticleGroup* __stdcall PTCreateParticleGroup();
	PARTY_API void __stdcall PTDestroyParticleGroup(ParticleGroup* group);
	PARTY_API void __stdcall PTSetRendererBlendingMode(ParticleGroup* group, int blending_mode);
	PARTY_API void __stdcall PTSetRendererTexture(ParticleGroup* group, const wchar_t* path);
	PARTY_API void __stdcall PTSetRendererScale(ParticleGroup* group, float x, float y);



};