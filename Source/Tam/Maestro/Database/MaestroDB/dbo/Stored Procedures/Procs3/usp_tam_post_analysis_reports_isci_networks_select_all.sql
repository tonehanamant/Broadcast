CREATE PROCEDURE usp_tam_post_analysis_reports_isci_networks_select_all
AS
SELECT
	*
FROM
	tam_post_analysis_reports_isci_networks WITH(NOLOCK)
