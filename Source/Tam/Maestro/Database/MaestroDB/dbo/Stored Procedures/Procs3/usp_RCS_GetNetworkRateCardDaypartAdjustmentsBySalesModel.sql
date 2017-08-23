-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 1/16/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_RCS_GetNetworkRateCardDaypartAdjustmentsBySalesModel
	@sales_model_id INT,
	@effective_date DATETIME
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		nrcda.*
	FROM
		dbo.network_rate_card_daypart_adjustments nrcda (NOLOCK)
	WHERE
		nrcda.sales_model_id=@sales_model_id
		AND @effective_date BETWEEN nrcda.start_date AND nrcda.end_date
END
