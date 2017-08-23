
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetNetworkIdsForLatestHispanicAndCableRateCardBook]
AS
BEGIN
	SELECT
		DISTINCT network_id
	FROM
		network_rate_card_details (NOLOCK)
		JOIN network_rate_cards (NOLOCK) ON network_rate_cards.id=network_rate_card_details.network_rate_card_id
END
