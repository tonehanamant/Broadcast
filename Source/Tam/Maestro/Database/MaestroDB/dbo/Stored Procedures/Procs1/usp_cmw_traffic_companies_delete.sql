CREATE PROCEDURE usp_cmw_traffic_companies_delete
(
	@id Int
)
AS
DELETE FROM cmw_traffic_companies WHERE id=@id
