// Fill out your copyright notice in the Description page of Project Settings.

#include "MappedFileReader.h"
#include "Engine/Engine.h"

#define BUF_SIZE 518

TCHAR szName[] = TEXT("MyTestMapFile");


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
	CloseHandle(hMapFile);
}

void UMappedFileReader::StructReceived() {
}

// Called every frame
void UMappedFileReader::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	if (hMapFile == NULL) {
		hMapFile = OpenFileMapping(
			FILE_MAP_ALL_ACCESS,
			FALSE,
			szName);

		if (hMapFile != NULL) {
			myStruct = (MyStruct*)MapViewOfFile(
				hMapFile,
				FILE_MAP_ALL_ACCESS,
				0,
				0,
				BUF_SIZE);
		}
	}

	if (myStruct != NULL) {
		if (myStruct->MyNumber > lastMessageIndex) {
			onStructReceived.Broadcast();
			lastMessageIndex = myStruct->MyNumber;
		}
	}
}

