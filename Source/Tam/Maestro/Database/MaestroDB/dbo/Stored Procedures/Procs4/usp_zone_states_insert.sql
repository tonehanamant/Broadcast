CREATE PROCEDURE usp_zone_states_insert
(
	@zone_id		Int,
	@state_id		Int,
	@weight		Float,
	@effective_date		DateTime
)
AS
INSERT INTO zone_states
(
	zone_id,
	state_id,
	weight,
	effective_date
)
VALUES
(
	@zone_id,
	@state_id,
	@weight,
	@effective_date
)

