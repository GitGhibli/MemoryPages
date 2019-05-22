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

struct GoToInstruction
{
public:
	int Index;
	float X;
	float Y;
	bool IsProcessed;
};

DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FStructReceivedDelegate, float, X, float, Y);

UCLASS(ClassGroup = (Custom), meta = (BlueprintSpawnableComponent))
class MEMORYPAGESUE4_API UMappedFileReader : public UActorComponent
{
	GENERATED_BODY()

public:
	// Sets default values for this component's properties
	UMappedFileReader();

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
	HANDLE InitFile = nullptr;
	HANDLE InitMutex = nullptr;
	bool Initialized = false;

	int GetInitContentSize();
	void ReadInitContent(int contentSize, LayerProxy* layers, Gis3DObjectProxy* gisObjects);
	void ReadInitializationFromMemory(LayerProxy* layers, Gis3DObjectProxy* gisObjects);
	
	HANDLE GoToFile = nullptr;
	HANDLE GoToMutex = nullptr;

	GoToInstruction* GoToInstruction;
	int LastGoToMessageIndex = 0;

	void InitializeGoToFile();
	bool ReadGoToMemory(float& x, float& y);
};