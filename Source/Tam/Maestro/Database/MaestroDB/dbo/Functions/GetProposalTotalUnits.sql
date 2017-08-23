

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProposalTotalUnits]
(
	@proposal_id INT
)
RETURNS INT
AS
BEGIN
	DECLARE @return AS INT
	
	SET @return = 0
	
	SET @return = (
		SELECT
			SUM(pd.num_spots)
		FROM
			proposal_details pd (NOLOCK)
		WHERE
			pd.proposal_id=@proposal_id
			AND include=1
			AND num_spots>0
	)
	
	RETURN @return
END


