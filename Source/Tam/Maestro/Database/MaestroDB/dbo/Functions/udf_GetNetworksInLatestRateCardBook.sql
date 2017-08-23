
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns zone_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetNetworksInLatestRateCardBook]
(
	@salesModel as varchar(63)
)
RETURNS TABLE
AS
RETURN
(
	SELECT
		DISTINCT network_id
	FROM
		network_rate_card_details (NOLOCK)
		JOIN network_rate_cards (NOLOCK) ON 
			network_rate_cards.id = network_rate_card_details.network_rate_card_id
		JOIN dbo.udf_GetLatestRateCardBook(@salesModel) latest_rate_card_book on
			latest_rate_card_book.network_rate_card_book_id = network_rate_cards.network_rate_card_book_id
);
