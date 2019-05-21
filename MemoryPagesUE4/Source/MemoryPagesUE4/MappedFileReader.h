// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Windows.h"
#include "Components/ActorComponent.h"
#include "MappedFileReader.generated.h"

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

UCLASS( ClassGroup=(Custom), meta=(BlueprintSpawnableComponent) )
class MEMORYPAGESUE4_API UMappedFileReader : public UActorComponent
{
	GENERATED_BODY()

public:	
	// Sets default values for this component's properties
	UMappedFileReader();

	DECLARE_DYNAMIC_MULTICAST_DELEGATE(FStructReceivedDelegate);
	void StructReceived();

	UPROPERTY(BlueprintAssignable)
	FStructReceivedDelegate onStructReceived;

protected:
	// Called when the game starts
	virtual void BeginPlay() override;

	virtual void EndPlay(const EEndPlayReason::Type EndPlayReason);

public:	
	// Called every frame
	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

		
private:
	HANDLE hMapFile;
	HANDLE GoToFile;
	bool Initialized = false;

	void ReadInitializationFromMemory(LayerProxy* layers, Gis3DObjectProxy* gisObjects);

	HANDLE GotoMutex;
	int LastGoToMessageIndex = 0;
	bool ReadGoToMemory(float* x, float* y);
};