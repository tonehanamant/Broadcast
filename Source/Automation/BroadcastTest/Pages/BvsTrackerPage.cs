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


namespace BroadcastTest.Pages
{
    class BvsTrackerPage : BasePageObject
    {
        //Element Locators//
        [FindsBy(How = How.Id, Using = "broadcast_status")]
        public IWebElement Status { get; set; }

        [FindsBy(How = How.Id, Using = "uploadButton")]
        public IWebElement UploadButton { get; set; }

        [FindsBy(How = How.Id, Using = "quarter_filter_input")]
        public IWebElement QuarterFilter { get; set; }

        [FindsBy(How = How.Id, Using = "advertiser_filter_input")]
        public IWebElement AdvertiserFilter { get; set; }

        [FindsBy(How = How.Id, Using = "schedule_search_filter_input")]
        public IWebElement SearchBox { get; set; }


    }
}
