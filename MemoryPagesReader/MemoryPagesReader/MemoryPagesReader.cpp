// MemoryPagesReader.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include "pch.h"
#include <list>
#include <iostream>
#include <Windows.h>

using namespace std;

struct LayerProxy
{
	int Id;
	int ParentId;
	wchar_t Name[256];
	byte R;
	byte G;
	byte B;
	byte A;
};

struct Gis3DObjectProxy
{
	int Id;
	int LayerId;
	wchar_t ShortName[256];
	wchar_t Name[256];
	wchar_t Description[256];
	float X;
	float Y;
	float Height;
};

TCHAR szName[] = TEXT("InitializationMapFile");
TCHAR szNameMessage[] = TEXT("GoToLocationMapFile");

HANDLE InitFile = nullptr;
HANDLE InitMutex = nullptr;

void ReadInitContent(int contentSize, list<LayerProxy>* layers, list<Gis3DObjectProxy>* gisObjects) {
	auto pointer = (byte*)MapViewOfFile(
		InitFile,
		FILE_MAP_ALL_ACCESS,
		0,
		0,
		contentSize);

	int layersCount = (int)pointer[4]; //count of layers which follow

	LayerProxy* layersProxies = (LayerProxy*)&pointer[8]; //Start of layers array
	for (auto i = 0; i < layersCount; i++)
	{
		layers->push_back(layersProxies[i]);
	}

	int layersSize = layersCount * sizeof(LayerProxy);
	int gisCount = (int)pointer[8 + layersSize]; //count of gisObjects which follow

	Gis3DObjectProxy* gisObjectsProxies = (Gis3DObjectProxy*)&pointer[8 + layersSize + 4]; //Start of gisObjectsArray
	for (auto i = 0; i < gisCount; i++)
	{
		gisObjects->push_back(gisObjectsProxies[i]);
	}

	pointer[contentSize - 1] = true;

	UnmapViewOfFile(pointer);
}

int GetInitContentSize() {
	HANDLE messageSizeView = MapViewOfFile(
		InitFile,
		FILE_MAP_ALL_ACCESS,
		0,
		0,
		4);

	int result = *(int*)messageSizeView;
	UnmapViewOfFile(messageSizeView);
	return result;
}

void ReadInitialMessage() {
	if (!InitFile) {
		InitFile = OpenFileMapping(
			FILE_MAP_ALL_ACCESS,
			FALSE,
			szName);
	}

	if (InitFile) {
		if (!InitMutex) {
			InitMutex = OpenMutex(
				MUTEX_ALL_ACCESS,
				FALSE,
				TEXT("MMFMutex"));
		}

		if (InitMutex != NULL && WaitForSingleObject(InitMutex, 1) == WAIT_OBJECT_0) {
			int contentSize = GetInitContentSize();
			if (contentSize == 0) {
				ReleaseMutex(InitMutex);
				return;
			}

			list<LayerProxy>* layers = new list<LayerProxy>();
			list<Gis3DObjectProxy>* gisObjects = new list<Gis3DObjectProxy>();
			ReadInitContent(contentSize, layers, gisObjects);

			ReleaseMutex(InitMutex);
		}

		CloseHandle(InitFile);
	}
}

int main()
{
	ReadInitialMessage();

	return 0;
}
