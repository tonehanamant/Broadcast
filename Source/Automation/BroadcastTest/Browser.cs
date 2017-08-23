using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Support;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Chrome;
using TechTalk.SpecFlow;
using NUnit.Framework;


namespace BroadcastTest
{
    [Binding]
    public static class Browser 
    {

        /*
         public static IWebDriver Current
         {
           get
           {
              if (!ScenarioContext.Current.ContainsKey("browser"))
              {
                  //Select IE browser
                  ScenarioContext.Current["browser"] = new InternetExplorerDriver();

                  //Select Firefox browser
                  //ScenarioContext.Current["browser"] = new FirefoxDriver();

                  //Select Chrome browser
                  //ScenarioContext.Current["browser"] = new ChromeDriver();
              }
              return (IWebDriver)ScenarioContext.Current["browser"];
           }
          }
    
        public IWebDriver driver;
        public StringBuilder verificationErrors;

        public Browser()
        {
            //driver = new FirefoxDriver(); //replace with required driver
            driver = new InternetExplorerDriver();
            //driver = new ChromeDriver();
          //driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(5)); //Get wait from config
            verificationErrors = new StringBuilder();
        }

        public IWebElement FindElementWithWait(By by, int secondsToWait = 10)
        {
            var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(secondsToWait));
            return wait.Until(d => d.FindElement(by));
        }

        public void RefreshPage() { }

        public void Navigate(){ }

 
        public void Close()
        {
            try
            {
                driver.Close();
                driver.Dispose();
            }
            catch
            {
                Assert.Fail("Browser failed to tear down");
            }
        }

         // This is assumed to live in a class that has access to the active `WebDriver` instance through `WebDriver` field/property. 
        public void WaitForAjax(int secondsToWait = 10)
        {
            var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(secondsToWait));
            wait.Until(d => (bool)((IJavaScriptExecutor)d).ExecuteScript("return jQuery.active == 0"));
        }

        // Allow execution of javascript commands in the browser. 
        public void executeJavascript(String script)
        {
        }

        public static void TakeScreenshot(this IWebDriver driver, string filename)
        {
            Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
            ss.SaveAsFile(filename, System.Drawing.Imaging.ImageFormat.Png);
        }

        */

        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => drv.FindElement(by));
            }
            return driver.FindElement(by);
        }

        public static ReadOnlyCollection<IWebElement> FindElements(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => (drv.FindElements(by).Count > 0) ? drv.FindElements(by) : null);
            }
            return driver.FindElements(by);
        }


    }
}
