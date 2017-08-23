-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/4/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_RCS_LookupNetworkRateCard
	@network_rate_card_book_id INT,
	@daypart_id INT,
	@rate_card_type_id INT
AS
BEGIN
	SELECT
		nrc.*
	FROM
		dbo.network_rate_cards nrc (NOLOCK)
	WHERE
		nrc.network_rate_card_book_id=@network_rate_card_book_id
		AND nrc.daypart_id=@daypart_id
		AND nrc.rate_card_type_id=@rate_card_type_id
END
