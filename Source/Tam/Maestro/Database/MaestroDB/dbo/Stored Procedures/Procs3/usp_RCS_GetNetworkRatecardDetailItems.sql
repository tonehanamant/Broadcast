-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Modified:	1/17/2011
-- Description:	Retrieves a list of networks.
-- Changes:		1/17/2011	Added @effective_date variable to use in the joining of networks to get the appropriate code.
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetNetworkRatecardDetailItems]
	@network_rate_card_id INT
AS
BEGIN
	DECLARE @effective_date DATETIME
	SET @effective_date = (
		SELECT 
			MIN(mm.start_date) 
		FROM 
			network_rate_cards nrc (NOLOCK)
			JOIN network_rate_card_books nrcb (NOLOCK) ON nrcb.id=nrc.network_rate_card_book_id
			JOIN media_months mm (NOLOCK) ON mm.[year]=nrcb.[year] 
				AND CASE mm.[month] WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END = nrcb.quarter
				AND (nrcb.media_month_id IS NULL OR mm.id=nrcb.media_month_id)
		WHERE
			nrc.id=@network_rate_card_id
	)
	
    SELECT 
		n.network_id,
		n.code
	FROM 
		network_rate_card_details nrcd (NOLOCK)
		JOIN network_rate_cards nrc (NOLOCK) ON nrc.id=nrcd.network_rate_card_id
		JOIN network_rate_card_books nrcb (NOLOCK) ON nrcb.id=nrc.network_rate_card_book_id
		JOIN uvw_network_universe n (NOLOCK) ON n.network_id=nrcd.network_id AND (n.start_date<=@effective_date AND (n.end_date>=@effective_date OR n.end_date IS NULL))
	WHERE
		nrcd.network_rate_card_id=@network_rate_card_id
END



/****** Object:  StoredProcedure [dbo].[usp_RCS_GetDisplayNetworkRateCardDetails]    Script Date: 01/17/2011 09:47:22 ******/
SET ANSI_NULLS ON
