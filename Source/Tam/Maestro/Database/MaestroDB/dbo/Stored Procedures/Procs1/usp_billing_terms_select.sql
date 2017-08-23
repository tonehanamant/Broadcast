CREATE PROCEDURE [dbo].[usp_billing_terms_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	dbo.billing_terms WITH(NOLOCK)
WHERE
	id = @id
