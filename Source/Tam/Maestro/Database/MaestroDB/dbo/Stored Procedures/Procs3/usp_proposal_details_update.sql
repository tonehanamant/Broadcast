CREATE PROCEDURE usp_proposal_details_update
(
	@id		Int,
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
UPDATE proposal_details SET
	proposal_id = @proposal_id,
	spot_length_id = @spot_length_id,
	network_id = @network_id,
	daypart_id = @daypart_id,
	ratings_daypart_id = @ratings_daypart_id,
	num_spots = @num_spots,
	proposal_rate = @proposal_rate,
	rate_card_rate = @rate_card_rate,
	start_date = @start_date,
	end_date = @end_date,
	topography_universe = @topography_universe,
	universal_scaling_factor = @universal_scaling_factor,
	include = @include,
	date_created = @date_created,
	date_last_modified = @date_last_modified
WHERE
	id = @id

