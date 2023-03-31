using System.Collections.Generic;

namespace SierraChartMT5Library
{
    public class OAOrder
    {
        public long Id { get; set; }
        public double Price { get; set; }
        public double SL { get; set; }
        public double PT { get; set; }
        public string Instrument { get; set; }
        public long Units { get; set; }
        public long Magic { get; set; }
        public string Comment { get; set; }
    }

    public class OAOrders : List<OAOrder>
    {
    }
}
