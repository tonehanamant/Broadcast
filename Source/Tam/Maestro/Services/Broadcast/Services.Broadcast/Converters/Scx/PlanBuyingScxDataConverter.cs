﻿using Common.Services;
using Common.Services.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Scx;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Services.Broadcast.Converters.Scx
{
    public interface IPlanBuyingScxDataConverter : IApplicationService
    {
        PlanBuyingScxFile ConvertData(PlanScxData data);
    }

    public class PlanBuyingScxDataConverter : ScxBaseConverter, IPlanBuyingScxDataConverter
    {
        public PlanBuyingScxDataConverter(IDaypartCache daypartCache, IDateTimeEngine dateTimeEngine)
            : base(daypartCache, dateTimeEngine)
        {
        }

        public PlanBuyingScxFile ConvertData(PlanScxData data)
        {
            // keep the unallocated markets.
            var adxObject = CreateAdxObject(data, filterUnallocated: false);

            var settings = new XmlWriterSettings() { Indent = true };
            var serializer = new XmlSerializer(adxObject.GetType());

            var stream = new MemoryStream();
            var writer = XmlWriter.Create(stream, settings);
            serializer.Serialize(writer, adxObject);
            writer.Flush();
            stream.Position = 0;

            var file = new PlanBuyingScxFile
            {
                PlanName = data.PlanName,
                GeneratedTimeStamp = data.Generated,
                ScxStream = stream
            };

            return file;
        }
    }
}