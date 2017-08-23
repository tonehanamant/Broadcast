CREATE PROCEDURE usp_cmw_traffic_delete
(
	@id Int
)
AS
DELETE FROM cmw_traffic WHERE id=@id
