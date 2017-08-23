
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/2/2011
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetNetworkRateCardRates]
	@network_rate_card_book_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		nrcr.*
	FROM 
		network_rate_card_rates nrcr (NOLOCK)
	WHERE
		nrcr.network_rate_card_detail_id IN (
			SELECT id FROM network_rate_card_details (NOLOCK) WHERE network_rate_card_id IN (
				SELECT id FROM network_rate_cards (NOLOCK) WHERE network_rate_card_book_id=@network_rate_card_book_id
			)
		)
END

