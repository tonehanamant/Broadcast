﻿using Common.Services;
using Common.Services.ApplicationServices;
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
    }

    public class InventoryScxDataConverter : ScxBaseConverter, IInventoryScxDataConverter
    {

        public InventoryScxDataConverter(IInventoryScxDataPrep inventoryScxDataPrep
            , IDaypartCache daypartCach) : base(daypartCach)
        {
        }

        /// <summary>
        /// Converts a list of ScxData objects into a list of InventoryScxFile objects
        /// </summary>
        /// <param name="data">List of ScxData objects to convert</param>
        /// <returns>List of InventoryScxFile objects</returns>
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
                    InventorySourceName = scxDataObject.InventorySource.Name,
                    UnitName = scxDataObject.UnitName
                });
            }

            return scxFiles;
        }

    }
}