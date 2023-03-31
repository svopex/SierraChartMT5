using MySql.Data.MySqlClient;
using OandaApiBusinessClass.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OandaApiBusinessClass.Facade
{
    public class SierraOrderFacade : BaseFacade<SierraOrderEntity>
    {
        public SierraOrderFacade() : base("SierraOrder") { }

        public static void DeleteAllS(string sierraInstrumentName)
        {
            SierraOrderFacade sierraChartOrderFacade = new SierraOrderFacade();
            sierraChartOrderFacade.DeleteAll(sierraInstrumentName);
        }

        private void DeleteAll(string sierraInstrumentName)
        {
            OpenConnection();
            MySqlCommand cmd = GetMySqlCommand("DELETE FROM " + this.tableName + " WHERE SierraInstrumentName = @SierraInstrumentName");
            cmd.Parameters.Add(new MySqlParameter("@SierraInstrumentName", sierraInstrumentName));
            cmd.ExecuteNonQuery();
            CloseConnection();
        }
    }
}
