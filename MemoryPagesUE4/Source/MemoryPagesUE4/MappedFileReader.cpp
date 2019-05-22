// Fill out your copyright notice in the Description page of Project Settings.

#include "MappedFileReader.h"
#include "Engine/Engine.h"

TCHAR szName[] = TEXT("InitializationMapFile");
TCHAR GoToFileName[] = TEXT("GotoLocationFile");

// Sets default values for this component's properties
UMappedFileReader::UMappedFileReader()
{
	// Set this component to be initialized when the game starts, and to be ticked every frame.  You can turn these features
	// off to improve performance if you don't need them.
	PrimaryComponentTick.bCanEverTick = true;

	// ...
}


// Called when the game starts
void UMappedFileReader::BeginPlay()
{
	Super::BeginPlay();
}

void UMappedFileReader::EndPlay(const EEndPlayReason::Type EndPlayReason) {
}

// Called every frame
void UMappedFileReader::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	if (!Initialized) {
		LayerProxy * layers = nullptr;
		Gis3DObjectProxy* gisObjects = nullptr;

		ReadInitializationFromMemory(layers, gisObjects);
	}
	else {
		float x;
		float y;

		if (ReadGoToMemory(x, y)) {
			onStructReceived.Broadcast(x, y);
		}
	}
}

void UMappedFileReader::InitializeGoToFile()
{
	GoToFile = OpenFileMapping(
		FILE_MAP_ALL_ACCESS,
		FALSE,
		GoToFileName);

	if (GoToFile) {
		GoToInstruction = (struct GoToInstruction*)MapViewOfFile(
			GoToFile,
			FILE_MAP_ALL_ACCESS,
			0,
			0,
			13);
	}
}

void UMappedFileReader::OpenGoToMutex()
{
	GoToMutex = OpenMutex(
		MUTEX_ALL_ACCESS,
		FALSE,
		TEXT("GoToLocationMutex"));

}

bool UMappedFileReader::ReadGoToMemory(float& x, float& y) {

	if (!GoToFile) {
		InitializeGoToFile();
	}

	if (GoToFile) {
		if (!GoToMutex) {
			OpenGoToMutex();
		}
		
		if (GoToMutex != NULL && WaitForSingleObject(GoToMutex, 1) == WAIT_OBJECT_0) {

			if (GoToInstruction != NULL)
			{
				if (GoToInstruction->Index > LastGoToMessageIndex) {
					UE_LOG(LogTemp, Warning, TEXT("Goto message came"));

					x = GoToInstruction->X;
					y = GoToInstruction->Y;

					GoToInstruction->IsProcessed = true;
					LastGoToMessageIndex++;

					ReleaseMutex(GoToMutex);
					return true;
				}
			}

			ReleaseMutex(GoToMutex);
		}
	}

	return false;
}

void UMappedFileReader::ReadInitializationFromMemory(LayerProxy * layers, Gis3DObjectProxy * gisObjects)
{
	if (hMapFile == NULL) {
		hMapFile = OpenFileMapping(
			FILE_MAP_ALL_ACCESS,
			FALSE,
			szName);
	}

	if (hMapFile != NULL) {

		HANDLE hMutex = OpenMutex(
			MUTEX_ALL_ACCESS,
			FALSE,
			TEXT("MMFMutex"));

		if (hMutex != NULL && WaitForSingleObject(hMutex, 1) == WAIT_OBJECT_0) {

			auto pointerToFileSize = (int*)MapViewOfFile(
				hMapFile,
				FILE_MAP_ALL_ACCESS,
				0,
				0,
				4);

			if (pointerToFileSize == nullptr || *pointerToFileSize == 0) {
				ReleaseMutex(hMutex);
				return;
			}

			UE_LOG(LogTemp, Warning, TEXT("Message size"));

			LPVOID pointer = MapViewOfFile(
				hMapFile,
				FILE_MAP_ALL_ACCESS,
				0,
				0,
				*pointerToFileSize + 4);

			int* layersCountPtr = (int*)pointer;

			LayerProxy* layers = (LayerProxy*)(layersCountPtr + 2);
			for (auto i = 0; i < *(layersCountPtr + 1); i++)
			{
				auto layerProxy = &layers[i];
				UE_LOG(LogTemp, Warning, TEXT("Layer initialized"));
			}

			int* gisCountPtr = (int*)&layers[*(layersCountPtr + 1)];

			Gis3DObjectProxy* gisObjects = (Gis3DObjectProxy*)(gisCountPtr + 1);
			for (auto i = 0; i < *gisCountPtr; i++)
			{
				auto gisProxy = &gisObjects[i];
				UE_LOG(LogTemp, Warning, TEXT("GisObject initialized"));
			}

			auto byteArray = (byte*)pointer;
			byteArray[*pointerToFileSize - 1] = true;

			Initialized = true;

			CloseHandle(hMapFile);
			ReleaseMutex(hMutex);
		}
	}
}

