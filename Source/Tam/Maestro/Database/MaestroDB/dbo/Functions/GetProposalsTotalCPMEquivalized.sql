
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProposalsTotalCPMEquivalized]
(
	@proposal_ids VARCHAR(MAX),
	@audience_id INT
)
RETURNS MONEY
AS
BEGIN
	DECLARE @return AS MONEY
	DECLARE @total_delivery AS FLOAT
	DECLARE @total_cost AS MONEY

	SET @total_delivery = dbo.GetProposalsTotalDeliveryEquivalized(@proposal_ids,@audience_id)
	SET @total_cost		= dbo.GetProposalsTotalCost(@proposal_ids)
	
	IF @total_delivery > 0.0
		SET @return = @total_cost / @total_delivery
	ELSE
		SET @return = 0.0
	
	RETURN @return
END

