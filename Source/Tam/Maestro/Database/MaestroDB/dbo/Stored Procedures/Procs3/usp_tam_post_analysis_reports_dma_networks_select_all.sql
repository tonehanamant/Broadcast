CREATE PROCEDURE [dbo].[usp_tam_post_analysis_reports_dma_networks_select_all]
AS
SELECT
	*
FROM
	tam_post_analysis_reports_dma_networks WITH(NOLOCK)
