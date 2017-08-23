-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/13/2010
-- Description:	
-- =============================================
-- usp_RCS_QueryNetworkRateCardDetailsByDate 1,getdate()
CREATE PROCEDURE [dbo].[usp_RCS_QueryNetworkRateCardDetailsByDate]
	@network_rate_card_id INT,
	@effective_date DATETIME
AS
BEGIN
    SELECT 
		n.network_id,
		n.code,
		nrcd.tier,
		nrcd.lock_rate,
		nrcd.hh_delivery,
		nrcd.minimum_cpm
	FROM 
		network_rate_card_details nrcd		(NOLOCK)
		JOIN network_rate_cards nrc			(NOLOCK) ON nrc.id=nrcd.network_rate_card_id
		JOIN network_rate_card_books nrcb	(NOLOCK) ON nrcb.id=nrc.network_rate_card_book_id
		JOIN uvw_network_universe n			(NOLOCK) ON n.network_id=nrcd.network_id AND (n.start_date<=@effective_date AND (n.end_date>=@effective_date OR n.end_date IS NULL))
	WHERE
		network_rate_card_id=@network_rate_card_id
	ORDER BY
		n.code
		
	SELECT
		nrcd.network_id,
		nrcr.spot_length_id,
		nrcr.rate
	FROM
		network_rate_card_rates nrcr (NOLOCK)
		JOIN network_rate_card_details nrcd (NOLOCK) ON nrcd.id=nrcr.network_rate_card_detail_id
			AND nrcd.network_rate_card_id=@network_rate_card_id
END
