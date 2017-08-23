CREATE PROCEDURE usp_zone_states_select
(
	@zone_id		Int,
	@state_id		Int
)
AS
SELECT
	*
FROM
	zone_states WITH(NOLOCK)
WHERE
	zone_id=@zone_id
	AND
	state_id=@state_id

