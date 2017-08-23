CREATE PROCEDURE usp_topography_system_group_histories_delete
(
	@topography_id		Int,
	@system_group_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	topography_system_group_histories
WHERE
	topography_id = @topography_id
 AND
	system_group_id = @system_group_id
 AND
	start_date = @start_date
