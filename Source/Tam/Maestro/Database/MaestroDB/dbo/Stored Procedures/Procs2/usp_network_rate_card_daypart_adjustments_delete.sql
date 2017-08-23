CREATE PROCEDURE [dbo].[usp_network_rate_card_daypart_adjustments_delete]
(
	@id Int
)
AS
DELETE FROM dbo.network_rate_card_daypart_adjustments WHERE id=@id
