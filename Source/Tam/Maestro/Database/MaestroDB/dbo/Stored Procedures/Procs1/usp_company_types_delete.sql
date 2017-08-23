CREATE PROCEDURE usp_company_types_delete
(
	@id Int
)
AS
DELETE FROM company_types WHERE id=@id
