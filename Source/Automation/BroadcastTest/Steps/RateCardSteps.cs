using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using BroadcastTest.Pages;
using BroadcastTest.Model;
using NUnit.Framework;
using BroadcastTest.Helpers;

namespace BroadcastTest.Steps
{
    [Binding]
    public sealed class RateCardSteps 
    {

        RateCardPage RateCard = new RateCardPage();

        [Given(@"I am on the Rate Card screen")]
        public void GivenIAmOnTheRateCardScreen()
        {
            RateCard.Initialize();
        }

        [Given(@"I am on the Rate Card screen with rate data loaded successfully")]
        public void GivenIAmOnTheRateCardScreenWithRateDataLoadedSuccessfully()
        {
            RateCard.Initialize();
            RateCard.ValidateStationsAreLoaded();
        }


        [When(@"Rate Data finishes loading")]
        public void WhenRateDataFinishesLoading()
        {
            RateCard.ValidateStationsAreLoaded();
        }

        [Then(@"I should see the following station data in Rate Management table")]
        public void ThenIShouldSeeTheFollowingRowsInRateManagementTable(Table table)
        {
            var stationRow = table.CreateSet<StationRateCard>();

            foreach (var station in stationRow)
            {
                //Console.WriteLine("Searching for Station: " + station.Station);
                RateCard.ValidateStationData(station);
            }            
        }



        [When(@"I upload xml file containing new rate data")]
        public void WhenIUploadXmlFileContainingNewRateData(Table table)
        {
            var xmlFiles = table.CreateSet<XmlImportFiles>();
            ScenarioContext.Current.Add("XmlFiles", xmlFiles); //Store files for other steps

            foreach (var file in xmlFiles)
            {
                RateCard.UploadStationRateXmlByDialog(file.FilePath, file.FileName);
            }

        }

        [When(@"Upload completed successfully")]
        public void WhenUploadCompletedSuccessfully()
        {
            ScenarioContext.Current.Pending();
        }


        [When(@"I delete the following rate data by station name")]
        public void WhenIDeleteTheFollowingRateDataByStationName(Table table)
        {
            var stationRow = table.CreateSet<StationRateCard>();

            foreach (var station in stationRow) 
            {
                //Console.WriteLine("Searching for Station: " + station.Station);
                
            } 
        }

        /// <summary>
        /// StationRateDataTest.feature steps
        /// </summary>
        /// <param name="stationName"> The Name of the station </param>
        [Given(@"I view the station rate modal for the station '(.*)'")]
        public void GivenIViewTheStationRateModalForTheStation(string stationName)
        {
            ScenarioContext.Current.Add("currentStationName", stationName); 
            RateCard.Initialize();
            RateCard.ValidateStationsAreLoaded();
            RateCard.OpenStationRateModal(stationName);
        }

        [When(@"I view all rates")]
        public void WhenIViewAllRates()
        {
            //ScenarioContext.Current.Pending();
            //Check title, get rate count?
            //Maybe get all the rates loaded for evaluation
            RateCard.GetAllRates();
        }

        [Then(@"I should see the following rates in the grid")]
        public void ThenIShouldSeeTheFollowingRatesInTheGrid(Table table)
        {
            var ratesGrid = table.CreateSet<RatesData>();
            ScenarioContext.Current.Add("RateValidation", true); //Set to false if validation fails across all rates.
            foreach (var rate in ratesGrid)
            {
                Console.WriteLine("Searching for rate in station:");
                rate.PrintRatesData();
                RateCard.ValidateStationRate(rate);
            }
            bool isPassed = (bool)ScenarioContext.Current["RateValidation"];
            if (!isPassed)
            {
                Assert.Fail("Not all rates were matched in the UI. Inspect the SpecFlow results for further details.");
            }
               

        }

        [When(@"I delete the rates")]
        public void WhenIDeleteTheRates(Table table)
        {
            var ratesGrid = table.CreateSet<RatesData>();

            foreach (var rate in ratesGrid)
            {
                Console.WriteLine("Deleting rate in station " + rate.AirTime);
                RateCard.DeleteStationRateRow(rate);
                //Rate card deleted                
            }
        }

        [Then(@"I should no longer see these rates in the grid")]
        public void ThenIShouldNoLongerSeeTheseRatesInTheGrid(Table table)
        {
            var ratesGrid = table.CreateSet<RatesData>();

            foreach (var rate in ratesGrid)
            {
                Console.WriteLine("Verifying rate does not exist in station " + rate.AirTime);
                RatesData searchRate = RateCard.FindStationRate(rate, RateCard.GetAllRates());
                if (searchRate != null)
                {
                    Console.WriteLine("The following rate still exists in data grid.");
                    searchRate.PrintRatesData();
                    Assert.Fail("A deleted rate still exists in data grid.");
                }
                else
                {
                    Console.WriteLine("Verified the rate is no longer found in the data grid.");
                }
                //Rate card deleted                
            }
        }

        [Then(@"I validate all station rates in UI using the Excel file for station '(.*)'")]
        public void ThenIValidateAllStationRatesInUIUsingTheExcelFileForStation(string stationName, Table table)
        {
            if (stationName.Count() < 1)
            {
                Console.WriteLine("Station Name is empty or null.");
            }

            var xmlFiles = table.CreateSet<XmlImportFiles>();

            foreach (var file in xmlFiles)
            {
                RateCard.ValidateStationRateByExcel(file.FilePath, file.FileName, stationName);
            }


        }


        [Then(@"I validate all station rates in UI using the Excel file")]
        public void ThenIValidateAllStationRatesInUIUsingTheExcelFile(Table table)
        {
            var xmlFiles = table.CreateSet<XmlImportFiles>();

            foreach (var file in xmlFiles)
            {
                //RateCard.ValidateStationRateByExcel(file.FilePath, file.FileName);
            }
        }

        [Then(@"I validate all station rates for station '(.*)' in UI using the imported XML")]
        public void ThenIValidateAllStationRatesForStationInUIUsingTheImportedXML(string stationName)
        {
            if (stationName.Count() < 1)
            {
                Console.WriteLine("Station Name is empty or null.");
            }

            List<XmlImportFiles> xmlFiles = (List<XmlImportFiles>)ScenarioContext.Current["XmlFiles"]; //Pull xml files from previous step.
           
            if (xmlFiles.Count() < 1)
            {
                Console.WriteLine("XML Files from previous step could not be found in scenario context.");
            }

            foreach (var file in xmlFiles)
            {

                RatesXml xmlFile = XmlFileHelper.DeserializeXMLFileToObject<RatesXml>(file.FilePath + "\\" + file.FileName);
               // RateCard.ValidateStationRateByXml(xmlFile, stationName); //Not using serialization yet
            }
        }


        [When(@"I view the station rate modal for the station '(.*)'")]
        public void WhenIViewTheStationRateModalForTheStation(string stationName)
        {
            ScenarioContext.Current.Add("currentStationName", stationName); 
            //RateCard.GetStationRates(stationName);
        }

        [When(@"I create the rates")]
        public void WhenICreateTheRates(Table table)
        {
            var rates = table.CreateSet<RatesData>();
            string stationName = ScenarioContext.Current["currentStationName"].ToString();

            foreach (var rate in rates)
            {
                Console.WriteLine("Creating following rate in station " + stationName);
                ScenarioContext.Current.Add("currentRate", rate); 
                RateCard.CreateNewRate(rate);
                //Rate card deleted                
            }
        }



        [Then(@"I validate all station rates for this station in UI using the XML")]
        public void ThenIValidateAllStationRatesForThisStationInUIUsingTheXML(Table table)
        {
            string stationName = ScenarioContext.Current["currentStationName"].ToString();
            if (stationName.Count() < 1)
            {
                Console.WriteLine("Station Name is empty or null.");
            }

            //Should be in previous step
            RateCard.GetStationRates(stationName);

            var xmlFiles = table.CreateSet<XmlImportFiles>(); //Pull xml files from previous step.

            foreach (var file in xmlFiles)
            {             
                //Parse XML method (more simple than deserialization)
                RateCard.ValidateAllStationRatesByXml(file.FilePath, file.FileName, stationName);               
                
            }
        }



        [When(@"I validate all station rates for station '(.*)' in UI using the XML")]
        public void WhenIValidateAllStationRatesForStationInUIUsingTheXML(string stationName, Table table)
        {
            if (stationName.Count() < 1)
            {
                Console.WriteLine("Station Name is empty or null.");
            }

            var xmlFiles = table.CreateSet<XmlImportFiles>(); //Pull xml files from previous step.

            foreach (var file in xmlFiles)
            {

                try
                {
                    RatesXml xmlFile = XmlFileHelper.DeserializeXMLFileToObject<RatesXml>(file.FilePath + "\\" + file.FileName);
                    //RateCard.ValidateStationRateByXml(xmlFile, stationName);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to deserialize provided XML file into rates data." + e);
                }

            }
        }

        [Then(@"XML UI validation completed successfully")]
        public void ThenXMLUIValidationCompletedSuccessfully()
        {
            ScenarioContext.Current.Pending();
        }



        [Then(@"I should see all of the rates in the grid that are in the xml file")]
        public void ThenIShouldSeeAllOfTheRatesInTheGridThatAreInTheXmlFile()
        {
            ScenarioContext.Current.Pending();
        }


        [Given(@"I convert flight date '(.*)' to '(.*)'")]
        public void GivenIConvertFlightDateTo(string p0, string p1)
        {
            RatesData rate = new RatesData();
            string newDate = rate.ConvertDateFromXls(p0);
            Assert.AreEqual(newDate, p1);

        }


        //Steps for editing existing rates
        [Given(@"I find the following rate to edit")]
        public void GivenIFindTheFollowingRateToEdit(Table table)
        {
            //Open the rate
            var ratesGrid = table.CreateSet<RatesData>();

            foreach (var rate in ratesGrid)
            {
                //Console.WriteLine("Finding rate to edit");
                RatesData searchRate = RateCard.FindStationRate(rate, RateCard.GetAllRates());
                if (searchRate != null)
                {
                    Console.WriteLine("Opened the rate to edit");
                    ScenarioContext.Current.Add("currentRate", searchRate); 
                    RateCard.OpenRateDataModal(searchRate);
                }
                else
                {
                    Console.WriteLine("The rate is not found in the data grid. Unable to perform edit.");
                    Assert.Fail("Unable to find the rate in grid to edit the rate. Could not match rate: ");
                    rate.PrintRatesData();
                }
                //Rate card deleted                
            }

        }

        [When(@"I edit the rate to the following values")]
        public void WhenIEditTheRateToTheFollowingValues(Table table)
        {
            string stationName = ScenarioContext.Current["currentStationName"].ToString();
            if (stationName.Length < 1)
            {
                Console.WriteLine("Station Name from previous step is empty or null.");
            }

            RatesData oldRate = (RatesData)ScenarioContext.Current["currentRate"];
            if (oldRate.Equals(null))
            {
                Console.WriteLine("Current rate from previous step is empty or null.");
            }

             //Open the rate
            var ratesGrid = table.CreateSet<RatesData>();
            
            foreach (var rate in ratesGrid)
            {
                //Console.WriteLine("Finding rate to edit");
                if (rate != null)
                {
                    //try
                    //{
                        RateCard.EditExistingRate(oldRate, rate);
                        ScenarioContext.Current.Add("newRate", rate);
                    //}
                    //catch (Exception e)
                    //{
                     //   Console.WriteLine("Unable to edit existing rate with new values.", e.ToString());
                      //  Assert.Fail("Unable to edit existing rate with new values. Error was encountered during edit.");
                    //}

                }
                else
                {
                    Assert.Fail("The rate is not found in the data grid. Unable to perform edit.");                    
                }
                //Rate card deleted                
            }          

        }

        [When(@"The edited rate is saved successfully")]
        public void WhenTheEditedRateIsSavedSuccessfully()
        {
            RateCard.SaveEditedRate();
        }

        [Then(@"The edited rate should appear in the list of rates for the station")]
        public void ThenTheEditedRateShouldAppearInTheListOfRatesForTheStation()
        {
            RatesData oldRate = (RatesData)ScenarioContext.Current["currentRate"];
            RatesData newRate = (RatesData)ScenarioContext.Current["newRate"];

            RatesData searchRateEdit = RateCard.FindStationRate(newRate, RateCard.GetAllRates());
            if (searchRateEdit != null)
            {
                Console.WriteLine("Found and verified the edited/new rate in this station's rates.");
                searchRateEdit.PrintRatesData(); //Print data from grid 
                
            }
            else
            {
                Console.WriteLine("The edited rate is not found in the data grid.");
                Assert.Fail("Unable to find the edited rate in grid after the edit.");
            }
            //Also check that the old rate does not exist as well (it should be removed).

            RatesData searchRateOld = RateCard.FindStationRate(newRate, RateCard.GetAllRates());
            if (searchRateOld == null)
            {
                Console.WriteLine("Verified the old rate no longer appears in the data grid.");

            }
            else
            {
                //Console.WriteLine("The old rate was not removed from the system and was found in the data grid.");
                Assert.Fail("The old rate was not removed from the system and was found in the data grid.");
            }


        }



    }
}
