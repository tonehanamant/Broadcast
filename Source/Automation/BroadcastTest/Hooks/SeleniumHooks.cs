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
using OpenQA.Selenium.Remote;
using NUnit.Framework;
using TechTalk.SpecFlow;
using BroadcastTest.Helpers;


namespace BroadcastTest.Hooks
{
    [Binding]
    class SeleniumHooks
    {
        RemoteWebDriver driver;
 
        [BeforeScenario]
        public void BeforeScenario()
        {
            driver = new InternetExplorerDriver();
            ScenarioContext.Current["driver"] = driver;
            //driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(5));

            //Get scenario tags for logging to 
            //Get Test Cycle from app.config
           

        }

        [AfterScenario]
        public void AfterScenario()
        {
            //Check to see if there were any errors in the scenario and if there were, take a screenshot of the browser
            if (ScenarioContext.Current.TestError != null)
            {
                try
                {
                    //TODO: Only take screenshot if this error is related to the UI view. 
                    string scenarioName = (ScenarioContext.Current.ScenarioInfo.Title.ToString()).Trim();
                    Console.WriteLine("Saving screenshot.");
                    ScreenShot ss = new ScreenShot(driver);
                    ss.CaptureScreenshot(scenarioName);
                    ss.CapturePageSource(scenarioName);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to save screenshot. " + e.ToString());
                }

            }

            driver.Dispose();
        }
 


    }
}
