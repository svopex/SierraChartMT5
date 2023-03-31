#include <stdio.h>
#include <stdlib.h>
#include <string>
#include "sierrachart.h"

SCDLLName("OandaConnector")

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

typedef int(__cdecl *MakePositionProcedure)(const char*, INT, OrdersStructType[], INT);

void InsertToOrderStruct(OrdersStructType* ordersStructType, s_SCTradeOrder* OrderDetails)
{
	ordersStructType->InternalOrderID = OrderDetails->InternalOrderID;
	ordersStructType->Symbol = OrderDetails->Symbol.GetChars();
	ordersStructType->OrderType = OrderDetails->OrderType.GetChars();
	ordersStructType->OrderQuantity = OrderDetails->OrderQuantity;
	ordersStructType->BuySell = OrderDetails->BuySell;
	ordersStructType->Price1 = OrderDetails->Price1;
	ordersStructType->OrderStatusCode = OrderDetails->OrderStatusCode;
	ordersStructType->ParentInternalOrderID = OrderDetails->ParentInternalOrderID;
	ordersStructType->OrderTypeAsInt = OrderDetails->OrderTypeAsInt;
  ordersStructType->AvgFillPrice = OrderDetails->AvgFillPrice;
}

SCSFExport scsf_OandaConnector(SCStudyGraphRef sc)
{
	if (sc.SetDefaults)
	{
		sc.GraphName = "OandaConnector";
		sc.StudyDescription = "OandaConnector";
		sc.GraphRegion = 0;
		sc.FreeDLL = 0;
		sc.AutoLoop = 0;
		return;
	}

	HINSTANCE hinstLib = LoadLibrary(TEXT("SierraChartServiceClientBridge.dll"));
	if (hinstLib != NULL)
	{
		MakePositionProcedure MakePosition = (MakePositionProcedure)GetProcAddress(hinstLib, "MakePosition");
		if (MakePosition != NULL)
		{
			OrdersStructType* ordersStructType = new OrdersStructType[1000];
			s_SCTradeOrder OrderDetails;
			int indexArray = 0;
			int indexGetOrderByIndex = 0;
			while (sc.GetOrderByIndex(indexGetOrderByIndex, OrderDetails) != SCTRADING_ORDER_ERROR)
			{
				indexGetOrderByIndex++;

				if (OrderDetails.OrderStatusCode != SCT_OSC_OPEN)
					continue;

				if (OrderDetails.ParentInternalOrderID == 0)	
					continue;

				InsertToOrderStruct(&ordersStructType[indexArray], &OrderDetails);
				indexArray++;
			}
			indexGetOrderByIndex = 0;
			while (sc.GetOrderByIndex(indexGetOrderByIndex, OrderDetails) != SCTRADING_ORDER_ERROR)
			{
				indexGetOrderByIndex++;

				bool linkExists = false;
				for (int i = 0; i < indexArray; i++)
				{
					if (ordersStructType[i].ParentInternalOrderID == OrderDetails.InternalOrderID)
					{
						linkExists = true;
						break;
					}
				}
				if (linkExists)
				{
					InsertToOrderStruct(&ordersStructType[indexArray], &OrderDetails);
					indexArray++;
				}
			}
			
			s_SCPositionData InternalPositionData;
			sc.GetTradePosition(InternalPositionData);
			MakePosition(InternalPositionData.Symbol.GetChars(), InternalPositionData.PositionQuantity, ordersStructType, indexArray);
		}
		FreeLibrary(hinstLib);
	}
}
