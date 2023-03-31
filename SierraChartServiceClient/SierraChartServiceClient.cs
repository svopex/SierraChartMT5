using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OandaApiBusinessClass.Facade;
using OandaApiBusinessClass.Entity;
using OandaApiBusinessClass;
using SierraChartMT5Library;

namespace SierraChartServiceClient
{
    public class ServeSierraChart
    {
        private const int SCT_ORDERTYPE_LIMIT = 1;
        private const int SCT_ORDERTYPE_STOP = 2;
        private const int BSE_BUY = 1;
        private const int BSE_SELL = 2;

        public class PairedOrder
        {
            public SierraOrder sierraOrder;
            public SierraOrder sierraOrderSL;
            public SierraOrder sierraOrderPT;
            public OAOrder oandaOrder;
        }
        
        private static List<PairedOrder> DoPairOrders(InstrumentConversion instrumentConversion, OAOrders OAOrders, List<SierraOrder> sierraOrders)
        {
            List<PairedOrder> pairedOrders = new List<PairedOrder>();

            SierraOrderFacade sierraChartOrderFacade = new SierraOrderFacade();
            List<SierraOrderEntity> sierraChartOrderEntities = sierraChartOrderFacade.LoadMultipleByColumn(
                SierraOrderFacade.GetPropertyName<SierraOrderEntity>(x => x.SierraInstrumentName), instrumentConversion.SierraInstrumentName);
            foreach(SierraOrderEntity sierraChartOrderEntity in sierraChartOrderEntities)
            {
                PairedOrder pairedOrder = new PairedOrder();
                pairedOrder.sierraOrder = sierraOrders.Find(x => x.InternalOrderID == sierraChartOrderEntity.SierraOrderId);
                pairedOrder.oandaOrder = OAOrders.Find(x => x.Id == sierraChartOrderEntity.OandaOrderId);
                if (pairedOrder.sierraOrder != null)
                {
                    pairedOrder.sierraOrderSL = sierraOrders.Find(x => x.ParentInternalOrderID == pairedOrder.sierraOrder.InternalOrderID && x.OrderTypeAsInt == SCT_ORDERTYPE_STOP);
                    pairedOrder.sierraOrderPT = sierraOrders.Find(x => x.ParentInternalOrderID == pairedOrder.sierraOrder.InternalOrderID && x.OrderTypeAsInt == SCT_ORDERTYPE_LIMIT);
                }
                if (pairedOrder.oandaOrder != null && pairedOrder.sierraOrder != null && pairedOrder.sierraOrderSL != null)
                {
                    pairedOrders.Add(pairedOrder);
                }
            }

            return pairedOrders;
        }

        private static void CloseUnpairedOandaOrders(OAOrders oaOrders, List<PairedOrder> pairedOrders/*, int sierraPosition*/)
        {
            List<Task> tasks = new List<Task>();
            foreach (OAOrder oaOrder in oaOrders)
            {
                if ((pairedOrders.Find(x => x.oandaOrder.Id == oaOrder.Id) == null))
                {
                    Task task = Task.Run(() => SvpMT5.Instance.CloseOrder(oaOrder.Id));
                    tasks.Add(task);
                }
            }
            Task.WaitAll(tasks.ToArray());
        }

        private static void ModifyOandaOrder(InstrumentConversion instrumentConversion, long marketOrderId, SierraOrder sierraOrder, double sierraOrderSLPrice, double sierraOrderPTPrice)
        {
            try
            {
                if (sierraOrder == null)
                {
                    Log.WriteMessage("ModifyOandaOrder(), sierraOrder == null");
                }
                SierraOrderFacade sierraChartOrderFacade = new SierraOrderFacade();
                SierraOrderEntity sierraOrderEntity = sierraChartOrderFacade.LoadByColumn(SierraOrderFacade.GetPropertyName<SierraOrderEntity>(x => x.SierraOrderId), sierraOrder.InternalOrderID);
                if (sierraOrderEntity == null)
                {
                    Log.WriteMessage("ModifyOandaOrder(), sierraOrderEntity == null");
                }

                if (sierraOrderEntity.SierraPT != sierraOrderPTPrice || sierraOrderEntity.SierraSL != sierraOrderSLPrice)
                {
                    if (sierraOrderEntity.OandaPrice == 0)
                    {
                        //sierraOrderEntity.OandaPrice = await SvpMT4.Instance.GetActualPrice(InstrumentConversion.SierraChartAccountId, instrumentConversion.OandaInstrumentName);
                        sierraOrderEntity.OandaPrice = SvpMT5.Instance.GetMarketOrderPrice(marketOrderId);
                    }

                    double compensation = (sierraOrder.BuySell == BSE_BUY ? -instrumentConversion.Compensation : instrumentConversion.Compensation);

                    OAOrder newOAMarketOrder = new OAOrder();
                    newOAMarketOrder.Id = sierraOrderEntity.OandaOrderId;
                    newOAMarketOrder.Instrument = instrumentConversion.OandaInstrumentName;
                    double sl;
                    if (sierraOrderSLPrice != 0)
                    {
                        sl = sierraOrder.AvgFillPrice - sierraOrderSLPrice;
                    }
                    else
                    {
                        sl = instrumentConversion.SL;
                    }
                    newOAMarketOrder.SL = sierraOrderEntity.OandaPrice - /*(sierraOrder.BuySell == BSE_BUY ? sl : -sl)*/ sl + compensation;

                    double pt;
                    if (sierraOrderPTPrice != 0)
                    {
                        pt = sierraOrderPTPrice - sierraOrder.AvgFillPrice;
                        newOAMarketOrder.PT = sierraOrderEntity.OandaPrice + /*(sierraOrder.BuySell == BSE_BUY ? pt : -pt)*/ pt + compensation;
                    }

                    SvpMT5.Instance.ModifyMarketOrder(newOAMarketOrder);

                    sierraOrderEntity.SierraPT = sierraOrderPTPrice;
                    sierraOrderEntity.SierraSL = sierraOrderSLPrice;
                    sierraChartOrderFacade.Save(sierraOrderEntity);
                }
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
                throw;
            }
        }

        private static void ModifyPairedSierraOrders(InstrumentConversion instrumentConversion, List<PairedOrder> pairedOrders)
        {
            List<Task> tasks = new List<Task>();
            foreach (PairedOrder pairedOrder in pairedOrders)
            {
                if (pairedOrder == null)
                {
                    throw new Exception("ModifyPairedSierraOrders(), pairedOrder == null");
                }
                if (pairedOrder.oandaOrder == null)
                {
                    throw new Exception("ModifyPairedSierraOrders(), pairedOrder.oandaOrder == null");
                }
                if (pairedOrder.sierraOrder == null)
                {
                    throw new Exception("ModifyPairedSierraOrders(), pairedOrder.sierraOrder == null");
                }
                Task task = Task.Run(() =>
                {
                    ModifyOandaOrder(instrumentConversion, pairedOrder.oandaOrder.Id, pairedOrder.sierraOrder,
                        pairedOrder.sierraOrderSL == null ? 0 : pairedOrder.sierraOrderSL.Price1,
                        pairedOrder.sierraOrderPT == null ? 0 : pairedOrder.sierraOrderPT.Price1);
                });
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
        }

        private static void MarketUnpairedSierraOrder(InstrumentConversion instrumentConversion, SierraOrder sierraOrder, OAOrder oaOrder)
        {
			SierraOrderFacade sierraChartOrderFacade = new SierraOrderFacade();

            OAOrder oaMarketOrder = new OAOrder();
            oaMarketOrder.Instrument = instrumentConversion.OandaInstrumentName;
            oaMarketOrder.Units = sierraOrder.BuySell == BSE_BUY ? sierraOrder.OrderQuantity : -sierraOrder.OrderQuantity;
            oaMarketOrder.Id = SvpMT5.Instance.CreateMarketOrder(oaMarketOrder.Instrument, oaMarketOrder.Units);			

			SierraOrderEntity sierraChartOrderEntity = new SierraOrderEntity();
            sierraChartOrderEntity.SierraOrderId = sierraOrder.InternalOrderID;
            sierraChartOrderEntity.OandaOrderId = oaMarketOrder.Id;
            sierraChartOrderEntity.SierraInstrumentName = instrumentConversion.SierraInstrumentName;
            sierraChartOrderEntity.SierraPrice = sierraOrder.AvgFillPrice;
            sierraChartOrderFacade.Save(sierraChartOrderEntity);

            oaOrder.Id = oaMarketOrder.Id;
            oaOrder.Units = oaMarketOrder.Units;
            oaOrder.Instrument = instrumentConversion.OandaInstrumentName;
        }

        private static void MarketUnpairedSierraOrders(InstrumentConversion instrumentConversion, List<SierraOrder> sierraOrders, List<PairedOrder> pairedOrders, OAOrders oaOrders)
        {
            SierraOrderFacade sierraChartOrderFacade = new SierraOrderFacade();
            
            List<Task> tasks = new List<Task>();
            foreach (SierraOrder sierraOrder in sierraOrders)
            {
                SierraOrderEntity sierraOrderEntity = sierraChartOrderFacade.LoadByColumn(SierraOrderFacade.GetPropertyName<SierraOrderEntity>(x => x.SierraOrderId), sierraOrder.InternalOrderID);

                if (sierraOrderEntity == null // objednavka nebyla nikdy pouzita
                    && sierraOrder.IsOrder // Neni to SL ani PT
                    && pairedOrders.Find(x => x.sierraOrder.InternalOrderID == sierraOrder.InternalOrderID) == null) // neni to sparovana objednavka
                {
                    OAOrder oaOrder = new OAOrder();
                    oaOrders.Add(oaOrder);

                    Task task = Task.Run(() =>
                    {
                        MarketUnpairedSierraOrder(instrumentConversion, sierraOrder, oaOrder);
                    });
                    tasks.Add(task);
                }
            }
            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// V SierraChart je otevrena kladna a zaroven zaporna pozice?
        /// </summary>
        public static bool SierraChartWrongPosition(int sierraPosition, List<SierraOrder> sierraOrders)
        {
            bool buy = false;
            bool sell = false;
            foreach(SierraOrder sierraOrder in sierraOrders)
            {
                if (sierraOrder.IsOrder)
                {
                    if (sierraOrder.BuySell == BSE_BUY)
                    {
                        buy = true;
                    }
                    if (sierraOrder.BuySell == BSE_SELL)
                    {
                        sell = true;
                    }
                }
            }
            return (buy && sell);
        }

        public static bool runned = false;
        //public static int counter = 0;

        /// <summary>
        /// Lze zrusit PT a nasledne lze uzavrit pozici uzavrenim SL.
        /// Vhodne napriklad pri rychlem pohybu.
        /// </summary>
        public static void MakePosition(string sierraInstrumentName, int sierraPosition, List<SierraOrder> sierraOrders)
        {
            try
            {
				Log.WriteMessage($"MakePosition(\"{sierraInstrumentName}\", {sierraPosition})");

				if (runned)
                {
                    return;
                }

                //if (counter++ < 2)
                //{
                //    return;
                //}
                //counter = 0;

                runned = true;

                if (string.IsNullOrWhiteSpace(sierraInstrumentName))
                {
                    Log.WriteMessage("sierraInstrumentNamed = " + sierraInstrumentName);
                    return;
                }

                if (SierraChartWrongPosition(sierraPosition, sierraOrders))
                {
                    Log.WriteMessage("SierraChartWrongPosition");
                    return;
                }

                if (sierraInstrumentName.IndexOf("[Sim]", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    sierraInstrumentName = sierraInstrumentName.Substring("[Sim]".Length);
                }
                //sierraInstrumentName = sierraInstrumentName.Substring(0, 2);

                InstrumentConversion instrumentConversion = InstrumentConversion.InstrumentConversions.Find(x => x.SierraInstrumentName == sierraInstrumentName);
                if (instrumentConversion == null)
                {
                    Log.WriteMessage("sierraInstrumentName" + sierraInstrumentName);
                    return;
                }

                OAOrders oaOrders = SvpMT5.Instance.GetMarketOrders(instrumentConversion.OandaInstrumentName);

                List<PairedOrder> pairedOrders = DoPairOrders(instrumentConversion, oaOrders, sierraOrders);

                CloseUnpairedOandaOrders(oaOrders, pairedOrders);

                MarketUnpairedSierraOrders(instrumentConversion, sierraOrders, pairedOrders, oaOrders);

                pairedOrders = DoPairOrders(instrumentConversion, oaOrders, sierraOrders);

                ModifyPairedSierraOrders(instrumentConversion, pairedOrders);

                if (sierraPosition == 0 && (int)oaOrders.Sum(x => x.Units) == 0)
                {
                    SierraOrderFacade.DeleteAllS(instrumentConversion.SierraInstrumentName);
                }
            }
            catch (AggregateException aggregateException)
            {
                foreach (Exception ex in aggregateException.InnerExceptions)
                {
                    Log.WriteException(ex);
                }
                throw aggregateException;
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
                throw ex;
            }
            finally
            {
                runned = false;
            }
        }
    }
}
