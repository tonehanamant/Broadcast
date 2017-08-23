CREATE PROCEDURE usp_topography_state_histories_select
(
	@topography_id		Int,
	@state_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	topography_state_histories WITH(NOLOCK)
WHERE
	topography_id=@topography_id
	AND
	state_id=@state_id
	AND
	start_date=@start_date

