
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_RCS_GetNetworkRateCards 1
CREATE PROCEDURE [dbo].[usp_RCS_GetNetworkRateCards]
	@network_rate_card_book_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		id,
		network_rate_card_book_id,
		rate_card_type_id,
		daypart_id
	FROM
		network_rate_cards (NOLOCK)
	WHERE
		network_rate_card_book_id=@network_rate_card_book_id
	ORDER BY
		id DESC
END

