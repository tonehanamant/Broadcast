
CREATE PROCEDURE [dbo].[usp_RCS_GetNetworkRateCardBookBypNetworkRateCardId]
	@network_rate_card_id INT
AS
BEGIN
	SELECT
		nrcb.*
	FROM
		network_rate_card_books nrcb (NOLOCK)
		JOIN network_rate_cards nrc (NOLOCK) ON nrc.network_rate_card_book_id=nrcb.id
			AND nrc.id=@network_rate_card_id	
END
