-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProposalDetailSubTotalCost]
(
	@proposal_detail_id INT
)
RETURNS MONEY
AS
BEGIN
	DECLARE @return AS MONEY
	
	SET @return = 0.0
	
	SET @return = (
		SELECT
			pd.proposal_rate * CAST(pd.num_spots AS MONEY)
		FROM
			proposal_details pd (NOLOCK)
		WHERE
			pd.id=@proposal_detail_id
	)
	
	RETURN @return
END
