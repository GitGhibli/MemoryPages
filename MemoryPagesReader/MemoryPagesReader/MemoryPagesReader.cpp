// MemoryPagesReader.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include "pch.h"
#include <iostream>
#include <Windows.h>

using namespace std;

struct MyStruct
{
	int MyNumber;

	int MyAnotherNumber;

	char MyText[255];

	char MyAnotherText[255];
};

#define BUF_SIZE 518

TCHAR szName[] = TEXT("MyTestMapFile");

int main()
{
	HANDLE hMapFile;

	char* pBuf;

	hMapFile = OpenFileMapping(
		FILE_MAP_ALL_ACCESS,
		FALSE,
		szName);

	if (hMapFile == NULL) {
		cout << "Could not open file";
		return 1;
	}

	pBuf = (char*)MapViewOfFile(
		hMapFile,
		FILE_MAP_ALL_ACCESS,
		0,
		0,
		BUF_SIZE);

	if (pBuf == NULL) {
		cout << "Could not map view of file";
		return 1;
	}

	MyStruct* myStruct = (MyStruct*)pBuf;

	CloseHandle(hMapFile);

	cout << myStruct->MyNumber << '\n';
	cout << myStruct->MyAnotherNumber << '\n';
	cout << myStruct->MyText << '\n';
	cout << myStruct->MyAnotherText << '\n';

	return 0;
}
