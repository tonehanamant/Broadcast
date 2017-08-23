using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support;
using OpenQA.Selenium.Remote;
using System.IO;
using System.Drawing.Imaging;
using System.Globalization;

namespace BroadcastTest.Helpers
{
    public class ScreenShot
    {

        public static string OutputFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FailedTests");

        private readonly RemoteWebDriver _webDriver;

        public ScreenShot(RemoteWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        public void CaptureScreenshot(string fileName = null)
        {
            var camera = (ITakesScreenshot)_webDriver;
            var screenshot = camera.GetScreenshot();

            var screenShotPath = GetOutputFilePath(fileName, "png");
            screenshot.SaveAsFile(screenShotPath, ImageFormat.Png);
        }

        public void CapturePageSource(string fileName = null)
        {
            var filePath = GetOutputFilePath(fileName, "html");
            File.WriteAllText(filePath, _webDriver.PageSource);
        }

        private string GetOutputFilePath(string fileName, string fileExtension)
        {
            if (!Directory.Exists(OutputFolder))
                Directory.CreateDirectory(OutputFolder);

            var windowTitle = _webDriver.Title;            
           
            // Creates a TextInfo based on the "en-US" culture.
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;

            // Changes a string to titlecase, then replace the spaces with empty
            string scenarioName = myTI.ToTitleCase(fileName).Replace(" ", "");

            fileName = string.Format("{0}{1}{2}.{3}", windowTitle, scenarioName, DateTime.Now.ToFileTime(), fileExtension).Replace(':', '.');
            var outputPath = Path.Combine(OutputFolder, fileName);
            var pathChars = Path.GetInvalidPathChars();
            var stringBuilder = new StringBuilder(outputPath);

            foreach (var item in pathChars)
                stringBuilder.Replace(item, '.');

            var screenShotPath = stringBuilder.ToString();
            Console.WriteLine("Saving Screenshot to:" + screenShotPath.ToString());
            return screenShotPath;
        }

    }
}
