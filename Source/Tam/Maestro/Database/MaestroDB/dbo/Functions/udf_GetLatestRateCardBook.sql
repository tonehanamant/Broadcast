
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns zone_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetLatestRateCardBook]
(	
	@salesModel as varchar(63)
)
RETURNS TABLE
AS
RETURN
(
	SELECT 
		TOP 1 network_rate_card_books.id network_rate_card_book_id
	FROM 
		network_rate_card_books (NOLOCK) 
		join sales_models (NOLOCK) ON
			sales_models.id = network_rate_card_books.sales_model_id
	WHERE 
		network_rate_card_books.date_approved IS NOT NULL 
		AND 
		@salesModel = sales_models.name
	ORDER BY
		network_rate_card_books.date_approved DESC
);
