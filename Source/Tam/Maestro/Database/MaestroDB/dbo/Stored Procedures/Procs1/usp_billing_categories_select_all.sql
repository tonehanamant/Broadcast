CREATE PROCEDURE usp_billing_categories_select_all
AS
SELECT
	*
FROM
	billing_categories WITH(NOLOCK)
