// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Kismet/BlueprintAsyncActionBase.h"
#include "SendFeedbackAsync.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE(FSendFeedback);

UCLASS()
class MEMORYPAGESUE4_API USendFeedbackAsync : public UBlueprintAsyncActionBase
{
	GENERATED_UCLASS_BODY()

public:
	UPROPERTY(BlueprintAssignable)
	FSendFeedback OnFeedbackSent;

	UFUNCTION(BlueprintCallable, meta = (BlueprintInternalUseOnly = "true", WorldContext = "WorldContextObject"), Category = "Flow Control")
	static USendFeedbackAsync* SendFeedbackAsync(const UObject* WorldContextObject, FString message);

	virtual void Activate() override;

	
private:
	const UObject* WorldContextObject;
	void Callback();

	FString FeedbackContent;
};
