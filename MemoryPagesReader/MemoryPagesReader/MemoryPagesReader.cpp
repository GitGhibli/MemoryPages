// MemoryPagesReader.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include "pch.h"
#include <iostream>
#include <Windows.h>

using namespace std;

struct LayerProxy
{
	int Id;
	int ParentId;
	char Name[256];
	byte R;
	byte G;
	byte B;
	byte A;
};

struct Gis3DObjectProxy
{
	int Id;
	int LayerId;
	char ShortName[256];
	char Name[256];
	char Description[256];
	float X;
	float Y;
	float Height;
};

TCHAR szName[] = TEXT("InitializationMapFile");
TCHAR szNameMessage[] = TEXT("GoToLocationMapFile");

int ReadInitialMessage() {
	HANDLE hMutex;

	hMutex = OpenMutex(
		MUTEX_ALL_ACCESS,            // request full access
		FALSE,                       // handle not inheritable
		TEXT("MMFMutex"));  // object name

	HANDLE hMapFile = OpenFileMapping(
		FILE_MAP_ALL_ACCESS,
		FALSE,
		szName);

	if (hMapFile == NULL) {
		cout << "Could not open file";
		return 1;
	}

	if (hMutex != NULL && WaitForSingleObject(hMutex, 1) == WAIT_OBJECT_0) {
		auto pointerToFileSize = (int*)MapViewOfFile(
			hMapFile,
			FILE_MAP_ALL_ACCESS,
			0,
			0,
			4);

		if (*pointerToFileSize == 0) {
			CloseHandle(hMapFile);
			ReleaseMutex(hMutex);
			return 0;
		}

		LPVOID pointer = MapViewOfFile(
			hMapFile,
			FILE_MAP_ALL_ACCESS,
			0,
			0,
			*pointerToFileSize);

		int* layersCountPtr = ((int*)pointer) + 1;

		LayerProxy* layerProxyPtr = (LayerProxy*)(layersCountPtr + 1);
		for (auto i = 0; i < *(layersCountPtr); i++)
		{
			auto layerProxy = &layerProxyPtr[i];
			cout << layerProxy->Id << '\n';
		}

		int* gisCountPtr = (int*)&layerProxyPtr[*(layersCountPtr)];

		Gis3DObjectProxy* gisProxyPtr = (Gis3DObjectProxy*)(gisCountPtr + 1);
		for (auto i = 0; i < *gisCountPtr; i++)
		{
			auto gisProxy = &gisProxyPtr[i];
			cout << gisProxy->Id << '\n';
		}

		auto byteArray = (byte*)pointer;
		//((int*)pointer)[0] = 9027;
		byteArray[*pointerToFileSize - 1] = true;

		CloseHandle(hMapFile);
		ReleaseMutex(hMutex);

		return 0;
	}
	return -1;
}

int main()
{
	ReadInitialMessage();
	
	return 0;
}
