using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using BroadcastTest.Model;

namespace BroadcastTest.Helpers
{
    class XmlFileHelper
    {

        public static T DeserializeXMLFileToObject<T>(string XmlFilename)
        {
            T returnObject = default(T);
            if (string.IsNullOrEmpty(XmlFilename)) return default(T);

            try
            {
                StreamReader xmlStream = new StreamReader(XmlFilename);
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                returnObject = (T)serializer.Deserialize(xmlStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to deserialize XML File: " + XmlFilename + "Due to exception: " + ex);
            }
            return returnObject;
        }

        /*
        public RateXml DeserializeRateData(string xmlFile)
        {
            try
            {
                RateXml xml;
                // Construct an instance of the XmlSerializer with the type  
                // of object that is being deserialized.  
                XmlSerializer mySerializer = new XmlSerializer(typeof(RateDataXml));

                // To read the file, create a FileStream.  
                FileStream fileStream = new FileStream(xmlFile, FileMode.Open);

                // Call the Deserialize method and cast to the object type.  
                return (RateXml)mySerializer.Deserialize(fileStream);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to deserialize XML file into RateDataXML object. File: "
                    + xmlFile + "error message: " + e);
            }

        }
         * */


    }
}
