// Fill out your copyright notice in the Description page of Project Settings.

#include "MappedFileReader.h"

#include "Windows.h"

struct MyStruct
{
	int MyNumber;

	int MyAnotherNumber;

	char MyText[255];

	char MyAnotherText[255];
};

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

	HANDLE hMapFile;

	hMapFile = OpenFileMapping(
		FILE_MAP_ALL_ACCESS,
		FALSE,
		szName);

	if (hMapFile != NULL) {
		char* pBuf;
		pBuf = (char*)MapViewOfFile(
			hMapFile,
			FILE_MAP_ALL_ACCESS,
			0,
			0,
			BUF_SIZE);

		MyStruct* myStruct;

		if (pBuf != NULL) {
			myStruct = (MyStruct*)pBuf;
		}

		CloseHandle(hMapFile);
	}
}


// Called every frame
void UMappedFileReader::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	// ...
}

