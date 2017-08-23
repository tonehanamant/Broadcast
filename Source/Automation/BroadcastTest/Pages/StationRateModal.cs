using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Chrome;
using NUnit.Framework;
using TechTalk.SpecFlow;
using BroadcastTest.Model;

namespace BroadcastTest.Pages
{
    class StationRateModal : BasePageObject
    {
        //Element Locators//

        [FindsBy(How = How.ClassName, Using = "modal_title")]
        public IWebElement ModalTitle { get; set; }

        [FindsBy(How = How.ClassName, Using = "close")]
        public IWebElement CloseButton { get; set; }

        [FindsBy(How = How.Id, Using = "broadcast_status")]
        public IWebElement StatusMessage { get; set; }

        [FindsBy(How = How.Id, Using = "rates_new_program_btn")]
        public IWebElement NewProgramButton { get; set; }

        [FindsBy(How = How.Id, Using = "rates_quarter_filter_input")]
        public IWebElement Filter { get; set; }

        [FindsBy(How = How.Id, Using = "stations_search_clear_btn")]
        public IWebElement ClearSearchButton { get; set; }

        [FindsBy(How = How.Id, Using = "grid_RatesGrid_records")]
        public IWebElement RatesGrid { get; set; }

        [FindsBy(How = How.Id, Using = "station_contacts_view")]
        public IWebElement StationContactsView { get; set; }

        [FindsBy(How = How.ClassName, Using = "btn-danger")]
        public IWebElement CancelButton { get; set; }

        [FindsBy(How = How.ClassName, Using = "btn-success")]
        public IWebElement SaveButton { get; set; }





        public class StationContacts { }


        public void CancelRateModal()
        {
            try
            {
                CancelButton.Click();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to save Station Modal due to exception: " + e);
            }
        }

        public void SaveRateModal()
        {
            try
            {
                SaveButton.Click();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to save Station Modal due to exception: " + e);
            }
        }

        public void CloseRateModal()
        {
            try
            {
                CloseButton.Click();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to close Station Rate modal due to exception: " + e);
            }
        }

    }
}
