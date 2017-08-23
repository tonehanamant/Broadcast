CREATE PROCEDURE [dbo].[usp_network_rate_card_daypart_adjustments_update]
(
	@id		Int,
	@sales_model_id		Int,
	@daypart_id		Int,
	@network_id		Int,
	@start_date		Date,
	@end_date		Date,
	@weight		Float
)
AS
UPDATE dbo.network_rate_card_daypart_adjustments SET
	sales_model_id = @sales_model_id,
	daypart_id = @daypart_id,
	network_id = @network_id,
	start_date = @start_date,
	end_date = @end_date,
	weight = @weight
WHERE
	id = @id
