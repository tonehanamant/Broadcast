
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_RCS_GetNetworkRateCardBooksByQuarter 2,2009,3
CREATE PROCEDURE [dbo].[usp_RCS_GetNetworkRateCardBooksByQuarter]
	@sales_model_id INT,
	@year INT,
	@quarter INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		nrcb.*
	FROM
		network_rate_card_books nrcb (NOLOCK)
	WHERE
		nrcb.sales_model_id=@sales_model_id
		AND nrcb.[year]=@year
		AND nrcb.[quarter]=@quarter
		AND nrcb.media_month_id IS NULL
END

