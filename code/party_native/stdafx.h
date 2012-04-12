#pragma once

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <xnamath.h>
#include <mmsystem.h>

#include <d3d9.h>
#include <d3dx9.h>

template <typename T> inline void SAFE_RELEASE(T*& ptr)
{
	if (ptr) 
	{ 
		ptr->Release();
		ptr = nullptr;
	}
}

#pragma warning(disable: 4100) // unreferenced parameter, grr @ spark