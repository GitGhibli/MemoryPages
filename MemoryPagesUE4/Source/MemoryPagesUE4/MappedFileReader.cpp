// Fill out your copyright notice in the Description page of Project Settings.

#include "MappedFileReader.h"
#include "Engine/Engine.h"
#include "Async.h"

TCHAR szName[] = TEXT("InitializationMapFile");
TCHAR GoToFileName[] = TEXT("GotoLocationFile");
TCHAR feedbackFileName[] = TEXT("FeedbackFile");

// Sets default values for this component's properties
UMappedFileReader::UMappedFileReader()
{
	// Set this component to be initialized when the game starts, and to be ticked every frame.  You can turn these features
	// off to improve performance if you don't need them.
	PrimaryComponentTick.bCanEverTick = true;

	// ...
}

void UMappedFileReader::SendFeedback(FString feedbackMessage)
{
	Buffer = new byte[1 + 1024 * 2];
	for (int i = 0; i < 1 + 1024 * 2; i++) {
		Buffer[i] = '\0';
	}

	*Buffer = false;

	auto messageContent = (TCHAR*)(&Buffer[1]);
	memcpy(messageContent, *feedbackMessage, feedbackMessage.Len() * 2 + 1);

	TryWriteToMemory();
}

void UMappedFileReader::TryWriteToMemory()
{
	if (WaitForSingleObject(FeedbackMutex, 1) == WAIT_OBJECT_0) {
		fseek(feedbackStream, 0, SEEK_SET);
		size_t f = fwrite(Buffer, 1, 1 + 1024 * 2, feedbackStream);
		if (f == 0) {
			UE_LOG(LogTemp, Warning, TEXT("Writing prohibited"));
		}

		int error = ferror(feedbackStream);

		fflush(feedbackStream);
		delete[] Buffer;

		FeedbackSent = true;
		ReleaseMutex(FeedbackMutex);
	}
	else {
		FeedbackSent = false;
	}
}

// Called when the game starts
void UMappedFileReader::BeginPlay()
{
	Super::BeginPlay();

	InitializeFeedbackFile();
}

void UMappedFileReader::EndPlay(const EEndPlayReason::Type EndPlayReason) {
	if (feedbackStream) {
		fclose(feedbackStream);
	}
	
	if (gotoStream) {
		fclose(gotoStream);
	}
}

// Called every frame
void UMappedFileReader::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	if (!Initialized && !Initializing) {
		Initialize();
	}
	else {
		float x;
		float y;

		if (ReadGoToMemory(x, y)) {
			onGoToReceived.Broadcast(x, y);
		}
	}

	if (!FeedbackSent) {
		TryWriteToMemory();
	}
}

bool UMappedFileReader::ReadGoToMemory(float& x, float& y) {

	if (!gotoStream) {
		gotoStream = _fsopen("D:\\Temp\\GotoLocationFile", "r+b", _SH_DENYNO);
	}

	if (gotoStream) {
		if (!GoToMutex) {
			GoToMutex = OpenMutex(
				MUTEX_ALL_ACCESS,
				FALSE,
				TEXT("GoToLocationMutex"));
		}

		if (GoToMutex != NULL && WaitForSingleObject(GoToMutex, 1) == WAIT_OBJECT_0) {

			fseek(gotoStream, 0, SEEK_SET);
			struct GoToInstruction* gotoInstruction = new GoToInstruction();
			gotoInstruction->IsProcessed = true;
			fread(gotoInstruction, sizeof(struct GoToInstruction), 1, gotoStream);

			if (!gotoInstruction->IsProcessed) {
				UE_LOG(LogTemp, Warning, TEXT("Goto message came"));

				x = gotoInstruction->X;
				y = gotoInstruction->Y;

				fseek(gotoStream, 0, SEEK_SET);
				byte* processed = new byte[1];
				*processed = 1;
				fwrite(processed, 1, 1, gotoStream);

				fflush(gotoStream);
				ReleaseMutex(GoToMutex);
				return true;
			}
		}

		ReleaseMutex(GoToMutex);
	}

	return false;
}

void UMappedFileReader::InitializeFeedbackFile()
{
	FeedbackMutex = OpenMutex(
		MUTEX_ALL_ACCESS,
		FALSE,
		TEXT("FeedbackMutex"));

	feedbackStream = _fsopen("D:\\Temp\\FeedbackFile", "r+b", _SH_DENYNO);
}

void UMappedFileReader::Initialize() {
	Initializing = true;
	TArray<FGis3DLayer>* layers = new TArray<FGis3DLayer>();
	TArray<FGis3DObject>* gisObjects = new TArray<FGis3DObject>();

	TFunction<void()> Task = [this, layers, gisObjects]() {
		ProcessInitMessage(layers, gisObjects);
	};

	TFunction<void()> Callback = [this, layers, gisObjects]() {
		if (Initialized) {
			UE_LOG(LogTemp, Warning, TEXT("Callback: layers count %d"), layers->Num());
			UE_LOG(LogTemp, Warning, TEXT("Callback: objects count %d"), gisObjects->Num());
			onInitializationReceived.Broadcast(*layers, *gisObjects);
		}

		Initializing = false;
	};

	Async(EAsyncExecution::ThreadPool, Task, Callback);
}

FGis3DLayer UMappedFileReader::ToLayer(LayerProxy proxy) {
	FGis3DLayer layer;
	layer.Id = proxy.Id;
	layer.Name = proxy.Name;
	layer.ParentId = proxy.ParentId;
	layer.Color = FColor(proxy.R, proxy.G, proxy.B, proxy.A);
	return layer;
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

void UMappedFileReader::ReadInitContent(int contentSize, TArray<FGis3DLayer>* layers, TArray<FGis3DObject>* gisObjects)
{
	int* layersCount = new int;
	fread(layersCount, sizeof(int), 1, stream);

	LayerProxy* layersProxies = new LayerProxy[*layersCount];
	fread(layersProxies, sizeof(LayerProxy), *layersCount, stream);

	for (auto i = 0; i < *layersCount; i++)
	{
		layers->Add(ToLayer(layersProxies[i]));
	}

	UE_LOG(LogTemp, Warning, TEXT("Message content: Layers received: %d"), *layersCount);

	int* objectsCount = new int;
	fread(objectsCount, sizeof(int), 1, stream);

	Gis3DObjectProxy* objectProxies = new Gis3DObjectProxy[*objectsCount];
	fread(objectProxies, sizeof(Gis3DObjectProxy), *objectsCount, stream);

	for (auto i = 0; i < *objectsCount; i++)
	{
		gisObjects->Add(ToFGis3DObject(objectProxies[i]));
	}

	UE_LOG(LogTemp, Warning, TEXT("Message content: Objects received: %d"), *objectsCount);

	fseek(stream, -1, SEEK_END);
	byte* processed = new byte[1];
	*processed = 1;
	size_t f = fwrite(processed, 1, 1, stream);

	fflush(stream);

	UE_LOG(LogTemp, Warning, TEXT("Procesed"));
}

int UMappedFileReader::GetInitContentSize() {
	int* buffer = new int();
	fread(buffer, sizeof(int), 1, stream);
	return *buffer;
}

void UMappedFileReader::ProcessInitMessage(TArray<FGis3DLayer>* layers, TArray<FGis3DObject>* gisObjects)
{
	if (!InitMutex) {
		InitMutex = OpenMutex(
			MUTEX_ALL_ACCESS,
			FALSE,
			TEXT("MMFMutex"));
		if (!InitMutex) {
			InitMutex = CreateMutex(
				NULL,
				FALSE,
				TEXT("MMFMutex"));
		}
	}

	if (InitMutex != NULL && WaitForSingleObject(InitMutex, 1) == WAIT_OBJECT_0) {
		stream = _fsopen("D:\\Temp\\InitializationFile", "r+b", _SH_DENYNO);

		if (stream) {
			UE_LOG(LogTemp, Warning, TEXT("Initializing"));
			int contentSize = GetInitContentSize();
			if (contentSize == 0) {
				ReleaseMutex(InitMutex);
				return;
			}

			ReadInitContent(contentSize, layers, gisObjects);

			Initialized = true;
			fflush(stream);
			fclose(stream);

			ReleaseMutex(InitMutex);
		}
	}
}