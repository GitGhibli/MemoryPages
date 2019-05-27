// Fill out your copyright notice in the Description page of Project Settings.

#include "SendFeedbackAsync.h"

USendFeedbackAsync::USendFeedbackAsync(const FObjectInitializer& ObjectInitializer) : Super(ObjectInitializer), WorldContextObject(nullptr)
{
}

USendFeedbackAsync* USendFeedbackAsync::SendFeedbackAsync(const UObject* WorldContextObject, FString content)
{
	USendFeedbackAsync* BlueprintNode = NewObject<USendFeedbackAsync>();
	BlueprintNode->WorldContextObject = WorldContextObject;
	BlueprintNode->FeedbackContent;
	return BlueprintNode;
}

void USendFeedbackAsync::Activate()
{
	//WorldContextObject->GetWorld()->GetTimerManager().SetTimerForNextTick(this, &UMyBlueprintAsyncActionBase::ExecuteAfterOneFrame);
	Callback();
}

void USendFeedbackAsync::Callback()
{
	OnFeedbackSent.Broadcast();
}