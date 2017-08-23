-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/21/2013
-- Description:	Returns all years based on flight of proposals and sales model.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalYearsBySalesModel]
	@sales_model_id INT
AS
BEGIN
	SELECT DISTINCT 
		mm.[year]
	FROM (
		SELECT
			MIN(pf.start_date) 'start_date', 
			MAX(pf.end_date) 'end_date'
		FROM
			proposal_flights pf (NOLOCK)
			JOIN proposal_sales_models psm (NOLOCK) ON psm.proposal_id=pf.proposal_id
				AND psm.sales_model_id IN (SELECT id FROM dbo.udf_GetSalesModelsFromSalesModel(@sales_model_id))
	) x
	JOIN media_weeks mw (NOLOCK) ON (mw.start_date <= x.end_date AND mw.end_date >= x.start_date)
	JOIN media_months mm (NOLOCK) ON mm.id=mw.media_month_id
	ORDER BY
		mm.[year] DESC
END
