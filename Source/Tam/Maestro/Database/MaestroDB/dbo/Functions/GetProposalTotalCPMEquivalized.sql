-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProposalTotalCPMEquivalized]
(
	@proposal_id INT,
	@audience_id INT
)
RETURNS MONEY
AS
BEGIN
	DECLARE @return AS MONEY
	DECLARE @total_delivery AS FLOAT
	DECLARE @total_cost AS MONEY

	SET @total_delivery = dbo.GetProposalTotalDeliveryEquivalized(@proposal_id,@audience_id)
	SET @total_cost		= dbo.GetProposalTotalCost(@proposal_id)
	
	IF @total_delivery > 0.0
		SET @return = @total_cost / @total_delivery
	ELSE
		SET @return = 0.0
	
	RETURN @return
END
