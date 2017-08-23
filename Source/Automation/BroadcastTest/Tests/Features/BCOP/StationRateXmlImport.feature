Feature: RateCardXmlImport
	Ensure basic XML Import has been verified to be functional. 

#Specify only specific rates to validate after xml import
#Should be made to generate a new rate in new XML document and then upload new document
@BCOP52 @SMOKE
Scenario: Upload station rate data 
	Given I am on the Rate Card screen with rate data loaded successfully
	When I upload xml file containing new rate data
	| FilePath | FileName |
	| C:\Users\bbernstein\Documents\Visual Studio 2013\Projects\BroadcastAutoTest\BroadcastTest\Test Data\Rate Data\BCOP-52 | Atlanta.WPCH.SYN.avWPCHTV8317325_1.xml |
	And Upload completed successfully
	Then I should see the following rates for station 'WPCH-TV 17.1'
	 | Air Time     | Program           | Genre | Rate 30 | Rate 15 | HH Impressions | Rating | Flight				  |
	 | M-F 9AM-10AM | CRIME WATCH DAILY |      | $125.00 | -       | 5              | 0.20   | 2016/09/26 - 2016/12/25 |

#Use XML file to upload rates, then verify all uploaded rates in the UI
@BCOP52
Scenario: Upload and validate all imported station rates using XML
	Given I am on the Rate Card screen with rate data loaded successfully
	When I upload xml file containing new rate data
	| FilePath | FileName |
	| C:\Users\bbernstein\Documents\Visual Studio 2013\Projects\BroadcastAutoTest\BroadcastTest\Test Data\Rate Data\BCOP-52 | Atlanta.WPCH.SYN.avWPCHTV8317325_1.xml |
	And Upload completed successfully
	Then I validate all station rates for station 'WPCH-TV 17.1' in UI using the imported XML 

#XML File already uploaded, just verify UI data matches all XML record data
@regression @BCOP52 
Scenario: Validate all imported station rates using XML
	Given I am on the Rate Card screen with rate data loaded successfully
	When  I view the station rate modal for the station 'WPCH-TV 17.1'
	Then  I validate all station rates for this station in UI using the XML 
	| FilePath | FileName |
	| C:\Users\bbernstein\Documents\Visual Studio 2013\Projects\BroadcastAutoTest\BroadcastTest\Test Data\Rate Data\BCOP-52 | Atlanta.WPCH.SYN.avWPCHTV8317325_1.xml |



