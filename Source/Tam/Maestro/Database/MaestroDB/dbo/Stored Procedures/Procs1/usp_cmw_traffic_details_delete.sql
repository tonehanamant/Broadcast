CREATE PROCEDURE usp_cmw_traffic_details_delete
(
	@id Int
)
AS
DELETE FROM cmw_traffic_details WHERE id=@id
