/*
public static void Buy(InstrumentConversion instrumentConversion, int sierraPosition, int oandaPosition, OAOrders OAOrders)
{
    int positionToBuy = sierraPosition - oandaPosition;

    OAOrder oaMarketOrder = new OAOrder();
    oaMarketOrder.Instrument = instrumentConversion.OandaInstrumentName;
    oaMarketOrder.Units = positionToBuy;
    oaMarketOrder.Id = OandaAccountApi.CreateMarketOrder(SierraChartAccountId, oaMarketOrder);

    if (oandaPosition == 0)
    {
        double price = OandaAccountApi.GetPrice(SierraChartAccountId, instrumentConversion.OandaInstrumentName, true);
        oaMarketOrder.SL = Math.Round(price - instrumentConversion.SL);
        OandaAccountApi.ModifyMarketOrder(SierraChartAccountId, oaMarketOrder);
    }
                if ((OAOrders.Count > 0) && (OAOrders[0].Units < 0))
                {
                    // Jsem v prodeji na oande, proto nejdrive uzaviram zaporne pozice
                    foreach (OAOrder oaOrder in OAOrders)
                    {
                        if (oandaPosition >= sierraPosition)
                        {
                            break;
                        }
                        OandaAccountApi.CloseOrder(SierraChartAccountId, oaOrder.Id);
                        oandaPosition++;
                    }
                }

                while (true)
                {
                    if (oandaPosition >= sierraPosition)
                    {
                        break;
                    }
                    OAOrder oaMarketOrder = new OAOrder();
                    oaMarketOrder.Instrument = instrumentConversion.OandaInstrumentName;
                    oaMarketOrder.Units = 1;
                    oaMarketOrder.SL = Math.Round(price - instrumentConversion.SL);
                    OandaAccountApi.CreateMarketOrder(SierraChartAccountId, oaMarketOrder);
                    oandaPosition++;
                }
    double price = OandaAccountApi.GetPrice(SierraChartAccountId, instrumentConversion.OandaInstrumentName, true);

    if ((OAOrders.Count > 0) && (OAOrders[0].Units < 0))
    {
        // Jsem v prodeji na oande, proto nejdrive uzaviram zaporne pozice
        foreach (OAOrder oaOrder in OAOrders)
        {
            if (oandaPosition >= sierraPosition)
            {
                break;
            }
            OandaAccountApi.CloseOrder(SierraChartAccountId, oaOrder.Id);
            oandaPosition++;
        }
    }

    while (true)
    {
        if (oandaPosition >= sierraPosition)
        {
            break;
        }
        OAOrder oaMarketOrder = new OAOrder();
        oaMarketOrder.Price = price + instrumentConversion.SL * 2;
        oaMarketOrder.Instrument = instrumentConversion.OandaInstrumentName;
        oaMarketOrder.Units = 1;
        oaMarketOrder.SL = Math.Round(price - instrumentConversion.SL);
        OandaAccountApi.CreateStopOrder(SierraChartAccountId, oaMarketOrder);
        oandaPosition++;
    }
 
}

public static void Sell(InstrumentConversion instrumentConversion, int sierraPosition, int oandaPosition, OAOrders OAOrders)
{
    int positionToSell = oandaPosition - sierraPosition;

    OAOrder oaMarketOrder = new OAOrder();
    oaMarketOrder.Instrument = instrumentConversion.OandaInstrumentName;
    oaMarketOrder.Units = -positionToSell;
    oaMarketOrder.Id = OandaAccountApi.CreateMarketOrder(SierraChartAccountId, oaMarketOrder);

    if (oandaPosition == 0)
    {
        double price = OandaAccountApi.GetPrice(SierraChartAccountId, instrumentConversion.OandaInstrumentName, false);
        oaMarketOrder.SL = Math.Round(price + instrumentConversion.SL);
        OandaAccountApi.ModifyMarketOrder(SierraChartAccountId, oaMarketOrder);
    }

    double price = OandaAccountApi.GetPrice(SierraChartAccountId, instrumentConversion.OandaInstrumentName, false);

    if ((OAOrders.Count > 0) && (OAOrders[0].Units > 0))
    {
        // Jsem v nakupu na oande, proto nejdrive uzaviram zaporne pozice
        foreach (OAOrder oaOrder in OAOrders)
        {
            if (oandaPosition <= sierraPosition)
            {
                break;
            }
            OandaAccountApi.CloseOrder(SierraChartAccountId, oaOrder.Id);
            oandaPosition--;
        }
    }

    while (true)
    {
        if (oandaPosition <= sierraPosition)
        {
            break;
        }
        OAOrder oaMarketOrder = new OAOrder();
        oaMarketOrder.Instrument = instrumentConversion.OandaInstrumentName;
        oaMarketOrder.Units = -1;
        oaMarketOrder.SL = Math.Round(price + instrumentConversion.SL);
        OandaAccountApi.CreateMarketOrder(SierraChartAccountId, oaMarketOrder);
        oandaPosition--;
    }
    double price = OandaAccountApi.GetPrice(SierraChartAccountId, instrumentConversion.OandaInstrumentName, false);

    if ((OAOrders.Count > 0) && (OAOrders[0].Units > 0))
    {
        // Jsem v nakupu na oande, proto nejdrive uzaviram zaporne pozice
        foreach (OAOrder oaOrder in OAOrders)
        {
            if (oandaPosition <= sierraPosition)
            {
                break;
            }
            OandaAccountApi.CloseOrder(SierraChartAccountId, oaOrder.Id);
            oandaPosition--;
        }
    }

    while (true)
    {
        if (oandaPosition <= sierraPosition)
        {
            break;
        }
        OAOrder oaMarketOrder = new OAOrder();
        oaMarketOrder.Price = price - instrumentConversion.SL * 2;
        oaMarketOrder.Instrument = instrumentConversion.OandaInstrumentName;
        oaMarketOrder.Units = -1;
        oaMarketOrder.SL = Math.Round(price + instrumentConversion.SL);
        OandaAccountApi.CreateStopOrder(SierraChartAccountId, oaMarketOrder);
        oandaPosition--;
    }
 
}
*/
