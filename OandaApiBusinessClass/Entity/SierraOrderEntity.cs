using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OandaApiBusinessClass.Entity
{
    /*
    SierraChart.exe.config:

    <?xml version="1.0" encoding="utf-8" ?>
    <configuration>
      <connectionStrings>
        <add name="SQLConnectionString" connectionString="Server=127.0.0.1;Database=oandaapi;Uid=root;Pwd=MySqlRoot;"/>
      </connectionStrings>
    </configuration> 

    CREATE TABLE `SierraOrder` (
        `Id` INT NOT NULL AUTO_INCREMENT,
        `SierraOrderId` INT NOT NULL,
        `OandaOrderId` BIGINT NULL DEFAULT NULL,
        `SierraInstrumentName` VARCHAR(80) NOT NULL,
        `SierraPrice` DOUBLE NOT NULL,
        `SierraSL` DOUBLE NOT NULL,
        `SierraPT` DOUBLE NOT NULL,
        `OandaPrice` DOUBLE NOT NULL,
        PRIMARY KEY(`Id`),
        UNIQUE INDEX `SierraOrder_SierraOrderId` (`SierraOrderId`) USING BTREE,
        UNIQUE INDEX `SierraOrder_OandaOrderId` (`OandaOrderId`) USING BTREE,
        INDEX `SierraOrder_InstrumentName` (`SierraInstrumentName`) USING BTREE
    )
    */
    public class SierraOrderEntity : BaseEntity
    {
        [BaseAttribute(UsedInSql = true)]
        public int SierraOrderId { get; set; }
        [BaseAttribute(UsedInSql = true)]
        public long OandaOrderId { get; set; }
        [BaseAttribute(UsedInSql = true)]
        public string SierraInstrumentName { get; set; }
        [BaseAttribute(UsedInSql = true)]
        public double SierraPrice { get; set; }
        [BaseAttribute(UsedInSql = true)]
        public double SierraSL { get; set; }
        [BaseAttribute(UsedInSql = true)]
        public double SierraPT { get; set; }
        [BaseAttribute(UsedInSql = true)]
        public double OandaPrice { get; set; }
    }

    public class SierraOrderEntities : List<SierraOrderEntity>
    {
        public SierraOrderEntities(List<SierraOrderEntity> sierraChartOrderEntities)
        {
            this.AddRange(sierraChartOrderEntities);
        }
    }
}
