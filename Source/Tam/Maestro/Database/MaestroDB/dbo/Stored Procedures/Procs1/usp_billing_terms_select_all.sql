CREATE PROCEDURE [dbo].[usp_billing_terms_select_all]
AS
SELECT
	*
FROM
	dbo.billing_terms WITH(NOLOCK)
