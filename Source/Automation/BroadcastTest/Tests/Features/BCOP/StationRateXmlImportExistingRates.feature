Feature: StationRateXmlImportExistingRates
	Once basic XML Import has been verified to be functional by running tests in StationRateXmlImport.feature, 
	let's generate XML files to verify that special cases are validated correctly by the UI import parser.

#Generate and Upload XML file with an existing rate, then verify the error message appears
@ignore
Scenario: Upload generated XML file that has no rates
	Given I am on the Rate Card screen with rate data loaded successfully
	When I generate new xml file containing the following rates for station 'WPCH-TV 17.1'
	 | Air Time | Program | Genre | Rate 30 | Rate 15 | HH Impressions | Rating | Flight |
	 | M-F 9AM-10AM | CRIME WATCH DAILY |      | $125.00 | -       | 5              | 0.20   | 2016/09/26 - 2016/12/25 |
	And Upload is not completed successfully
	Then I receive the following message in the pop up modal dialog
	| ModalTitle | Message                 |
	| Error      | Unable to load XML file. The XML file selected has already been loaded. |