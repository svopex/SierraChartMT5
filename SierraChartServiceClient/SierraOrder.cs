using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SierraChartServiceClient
{
    public class SierraOrder
    {
        public int InternalOrderID;
        public string Symbol;
        public string OrderType;
        public int OrderQuantity;
        public int BuySell;
        public double Price1;
        public int OrderStatusCode;
        public int ParentInternalOrderID;
        public int OrderTypeAsInt;
        public double AvgFillPrice;

        /// <summary>
        /// Neni to PT/SL, je to objednavka
        /// </summary>
        public bool IsOrder
        {
            get
            {
                return ParentInternalOrderID == 0;
            }
        }
    }
}
