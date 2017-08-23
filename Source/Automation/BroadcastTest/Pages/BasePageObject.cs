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
    public class BasePageObject
    {
        public static IWebDriver driver; //Replace with overloaded web driver instance from SeleniumHooks.cs


    }
}
