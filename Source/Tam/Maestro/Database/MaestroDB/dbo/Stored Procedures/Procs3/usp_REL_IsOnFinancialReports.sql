
CREATE Procedure [dbo].[usp_REL_IsOnFinancialReports]
    (
        @system_id int,
        @release_id int
    )
AS
	select is_on_financial_reports
	from release_system_settings
	where release_id = @release_id
	and system_id = @system_id

