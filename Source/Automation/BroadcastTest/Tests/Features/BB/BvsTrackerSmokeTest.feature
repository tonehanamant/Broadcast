@ignore
Feature: BvsTrackerSmokeTest

@ignore
Scenario: Schedule Data has been preloaded
	Given I am on the BVS Tracker screen
	When Schedule Data finishes loading
	Then I should see the following Schedule data
	| Schedule  | Advertiser | Estimate | Start Date | End Date | Spots Booked | Spots Delivered | Out of Spec | Posting Book | Primary Demo Booked Imp. |  Primary Demo Delivered Imp. | 


##From Broadcast
@ignore
Scenario: Validate all station rates using Excel
	Given I am on the BVS Tracker screen
	Then I validate all station rates in UI using the Excel file for station 'WPCH-TV 17.1'
	| FilePath | FileName |
	| C:\Users\bbernstein\Documents\Visual Studio 2013\Projects\BroadcastAutoTest\BroadcastTest\Test Data\Rate Data\BCOP-52 | Atlanta.WPCH.SYN 4Q16.xls |
	
@ignore
Scenario: Upload and validate all station rates using Excel
	Given I am on the Rate Card screen with rate data loaded successfully
	When I upload xml file containing new rate data
	| FilePath | FileName |
	| C:\Users\bbernstein\Documents\Visual Studio 2013\Projects\BroadcastAutoTest\BroadcastTest\Test Data\Rate Data\BCOP-52 | Atlanta.WPCH.SYN.avWPCHTV8317325_1.xml |
	And Upload completed successfully
	Then I validate all station rates in UI using the Excel file
	| FilePath | FileName |
	| C:\Users\bbernstein\Documents\Visual Studio 2013\Projects\BroadcastAutoTest\BroadcastTest\Test Data\Rate Data\BCOP-52 | Atlanta.WPCH.SYN 4Q16.xls |
