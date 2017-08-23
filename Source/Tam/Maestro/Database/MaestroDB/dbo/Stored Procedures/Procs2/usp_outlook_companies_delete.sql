CREATE PROCEDURE usp_outlook_companies_delete
(
	@id Int
)
AS
DELETE FROM outlook_companies WHERE id=@id
