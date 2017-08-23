CREATE PROCEDURE usp_proposal_details_insert
(
	@id		Int		OUTPUT,
	@proposal_id		Int,
	@spot_length_id		Int,
	@network_id		Int,
	@daypart_id		Int,
	@ratings_daypart_id		Int,
	@num_spots		Int,
	@proposal_rate		Money,
	@rate_card_rate		Money,
	@start_date		DateTime,
	@end_date		DateTime,
	@topography_universe		Float,
	@universal_scaling_factor		Float,
	@include		Bit,
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
INSERT INTO proposal_details
(
	proposal_id,
	spot_length_id,
	network_id,
	daypart_id,
	ratings_daypart_id,
	num_spots,
	proposal_rate,
	rate_card_rate,
	start_date,
	end_date,
	topography_universe,
	universal_scaling_factor,
	include,
	date_created,
	date_last_modified
)
VALUES
(
	@proposal_id,
	@spot_length_id,
	@network_id,
	@daypart_id,
	@ratings_daypart_id,
	@num_spots,
	@proposal_rate,
	@rate_card_rate,
	@start_date,
	@end_date,
	@topography_universe,
	@universal_scaling_factor,
	@include,
	@date_created,
	@date_last_modified
)

SELECT
	@id = SCOPE_IDENTITY()

