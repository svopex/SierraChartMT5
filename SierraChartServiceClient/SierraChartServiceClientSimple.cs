using System;
using System.Collections.Generic;
using System.Linq;

namespace SierraChartServiceClient
{
/*
    public class ServeSierraChartSimple
    {               
        public static void Buy(InstrumentConversion instrumentConversion, int sierraPosition, int oandaPosition, OAOrders OAOrders)
        {
            int positionToBuy = sierraPosition - oandaPosition;

            OAOrder oaMarketOrder = new OAOrder();
            oaMarketOrder.Instrument = instrumentConversion.OandaInstrumentName;
            oaMarketOrder.Units = positionToBuy;
            oaMarketOrder.Id = OandaAccountApi.CreateMarketOrder(InstrumentConversion.SierraChartAccountId, oaMarketOrder);

            if (oandaPosition == 0)
            {
                // nastav SL pri prvni objednavce
                double price = OandaAccountApi.GetPrice(InstrumentConversion.SierraChartAccountId, instrumentConversion.OandaInstrumentName, true);
                oaMarketOrder.SL = Math.Round(price - instrumentConversion.SL);
                OandaAccountApi.ModifyMarketOrder(InstrumentConversion.SierraChartAccountId, oaMarketOrder);
            }
        }

        public static void Sell(InstrumentConversion instrumentConversion, int sierraPosition, int oandaPosition, OAOrders OAOrders)
        {
            int positionToSell = oandaPosition - sierraPosition;

            OAOrder oaMarketOrder = new OAOrder();
            oaMarketOrder.Instrument = instrumentConversion.OandaInstrumentName;
            oaMarketOrder.Units = -positionToSell;
            oaMarketOrder.Id = OandaAccountApi.CreateMarketOrder(InstrumentConversion.SierraChartAccountId, oaMarketOrder);

            if (oandaPosition == 0)
            {
                // nastav SL pri prvni objednavce
                double price = OandaAccountApi.GetPrice(InstrumentConversion.SierraChartAccountId, instrumentConversion.OandaInstrumentName, false);
                oaMarketOrder.SL = Math.Round(price + instrumentConversion.SL);
                OandaAccountApi.ModifyMarketOrder(InstrumentConversion.SierraChartAccountId, oaMarketOrder);
            }
        }

        public static void MakePosition(string sierraInstrumentName, int sierraPosition, List<SierraOrder> sierraOrders)
        {
            if (sierraInstrumentName.IndexOf("[Sim]", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                sierraInstrumentName = sierraInstrumentName.Substring("[Sim]".Length);
            }
            sierraInstrumentName = sierraInstrumentName.Substring(0, 2);

            InstrumentConversion instrumentConversion = InstrumentConversion.InstrumentConversions.Find(x => x.SierraInstrumentName == sierraInstrumentName);
                        
            OAOrders OAOrders = OandaAccountApi.GetMarketOrders(InstrumentConversion.SierraChartAccountId, instrumentConversion.OandaInstrumentName);

            int oandaPosition = (int)OAOrders.Sum(x => x.Units);

            if (sierraPosition == oandaPosition)
            {
                return;
            }
            if (sierraPosition > oandaPosition)
            {
                Buy(instrumentConversion, sierraPosition, oandaPosition, OAOrders);
            }
            if (sierraPosition < oandaPosition)
            {
                Sell(instrumentConversion, sierraPosition, oandaPosition, OAOrders);
            }
        }
    }
*/
}
