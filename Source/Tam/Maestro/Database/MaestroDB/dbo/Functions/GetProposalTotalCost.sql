
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
-- SELECT dbo.GetProposalTotalCost(23483)
CREATE FUNCTION [dbo].[GetProposalTotalCost]
(
	@proposal_id INT
)
RETURNS MONEY
AS
BEGIN
	DECLARE @return AS MONEY
	
	SET @return = 0.0
	
	SET @return = (
		SELECT
			SUM(pd.proposal_rate * CAST(pd.num_spots AS MONEY))
		FROM
			proposal_details pd (NOLOCK)
		WHERE
			pd.proposal_id=@proposal_id
	)
	
	RETURN @return
END

