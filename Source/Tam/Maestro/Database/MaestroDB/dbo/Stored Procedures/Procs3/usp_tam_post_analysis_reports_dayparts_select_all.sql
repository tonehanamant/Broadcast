-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/26/2014 08:25:53 AM
-- Description:	Auto-generated method to select all tam_post_analysis_reports_dayparts records.
-- =============================================
CREATE PROCEDURE [dbo].[usp_tam_post_analysis_reports_dayparts_select_all]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[tam_post_analysis_reports_dayparts].*
	FROM
		[dbo].[tam_post_analysis_reports_dayparts] WITH(NOLOCK)
END
