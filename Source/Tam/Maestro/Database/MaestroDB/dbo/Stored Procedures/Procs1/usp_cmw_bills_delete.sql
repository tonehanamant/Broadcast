CREATE PROCEDURE usp_cmw_bills_delete
(
	@id Int
)
AS
DELETE FROM cmw_bills WHERE id=@id
