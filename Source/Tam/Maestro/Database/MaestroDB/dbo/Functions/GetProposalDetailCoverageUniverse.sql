-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProposalDetailCoverageUniverse]
(
	@proposal_detail_id INT,
	@audience_id INT 
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return AS FLOAT
	
	SET @return = 0.0
	
	SET @return = (
		SELECT
			pda.us_universe * pd.universal_scaling_factor
		FROM
			proposal_detail_audiences pda (NOLOCK)
			JOIN proposal_details pd (NOLOCK) ON pd.id=pda.proposal_detail_id
		WHERE
			pda.proposal_detail_id=@proposal_detail_id
			AND pda.audience_id=@audience_id
	)
	
	RETURN @return
END
