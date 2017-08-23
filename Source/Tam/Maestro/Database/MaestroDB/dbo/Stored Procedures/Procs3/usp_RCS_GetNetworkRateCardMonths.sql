
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_RCS_GetNetworkRateCardMonths 1,2009
CREATE PROCEDURE [dbo].[usp_RCS_GetNetworkRateCardMonths]
	@sales_model_id INT,
	@year INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		DISTINCT 
			media_month_id,
			CASE media_months.month 
				WHEN 1 THEN 'January'
				WHEN 2 THEN 'February'
				WHEN 3 THEN 'March'
				WHEN 4 THEN 'April'
				WHEN 5 THEN 'May'
				WHEN 6 THEN 'June'
				WHEN 7 THEN 'July'
				WHEN 8 THEN 'August'
				WHEN 9 THEN 'September'
				WHEN 10 THEN 'October'
				WHEN 11 THEN 'November'
				WHEN 12 THEN 'December'
			END				
	FROM 
		network_rate_card_books (NOLOCK)
		JOIN media_months (NOLOCK) ON media_months.id=network_rate_card_books.media_month_id
	WHERE
		network_rate_card_books.sales_model_id=@sales_model_id
		AND network_rate_card_books.year=@year
		AND network_rate_card_books.media_month_id IS NOT NULL
	ORDER BY 
		media_month_id ASC
END

