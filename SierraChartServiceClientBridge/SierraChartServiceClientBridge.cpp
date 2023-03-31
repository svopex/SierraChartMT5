// This is the main DLL file.
#include "stdafx.h"

#include "SierraChartServiceClientBridge.h"

using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;

#ifdef __cplusplus    // If used by C++ code, 
extern "C" {          // we need to export the C interface
#endif

typedef struct
{
	int InternalOrderID;
	const char* Symbol;
	const char* OrderType;
	int OrderQuantity;
	int BuySell;
	double Price1;
	int OrderStatusCode;
	int ParentInternalOrderID;
	int OrderTypeAsInt;
	double AvgFillPrice;
} OrdersStructType;

#ifdef __cplusplus
}
#endif

void MakePositionWrapper(const char* symbol, int positionQuantity, OrdersStructType c_ordersStructType[], int ordersStructTypeCount)
{
	System::Collections::Generic::List<SierraChartServiceClient::SierraOrder^>^ sierraOrders
		= gcnew System::Collections::Generic::List<SierraChartServiceClient::SierraOrder^>();

	for (int i = 0; i < ordersStructTypeCount; i++)
	{
		SierraChartServiceClient::SierraOrder^ SierraOrder = gcnew SierraChartServiceClient::SierraOrder();
		SierraOrder->InternalOrderID = c_ordersStructType[i].InternalOrderID;
		SierraOrder->Symbol = gcnew System::String(c_ordersStructType[i].Symbol);
		SierraOrder->OrderType = gcnew System::String(c_ordersStructType[i].OrderType);
		SierraOrder->OrderQuantity = c_ordersStructType[i].OrderQuantity;
		SierraOrder->BuySell = c_ordersStructType[i].BuySell;
		SierraOrder->Price1 = c_ordersStructType[i].Price1;
		SierraOrder->OrderStatusCode = c_ordersStructType[i].OrderStatusCode;
		SierraOrder->ParentInternalOrderID = c_ordersStructType[i].ParentInternalOrderID;
		SierraOrder->OrderTypeAsInt = c_ordersStructType[i].OrderTypeAsInt;
		SierraOrder->AvgFillPrice = c_ordersStructType[i].AvgFillPrice;
		sierraOrders->Add(SierraOrder);


	}

	System::String^ const stringSymbol = gcnew System::String(symbol);
	SierraChartServiceClient::ServeSierraChart::MakePosition(stringSymbol, positionQuantity, sierraOrders);
}

#ifdef __cplusplus    // If used by C++ code, 
extern "C" {          // we need to export the C interface
#endif

__declspec(dllexport) void __cdecl MakePosition(const char* symbol, int positionQuantity, OrdersStructType c_ordersStructType[], int ordersStructTypeCount)
{
	MakePositionWrapper(symbol, positionQuantity, c_ordersStructType, ordersStructTypeCount);
}

#ifdef __cplusplus
}
#endif
