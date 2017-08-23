CREATE PROCEDURE [dbo].[usp_tam_post_network_details_update]
(
	@id		Int,
	@enabled		Bit,
	@tam_post_proposal_id		Int,
	@network_id		Int,
	@subscribers		BigInt,
	@units		Float,
	@rounded_units		Int,
	@total_spots		Int
)
AS
UPDATE tam_post_network_details SET
	enabled = @enabled,
	tam_post_proposal_id = @tam_post_proposal_id,
	network_id = @network_id,
	subscribers = @subscribers,
	units = @units,
	rounded_units = @rounded_units,
	total_spots = @total_spots
WHERE
	id = @id
