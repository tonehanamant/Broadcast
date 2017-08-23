Feature: AutomationSelfCheck
	Test the automation framework itself to ensure it is properly evaluating the application under test. 

	@ignore
Scenario: Test Date Conversion
 Given I convert flight date 'Dec25/16' to '2016/12/25'
