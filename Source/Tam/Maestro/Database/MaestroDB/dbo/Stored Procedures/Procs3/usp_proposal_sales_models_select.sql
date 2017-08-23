CREATE PROCEDURE usp_proposal_sales_models_select
(
	@proposal_id		Int,
	@sales_model_id		Int
)
AS
SELECT
	*
FROM
	proposal_sales_models WITH(NOLOCK)
WHERE
	proposal_id=@proposal_id
	AND
	sales_model_id=@sales_model_id

