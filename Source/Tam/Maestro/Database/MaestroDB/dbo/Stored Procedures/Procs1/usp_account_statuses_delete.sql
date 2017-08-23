CREATE PROCEDURE usp_account_statuses_delete
(
	@id Int
)
AS
DELETE FROM account_statuses WHERE id=@id
