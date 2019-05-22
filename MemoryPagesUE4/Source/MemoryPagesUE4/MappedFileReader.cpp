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
	UnmapViewOfFile(GoToInstruction);
	//CloseHandle(GoToFile);
}

// Called every frame
void UMappedFileReader::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	if (!Initialized) {
		LayerProxy * layers = nullptr;
		TArray<FGis3DObject>* gisObjects = new TArray<FGis3DObject>();

		ReadInitializationFromMemory(layers, gisObjects);

		if (Initialized) {
			onInitializationReceived.Broadcast(*gisObjects);
		}
	}
	else {
		float x;
		float y;

		if (ReadGoToMemory(x, y)) {
			onGoToReceived.Broadcast(x, y);
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

bool UMappedFileReader::ReadGoToMemory(float& x, float& y) {

	if (!GoToFile) {
		InitializeGoToFile();
	}

	if (GoToFile) {
		if (!GoToMutex) {
			GoToMutex = OpenMutex(
				MUTEX_ALL_ACCESS,
				FALSE,
				TEXT("GoToLocationMutex"));
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

FGis3DObject UMappedFileReader::ToFGis3DObject(Gis3DObjectProxy proxy) {
	FGis3DObject gisObject;
	gisObject.Id = proxy.Id;
	gisObject.LayerId = proxy.LayerId;
	gisObject.ShortName = proxy.ShortName;
	gisObject.Name = proxy.Name;
	gisObject.Description = proxy.Description;
	gisObject.X = proxy.X;
	gisObject.Y = proxy.Y;
	gisObject.Height = proxy.Height;

	return gisObject;
}

void UMappedFileReader::ReadInitContent(int contentSize, LayerProxy* layers, TArray<FGis3DObject>* gisObjects)
{
	auto pointer = (byte*)MapViewOfFile(
		InitFile,
		FILE_MAP_ALL_ACCESS,
		0,
		0,
		contentSize);

	int layersCount = (int)pointer[4]; //count of layers which follow

	layers = (LayerProxy*)&pointer[8]; //Start of layers array
	for (auto i = 0; i < layersCount; i++)
	{
		auto layerProxy = &layers[i];
		UE_LOG(LogTemp, Warning, TEXT("Layer initialized"));
	}

	int layersSize = layersCount * sizeof(LayerProxy);
	int gisCount = (int)pointer[8 + layersSize]; //count of gisObjects which follow

	Gis3DObjectProxy* gisObjectsProxies = (Gis3DObjectProxy*)&pointer[8 + layersSize + 4]; //Start of gisObjectsArray
	for (auto i = 0; i < gisCount; i++)
	{
		gisObjects->Add(ToFGis3DObject(gisObjectsProxies[i]));
		UE_LOG(LogTemp, Warning, TEXT("GisObject initialized"));
	}

	pointer[contentSize - 1] = true;
}

int UMappedFileReader::GetInitContentSize() {
	return *(int*)MapViewOfFile(
		InitFile,
		FILE_MAP_ALL_ACCESS,
		0,
		0,
		4);
}

void UMappedFileReader::ReadInitializationFromMemory(LayerProxy * layers, TArray<FGis3DObject>* gisObjects)
{
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

			ReadInitContent(contentSize, layers, gisObjects);

			Initialized = true;

			ReleaseMutex(InitMutex);
		}

		CloseHandle(InitFile);
	}
}

