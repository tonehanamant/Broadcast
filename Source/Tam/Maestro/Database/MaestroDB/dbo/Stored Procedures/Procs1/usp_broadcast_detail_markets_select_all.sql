CREATE PROCEDURE [dbo].[usp_broadcast_detail_markets_select_all]
AS
SELECT
	*
FROM
	broadcast_detail_markets WITH(NOLOCK)

