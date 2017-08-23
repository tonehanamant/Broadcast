
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetNetworkRateCardBooksByMonth]
	@sales_model_id INT,
	@year INT,
	@media_month_id INT
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
		AND nrcb.media_month_id=@media_month_id
END

