using SierraChartMT5Library;

namespace SierraChartMT4
{
    class Program
    {
        static void Main(string[] args)
        {
            SvpMT5.Instance.Test();
            long orderId = SvpMT5.Instance.CreateMarketOrder("[NQ100]", 1);            
            OAOrders oaOrders = SvpMT5.Instance.GetMarketOrders("[NQ100]");
            double openPrice = SvpMT5.Instance.GetMarketOrderPrice(oaOrders[0].Id);
            oaOrders[0].PT = openPrice + 10;
            oaOrders[0].SL = openPrice - 10;
            SvpMT5.Instance.ModifyMarketOrder(oaOrders[0]);
            SvpMT5.Instance.CloseOrder(orderId);
        }
    }
}
