CREATE PROCEDURE [dbo].[usp_tam_post_analysis_reports_iscis_select_all]
AS
SELECT
	*
FROM
	tam_post_analysis_reports_iscis WITH(NOLOCK)
