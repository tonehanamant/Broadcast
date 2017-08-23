-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetPrimaryDaypartOfProposal]
(
	@proposal_id INT
)
RETURNS INT
AS
BEGIN
	DECLARE @return INT
	
	SELECT TOP 1
		@return = pd.daypart_id
	FROM
		proposal_details pd (NOLOCK)
	WHERE
		pd.proposal_id=@proposal_id
	GROUP BY
		pd.daypart_id
	ORDER BY
		COUNT(*) DESC

	RETURN @return
END
