CREATE PROCEDURE [dbo].[usp_tam_post_network_details_insert]
(
	@id		Int		OUTPUT,
	@enabled		Bit,
	@tam_post_proposal_id		Int,
	@network_id		Int,
	@subscribers		BigInt,
	@units		Float,
	@rounded_units		Int,
	@total_spots		Int
)
AS
INSERT INTO tam_post_network_details
(
	enabled,
	tam_post_proposal_id,
	network_id,
	subscribers,
	units,
	rounded_units,
	total_spots
)
VALUES
(
	@enabled,
	@tam_post_proposal_id,
	@network_id,
	@subscribers,
	@units,
	@rounded_units,
	@total_spots
)

SELECT
	@id = SCOPE_IDENTITY()
