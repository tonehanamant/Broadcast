CREATE PROCEDURE usp_topography_state_histories_delete
(
	@topography_id		Int,
	@state_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	topography_state_histories
WHERE
	topography_id = @topography_id
 AND
	state_id = @state_id
 AND
	start_date = @start_date
