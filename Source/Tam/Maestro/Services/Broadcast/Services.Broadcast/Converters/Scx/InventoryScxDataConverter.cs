using Common.Services;
using Common.Services.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Entities.spotcableXML;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Services.Broadcast.Converters.Scx
{
    public interface IInventoryScxDataConverter : IApplicationService
    {
        /// <summary>
        /// Converts a list of ScxData objects into a list of InventoryScxFile objects
        /// </summary>
        /// <param name="data">List of ScxData objects to convert</param>
        /// <returns>List of InventoryScxFile objects</returns>
        List<InventoryScxFile> ConvertInventoryData(List<ScxData> data);

        /// <summary>
        /// Creates an adx object from the ScxData object
        /// </summary>
        /// <param name="data">ScxData object to be converted</param>
        /// <returns>adx object</returns>
        adx CreateAdxObject(ScxData data);
        /// <summary>
        /// Converts a list of open market ScxData objects into a list of open maarket InventoryScxFile objects
        /// </summary>
        /// <param name="data">List of ScxData objects to convert</param>
        /// <returns>List of open market InventoryScxFile objects</returns>
        List<OpenMarketInventoryScxFile> ConvertOpenMrketInventoryData(List<OpenMarketScxData> data);
        /// <summary>
        /// Creates an adx object from the open market ScxData object
        /// </summary>
        /// <param name="data">ScxData object to be converted</param>
        /// <returns>adx object</returns>
        adx CreateAdxObjectForOpenMarket(OpenMarketScxData data);
    }

    public class InventoryScxDataConverter : ScxBaseConverter,IInventoryScxDataConverter
    {
        public InventoryScxDataConverter(IDaypartCache daypartCache, IDateTimeEngine dateTimeEngine) 
            : base(daypartCache, dateTimeEngine)
        {
        }

        /// <inheritdoc />
        public adx CreateAdxObject(ScxData data)
        {
            return CreateAdxObject(data, filterUnallocated: true);
        }

        /// <inheritdoc />
        public List<InventoryScxFile> ConvertInventoryData(List<ScxData> data)
        {
            List<InventoryScxFile> scxFiles = new List<InventoryScxFile>();

            foreach (var scxDataObject in data)
            {
                adx a = CreateAdxObject(scxDataObject);

                if (a == null)
                    return new List<InventoryScxFile>();

                string xml = a.Serialize();
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
                scxFiles.Add(new InventoryScxFile
                {
                    ScxStream = stream,
                    InventorySource = scxDataObject.InventorySource,
                    DaypartCodeId = scxDataObject.DaypartCodeId,
                    UnitName = scxDataObject.UnitName,
                    StartDate = scxDataObject.StartDate,
                    EndDate = scxDataObject.EndDate
                });
            }

            return scxFiles;
        }

        /// <inheritdoc />
        public List<OpenMarketInventoryScxFile> ConvertOpenMrketInventoryData(List<OpenMarketScxData> data)
        {
            List<OpenMarketInventoryScxFile> scxFiles = new List<OpenMarketInventoryScxFile>();

            foreach (var scxDataObject in data)
            {
                adx a = CreateAdxObjectForOpenMarket(scxDataObject);

                if (a == null)
                    return new List<OpenMarketInventoryScxFile>();

                string xml = a.Serialize();
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
                scxFiles.Add(new OpenMarketInventoryScxFile
                {
                    ScxStream = stream,
                    InventorySource = scxDataObject.InventorySource,
                    DaypartIds = scxDataObject.DaypartIds,
                    StartDate = scxDataObject.StartDate,
                    EndDate = scxDataObject.EndDate,
                    MarketRank = scxDataObject.MarketRank,
                    Affiliate = scxDataObject.Affiliate
                });
            }

            return scxFiles;
        }
        /// <inheritdoc />
        public adx CreateAdxObjectForOpenMarket(OpenMarketScxData data)
        {
            return CreateAdxObjectForOpenMarket(data, filterUnallocated: true);
        }
    }
}
