-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/25/2010
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetDisplayProposals_ForSalesModel 1,NULL,NULL
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayProposals_ForSalesModel]
	@sales_model_id INT,
	@year INT,
	@quarter INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT DISTINCT
		dp.*
	FROM 
		uvw_display_proposals dp
		JOIN proposal_sales_models psm (NOLOCK) ON psm.proposal_id=dp.id AND psm.sales_model_id IN (SELECT id FROM dbo.udf_GetSalesModelsFromSalesModel(@sales_model_id))
		JOIN media_months mm (NOLOCK) ON (mm.start_date <= dp.end_date AND mm.end_date >= dp.start_date)
	WHERE
		dp.proposal_status_id<>7
		AND (@year IS NULL OR @year=mm.[year])
		AND (@quarter IS NULL OR @quarter=CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END)
	ORDER BY 
		dp.id DESC
END
