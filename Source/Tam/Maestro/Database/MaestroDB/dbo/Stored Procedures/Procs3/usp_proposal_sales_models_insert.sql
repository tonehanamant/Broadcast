CREATE PROCEDURE usp_proposal_sales_models_insert
(
	@proposal_id		Int,
	@sales_model_id		Int
)
AS
INSERT INTO proposal_sales_models
(
	proposal_id,
	sales_model_id
)
VALUES
(
	@proposal_id,
	@sales_model_id
)

