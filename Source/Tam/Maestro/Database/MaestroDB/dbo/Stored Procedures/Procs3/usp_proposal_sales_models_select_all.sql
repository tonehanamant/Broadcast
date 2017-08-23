CREATE PROCEDURE usp_proposal_sales_models_select_all
AS
SELECT
	*
FROM
	proposal_sales_models WITH(NOLOCK)
