CREATE PROCEDURE usp_topography_states_select
(
	@topography_id		Int,
	@state_id		Int
)
AS
SELECT
	*
FROM
	topography_states WITH(NOLOCK)
WHERE
	topography_id=@topography_id
	AND
	state_id=@state_id

