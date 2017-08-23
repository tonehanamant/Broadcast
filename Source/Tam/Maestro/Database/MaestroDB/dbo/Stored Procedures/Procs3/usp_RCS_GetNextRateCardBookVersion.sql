
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_RCS_GetNextRateCardBookVersion 2009,3,null,1
CREATE PROCEDURE [dbo].[usp_RCS_GetNextRateCardBookVersion]
	@year INT,
	@quarter INT,
	@media_month_id INT,
	@sales_model_id INT
AS
BEGIN
    SELECT
		MAX(version)
	FROM
		network_rate_card_books (NOLOCK)
	WHERE
		year=@year
		AND quarter=@quarter
		AND (@media_month_id IS NULL OR media_month_id=@media_month_id)
		AND sales_model_id=@sales_model_id
END

