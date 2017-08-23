CREATE PROCEDURE [dbo].[usp_network_rate_card_daypart_adjustments_select_all]
AS
SELECT
	*
FROM
	dbo.network_rate_card_daypart_adjustments WITH(NOLOCK)
