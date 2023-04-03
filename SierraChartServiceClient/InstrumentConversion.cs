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
			new InstrumentConversion() { SierraInstrumentName = "XAUUSD", OandaInstrumentName = "XAUUSD", SL = 5, Compensation = 0.01 },
            new InstrumentConversion() { SierraInstrumentName = "NQ", OandaInstrumentName = "USTEC", SL = 5, Compensation = 0.6 },
            new InstrumentConversion() { SierraInstrumentName = "ES", OandaInstrumentName = "US500", SL = 2.5, Compensation = 0.2},
            new InstrumentConversion() { SierraInstrumentName = "YM", OandaInstrumentName = "US30", SL = 20, Compensation = 1 }
			//new InstrumentConversion() { SierraInstrumentName = "NQ", OandaInstrumentName = "[NQ100]", SL = 5, Compensation = 0.8 },
            //new InstrumentConversion() { SierraInstrumentName = "ES", OandaInstrumentName = "[SP500]", SL = 2.5, Compensation = 0.4},
            //new InstrumentConversion() { SierraInstrumentName = "YM", OandaInstrumentName = "[DJI30]", SL = 20, Compensation = 1 }
        };
    }
}
