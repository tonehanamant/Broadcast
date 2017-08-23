CREATE PROCEDURE usp_billing_cycles_delete
(
	@id Int
)
AS
DELETE FROM billing_cycles WHERE id=@id
