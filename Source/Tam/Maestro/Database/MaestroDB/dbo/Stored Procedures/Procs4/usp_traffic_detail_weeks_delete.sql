CREATE PROCEDURE usp_traffic_detail_weeks_delete
(
	@id Int)
AS
DELETE FROM traffic_detail_weeks WHERE id=@id
