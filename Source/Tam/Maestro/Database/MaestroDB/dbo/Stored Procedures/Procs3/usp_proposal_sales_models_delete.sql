CREATE PROCEDURE usp_proposal_sales_models_delete
(
	@proposal_id		Int,
	@sales_model_id		Int)
AS
DELETE FROM
	proposal_sales_models
WHERE
	proposal_id = @proposal_id
 AND
	sales_model_id = @sales_model_id
