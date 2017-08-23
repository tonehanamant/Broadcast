CREATE PROCEDURE usp_topography_system_histories_delete
(
	@topography_id		Int,
	@system_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	topography_system_histories
WHERE
	topography_id = @topography_id
 AND
	system_id = @system_id
 AND
	start_date = @start_date
