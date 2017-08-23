Feature: RateCardSmokeTest

@BCOP53 @AUTOTEST_BCOP284_STEP4 
Scenario: Verify preloaded station data
	Given I am on the Rate Card screen
	When Rate Data finishes loading
	Then I should see the following station data in Rate Management table
	 | Station  | Affiliate | Market                     | Rate Data Through | Last Update |
	 | AZSD     | AZA       | San Diego                  | 00                | 00          |
	 | KRTV 3.1 | CBS       | Great Falls                | 00                | 00          |
	 | XHOR     | I-S       | Harlingen-Wslco-Brnsvl-Mca | 00                | 00          |


