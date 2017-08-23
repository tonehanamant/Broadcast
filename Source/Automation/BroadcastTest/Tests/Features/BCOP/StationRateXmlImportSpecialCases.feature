Feature: StationRateXmlImportSpecialCases
	Once basic XML Import has been verified to be functional by running tests in StationRateXmlImport.feature, 
	let's generate XML files to verify that special cases are validated correctly by the UI import parser.

#Use invalid XML file to attempt to upload rates, then verify the error message appears
@BCOP82 @AUTOTEST_BCOP159_STEP1
Scenario: Upload invalid XML schema file and verify import
	Given I am on the Rate Card screen with rate data loaded successfully
	When I upload xml file containing invalid rate data
	| FilePath | FileName |
	| C:\Users\bbernstein\Documents\Visual Studio 2013\Projects\BroadcastAutoTest\BroadcastTest\Test Data\Rate Data\BCOP-82 | InvalidSchema.WPCH.SYN.avWPCHTV8317325.xml |
	And Upload is not completed successfully
	Then I receive the following message in the pop up modal dialog
	| ModalTitle | Message                 |
	| Error      | Not a valid import file |

#Use XML file to upload file that has already been uploaded, then verify the error message appears
@BCOP82 @AUTOTEST_BCOP159_STEP2
Scenario: Upload XML file that was previously imported successfully
	Given I am on the Rate Card screen with rate data loaded successfully
	When I upload xml file containing new rate data
	| FilePath | FileName |
	| C:\Users\bbernstein\Documents\Visual Studio 2013\Projects\BroadcastAutoTest\BroadcastTest\Test Data\Rate Data\BCOP-82 | RatesExist.WPCH.SYN.avWPCHTV8317325.xml |
	And Upload is not completed successfully
	Then I receive the following message in the pop up modal dialog
	| ModalTitle | Message                 |
	| Error      | Unable to load XML file. The XML file selected has already been loaded. |


#Generate and Upload XML file with invalid Station Data, then verify the error message appears
@BCOP82 @AUTOTEST_BCOP159_STEP3
Scenario: Upload generated XML file that has invalid station name
	Given I am on the Rate Card screen with rate data loaded successfully
	When I generate new xml file containing the following rates for station 'InvalidStationName'
	 | Air Time     | Program           | Genre | Rate 30 | Rate 15 | HH Impressions | Rating | Flight				  |
	 | M-F 9AM-10AM | CRIME WATCH DAILY |      | $125.00 | -       | 5              | 0.20   | 2016/09/26 - 2016/12/25 |
	And Upload is not completed successfully
	Then I receive the following message in the pop up modal dialog
	| ModalTitle | Message                 |
	| Error      | Unable to load XML file. The XML file selected has already been loaded. |

#Generate and Upload XML file with no Rates for Program, then verify the error message appears
@BCOP-82 @AUTOTEST_BCOP159_STEP4
Scenario: Upload generated XML file that has no rates for program
	Given I am on the Rate Card screen with rate data loaded successfully
	When I generate new xml file containing the following rates for station 'WPCH-TV 17.1'
	 | Air Time | Program | Genre | Rate 30 | Rate 15 | HH Impressions | Rating | Flight |
	 |  9-10AM | CRIME WATCH DAILY |      | - | -       | 0.5              | 50   | 2016/09/26 - 2016/12/25 |
	And Upload is not completed successfully
	Then I receive the following message in the pop up modal dialog
	| ModalTitle | Message                 |
	| Error      | Unable to load XML file. The XML file selected has already been loaded. |

#Generate and Upload XML file with invalid rate, then verify the error message appears
@BCOP-82 @AUTOTEST_BCOP159_STEP5
Scenario: Upload generated XML file that has invalid rates
	Given I am on the Rate Card screen with rate data loaded successfully
	When I generate new xml file containing the following rates for station 'WPCH-TV 17.1'
	 | Air Time | Program | Genre | Rate 30 | Rate 15 | HH Impressions | Rating | Flight |
	 | 9-10AM | CRIME WATCH DAILY |      | 125 | -       | 0.5              | 50   | 2016/09/26 - 2016/12/25 |
	And Upload is not completed successfully
	Then I receive the following message in the pop up modal dialog
	| ModalTitle | Message                 |
	| Error      | Unable to load XML file. The XML file selected has already been loaded. |



