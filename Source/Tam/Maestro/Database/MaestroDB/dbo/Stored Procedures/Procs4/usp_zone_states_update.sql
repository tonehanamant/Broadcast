CREATE PROCEDURE usp_zone_states_update
(
	@zone_id		Int,
	@state_id		Int,
	@weight		Float,
	@effective_date		DateTime
)
AS
UPDATE zone_states SET
	weight = @weight,
	effective_date = @effective_date
WHERE
	zone_id = @zone_id AND
	state_id = @state_id
