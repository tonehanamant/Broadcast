-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/14/2014
-- Description:	
-- =============================================
CREATE FUNCTION udf_GetSalesModelFromTrafficId
(
	@traffic_id INT
)
RETURNS BIT
AS
BEGIN
	DECLARE @sales_model_id INT;
	SELECT
		@sales_model_id = MIN(psm.sales_model_id)
	FROM
		proposal_sales_models psm (NOLOCK)
		JOIN traffic_proposals tp (NOLOCK) ON tp.proposal_id=psm.proposal_id
			AND tp.traffic_id=@traffic_id;
	RETURN @sales_model_id;
END
