﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.1.0.0
//      SpecFlow Generator Version:2.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace BroadcastTest.Tests.Features.BCOP
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("StationRateXmlImportSpecialCases")]
    public partial class StationRateXmlImportSpecialCasesFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "StationRateXmlImportSpecialCases.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "StationRateXmlImportSpecialCases", "\tOnce basic XML Import has been verified to be functional by running tests in Sta" +
                    "tionRateXmlImport.feature, \r\n\tlet\'s generate XML files to verify that special ca" +
                    "ses are validated correctly by the UI import parser.", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Upload invalid XML schema file and verify import")]
        [NUnit.Framework.CategoryAttribute("BCOP82")]
        [NUnit.Framework.CategoryAttribute("AUTOTEST_BCOP159_STEP1")]
        public virtual void UploadInvalidXMLSchemaFileAndVerifyImport()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Upload invalid XML schema file and verify import", new string[] {
                        "BCOP82",
                        "AUTOTEST_BCOP159_STEP1"});
#line 7
this.ScenarioSetup(scenarioInfo);
#line 8
 testRunner.Given("I am on the Rate Card screen with rate data loaded successfully", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "FilePath",
                        "FileName"});
            table1.AddRow(new string[] {
                        "C:\\Users\\bbernstein\\Documents\\Visual Studio 2013\\Projects\\BroadcastAutoTest\\Broad" +
                            "castTest\\Test Data\\Rate Data\\BCOP-82",
                        "InvalidSchema.WPCH.SYN.avWPCHTV8317325.xml"});
#line 9
 testRunner.When("I upload xml file containing invalid rate data", ((string)(null)), table1, "When ");
#line 12
 testRunner.And("Upload is not completed successfully", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "ModalTitle",
                        "Message"});
            table2.AddRow(new string[] {
                        "Error",
                        "Not a valid import file"});
#line 13
 testRunner.Then("I receive the following message in the pop up modal dialog", ((string)(null)), table2, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Upload XML file that was previously imported successfully")]
        [NUnit.Framework.CategoryAttribute("BCOP82")]
        [NUnit.Framework.CategoryAttribute("AUTOTEST_BCOP159_STEP2")]
        public virtual void UploadXMLFileThatWasPreviouslyImportedSuccessfully()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Upload XML file that was previously imported successfully", new string[] {
                        "BCOP82",
                        "AUTOTEST_BCOP159_STEP2"});
#line 19
this.ScenarioSetup(scenarioInfo);
#line 20
 testRunner.Given("I am on the Rate Card screen with rate data loaded successfully", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "FilePath",
                        "FileName"});
            table3.AddRow(new string[] {
                        "C:\\Users\\bbernstein\\Documents\\Visual Studio 2013\\Projects\\BroadcastAutoTest\\Broad" +
                            "castTest\\Test Data\\Rate Data\\BCOP-82",
                        "RatesExist.WPCH.SYN.avWPCHTV8317325.xml"});
#line 21
 testRunner.When("I upload xml file containing new rate data", ((string)(null)), table3, "When ");
#line 24
 testRunner.And("Upload is not completed successfully", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "ModalTitle",
                        "Message"});
            table4.AddRow(new string[] {
                        "Error",
                        "Unable to load XML file. The XML file selected has already been loaded."});
#line 25
 testRunner.Then("I receive the following message in the pop up modal dialog", ((string)(null)), table4, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Upload generated XML file that has invalid station name")]
        [NUnit.Framework.CategoryAttribute("BCOP82")]
        [NUnit.Framework.CategoryAttribute("AUTOTEST_BCOP159_STEP3")]
        public virtual void UploadGeneratedXMLFileThatHasInvalidStationName()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Upload generated XML file that has invalid station name", new string[] {
                        "BCOP82",
                        "AUTOTEST_BCOP159_STEP3"});
#line 32
this.ScenarioSetup(scenarioInfo);
#line 33
 testRunner.Given("I am on the Rate Card screen with rate data loaded successfully", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Air Time",
                        "Program",
                        "Genre",
                        "Rate 30",
                        "Rate 15",
                        "HH Impressions",
                        "Rating",
                        "Flight"});
            table5.AddRow(new string[] {
                        "M-F 9AM-10AM",
                        "CRIME WATCH DAILY",
                        "",
                        "$125.00",
                        "-",
                        "5",
                        "0.20",
                        "2016/09/26 - 2016/12/25"});
#line 34
 testRunner.When("I generate new xml file containing the following rates for station \'InvalidStatio" +
                    "nName\'", ((string)(null)), table5, "When ");
#line 37
 testRunner.And("Upload is not completed successfully", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "ModalTitle",
                        "Message"});
            table6.AddRow(new string[] {
                        "Error",
                        "Unable to load XML file. The XML file selected has already been loaded."});
#line 38
 testRunner.Then("I receive the following message in the pop up modal dialog", ((string)(null)), table6, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Upload generated XML file that has no rates for program")]
        [NUnit.Framework.CategoryAttribute("BCOP-82")]
        [NUnit.Framework.CategoryAttribute("AUTOTEST_BCOP159_STEP4")]
        public virtual void UploadGeneratedXMLFileThatHasNoRatesForProgram()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Upload generated XML file that has no rates for program", new string[] {
                        "BCOP-82",
                        "AUTOTEST_BCOP159_STEP4"});
#line 44
this.ScenarioSetup(scenarioInfo);
#line 45
 testRunner.Given("I am on the Rate Card screen with rate data loaded successfully", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Air Time",
                        "Program",
                        "Genre",
                        "Rate 30",
                        "Rate 15",
                        "HH Impressions",
                        "Rating",
                        "Flight"});
            table7.AddRow(new string[] {
                        "9-10AM",
                        "CRIME WATCH DAILY",
                        "",
                        "-",
                        "-",
                        "0.5",
                        "50",
                        "2016/09/26 - 2016/12/25"});
#line 46
 testRunner.When("I generate new xml file containing the following rates for station \'WPCH-TV 17.1\'" +
                    "", ((string)(null)), table7, "When ");
#line 49
 testRunner.And("Upload is not completed successfully", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "ModalTitle",
                        "Message"});
            table8.AddRow(new string[] {
                        "Error",
                        "Unable to load XML file. The XML file selected has already been loaded."});
#line 50
 testRunner.Then("I receive the following message in the pop up modal dialog", ((string)(null)), table8, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Upload generated XML file that has invalid rates")]
        [NUnit.Framework.CategoryAttribute("BCOP-82")]
        [NUnit.Framework.CategoryAttribute("AUTOTEST_BCOP159_STEP5")]
        public virtual void UploadGeneratedXMLFileThatHasInvalidRates()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Upload generated XML file that has invalid rates", new string[] {
                        "BCOP-82",
                        "AUTOTEST_BCOP159_STEP5"});
#line 56
this.ScenarioSetup(scenarioInfo);
#line 57
 testRunner.Given("I am on the Rate Card screen with rate data loaded successfully", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "Air Time",
                        "Program",
                        "Genre",
                        "Rate 30",
                        "Rate 15",
                        "HH Impressions",
                        "Rating",
                        "Flight"});
            table9.AddRow(new string[] {
                        "9-10AM",
                        "CRIME WATCH DAILY",
                        "",
                        "125",
                        "-",
                        "0.5",
                        "50",
                        "2016/09/26 - 2016/12/25"});
#line 58
 testRunner.When("I generate new xml file containing the following rates for station \'WPCH-TV 17.1\'" +
                    "", ((string)(null)), table9, "When ");
#line 61
 testRunner.And("Upload is not completed successfully", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "ModalTitle",
                        "Message"});
            table10.AddRow(new string[] {
                        "Error",
                        "Unable to load XML file. The XML file selected has already been loaded."});
#line 62
 testRunner.Then("I receive the following message in the pop up modal dialog", ((string)(null)), table10, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
