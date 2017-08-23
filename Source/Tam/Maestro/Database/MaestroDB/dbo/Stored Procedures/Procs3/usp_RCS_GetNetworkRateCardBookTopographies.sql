-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/6/2012
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetNetworkRateCardBookTopographies]
	@network_rate_card_book_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		nrcbt.*
	FROM 
		network_rate_card_book_topographies nrcbt (NOLOCK)
		JOIN topographies t (NOLOCK) ON t.id=nrcbt.topography_id
	WHERE
		nrcbt.network_rate_card_book_id=@network_rate_card_book_id
END
