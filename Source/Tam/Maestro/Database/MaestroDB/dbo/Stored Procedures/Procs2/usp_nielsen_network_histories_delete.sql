CREATE PROCEDURE usp_nielsen_network_histories_delete
(
	@nielsen_network_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	nielsen_network_histories
WHERE
	nielsen_network_id = @nielsen_network_id
 AND
	start_date = @start_date
