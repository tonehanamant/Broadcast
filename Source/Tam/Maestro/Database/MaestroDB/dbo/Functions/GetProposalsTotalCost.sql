

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
-- SELECT dbo.GetProposalTotalCosts('23483')
CREATE FUNCTION [dbo].[GetProposalsTotalCost]
(
	@proposal_ids VARCHAR(MAX)
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
			pd.proposal_id IN (
				SELECT id FROM dbo.SplitIntegers(@proposal_ids)
			)
	)
	
	RETURN @return
END


