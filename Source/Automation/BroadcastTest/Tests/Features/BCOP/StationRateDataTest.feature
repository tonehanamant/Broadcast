Feature: StationRateDataTest

@BCOP53 @MANUAL_BCOP154
Scenario: Verify user can view rates for a station
	Given I view the station rate modal for the station 'WPCH-TV 17.1'
	When I view all rates
	Then I should see the following rates in the grid
	| Air Time     | Program           | Genre | Rate 30 | Rate 15 | HH Impressions | Rating | Flight                 |
	| M-F 9AM-10AM | CRIME WATCH DAILY |      | $15.00 | -       | 5.00              | 0.20   | 2016/09/26 - 2016/12/25 |
	| M-F 1PM-2PM  | JUDGE MABLEAN     |      | $10.00 | -       | 15.80             | 0.70   | 2016/09/26 - 2016/12/25 |


@BCOP67 @BCOP93 @MANUAL_BCOP165 @AUTOTEST_BCOP164_STEP4 
Scenario: Create rates for a station program 
	Given I view the station rate modal for the station 'WPCH-TV 17.1'
	When I create the rates 
	| Air Time     | Program           | Genre | Rate 30 | Rate 15 | HH Impressions | Rating | Flight                 |
	| M-F 9AM-10AM | CRIME WATCH DAILY |      | $125.00 | -       | 5.00              | 0.20   | 2016/09/26 - 2016/12/25 |
	## FOR BCOP-93 ONLY
	#And The new rate is saved successfully 
	#Then I should see the new rates in the grid

@BCOP129 @MANUAL_BCOP164 @AUTOTEST_BCOP164_STEP4 
Scenario: Delete rates for a station
	Given I view the station rate modal for the station 'WPCH-TV 17.1'
	When I delete the rates 
	| Air Time     | Program           | Genre | Rate 30 | Rate 15 | HH Impressions | Rating | Flight                 |
	| SA 1PM-1:30PM | LAST MAN STANDING	|      | $150.00 | -       | 9.20             | 0.40   | 2016/09/26 - 2016/12/25 |
	Then I should no longer see these rates in the grid
	| Air Time     | Program           | Genre | Rate 30 | Rate 15 | HH Impressions | Rating | Flight				  |
	| SA 1PM-1:30PM | LAST MAN STANDING	|      | $150.00 | -       | 9.20             | 0.40   | 2016/09/26 - 2016/12/25 |


@BCOP54 @BCOP124 @AUTOTEST_BCOP289_STEP1 
Scenario: Edit an existing rate for a station
	Given I view the station rate modal for the station 'WPCH-TV 17.1'
	And I find the following rate to edit
	| Air Time     | Program           | Genre | Rate 30 | Rate 15 | HH Impressions | Rating | Flight                 |
	| M-F 9AM-10AM | CRIME WATCH DAILY |      | $125.00 | -       | 5.00              | 0.20   | 2016/09/26 - 2016/12/25 |
	When I edit the rate to the following values
	| Air Time     | Program           | Genre | Rate 30 | Rate 15 | HH Impressions | Rating | Flight				  |
	| M-F 9AM-10AM | CRIME WATCH DAILY |   test   | $135.00 | $100.00       | 12.00              | 0.30   | 2016/09/26 - 2016/12/25 |
	#BCOP-124##############
	#And The edited rate is saved successfully 
	#Then The edited rate should appear in the list of rates for the station

