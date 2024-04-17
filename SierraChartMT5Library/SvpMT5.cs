using System;
using System.Configuration;
using System.Threading;
using MtApi5;
using OandaApiBusinessClass;

namespace SierraChartMT5Library
{
    public class SvpMT5
    {
        private readonly MtApi5Client apiClient = new MtApi5Client();

		private double GetDivider(string instrument)
        {
            string value = ConfigurationManager.AppSettings[instrument];
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0.01;
            }
            else
            {
                return Convert.ToDouble(value);
            }
		}

		public static SvpMT5 Instance
        {
            get
            {
                if (instance == null || instance.apiClient.ConnectionState != Mt5ConnectionState.Connected)
                {
                    instance = new SvpMT5();
                    instance.apiClient.BeginConnect(8228);
                    while(instance.apiClient.ConnectionState != Mt5ConnectionState.Connected)
                    {
                        Thread.Sleep(100);
                    }
                }
				return instance;
            }
        }

        private static SvpMT5 instance;

		public string Symbol => apiClient.ChartSymbol(0);

		public void CloseOrder(long orderId)
        {
            apiClient.PositionClose((ulong)orderId);
        }

        public double GetMarketOrderPrice(long marketOrderId)
        {
            if (apiClient.PositionSelectByTicket((ulong)marketOrderId))
            {
                return apiClient.PositionGetDouble(ENUM_POSITION_PROPERTY_DOUBLE.POSITION_PRICE_OPEN);
            }
            return 0;
        }

        public void ModifyMarketOrder(OAOrder newOAMarketOrder)
        {
            apiClient.PositionModify((ulong)newOAMarketOrder.Id, newOAMarketOrder.SL, newOAMarketOrder.PT);
        }

        public long CreateMarketOrder(string instrument, long units, ulong magic = 0, string comment = null)
        {            
            MqlTradeRequest mqlTradeRequest = new MqlTradeRequest();
            mqlTradeRequest.Action = ENUM_TRADE_REQUEST_ACTIONS.TRADE_ACTION_DEAL;
            mqlTradeRequest.Symbol = instrument;
            mqlTradeRequest.Volume = ((double)Math.Abs(units)) * GetDivider(instrument);
            mqlTradeRequest.Type = units > 0 ? ENUM_ORDER_TYPE.ORDER_TYPE_BUY : ENUM_ORDER_TYPE.ORDER_TYPE_SELL;
            mqlTradeRequest.Magic = magic;
            // Todle zde musi byt kvuli ICMARKETS, jinak objednavky nechodi
			mqlTradeRequest.Type_filling = ENUM_ORDER_TYPE_FILLING.ORDER_FILLING_IOC;
			mqlTradeRequest.Comment = comment;
            MqlTradeResult mqlTradeResult;
            apiClient.OrderSend(mqlTradeRequest, out mqlTradeResult);
			Log.WriteMessage($"CreateMarketOrder - {instrument} {mqlTradeResult.Retcode} {mqlTradeResult.Comment}");
			return (long)mqlTradeResult.Order;
        }

        public OAOrders GetMarketOrders(string instrument)
        {
            OAOrders oaOrders = new OAOrders();

            int total = apiClient.PositionsTotal();
            for (int i = 0; i < total; i++)
            {
                OAOrder oaOrder = new OAOrder();
                ulong ticket = apiClient.PositionGetTicket(i);
                apiClient.PositionSelectByTicket(ticket);
                oaOrder.Id = (long)ticket;
                oaOrder.Price = apiClient.PositionGetDouble(ENUM_POSITION_PROPERTY_DOUBLE.POSITION_PRICE_OPEN);
                oaOrder.SL = apiClient.PositionGetDouble(ENUM_POSITION_PROPERTY_DOUBLE.POSITION_SL);
                oaOrder.PT = apiClient.PositionGetDouble(ENUM_POSITION_PROPERTY_DOUBLE.POSITION_TP);
				oaOrder.Instrument = apiClient.PositionGetString(ENUM_POSITION_PROPERTY_STRING.POSITION_SYMBOL);
				oaOrder.Units = (long)(apiClient.PositionGetDouble(ENUM_POSITION_PROPERTY_DOUBLE.POSITION_VOLUME) / GetDivider(oaOrder.Instrument));
                oaOrder.Magic = apiClient.PositionGetInteger(ENUM_POSITION_PROPERTY_INTEGER.POSITION_MAGIC);
                oaOrder.Comment = apiClient.PositionGetString(ENUM_POSITION_PROPERTY_STRING.POSITION_COMMENT);

                if (oaOrder.Instrument == instrument)
                {
                    oaOrders.Add(oaOrder);
                }
            }
            return oaOrders;
        }
    }
}
