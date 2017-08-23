CREATE PROCEDURE [dbo].[usp_tam_post_system_details_insert]
(
	@id		Int		OUTPUT,
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
INSERT INTO tam_post_system_details
(
	enabled,
	tam_post_proposal_id,
	business_id,
	system_id,
	network_id,
	subscribers,
	units,
	total_spots
)
VALUES
(
	@enabled,
	@tam_post_proposal_id,
	@business_id,
	@system_id,
	@network_id,
	@subscribers,
	@units,
	@total_spots
)

SELECT
	@id = SCOPE_IDENTITY()
