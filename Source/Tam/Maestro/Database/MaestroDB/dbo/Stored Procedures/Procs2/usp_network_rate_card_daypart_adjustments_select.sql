CREATE PROCEDURE [dbo].[usp_network_rate_card_daypart_adjustments_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	dbo.network_rate_card_daypart_adjustments WITH(NOLOCK)
WHERE
	id = @id
