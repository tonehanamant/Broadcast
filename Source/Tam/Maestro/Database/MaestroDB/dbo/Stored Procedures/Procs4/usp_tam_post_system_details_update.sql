CREATE PROCEDURE [dbo].[usp_tam_post_system_details_update]
(
	@id		Int,
	@enabled		Bit,
	@tam_post_proposal_id		Int,
	@business_id		Int,
	@system_id		Int,
	@network_id		Int,
	@subscribers		BigInt,
	@units		Float,
	@total_spots		Int
)
AS
UPDATE tam_post_system_details SET
	enabled = @enabled,
	tam_post_proposal_id = @tam_post_proposal_id,
	business_id = @business_id,
	system_id = @system_id,
	network_id = @network_id,
	subscribers = @subscribers,
	units = @units,
	total_spots = @total_spots
WHERE
	id = @id
