-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 12/5/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetTopographiesByNetworkRateCardBook]
	@network_rate_card_book_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		t.*
	FROM
		dbo.network_rate_card_book_topographies nrcbt (NOLOCK)
		JOIN dbo.topographies t (NOLOCK) ON t.id=nrcbt.topography_id
	WHERE
		nrcbt.network_rate_card_book_id=@network_rate_card_book_id
END
