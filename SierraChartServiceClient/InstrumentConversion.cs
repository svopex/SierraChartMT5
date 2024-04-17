using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SierraChartServiceClient
{
    public class InstrumentConversion
    {
        public string SierraInstrumentName { get; set; }
        public string OandaInstrumentName { get; set; }
        public double SL { get; set; }
        public double Compensation { get; set; }

        public static List<InstrumentConversion> InstrumentConversions = new List<InstrumentConversion>()
        {
            /*
              Vypocet - kompenzace = spread na instrumentu u brokera - kolik je skutecny spread na burze
            */
            // CapitalMarkets
			new InstrumentConversion() { SierraInstrumentName = "NQ", OandaInstrumentName = "NSDQ_raw", SL = 20, Compensation = 1.72 },
			new InstrumentConversion() { SierraInstrumentName = "ES", OandaInstrumentName = "SP_raw", SL = 5, Compensation = 0.57 },
			new InstrumentConversion() { SierraInstrumentName = "YM", OandaInstrumentName = "DOW_raw", SL = 40, Compensation = 1.96 }
            // FTMO - urceno vypoctem
			//new InstrumentConversion() { SierraInstrumentName = "NQ", OandaInstrumentName = "US100.cash", SL = 5, Compensation = 0.45 },
			//new InstrumentConversion() { SierraInstrumentName = "ES", OandaInstrumentName = "US500.cash", SL = 2.5, Compensation = 0.15 },
			//new InstrumentConversion() { SierraInstrumentName = "YM", OandaInstrumentName = "US30.cash", SL = 20, Compensation = 0.4 }
            // BillionsClub - urceno vypoctem
			//new InstrumentConversion() { SierraInstrumentName = "NQ", OandaInstrumentName = "NSDQ_raw", SL = 5, Compensation = 0.65 },
			//new InstrumentConversion() { SierraInstrumentName = "ES", OandaInstrumentName = "SP_raw", SL = 2.5, Compensation = 0.25 },
			//new InstrumentConversion() { SierraInstrumentName = "YM", OandaInstrumentName = "DOW_raw", SL = 20, Compensation = 0.9 }
            // Ic markets - urceno pouze okometricky
			//new InstrumentConversion() { SierraInstrumentName = "XAUUSD", OandaInstrumentName = "XAUUSD", SL = 5, Compensation = 0.05 },
            //new InstrumentConversion() { SierraInstrumentName = "NQ", OandaInstrumentName = "USTEC", SL = 5, Compensation = 0.5 },
            //new InstrumentConversion() { SierraInstrumentName = "ES", OandaInstrumentName = "US500", SL = 2.5, Compensation = 0.1 },
            //new InstrumentConversion() { SierraInstrumentName = "YM", OandaInstrumentName = "US30", SL = 20, Compensation = 0.5 }
            // Oanda - urceno pouze okometricky
			//new InstrumentConversion() { SierraInstrumentName = "NQ", OandaInstrumentName = "[NQ100]", SL = 5, Compensation = 0.8 },
            //new InstrumentConversion() { SierraInstrumentName = "ES", OandaInstrumentName = "[SP500]", SL = 2.5, Compensation = 0.4},
            //new InstrumentConversion() { SierraInstrumentName = "YM", OandaInstrumentName = "[DJI30]", SL = 20, Compensation = 1 }
        };
    }
}
