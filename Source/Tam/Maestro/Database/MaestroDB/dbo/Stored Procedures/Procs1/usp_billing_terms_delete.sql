CREATE PROCEDURE [dbo].[usp_billing_terms_delete]
(
	@id Int
)
AS
DELETE FROM dbo.billing_terms WHERE id=@id
