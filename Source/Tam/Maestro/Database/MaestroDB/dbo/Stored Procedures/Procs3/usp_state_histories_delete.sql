CREATE PROCEDURE usp_state_histories_delete
(
	@state_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	state_histories
WHERE
	state_id = @state_id
 AND
	start_date = @start_date
