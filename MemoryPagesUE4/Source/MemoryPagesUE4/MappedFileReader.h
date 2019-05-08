// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Windows.h"
#include "Components/ActorComponent.h"
#include "MappedFileReader.generated.h"

struct MyStruct
{
	int MyNumber;

	int MyAnotherNumber;

	char MyText[255];

	char MyAnotherText[255];
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

	int lastMessageIndex = 0;
	MyStruct* myStruct;
};