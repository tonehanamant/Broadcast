using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using Tam.Maestro.Services.ContractInterfaces;

namespace Tam.Maestro.Services.ServiceManager.Service
{
    public delegate void OnUriChangedDelegate(ServiceManagerServiceEventArgs p1);
    public delegate void OnResourceChangedDelegate(ServiceManagerResourceEventArgs p1);

    public class EnvironmentSettings : IDisposable
    {
        //Create a singleton for this class
        static volatile EnvironmentSettings _Singleton;
        static object _Sync = new object();

        //First Dimension is environment, second dimension is service
        private Dictionary<TAMEnvironment, Dictionary<TAMService, ServiceItem>> _ServiceItem;
        private Dictionary<TAMEnvironment, Dictionary<TAMService, ServiceItem>> _OldServiceItems;

        private Dictionary<TAMEnvironment, Dictionary<TAMResource, ResourceItem>> _ResourceItem;
        private Dictionary<TAMEnvironment, Dictionary<TAMResource, ResourceItem>> _OldResourceItem;

        private enum ServiceFieldType { PORT, NAME, HOST };

        private string _FileName = "";
        private Thread _FileWatchThread;
        private bool _IsFileWatchRunning = false;

        public event OnUriChangedDelegate UriChangedEvent;
        public event OnResourceChangedDelegate ResourceChangedEvent;

        public static EnvironmentSettings Handler
        {
            get
            {
                lock (_Sync)
                {
                    if (_Singleton == null)
                    {
                        _Singleton = new EnvironmentSettings();
                    }
                }
                return _Singleton;
            }
        }

        #region ServiceManagerContract Get Calls
        public string GetUri(TAMService pS, TAMEnvironment pE)
        {
            return "net.tcp://" + _ServiceItem[pE][pS].Host + ":" + _ServiceItem[pE][pS].Port + "/" + _ServiceItem[pE][pS].Name;
        }
        private string GetOldUri(TAMService pS, TAMEnvironment pE)
        {
            return "net.tcp://" + _OldServiceItems[pE][pS].Host + ":" + _OldServiceItems[pE][pS].Port + "/" + _OldServiceItems[pE][pS].Name;
        }
        public string GetResource(TAMResource pS, TAMEnvironment pE)
        {
            return this._ResourceItem[pE][pS].ResourceString;
        }
        public string GetOldResource(TAMResource pS, TAMEnvironment pE)
        {
            return _OldResourceItem[pE][pS].ResourceString;
        }
        #endregion

        public EnvironmentSettings()
        {
            _ServiceItem = new Dictionary<TAMEnvironment, Dictionary<TAMService, ServiceItem>>();
            _OldServiceItems = new Dictionary<TAMEnvironment, Dictionary<TAMService, ServiceItem>>();

            _ResourceItem = new Dictionary<TAMEnvironment, Dictionary<TAMResource, ResourceItem>>();
            _OldResourceItem = new Dictionary<TAMEnvironment, Dictionary<TAMResource, ResourceItem>>();

            //Loop through each one and instantiate it in memory
            foreach (TAMEnvironment lTamEnvironment in Enum.GetValues(typeof(TAMEnvironment)))
            {
                this._ServiceItem[lTamEnvironment] = new Dictionary<TAMService, ServiceItem>();
                this._OldServiceItems[lTamEnvironment] = new Dictionary<TAMService, ServiceItem>();
                this._ResourceItem[lTamEnvironment] = new Dictionary<TAMResource, ResourceItem>();
                this._OldResourceItem[lTamEnvironment] = new Dictionary<TAMResource, ResourceItem>();

                //Services
                foreach (TAMService lTamService in Enum.GetValues(typeof(TAMService)))
                {
                    this._ServiceItem[lTamEnvironment][lTamService] = new ServiceItem(0, "", "");
                    this._OldServiceItems[lTamEnvironment][lTamService] = new ServiceItem(0, "", "");
                }
                //Resources
                foreach (TAMResource lTamResource in Enum.GetValues(typeof(TAMResource)))
                {
                    this._ResourceItem[lTamEnvironment][lTamResource] = new ResourceItem("");
                    this._OldResourceItem[lTamEnvironment][lTamResource] = new ResourceItem("");
                }
            }

            this._FileName = ConfigurationManager.AppSettings["XMLFilePath"];
            this.LoadSettingsFromXML();

            this._FileWatchThread = new Thread(this.CheckXMLFile);
            this._FileWatchThread.Start();
        }

        private void CheckXMLFile()
        {
            try
            {

                this._IsFileWatchRunning = true;
                FileInfo f = new FileInfo(_FileName);
                DateTime lLastWrite = f.LastWriteTime;
                List<TAMService> lListService = new List<TAMService>();
                List<TAMResource> lListResource = new List<TAMResource>();


                while (_IsFileWatchRunning == true)
                {
                    f = new FileInfo(_FileName);
                    if (lLastWrite != f.LastWriteTime)
                    {
                        System.Diagnostics.Debug.WriteLine("CHANGE ON: " + f.LastWriteTime.ToString());

                        //Get a copy of the array.
                        foreach (TAMEnvironment lTamEnvironment in Enum.GetValues(typeof(TAMEnvironment)))
                        {
                            foreach (TAMService lTamService in Enum.GetValues(typeof(TAMService)))
                                this._ServiceItem[lTamEnvironment][lTamService].Copy(this._OldServiceItems[lTamEnvironment][lTamService]);

                            foreach (TAMResource lTamResource in Enum.GetValues(typeof(TAMResource)))
                                this._ResourceItem[lTamEnvironment][lTamResource].Copy(this._OldResourceItem[lTamEnvironment][lTamResource]);
                        }

                        //Get the list of changed services and resources per environment
                        for (int i = 0; i < Enum.GetNames(typeof(TAMEnvironment)).Length; i++)
                        {
                            lListService = new List<TAMService>();
                            lListResource = new List<TAMResource>();

                            //When we check for change, if we have it, we will also
                            //update our array of service item objects on the fly!!
                            GetChangedServices(lListService, (TAMEnvironment)i);
                            GetChangedResources(lListResource, (TAMEnvironment)i);

                            //We can raise the event immediately because we update our
                            //settings on the fly
                            for (int j = 0; j < lListService.Count; j++)
                            {
                                UriChangedEvent(new ServiceManagerServiceEventArgs(this.GetOldUri(lListService[j], (TAMEnvironment)i), this.GetUri(lListService[j], (TAMEnvironment)i), (TAMEnvironment)i, lListService[j]));
                            }

                            for (int j = 0; j < lListResource.Count; j++)
                            {
                                ResourceChangedEvent(new ServiceManagerResourceEventArgs(this.GetOldResource(lListResource[j], (TAMEnvironment)i), this.GetResource(lListResource[j], (TAMEnvironment)i), (TAMEnvironment)i, lListResource[j]));
                            }

                        }

                        lLastWrite = f.LastWriteTime;
                    }
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch (System.Exception ee)
            {
                System.Diagnostics.Debug.WriteLine("Problem detected in file check thread: " + ee.Message);
            }
        }

        private bool DidServiceXMLChange(XmlNode pNode, ServiceItem pItem)
        {
            ServiceFieldType lCurrentServiceType = GetServiceFieldType(pNode.Name);

            if (lCurrentServiceType == ServiceFieldType.HOST)
            {
                if (pItem.Host != pNode.InnerText)
                {
                    pItem.Host = pNode.InnerText;
                    return true;
                }
            }
            else if (lCurrentServiceType == ServiceFieldType.PORT)
            {
                if (pItem.Port != System.Convert.ToInt16(pNode.InnerText))
                {
                    pItem.Port = System.Convert.ToInt16(pNode.InnerText);
                    return true;
                }
            }
            else
            {
                if (pItem.Name != pNode.InnerText)
                {
                    pItem.Name = pNode.InnerText;
                    return true;
                }
            }

            return false;
        }

        public void GetChangedServices(List<TAMService> pList, TAMEnvironment p1)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(this._FileName);

                XmlNode newNode;

                TAMService lCurrentService = TAMService.AffidavitService;
                TAMEnvironment lCurrentEnvironment = p1;

                newNode = GetEnvironmentNode(p1, xmlDoc);
                newNode = newNode.ChildNodes[0]; //Services should always come first
                //Get List of services now
                XmlNodeList lList = newNode.ChildNodes;
                XmlNodeList lServiceKeys;

                //Loop through services and see if anything changed
                for (int j = 0; j < lList.Count; j++)
                {
                    lCurrentService = GetServiceValue(lList[j].Name);
                    lServiceKeys = lList[j].ChildNodes;

                    if ((!pList.Contains(lCurrentService)) && DidServiceXMLChange(lServiceKeys[0], this._ServiceItem[p1][lCurrentService]))
                        pList.Add(lCurrentService);
                    if ((!pList.Contains(lCurrentService)) && DidServiceXMLChange(lServiceKeys[1], _ServiceItem[p1][lCurrentService]))
                        pList.Add(lCurrentService);
                    if ((!pList.Contains(lCurrentService)) && DidServiceXMLChange(lServiceKeys[2], _ServiceItem[p1][lCurrentService]))
                        pList.Add(lCurrentService);
                }

                System.Console.WriteLine("The reading of changed environments was successful");
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Console.WriteLine(e.Message + ": There was an error loading the XML file");
            }
        }
        public void GetChangedResources(List<TAMResource> pList, TAMEnvironment p1)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(this._FileName);

                XmlNode newNode;

                //TAMService lCurrentService = TAMService.AffidavitComposerService;
                TAMResource lCurrentResource = TAMResource.MaestroConnectionString;
                TAMEnvironment lCurrentEnvironment = p1;

                newNode = GetEnvironmentNode(p1, xmlDoc);
                newNode = newNode.ChildNodes[1]; //Resources should always come second
                //Get List of services now
                XmlNodeList lList = newNode.ChildNodes;
                XmlNodeList lServiceKeys;

                for (int j = 0; j < lList.Count; j++)
                {
                    lCurrentResource = GetResourceValue(lList[j].Name);
                    lServiceKeys = lList[j].ChildNodes;
                    if ((!pList.Contains(lCurrentResource)) && _ResourceItem[p1][lCurrentResource].ResourceString != lServiceKeys[0].InnerText)
                    {
                        _ResourceItem[p1][lCurrentResource].ResourceString = lServiceKeys[0].InnerText;
                        pList.Add(lCurrentResource);
                    }
                }

                System.Console.WriteLine("The reading of changed environments was successful");
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Console.WriteLine(e.Message + ": There was an error loading the XML file");
            }
        }
        public Tam.Maestro.Data.DataAccess.DatabaseSelection ConvertTamResource(TAMResource pTamResource)
        {
            switch (pTamResource)
            {
                case (TAMResource.ExternalRatingsConnectionString):
                    return Data.DataAccess.DatabaseSelection.ExternalRatings;
                case (TAMResource.MaestroAnalysisConnectionString):
                    return Data.DataAccess.DatabaseSelection.MaestroAnalysis;
                case (TAMResource.MaestroConnectionString):
                    return Data.DataAccess.DatabaseSelection.Maestro;
                case (TAMResource.PostLogAnalysisConnectionString):
                    return Data.DataAccess.DatabaseSelection.PostLogAnalysis;
                case (TAMResource.PostLogConnectionString):
                    return Data.DataAccess.DatabaseSelection.Postlog;
                case (TAMResource.ProgramsConnectionString):
                    return Data.DataAccess.DatabaseSelection.Programs;
                case (TAMResource.RentrakConnectionString):
                    return Data.DataAccess.DatabaseSelection.Rentrak;
                case (TAMResource.CableTrackConnectionString):
                    return Data.DataAccess.DatabaseSelection.CableTrack;
                case (TAMResource.InventoryConnectionString):
                    return Data.DataAccess.DatabaseSelection.Inventory;
                case (TAMResource.NielsenCableConnectionString):
                    return Data.DataAccess.DatabaseSelection.NielsenCable;
                case (TAMResource.BroadcastForecastConnectionString):
                    return Data.DataAccess.DatabaseSelection.BroadcastForecast;
                default:
                    throw new Exception("TAMResource to DatabaseSelection not defined.");
            }
        }

        private ServiceFieldType GetServiceFieldType(string pValue)
        {
            if (pValue == "name")
                return ServiceFieldType.NAME;
            else if (pValue == "host")
                return ServiceFieldType.HOST;
            else //if (pValue == "port")
                return ServiceFieldType.PORT;
        }

        private TAMService GetServiceValue(string pValue)
        {
            try
            {
                if (pValue == "ARS")
                    return TAMService.AudienceAndRatingsService;
                else if (pValue == "ARSLoader")
                    return TAMService.AudienceAndRatingsLoaderService;
                else if (pValue == "ACS")
                    return TAMService.AffidavitService;
                else if (pValue == "PCS")
                    return TAMService.ProposalService;
                else if (pValue == "BOMS")
                    return TAMService.BusinessObjectsManagerService;
                else if (pValue == "STS2")
                    return TAMService.SystemTopographyService;
                else if (pValue == "MAS")
                    return TAMService.MaestroAdministrationService;
                else if (pValue == "MCS")
                    return TAMService.MaterialService;
                else if (pValue == "RCS")
                    return TAMService.RateCardService;
                else if (pValue == "REL")
                    return TAMService.ReleaseService;
                else if (pValue == "RS")
                    return TAMService.ReportingService;
                else if (pValue == "ICS")
                    return TAMService.InventoryService;
                else if (pValue == "DES")
                    return TAMService.DeliveryEstimationService;
                else if (pValue == "PST")
                    return TAMService.PostingService;
                else if (pValue == "BRS")
                    return TAMService.CmwTrafficService;
                else if (pValue == "TCS")
                    return TAMService.TrafficService;
                else if (pValue == "PLS")
                    return TAMService.PostLogService;
                else if (pValue == "FTP")
                    return TAMService.FTPService;
                else if (pValue == "ACCT")
                    return TAMService.AccountingService;
                else if (pValue == "BS")
                    return TAMService.BroadcastService;
                else if (pValue == "REL2")
                    return TAMService.ReleaseService2;
                else if (pValue == "SingletonTestService")
                    return TAMService.SingletonTestService;
                else if (pValue == "CUS")
                    return TAMService.CoverageUniverseService;
                else
                    return (TAMService)(-1);
            }
            catch
            {
                throw new Exception("Could not parse service name resource from XML file");
            }
        }
        private TAMResource GetResourceValue(string pValue)
        {
            pValue = pValue.ToLower();
            if (pValue == "externalrating")
                return TAMResource.ExternalRatingsConnectionString;
            else if (pValue == "maestro")
                return TAMResource.MaestroConnectionString;
            else if (pValue == "maestroanalysis")
                return TAMResource.MaestroAnalysisConnectionString;
            else if (pValue == "postloganalysis")
                return TAMResource.PostLogAnalysisConnectionString;
            else if (pValue == "postlogstaging")
                return TAMResource.PostLogConnectionString;
            else if (pValue == "programs")
                return TAMResource.ProgramsConnectionString;
            else if (pValue == "rentrak")
                return TAMResource.RentrakConnectionString;
            else if (pValue == "cabletrack")
                return TAMResource.CableTrackConnectionString;
            else if (pValue == "nielsencable")
                return TAMResource.NielsenCableConnectionString;
            else if (pValue == "inventory")
                return TAMResource.InventoryConnectionString;
            else if (pValue == "broadcast")
                return TAMResource.BroadcastConnectionString;
            else if (pValue == "broadcastforecast")
                return TAMResource.BroadcastForecastConnectionString;
            else
                return (TAMResource)(-1);
        }
        private XmlNode GetEnvironmentNode(TAMEnvironment p1, XmlDocument xmlDoc)
        {
            try
            {
                if (p1 == TAMEnvironment.DEV)
                    return xmlDoc.GetElementsByTagName("dev")[0];
                else if (p1 == TAMEnvironment.LOCAL)
                    return xmlDoc.GetElementsByTagName("local")[0];
                else if (p1 == TAMEnvironment.PROD)
                    return xmlDoc.GetElementsByTagName("prod")[0];
                else if (p1 == TAMEnvironment.UAT)
                    return xmlDoc.GetElementsByTagName("uat")[0];
                else
                    return xmlDoc.GetElementsByTagName("qa")[0];
            }
            catch (System.Exception ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message + ": Error parsing xml file and getting node for environment");
                return null;
            }
        }

        private void SetXMLValue(XmlNode p1, ServiceItem p2)
        {
            //Here, if we see a connection string, we are only storing it in the name field of the serviceItem object.
            //This is because we only need one object to store the connection and instead of creating a new object, we 
            //will simply store what we need in this field to avoid creating a new class.
            ServiceFieldType lTemp = GetServiceFieldType(p1.Name);

            if (lTemp == ServiceFieldType.HOST)
                p1.InnerText = p2.Host;
            else if (lTemp == ServiceFieldType.NAME)
                p1.InnerText = p2.Name;
            else if (lTemp == ServiceFieldType.PORT)
                p1.InnerText = p2.Port.ToString();
        }
        private void GetXMLValue(XmlNode p1, ServiceItem p2)
        {
            ServiceFieldType lCurrentServiceType = GetServiceFieldType(p1.Name);
            if (lCurrentServiceType == ServiceFieldType.HOST)
                p2.Host = p1.InnerText;
            else if (lCurrentServiceType == ServiceFieldType.PORT)
                p2.Port = System.Convert.ToInt16(p1.InnerText);
            else
                p2.Name = p1.InnerText;

        }

        private bool CheckForRootNames(string pName)
        {
            if (pName == "services" || pName == "resources" || pName == "environment")
                return true;
            else
                return false;
        }

        public bool ChangeServiceSettings(TAMEnvironment p1, ServiceItem[] p3)
        {
            try
            {
                //We need to open up the XML document

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(this._FileName);

                XmlNode newNode;

                newNode = GetEnvironmentNode(p1, xmlDoc);
                newNode = newNode.ChildNodes[0]; //Services should always come first

                //Get List of services now
                XmlNodeList lList = newNode.ChildNodes;
                XmlNodeList lServiceKeys;
                //When a change is sent, it is sent as an entire environment
                //We change all services in this environment only
                for (int i = 0; i < Enum.GetNames(typeof(TAMService)).Length; i++)
                {
                    //Now loop through the XMLList we have    
                    for (int j = 0; j < lList.Count; j++)
                    {
                        //This conditional checks to make sure we update the correct service values.
                        if (GetServiceValue(lList[j].Name) == (TAMService)i && !CheckForRootNames(lList[j].Name))
                        {
                            //TODO: Check to make sure service is reachable at these new settings first...
                            //But only check if any of the values are different from what we currently have.
                            //We do not want to trash other changes if the service has not really been implemented
                            //yet

                            //Make sure we are not on the connection string, if we are, we know it needs to be changed in one spot
                            //only, not the next three!!!
                            //If good, write the new values and update our array too!!
                            lServiceKeys = lList[j].ChildNodes;
                            SetXMLValue(lServiceKeys[0], p3[i]);
                            SetXMLValue(lServiceKeys[1], p3[i]);
                            SetXMLValue(lServiceKeys[2], p3[i]);

                            break;
                        }
                    }
                }

                try
                {
                    File.Delete(_FileName.Substring(0, _FileName.Length - 4) + ".bak");
                    File.Copy(_FileName, _FileName.Substring(0, _FileName.Length - 4) + ".bak");
                }
                catch (System.Exception e2)
                {
                    System.Diagnostics.Debug.WriteLine(e2.Message);
                }

                //Now that the file is backed up and deleted, save the new file with the same name

                XmlTextWriter tw = new XmlTextWriter(_FileName, Encoding.ASCII);

                try
                {
                    tw.Formatting = Formatting.Indented; //this preserves indentation
                    xmlDoc.Save(tw);
                }
                finally
                {
                    tw.Close();
                }

                return true;
            }
            catch (System.Exception ee)
            {
                throw new Exception(ee.Message);
            }

        }
        public bool ChangeResourceSettings(TAMEnvironment p1, ResourceItem[] p3)
        {
            try
            {
                //We need to open up the XML document

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(this._FileName);

                XmlNode newNode;

                newNode = GetEnvironmentNode(p1, xmlDoc);
                newNode = newNode.ChildNodes[1]; //Services should always come first

                //Get List of services now
                XmlNodeList lList = newNode.ChildNodes;
                XmlNodeList lResourceKeys;
                //When a change is sent, it is sent as an entire environment
                //We change all services in this environment only
                for (int i = 0; i < Enum.GetNames(typeof(TAMResource)).Length; i++)
                {
                    //Now loop through the XMLList we have    
                    for (int j = 0; j < lList.Count; j++)
                    {
                        //This conditional checks to make sure we update the correct service values.
                        if (GetResourceValue(lList[j].Name) == (TAMResource)i && !CheckForRootNames(lList[j].Name))
                        {
                            //TODO: Check to make sure service is reachable at these new settings first...
                            //But only check if any of the values are different from what we currently have.
                            //We do not want to trash other changes if the service has not really been implemented
                            //yet

                            //Make sure we are not on the connection string, if we are, we know it needs to be changed in one spot
                            //only, not the next three!!!
                            //If good, write the new values and update our array too!!
                            lResourceKeys = lList[j].ChildNodes;
                            lResourceKeys[0].InnerText = p3[i].ResourceString;

                            //We did not implement a copy in the class so just take all the fields
                            //separate here
                            break;
                        }
                    }
                }

                try
                {
                    File.Delete(_FileName.Substring(0, _FileName.Length - 4) + ".bak");
                    File.Copy(_FileName, _FileName.Substring(0, _FileName.Length - 4) + ".bak");
                }
                catch (System.Exception e2)
                {
                    System.Diagnostics.Debug.WriteLine(e2.Message);
                }

                //Now that the file is backed up and deleted, save the new file with the same name

                XmlTextWriter tw = new XmlTextWriter(_FileName, Encoding.ASCII);

                try
                {
                    tw.Formatting = Formatting.Indented; //this preserves indentation
                    xmlDoc.Save(tw);
                }
                finally
                {
                    tw.Close();
                }

                return true;
            }
            catch (System.Exception ee)
            {
                throw new Exception(ee.Message);
            }

        }

        public void LoadSettingsFromXML()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(this._FileName);

                XmlNode newNode;

                TAMService lCurrentService = TAMService.AffidavitService;
                TAMResource lCurrentResource = TAMResource.MaestroConnectionString;

                //Loop through each environment and load settings
                foreach (TAMEnvironment lTamEnvironment in Enum.GetValues(typeof(TAMEnvironment)))
                {
                    newNode = GetEnvironmentNode(lTamEnvironment, xmlDoc);
                    newNode = newNode.ChildNodes[0]; //Services should always come first
                    //Get List of services now
                    XmlNodeList lList = newNode.ChildNodes;
                    XmlNodeList lServiceKeys;

                    //Loop through services and store values from XML
                    for (int j = 0; j < lList.Count; j++)
                    {
                        lCurrentService = GetServiceValue(lList[j].Name);

                        if ((int)lCurrentService == -1)
                        {
                            Console.WriteLine("Error: Skipping Node " + lList[j].Name + ", Type Does Not Exist");
                            continue;
                        }

                        lServiceKeys = lList[j].ChildNodes;

                        GetXMLValue(lServiceKeys[0], _ServiceItem[lTamEnvironment][lCurrentService]);
                        GetXMLValue(lServiceKeys[1], _ServiceItem[lTamEnvironment][lCurrentService]);
                        GetXMLValue(lServiceKeys[2], _ServiceItem[lTamEnvironment][lCurrentService]);
                    }

                    //NOW Loop through resources and store those values
                    newNode = GetEnvironmentNode(lTamEnvironment, xmlDoc);
                    newNode = newNode.ChildNodes[1]; //Resources should always come second
                    //Get List of resources now
                    lList = newNode.ChildNodes;
                    //Loop through services and store values from XML
                    for (int j = 0; j < lList.Count; j++)
                    {
                        lCurrentResource = GetResourceValue(lList[j].Name);
                        lServiceKeys = lList[j].ChildNodes;
                        _ResourceItem[lTamEnvironment][lCurrentResource].ResourceString = lServiceKeys[0].InnerText;
                    }
                }
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Console.WriteLine(e.Message + ": There was an error loading the XML file");
            }
        }

        public void Dispose()
        {
            _IsFileWatchRunning = false;

            try
            {
                if (_FileWatchThread.ThreadState == ThreadState.WaitSleepJoin || _FileWatchThread.ThreadState == ThreadState.Running)
                {
                    _FileWatchThread.Abort();
                }
            }
            catch (System.Exception ee)
            {
                System.Diagnostics.Debug.WriteLine("Problem on thread abort" + ee.Message);
            }
        }
        public override string ToString()
        {
            return ("EnvironmentSettings");
        }

    }
}
