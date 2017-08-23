CREATE PROCEDURE usp_billing_categories_delete
(
	@id Int
)
AS
DELETE FROM billing_categories WHERE id=@id
