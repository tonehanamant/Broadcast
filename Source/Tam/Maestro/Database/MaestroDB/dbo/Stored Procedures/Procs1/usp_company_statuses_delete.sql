CREATE PROCEDURE usp_company_statuses_delete
(
	@id Int
)
AS
DELETE FROM company_statuses WHERE id=@id
