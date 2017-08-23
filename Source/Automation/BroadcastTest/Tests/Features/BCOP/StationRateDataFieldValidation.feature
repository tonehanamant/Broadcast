Feature: StationRateDataFieldValidation

@BCOP67 @AUTOTEST_BCOP300_STEP1 
Scenario: Removing all rates shows field validation message
	Given I view the station rate modal for the station 'WPCH-TV 17.1'
	And I find the following rate to edit
	| Air Time     | Program           | Genre | Rate 30 | Rate 15 | HH Impressions | Rating | Flight                 |
	| M-F 9AM-10AM | CRIME WATCH DAILY |      | $125.00 | -       | 5.00              | 0.20   | 2016/09/26 - 2016/12/25 |
	When I remove all rate values
	Then I should receive a field validation error message
	| FieldName | Message            |
	| 30 Spot Cost   | One spot required. |
	| 15 Spot Cost   | One spot required. |



@BCOP67 @AUTOTEST_BCOP300_STEP2 
Scenario Outline: Effective date beyond end date of flight shows field validation message
	Given I view the station rate modal for the station 'WPCH-TV 17.1'
	And I find the following rate to edit
	| Air Time     | Program           | Genre | Rate 30 | Rate 15 | HH Impressions | Rating | Flight                  |
	| M-F 9AM-10AM | CRIME WATCH DAILY |       | $125.00 | -       | 5.00           | 0.20   | 2016/09/26 - 2016/12/25 |
	When I set <FieldName> to value <Value>
	Then I should receive a field validation error message
	| FieldName   | Message                          |
	| <FieldName> | Date is beyond the expected time | 

	Examples: 
	| FieldName      | Value      |
	| Effective Date | 2016/12/30 |
	| Effective Date | 2016/12/26 |
	| Effective Date | 2017/12/24 |

@BCOP67 @AUTOTEST_BCOP300_STEP3 
Scenario Outline: Effective date can only be set to a Monday
	Given I view the station rate modal for the station 'WPCH-TV 17.1'
	And I create new rate
	When I set <FieldName> to value <Value>
	Then The effective date should be set to the nearest monday <New Value>

	Examples: 
	| FieldName      | Value      | New Value  |
	| Effective Date | 2016/11/15 | 2016/11/14 |
	| Effective Date | 2017/01/27 | 2017/01/23 |
	| Effective Date | 2017/01/23 | 2017/01/23 |




@BCOP67 @AUTOTEST_BCOP300_STEP4 
Scenario Outline: Hiatus weeks can be set for new flight
	Given I view the station rate modal for the station 'WPCH-TV 17.1'
	And I create new rate
	When I set <Flight> 
	Then I can set these hiatus weeks for flight
	| Flight                  | Hiatus Week             |
	| 2016/11/07 - 2016/12/26 | 2016/09/26 - 2016/09/26 |
	| 2016/11/07 - 2016/12/26 | 2016/09/26 - 2016/09/26 |
	| 2016/11/07 - 2016/12/26 | 2016/09/26 - 2016/09/26 |
	| 2016/11/07 - 2016/12/26 | 2016/09/26 - 2016/09/26 |
	| 2016/11/07 - 2016/12/26 | 2016/09/26 - 2016/09/26 |
	| 2016/11/07 - 2016/12/26 | 2016/09/26 - 2016/09/26 |





