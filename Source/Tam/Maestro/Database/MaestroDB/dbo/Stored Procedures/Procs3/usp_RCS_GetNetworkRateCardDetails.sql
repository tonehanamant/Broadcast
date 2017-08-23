-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/25/2011
-- Description:	
-- =============================================
-- usp_RCS_GetNetworkRateCardDetails 1
CREATE PROCEDURE [dbo].[usp_RCS_GetNetworkRateCardDetails]
	@network_rate_card_book_id INT
AS
BEGIN
    SELECT 
		nrcd.*
	FROM 
		network_rate_card_details nrcd (NOLOCK)
		JOIN network_rate_cards nrc (NOLOCK) ON nrc.id=nrcd.network_rate_card_id
			AND nrc.network_rate_card_book_id=@network_rate_card_book_id
END
