CREATE PROCEDURE usp_network_histories_delete
(
	@network_id		Int,
	@start_date		DateTime)
AS
BEGIN
DELETE FROM
	dbo.network_histories
WHERE
	network_id = @network_id
	AND
	start_date = @start_date
END
