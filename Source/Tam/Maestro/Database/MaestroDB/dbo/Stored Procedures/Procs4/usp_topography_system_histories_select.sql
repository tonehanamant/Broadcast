CREATE PROCEDURE usp_topography_system_histories_select
(
	@topography_id		Int,
	@system_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	topography_system_histories WITH(NOLOCK)
WHERE
	topography_id=@topography_id
	AND
	system_id=@system_id
	AND
	start_date=@start_date

