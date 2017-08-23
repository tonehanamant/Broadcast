CREATE PROCEDURE usp_nielsen_suggested_rates_delete
(
	@id Int
)
AS
DELETE FROM nielsen_suggested_rates WHERE id=@id
