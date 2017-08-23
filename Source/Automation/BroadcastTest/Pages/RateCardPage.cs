using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Interactions.Internal;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using TechTalk.SpecFlow;
using BroadcastTest.Model;
using System.Configuration;
using BroadcastTest.Helpers;
using System.Data;
using System.Xml;

//using BroadcastTest.Utils.W2uiTools;

namespace BroadcastTest.Pages
{
    class RateCardPage : BasePageObject
    {

        public static string url = "http://cadapps-qa1/BroadCast/Rates";
       // public static string BaseUrl = ConfigurationManager.AppSettings["BaseUrl"];

        //Element Locators - Stations//
        [FindsBy(How = How.Id, Using = "broadcast_status")]
        public IWebElement Status { get; set; }

        [FindsBy(How = How.Id, Using = "uploadButton")]
        public IWebElement UploadButton { get; set; }

        [FindsBy(How = How.Id, Using = "stations_filter_input")]
        public IWebElement AllStationsFilter { get; set; }

        [FindsBy(How = How.Id, Using = "stations_search_input")]
        public IWebElement SearchBox { get; set; }

        [FindsBy(How = How.Id, Using = "stations_search_clear_btn")]
        public IWebElement ClearSearchButton { get; set; }


        ///
        //Element Locators - Station Rate Modal (Move into its own PageObject???)
        [FindsBy(How = How.Id, Using = "grid_StationsGrid_records")]
        public IWebElement StationsGrid { get; set; }

        [FindsBy(How = How.Id, Using = "station_modal_view")]
        public IWebElement StationsRateModal { get; set; }

        [FindsBy(How = How.ClassName, Using = "modal_title")]
        public IWebElement StationsRateModalTitle { get; set; }

        [FindsBy(How = How.Id, Using = "grid_RatesGrid_records")]
        public IWebElement StationRatesGrid { get; set; }

        [FindsBy(How = How.Id, Using = "grid_RatesGrid_footer")]
        public IWebElement StationRatesGridFooter { get; set; }

        ///
        //Element Locators - Error Message Modal 
        [FindsBy(How = How.Id, Using = "default_error_modal")]
        public IWebElement UploadErrorMessageModal { get; set; }

        [FindsBy(How = How.Id, Using = "default_error_text")]
        public IWebElement UploadErrorText { get; set; }

        [FindsBy(How = How.Id, Using = "default_error_message")]
        public IWebElement ErrorMessage { get; set; }

        [FindsBy(How = How.Id, Using = "default_error_message")]
        public IWebElement UploadErrorMessageOkButton { get; set; }

        [FindsBy(How = How.ClassName, Using = "close")]
        public IWebElement UploadErrorMessageClose { get; set; }

        /// 
        //Element Locators - Station Rate Edit/Create Modal
        [FindsBy(How = How.ClassName, Using = "rates_new_program_btn")]
        public IWebElement RateDataCreateNewProgramBtn { get; set; } //Duplicate ID

        [FindsBy(How = How.ClassName, Using = "modal_title")]
        public IWebElement RateDataEditModalTitle { get; set; } //Duplicate ID

        [FindsBy(How = How.Id, Using = "update_program_name_input")]
        public IWebElement RateDataProgramName { get; set; }

        [FindsBy(How = How.Id, Using = "update_program_airtime_input")]
        public IWebElement RateDataAirTime { get; set; }

        [FindsBy(How = How.Id, Using = "update_program_genre_input")]
        public IWebElement RateDataGenre { get; set; }

        [FindsBy(How = How.Id, Using = "update_program_flight_input")]
        public IWebElement RateDataFlight { get; set; }

        [FindsBy(How = How.Id, Using = "update_program_spot15_input")]
        public IWebElement RateDataSpot15 { get; set; }

        [FindsBy(How = How.Id, Using = "update_program_spot30_input")]
        public IWebElement RateDataSpot30 { get; set; }

        [FindsBy(How = How.Id, Using = "update_program_hhimpressions_input")]
        public IWebElement RateDataHhImpressions { get; set; }

        [FindsBy(How = How.Id, Using = "update_program_hhrating_input")]
        public IWebElement RateDataHhRating{ get; set; }

        [FindsBy(How = How.Id, Using = "update_program_effective_date_input")]
        public IWebElement RateDataEffectiveDate { get; set; }

        [FindsBy(How = How.ClassName, Using = "btn-default")]
        public IWebElement RateDataCancelButton { get; set; }

        [FindsBy(How = How.ClassName, Using = "close")]
        public IWebElement RateDataCloseModal { get; set; }

        [FindsBy(How = How.Id, Using = "update_program_save_btn")]
        public IWebElement RateDataSaveButton { get; set; } 


        //Private properties
        private List<RatesData> AllRatesRecords = new List<RatesData>();
        private List<IWebElement> AllRatesElements = new List<IWebElement>();

        [SetUp]
        public void Initialize()
        {
            //Navigate to Execute automation demo page
            try
            {
                driver = (IWebDriver)ScenarioContext.Current["driver"];

                driver.Navigate().GoToUrl(url);
                PageFactory.InitElements(driver, this);

                Console.WriteLine("Using WebDriver: " + driver.GetType().ToString());
                Console.WriteLine("Opened URL: " + url);

                Assert.AreEqual("Broadcast", driver.Title); //Only valid for this page object
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception encountered creating web driver session.", e.ToString());
            }

        }

        //TBD: method to scroll to record in grid
        //TBD: method to fetch all records from grid (for big data validations across many rows)

        //TBD: Method to find row in grid, return object containing desired row FetchRow(string match text)
        

        public void ValidateStationsAreLoaded()
        {
            Console.WriteLine("Status message: " + Status.Text.ToString());


            while (Status.Text.Equals("Status: Ready")) //Stations not yet loaded, wait for stations to load or detect error.
            {
                Thread.Sleep(5); //This is bad, should implement another way to wait into driver.
                PageFactory.InitElements(driver, this); //Just use wait for element instead

                //driver.Manage().Timeouts().ImplicitlyWait(10, TimeUnit.SECONDS);

                if (Status.Text.Equals("Status: Error - Load Stations"))
                {
                    //Write error to log file
                    Console.WriteLine("There was an error loading stations. Error Message:" + ErrorMessage.Text);

                    Assert.Fail("Status message indicates an error.");
                }
            }

            if (!ErrorMessage.Text.Equals(""))
            {
                //Write error to log file
                PageFactory.InitElements(driver, this);
                IWebElement errorMsg = driver.FindElement(By.Id("default_error_message"));
                Console.WriteLine("There was an error. Error Message:" + errorMsg.Text);

                Assert.Fail("Status message indicates an error.");
            }

            Assert.AreEqual("Status: Success - Load Stations", Status.Text.ToString());
        }


        public bool TestStationRate(RatesData expectedRate)
        {
            //if AllRatesInModal is empty run GetAllRates()
            if (AllRatesRecords.Count() <= 0)
            {
                AllRatesRecords = GetAllRates();
            }

            RatesData matchedRate = FindStationRate(expectedRate, AllRatesRecords);

            if (matchedRate != null)
            {
                return true;
            }
            else 
            {
                return false; 
            }
        }
        
        public void ValidateStationRate(RatesData expectedRate)
        {
            //Finds the rate in list of all rates,
            
            //if AllRatesInModal is empty run GetAllRates()
            if (AllRatesRecords.Count() <= 0)
            {
                AllRatesRecords = GetAllRates();
            }

            RatesData matchedRate = FindStationRate(expectedRate, AllRatesRecords);

            if (matchedRate != null)
            {
                Console.WriteLine("Found this rate in UI data grid. Values from UI Grid: ");
                matchedRate.PrintRatesData();
                /*
                try
                {
                    //TODO: Break this up for column level validation.
                    if (matchedRate.AirTime.Equals(expectedRate.AirTime) 
                        && matchedRate.Program.Equals(expectedRate.Program)
                        && matchedRate.Flight.Equals(expectedRate.Flight)
                        && matchedRate.Genre.Equals(expectedRate.Genre)
                        && matchedRate.Rate30.Equals(expectedRate.Rate30)
                        && matchedRate.Rate15.Equals(expectedRate.Rate15)
                        && matchedRate.HhImpressions.Equals(expectedRate.HhImpressions)
                        && matchedRate.Rating.Equals(expectedRate.Rating))
                    {
                        Console.WriteLine("Found and verified the following rate in grid: ");
                        matchedRate.PrintRatesData();
                        // Assert.Pass("Found and verified the rate.");
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to verify the rate in the grid due to exception: " + e);
                }
                 */                
            }
            else
            {
                Console.WriteLine("Unable to find this rate in the grid for this station: ");
                if ((bool)ScenarioContext.Current["RateValidation"])
                {
                    ScenarioContext.Current["RateValidation"] = false; //Set validation failure if still true.
                }
                expectedRate.PrintRatesData();
                
            }


        }

        public RatesData FindStationRate(RatesData searchRate, List<RatesData> allRates)
        {            
            foreach (RatesData rate in allRates)
            {
                if (CompareStationRate(searchRate, rate))
                {
                    return rate;
                }
            }

            return null;    
        }
  

  
  
        public bool CompareStationRate(RatesData expectedRate, RatesData testRate)
        {

            //Explicitly compare each column against expectedRate. Yes, its slow but isn't it nice you don't have to do this?
            //Can't compare objects, only these string values. 
            if (!ValidateStringValues(expectedRate.AirTime, testRate.AirTime)) 
            {
                //Console.WriteLine("Air Time does not match: expected = " + expectedRate.AirTime + ", actual = " + testRate.AirTime);
                return false;
            }
            else if (!ValidateStringValues(expectedRate.Program, testRate.Program)) 
            {
                //Console.WriteLine("Program does not match: expected = " + expectedRate.Program + ", actual = " + testRate.Program);
                return false;
            }
            else if (!ValidateStringValues(expectedRate.Genre, testRate.Genre)) 
            {
                //Console.WriteLine("Genre does not match: expected = " + expectedRate.Genre + ", actual = " + testRate.Genre);
                return false;        
            }
            else if (!ValidateStringValues(expectedRate.Rate30, testRate.Rate30)) 
            {
                //Console.WriteLine("30 Rate does not match: expected = " + expectedRate.Rate30 + ", actual = " + testRate.Rate30);
                return false;                
            }
            else if (!ValidateStringValues(expectedRate.Rate15, testRate.Rate15)) 
            {
                //Console.WriteLine("15 Rate does not match: expected = " + expectedRate.Rate15 + ", actual = " + testRate.Rate15);
                return false; 
            }
            else if (!ValidateStringValues(expectedRate.HhImpressions, testRate.HhImpressions)) 
            {
                //Console.WriteLine("HH Impressions does not match: expected = " + expectedRate.HhImpressions + ", actual = " + testRate.HhImpressions);
                return  false;                 
            }
            else if (!ValidateStringValues(expectedRate.Rating, testRate.Rating)) 
            {
                //Console.WriteLine("Rating does not match: expected = " + expectedRate.Rating + ", actual = " + testRate.Rating);
                return  false; 
            }
            else if (!ValidateStringValues(expectedRate.Flight, testRate.Flight))
            {
                //Console.WriteLine("Flight does not match: expected = " + expectedRate.Flight + ", actual = " + testRate.Flight);
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool ValidateStringValues(string val_1, string val_2)
        {

            if (val_1 == null)
            {
                val_1 = "";
            }
            else if (val_2 == null)
            {
                val_2 = "";
            }

            if (val_1.Equals(val_2))
            {
                return true;
            }
            else
            {
                return false;
            }
   
        }


        public List<IWebElement> GetAllRateElements()
        {
            //Do I need to scroll to get all rates?
            List<IWebElement> allRates = new List<IWebElement>();
            //Must have rates data modal opened

            //PageFactory.InitElements(driver, this); //Just use wait for element instead

            ReadOnlyCollection<IWebElement> rowsOdd = StationRatesGrid.FindElements(By.ClassName("w2ui-odd"));
            ReadOnlyCollection<IWebElement> rowsEven = StationRatesGrid.FindElements(By.ClassName("w2ui-even"));

            RatesData rate = new RatesData();
            //Extract rates from row containers
            foreach (IWebElement row in rowsOdd)
            {
                allRates.Add(row);
            }

            foreach (IWebElement row in rowsEven)
            {
                allRates.Add(row);
            }

            Console.WriteLine("Extracted " + allRates.Count() + " rate element rows from this station's data grid.");
            AllRatesElements = allRates;

            return allRates;
        }

        public IWebElement FindRateElement(RatesData searchRate)
        {
            //Given a rate, find the rate row in grid
            //List<IWebElement> allRates = GetAllRateElements();

            //Ensure we have all rates elements available
            if (AllRatesElements.Count() < 1)
            {
                AllRatesElements = GetAllRateElements();
            }
            //Assert.Fail("We do not have all rate elements to do proper search")

            foreach (IWebElement rateRow in AllRatesElements)
            {
                RatesData rate = GetRateFromTableRow(rateRow);
                if (CompareStationRate(searchRate, rate))
                {
                    return rateRow;
                }
            }

            return null;

        }

        public void DeleteStationRateRow(RatesData searchRate)
        {
            IWebElement rateRow = FindRateElement(searchRate);

            if (rateRow != null)
            {
                DeleteStationRate(rateRow);
            }
        }

        
        public void SetAllRatesRecordsFromRatesElements()
        {

            //Set AllRatesRecords from AllRatesElements without having to parse page elements again
            if (AllRatesElements.Count() > 1)
            {
                //If there are rates elements, populate list of rate records
                List<RatesData> rateRecords = new List<RatesData>();
                foreach (IWebElement row in AllRatesElements)
                {
                    RatesData rate = GetRateFromTableRow(row);
                    rateRecords.Add(rate);
                }
                AllRatesRecords = rateRecords;
            }


        }

        //Redundant if you get elements first, shouldn't use this class. Use only when all you need are list of rates data from the station table.
        public List<RatesData> GetAllRates()
        {
            //Do I need to scroll to get all rates?
            List<RatesData> allRates = new List<RatesData>();
            //Must have rates data modal opened
           
            if (!StationRatesGrid.Displayed)
            {
                PageFactory.InitElements(driver, this); //Build in wait for element instead
            }

           // PageFactory.InitElements(driver, this); //Build in wait for element instead
            

            ReadOnlyCollection<IWebElement> rowsOdd = StationRatesGrid.FindElements(By.ClassName("w2ui-odd"));
            ReadOnlyCollection<IWebElement> rowsEven = StationRatesGrid.FindElements(By.ClassName("w2ui-even"));

            List<IWebElement> allRateElems = new List<IWebElement>();

            RatesData rate = new RatesData();
            //Extract rates from row containers
            foreach (IWebElement row in rowsOdd)
            {
                rate = GetRateFromTableRow(row);
                allRates.Add(rate);
                allRateElems.Add(row);
            }

            foreach (IWebElement row in rowsEven)
            {
                rate = GetRateFromTableRow(row);
                allRates.Add(rate);
                allRateElems.Add(row);
            }

            Console.WriteLine("Extracted " + allRates.Count() + " rates from this station for validation.");
            AllRatesRecords = allRates; //Store all rates for future step?
            AllRatesElements = allRateElems;

            return allRates;

        }

        public RatesData GetRateFromTableRow(IWebElement row)
        {
            RatesData rate = new RatesData();
            try
            {
                ReadOnlyCollection<IWebElement> rowData = row.FindElements(By.ClassName("w2ui-grid-data"));

                rate.AirTime = rowData[0].Text;
                rate.Program = rowData[1].Text;
                rate.Genre = rowData[2].Text;
                rate.Rate30 = rowData[3].Text;
                rate.Rate15 = rowData[4].Text;
                rate.HhImpressions = rowData[5].Text;
                rate.Rating = rowData[6].Text;
                rate.Flight = rowData[7].Text;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to capture a rate from the station. Exception:" + e);
            }


            return rate;
        }


        /*
        public void ValidateStationInTableBySearch(StationRateCard expectedRow)
        {
            //Navigate data grid by search instead of scrolling
            SearchForStation(expectedRow.Station);
            //Check data against expectedRow. 
            
        }
        */

        public void ClearSearchBox()
        {
            if (ClearSearchButton.Displayed)
            {
                ClearSearchButton.Click();
            }
            if (!SearchBox.Text.Equals(""))
            {
                SearchBox.Clear();
                Assert.Fail("Unable to clear search box using button. Cleared it with Selenium instead.");
            }
        }

        public void UploadStationRateXmlByDialog(string filePath, string fileName)
        {
            //Check if File exists
            string importFile = filePath + "\\" + fileName;

            UploadButton.SendKeys(importFile);

            string errorMsg = GetUploadErrorMessage();
            if (!errorMsg.Equals(null) || !errorMsg.Equals(""))
            {
                Console.WriteLine("There was an error uploading the XML file due to: " + errorMsg);
                Console.WriteLine("Closing error message dialog.");
                UploadErrorMessageClose.Click();
            }
            else
            {
                ValidateStationsAreLoaded(); //This should 
            }
            //Verify

        }


        public string GetUploadErrorMessage()
        {
            if (!ErrorMessage.Text.Equals(null) || !ErrorMessage.Text.Equals(""))
            {
                return ErrorMessage.Text;
            }
            else return null;
        }

        //Invalid, not deleting stations
        public void DeleteByStation(string stationName)
        {
            
        }

        public void DeleteStationRate(IWebElement stationRateRow)
        {
            if (!stationRateRow.Equals(null))
            {
                ClickStationRateContextMenuOption(stationRateRow, "Delete");

            }
        }

        public void ClickStationRateContextMenuOption(IWebElement stationRow, string menuOptionName)
        {
            Actions action = new Actions(driver); //Double Click Row - build into interface method
            action.ContextClick(stationRow);
            action.Perform();

            PageFactory.InitElements(driver, this);

            IWebElement contextMenu = driver.FindElement(By.ClassName("menu"));
            //ReadOnlyCollection<IWebElement> menuItems = GetContextMenuOptions();
            ReadOnlyCollection<IWebElement> menuItems = contextMenu.FindElements(By.ClassName("menu-text"));   
            var isClicked = false;

            foreach (IWebElement item in menuItems)
            {
                // 
                if (!item.Text.Equals(""))
                {
                    if (item.Text.Contains(menuOptionName))
                    {
                        Console.WriteLine("Found delete option in context menu.");
                    //    item.Click();
                        isClicked = true;
                    }
                }
            }

            if (!isClicked)
            {
                Console.WriteLine("Menu option: " + menuOptionName + " was not found in context menu.");
                //Assert.Fail("Station was not found in the data grid.");
            }


        }

        public ReadOnlyCollection<IWebElement> GetContextMenuOptions()
        {
           // menu = driver.FindElement(By.ClassName("w2ui-drop-menu"));
            return driver.FindElements(By.ClassName("menu-text"));      
        }


        public void OpenStationRateModal(string stationName)
        {

            IWebElement stationRow = FindStationByName(stationName);
            if (!stationRow.Equals(null))
            {
                Actions action = new Actions(driver); //Double Click Row - build into interface method
                action.DoubleClick(stationRow);
                action.Perform();
            }
            //ValidateStationRateModal();
        }

        public void OpenRateDataModal(RatesData rate)
        {

            IWebElement rateRow = FindRateElement(rate);
            if (!rateRow.Equals(null))
            {
                //Do we need to scroll to rate first? try click to focus:
                rateRow.Click();
                //Double Click Row - build into interface method
                Actions action = new Actions(driver); 
                action.DoubleClick(rateRow);
                action.Perform();
                
            }
            //ValidateStationRateModal();
        }

        public void ValidateStationRateModal()
        {
            //WaitForModal(); //Takes a while to load, should ensure wait is good

            //Validate the correct modal opened for this station
            /*
            string title = GetStationRateModalTitle();
            if (title.Equals(stationName))
            {
                Console.WriteLine("Viewing rate data modal for station: " + stationName);
            }
            else
            {
                Assert.Fail("Unable to view rate data modal for station:" + stationName);
            }
            */
        }

        public string GetStationRateModalTitle()
        {
            PageFactory.InitElements(driver, this);

            string modalTitle = StationsRateModalTitle.Text;
            Console.WriteLine("Opened station Rate Modal: " + modalTitle);

            return modalTitle;
            //TODO: Parse modal title so we know if its rate modal or error modal, etc. 
            /*
            StationRateCard stationInfo = null;
            if (!StationsRateModalTitle.Equals(null))
            {
                stationInfo.Station = spans[0].Text;
                stationInfo.Affiliate = spans[1].Text;
                stationInfo.Market = spans[2].Text;
            }
           
            return stationInfo;
             * */
        }

        public void WaitForModal()
        { }

        public void EditModalTxtValue(IWebElement element, string fieldName, string currentTxt, string newTxt)
        {
            //Only edit the value if there is a new value detected 
            if (!newTxt.Equals(currentTxt))
            {
                Console.WriteLine("Editing " + fieldName + " value from \'" + currentTxt + "\', to \'" + newTxt + "\'");
                element.Clear();
                element.SendKeys(newTxt);

                //Set focus outside of form field to trigger validation on form field
                element.SendKeys(Keys.Tab);
                // IWebElement focusDiv = driver.FindElement(By.ClassName("modal-body"));
                // focusDiv.Click(); //Set focus outside of form field to trigger validation on form field
                
                //Verify the new txt has been set in the form field, currently doesn't work:
                //if (!element.Text.Equals(newTxt))
                //{
                //    Assert.Fail(fieldName + " value was not set properly in edit rate form.");
                //}

                try
                {
                   //Look for form field validation error messages
                    string elId = element.GetAttribute("id").ToString();
                    IWebElement elErrorMsg = null;
                    PageFactory.InitElements(driver, this);
                    if (IsElementPresent(By.Id(elId + "-error")))
                    {
                        elErrorMsg = driver.FindElement(By.Id(elId + "-error"));
                    }

                    if (elErrorMsg != null)
                    {
                        //There was an error message for this form field edit.
                        Console.WriteLine("Field validation error message was found: " + elErrorMsg.Text);
                        //Don't assert a fail, we might be testing that error message is received
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("NoSuchElementException"))
                        Console.WriteLine("No field validation error messages were found");
                    else
                        Console.WriteLine("Unable to capture field validation error message. Need to handle exception: " + e.ToString());
                }

            }


        }

        //Move into driver extension methods
        private bool IsElementPresent(By by)
        {
            try
            {
                driver.FindElement(by);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public void CreateNewRate(RatesData newRate)
        {
            //Check if rate exists? if not create new rate

            //Click 'New Program' button on station modal
            RateDataCreateNewProgramBtn.Click();
            //Set txt fields first
            //RateDataProgramName.SendKeys(newRate.Program);
            EditModalTxtValue(RateDataProgramName, "Program Name", "", newRate.Program);
            EditModalTxtValue(RateDataHhImpressions, "HhImpressions", "", newRate.ConvertHhImpressionsToUI(newRate.HhImpressions));
            EditModalTxtValue(RateDataHhRating, "Rating", "", newRate.Rating);
            
            //Set Effective Date with date picker
            //Set effective date manually

            //Set AirTime

            //Set Flight

        }


        public void EditExistingRate(RatesData currentRate, RatesData newRate)
        {
            //Opening the rate is in its own step, should already be at rate edit modal 

            //Attempt to edit all fields based on current/new rates data:
            //EditModalTxtValue(RateDataAirTime, "Air Time", currentRate.AirTime, newRate.AirTime); //Not editable yet
            //EditModalTxtValue(RateDataProgramName, "Program", currentRate.Program, newRate.Program); //Not editable yet
            EditModalTxtValue(RateDataGenre, "Genre", currentRate.Genre, newRate.Genre);
           // EditModalTxtValue(RateDataSpot15, "15 Rate", currentRate.Rate15, newRate.Rate15);
            //EditModalTxtValue(RateDataSpot30, "30 Rate", currentRate.Rate30, newRate.Rate30);
            EditModalTxtValue(RateDataHhImpressions, "HhImpressions", 
                currentRate.ConvertHhImpressionsToUI(currentRate.HhImpressions), 
                newRate.ConvertHhImpressionsToUI(newRate.HhImpressions));
            EditModalTxtValue(RateDataHhRating, "Rating", currentRate.Rating, newRate.Rating);
            //EditModalTxtValue(RateDataFlight, "Flight", currentRate.Flight, newRate.Flight); //Not editable yet
           

            //TODO: Set effective date to next monday

        }

        public void SaveEditedRate()
        {
            RateDataSaveButton.Click();

            //Ensure modal has closed.
        }


        public List<RatesData>ParseRateXmlFile(string fileName, string filePath, string stationName)
        {
            //Parse XML file directly with Linq
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.Load(fileName + "\\" + filePath); // Load the XML document from the specified file

            // Get elements
            var nsm = new XmlNamespaceManager(xmlDoc.NameTable);
            nsm.AddNamespace("s", "http://www.AAAA.org/schemas/spotTVCableProposal");
            nsm.AddNamespace("tvb-tp", "http://www.AAAA.org/schemas/TVBGeneralTypes");


            var elemList = xmlDoc.SelectNodes("//s:AvailLineWithDetailedPeriods", nsm);
            //XmlNodeList rateNodes = xmlDoc.GetElementsByTagName("AvailLineWithDetailedPeriods");

            List<RatesData> allXmlRates = new List<RatesData>();

            foreach (XmlNode rateNode in elemList)
            {
                RatesData rate = new RatesData();
               
                //Assume single <DayTime/> in <DayTimes/> for now, may need to handle multiple ones in future.
                var dayTimesNode = rateNode.SelectSingleNode("s:DayTimes", nsm);
                var dayTimeNode = dayTimesNode.SelectSingleNode("s:DayTime", nsm);

                DayTime dayTime = new DayTime();
                dayTime.StartTime = dayTime.TimeAsDateTime(dayTimeNode.SelectSingleNode("s:StartTime", nsm).InnerText);
                dayTime.EndTime = dayTime.TimeAsDateTime(dayTimeNode.SelectSingleNode("s:EndTime", nsm).InnerText);
                var days = dayTimeNode.SelectSingleNode("s:Days", nsm);
                dayTime.SetDay("Monday", days.SelectSingleNode("tvb-tp:Monday", nsm).InnerText);
                dayTime.SetDay("Tuesday", days.SelectSingleNode("tvb-tp:Tuesday", nsm).InnerText);
                dayTime.SetDay("Wednesday", days.SelectSingleNode("tvb-tp:Wednesday", nsm).InnerText);
                dayTime.SetDay("Thursday", days.SelectSingleNode("tvb-tp:Thursday", nsm).InnerText);
                dayTime.SetDay("Friday", days.SelectSingleNode("tvb-tp:Friday", nsm).InnerText);
                dayTime.SetDay("Saturday", days.SelectSingleNode("tvb-tp:Saturday", nsm).InnerText);
                dayTime.SetDay("Sunday", days.SelectSingleNode("tvb-tp:Sunday", nsm).InnerText);
                rate.AirTime = dayTime.GetAirTime();
   

                rate.Program = rateNode.SelectSingleNode("s:AvailName", nsm).InnerText;
                rate.Genre = ""; //Not set yet, TBD
                
                string spotLength = rateNode.SelectSingleNode("s:SpotLength", nsm).InnerText;

               //var periods = rateNode.SelectSingleNode("s:Periods", nsm);
                //var periodNodes = periods.SelectSingleNode("s:DetailedPeriod", nsm);
                var detailedPeriod = rateNode.SelectSingleNode("s:Periods", nsm).ChildNodes[0];
                // var detailedPeriod = rateNode.SelectSingleNode("s:DetailedPeriod", nsm);


                string startDate = "";
                string endDate = "";

                //Start/end dates in attributes for flight
                if (detailedPeriod.Attributes != null)
                {
                    var start = detailedPeriod.Attributes["startDate"];
                    if (start != null)                             
                    {
                        startDate = start.Value;
                    }
                    else
                    {
                        throw new InvalidOperationException("Node attribute 'startDate' not found.");
                    }

                    var end = detailedPeriod.Attributes["endDate"];
                    if (end != null)                             
                    {
                        endDate = end.Value;
                    }
                    else
                    {
                        throw new InvalidOperationException("Node attribute 'endDate' not found.");
                    }

                        
                }
                //Set Flight using start/end dates, may need to adjust in future
                if (!startDate.Equals("") && !endDate.Equals("")) //Should check for valid date, for now just check we have something
                {
                    rate.SetFlight(startDate, endDate);
                }
                    
                //Set Rate
                string xmlRate = "$" + detailedPeriod.SelectSingleNode("s:Rate", nsm).InnerText; //Append $, UI rates has trailing '$'
                if (spotLength.Equals("00:00:30") && spotLength != null)
                {
                    rate.Rate30 = xmlRate;
                    rate.Rate15 = "-";   //Use '-' for no rate                       
                }
                else if (spotLength.Equals("00:00:15") && spotLength != null)
                {
                    rate.Rate30 ="-"; //Use '-' for no rate
                    rate.Rate15 = xmlRate;   
                }
                else 
                {
                    throw new InvalidOperationException("Invalid SpotLength identified for this period.");
                }
                      
                //Set HH Impressions and Rating
                var demoNodes = detailedPeriod.SelectSingleNode("s:DemoValues", nsm).SelectNodes("s:DemoValue", nsm);
                foreach (XmlNode demNode in demoNodes)
                {
                    if (demNode.Attributes != null)
                    {
                        var demValue = demNode.Attributes["demoRef"];
                        if (demValue != null)
                        {
                            if (demValue.Value.Equals("DM0"))
                            {
                                double hhImp = Convert.ToDouble(demNode.InnerText);
                                rate.HhImpressions = Math.Round(hhImp, 2).ToString("0.00"); ;//UI uses 2 decimal palces
                            }
                            else if (demValue.Value.Equals("DM1"))
                            {
                                double rating = Convert.ToDouble(demNode.InnerText);
                                rate.Rating = Math.Round(rating, 2).ToString("0.00");  //UI uses 2 decimal palces
                            }

                        }
                    }
                }
       

                allXmlRates.Add(rate);
            }

            Console.WriteLine("XML Parsing completed. Found " + allXmlRates.Count() + " rates for validation. Listing rates:");
            foreach (RatesData rate in allXmlRates)
            {
                rate.PrintRatesData();
            }

            return allXmlRates;

        }

        public void ValidateAllStationRatesByXml(string filePath, string fileName, string stationName)
        {
            List<RatesData> allXmlRates = ParseRateXmlFile(filePath, fileName, stationName);
           // ValidateStationRateByListCompare(allXmlRates, AllRatesInModal, stationName);

            List<RatesData> matchedRates = new List<RatesData>();
            List<RatesData> unmatchedRates = new List<RatesData>();
            
            foreach (RatesData rate in allXmlRates)
            {
                bool result = TestStationRate(rate);
                if (result)
                {
                    matchedRates.Add(rate);
                }
                else
                    unmatchedRates.Add(rate);

            }
            Console.WriteLine("The following rates were verified in the UI from the XML:");
            foreach (RatesData rate in matchedRates)
            {
                rate.PrintRatesData();
            }

            if (matchedRates.Count() == allXmlRates.Count())
            {
                Console.WriteLine("Found all " + allXmlRates.Count() + " rates from the xml in the UI.");
            }

            if (matchedRates.Count() < allXmlRates.Count())
            {
                Console.WriteLine("Found " + matchedRates.Count() + " rates from the xml in the UI.");
                Console.WriteLine("The following rates were not found in the UI but exist in the XML:");
                foreach (RatesData rate in unmatchedRates)
                {
                    rate.PrintRatesData();
                }
                Assert.Fail("There were rates in the XML file which were not in the station. Inspect SpecFlow results to identify unmatched row.");
            }
            
        }


        public void GetStationRates(string stationName)
        {
            //Open station to get all rates
            OpenStationRateModal(stationName);

            //Ensure we have all current rates from UI modal
            AllRatesRecords = GetAllRates(); 
        }

        //Use deserialized RatesXml to validate
        public List<RatesData> ParseRateXmlObj(RatesXml xmlObj)
        {
            //Use serialized XML to get all XML Rates, might be overkill for what we are parsing. 
            // List<RatesData> allXmlRates = new List<RatesData>();
            //xmlObj.

            return null;
        }

        public void ValidateStationRateByListCompare(List<RatesData> expectedRates, List<RatesData> uiGridRates, string stationName)
        {


            // check that [(A-B) Union (B-A)] is empty
            var areEquivalent = !uiGridRates.Except(expectedRates).Union(expectedRates.Except(uiGridRates)).Any();
            //var areEquivalent = !uiGridRates.Except(expectedRates).Any();

            var nonMatching = uiGridRates.Except(expectedRates);
            Console.WriteLine(nonMatching.Count().ToString() + "Non matching rates found. Listing rates:");

            foreach (RatesData notMatch in nonMatching)
            {
                Console.WriteLine("Rate did not match:");
                notMatch.PrintRatesData();
            }

            if (!areEquivalent)
            {
                Console.WriteLine("Expected rates do not all match the station rates found in UI for " + stationName);
                Console.WriteLine("Expected rate total:" + expectedRates.Count() + ", UI Station Rates total: " + uiGridRates.Count());
            }
            else
            {

            }

        }


        //Validate using Excel rates
        public void ValidateStationRateByExcel(string filePath, string fileName, string stationName)
        {
            //Check if File exists
            ExcelHelper xlsHelper = new ExcelHelper();
            DataTable rateDataTbl = xlsHelper.ReadExcelToTable(filePath + "\\" + fileName);
            List<RatesData> allXlsRates = ParseXlsRates(rateDataTbl);

            //Open station to get all rates
            OpenStationRateModal(stationName);  
            /*
            //SLOW - Validate all rates from Xls in Station Rates Modal
            foreach (RatesData rate in allXlsRates)
            {
                ValidateStationRate(rate);
            }
            */


            //Ensure we have all current rates from UI modal
            AllRatesRecords = GetAllRates();

            // check that [(A-B) Union (B-A)] is empty
            //var areEquivalent = !AllRatesInModal.Except(allXlsRates).Union(allXlsRates.Except(AllRatesInModal)).Any();
            var areEquivalent = !allXlsRates.Except(AllRatesRecords).Any();

            var nonMatching = allXlsRates.Except(AllRatesRecords);
            Console.WriteLine(nonMatching.Count().ToString() + "Non matching rates found. Listing rates:");

            foreach (RatesData notMatch in nonMatching )
            {
                Console.WriteLine("Rate did not match:");
                notMatch.PrintRatesData();
            }

            if (!areEquivalent)
            {
                Console.WriteLine("XLS rates do not all match the station rates found for " + stationName);
                Console.WriteLine("XLS rate total:" + allXlsRates.Count() + ", UI Station Rates total: " + AllRatesRecords.Count());
            }
            else
            {

            }

        }

        public List<RatesData> ParseXlsRates(DataTable ratesTable)
        {

            List<RatesData> allXlsRates = new List<RatesData>();

            //If row has rate data, store rate to list
            //Valid row - Day/Time not empty, RTG not empty

            //Extract rates from rows
            foreach (DataRow row in ratesTable.Rows)
            {
                //Does this row have a rate?
                
                //If row has rate, store rate                
                RatesData rate = new RatesData();

                string DayTime = row["F1"].ToString();
                string rating= row["F7"].ToString();
                if (DayTime.Length > 0 && !DayTime.Equals("Day/Time") )
                {
                    if (rating.Length > 1 && !rating.Equals("RTG"))
                    {
                        rate.SetAirTimeFromXls(DayTime);//Convert DayTime into AirTime format
                        rate.Rating = rating;
                        rate.Program = row["F2"].ToString();
                        var startDate = row["F3"]; //Typically empty in XLS files
                        var endDate = row["F4"];                       
                        rate.SetFlight(startDate.ToString(), endDate.ToString());
                        rate.Rate30 = row["F5"].ToString();
                        rate.Rate15 = "-"; //Don't see 15 Rate in XLS files, use default: "-" for empty rate.
                        rate.HhImpressions = row["F6"].ToString();
                        rate.Genre = ""; //Don't see Genre in XLS files

                        allXlsRates.Add(rate);
                    }

                }


            }

            return allXlsRates;

        }

        public IWebElement FindStationRate(RatesData expectedRate)
        {

            OpenStationRateModal(expectedRate.StationName);


            Console.WriteLine("Searching for Station Rate - AirTime: " 
                + expectedRate.AirTime + " Program: " + expectedRate.Program);
            //Must have rates data modal opened

            ReadOnlyCollection<IWebElement> rowsOdd = StationRatesGrid.FindElements(By.ClassName("w2ui-odd"));
            ReadOnlyCollection<IWebElement> rowsEven = StationRatesGrid.FindElements(By.ClassName("w2ui-even"));
                
            //Extract rates from row containers
            foreach (IWebElement row in rowsOdd)
            {
                if (CompareStationRate(expectedRate, GetRateFromTableRow(row)))
                {
                    return row;
                }
            }

            foreach (IWebElement row in rowsEven)
            {
                if (CompareStationRate(expectedRate, GetRateFromTableRow(row)))
                {
                    return row;
                }
            }

            Console.WriteLine("Did not find the following station rate in the grid:");
            expectedRate.PrintRatesData();
            return null;

        }



        public IWebElement FindStationByName(string stationName)
        {
            //If text exists in SearchBox, clear it out:
            ValidateStationsAreLoaded();
            //Station results are unique if searching by station name. 
            ClearSearchBox();
            SearchBox.Click();
            SearchBox.SendKeys(stationName);
            SearchBox.SendKeys(Keys.Enter);

            Console.WriteLine("Searching for Station Name: ", stationName);
            // IWebElement searchEl = driver.FindElement(By.XPath("//div[contains(@title, 'ValidationSummary')]")).Text );
            //IWebElement element = StationsGrid.FindElements(By.ClassName("w2ui-grid-data")).FindElement(By.TagName("div"));
            ReadOnlyCollection<IWebElement> values = StationsGrid.FindElements(By.ClassName("w2ui-grid-data"));
            string match = "";

            foreach (IWebElement value in values)
            {
              // 
                if (!value.Text.Equals(""))
                {
                    if (value.Text.Contains(stationName))
                    {
                        match = value.Text;
                        Console.WriteLine("Station has been found in the data grid: " + match);
                        return value; //Return the web element for processing.
                    }
                }               
            }

            if (!match.Contains(stationName))
            {
                Console.WriteLine("Station was not found in the data grid: " + stationName);
                //Assert.Fail("Station was not found in the data grid.");
                return null;
            }   
            else { return null; }

        }

        public void ValidateStationData(StationRateCard expectedStation)
        {
            
            //Station results are unique if searching by station name. 
            ClearSearchBox(); //If text exists in SearchBox, clear it out. This may not be the first execution.
            SearchBox.Click();
            SearchBox.SendKeys(expectedStation.Station);
            SearchBox.SendKeys(Keys.Enter);

            Console.WriteLine("Searching for Station Name: " + expectedStation.Station);
            // IWebElement searchEl = driver.FindElement(By.XPath("//div[contains(@title, 'ValidationSummary')]")).Text );
            //IWebElement element = StationsGrid.FindElements(By.ClassName("w2ui-grid-data")).FindElement(By.TagName("div"));
            ReadOnlyCollection<IWebElement> values = StationsGrid.FindElements(By.ClassName("w2ui-grid-data"));
            string station = "";
            string affiliate = "";
            string market = "";
            string rateDataThrough = "00"; //Using "00" as null in Steps Definition
            string lastUpdate = "00";

            foreach (IWebElement value in values)
            {
                // 
                if (!value.Text.Equals(""))
                {
                    if (value.Text.Contains(expectedStation.Station))
                    {
                        station = value.Text;
                        Console.WriteLine("Station has been found in the data grid: " + station);
                        //Assert.Pass("Station has been found in the data grid.");
                    }
                    else if(value.Text.Contains(expectedStation.Affiliate))
                    {
                        affiliate = value.Text;
                        Console.WriteLine("Affiliate has been verified for this station: " + affiliate);
                    }
                    else if (value.Text.Contains(expectedStation.Market))
                    {
                        market = value.Text;
                        Console.WriteLine("Market has been verified for this station: " + market);
                    }
                    else if (value.Text.Contains(expectedStation.RateDataThrough))
                    {
                        rateDataThrough = value.Text;
                        Console.WriteLine("Rate Data Through has been verified for this station: " + rateDataThrough);
                    }
                    else if (value.Text.Contains(expectedStation.LastUpdate))
                    {
                        lastUpdate = value.Text;
                        Console.WriteLine("Last Update has been verified for this station: " + lastUpdate);
                    }
                }

            }

            if (!station.Contains(expectedStation.Station))
            {
                Console.WriteLine("Station record was not found in the data grid: " + expectedStation.Station);
                Assert.Fail("Station record was not found in the data grid.");
            }
            else if (!affiliate.Contains(expectedStation.Affiliate))
            {
                Console.WriteLine("Affiliate was not found in this record: " + expectedStation.Affiliate);
                Assert.Fail("Affiliate was not found in in this record.");
            }
            else if (!market.Contains(expectedStation.Market))
            {
                Console.WriteLine("Market was not found in this record: " + expectedStation.Market);
                Assert.Fail("Market was not found in in this record.");
            }
            else if (!rateDataThrough.Contains(expectedStation.RateDataThrough))
            {
                Console.WriteLine("Rate Data Through was not found in this record: ", expectedStation.RateDataThrough);
                Assert.Fail("Rate Data Through was not found in this record.");
            }
            else if (!lastUpdate.Contains(expectedStation.LastUpdate))
            {
                Console.WriteLine("Last update was not found in this record: ", expectedStation.LastUpdate);
                Assert.Fail("Rate Data Through was not found in in this record.");
            }


        }

        //Select station from search results

    }
}
