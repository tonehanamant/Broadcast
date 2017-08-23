-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION GetProposalSalesModel
(
	@proposal_id INT
)
RETURNS INT
AS
BEGIN
	DECLARE @return INT

	SET @return = (
		SELECT TOP 1 sales_model_id FROM proposal_sales_models (NOLOCK) WHERE proposal_id=@proposal_id
	)

	RETURN @return
END
