CREATE PROCEDURE usp_nielsen_suggested_rate_details_delete
(
	@id Int
)
AS
DELETE FROM nielsen_suggested_rate_details WHERE id=@id
