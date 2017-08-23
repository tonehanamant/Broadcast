CREATE PROCEDURE usp_network_break_histories_delete
(
	@network_break_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	network_break_histories
WHERE
	network_break_id = @network_break_id
 AND
	start_date = @start_date
