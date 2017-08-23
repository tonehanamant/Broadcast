CREATE PROCEDURE usp_traffic_audiences_delete
(
	@traffic_id		Int,
	@audience_id		Int)
AS
DELETE FROM
	traffic_audiences
WHERE
	traffic_id = @traffic_id
 AND
	audience_id = @audience_id
