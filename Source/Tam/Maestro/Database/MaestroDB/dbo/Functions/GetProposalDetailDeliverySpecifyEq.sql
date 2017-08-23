-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/5/2015
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProposalDetailDeliverySpecifyEq]
(
	@proposal_detail_id INT,
	@audience_id INT,
	@is_equivalized BIT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return AS FLOAT
	
	SET @return = (
		SELECT
			((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0) * CASE @is_equivalized WHEN 1 THEN sl.delivery_multiplier ELSE 1 END
		FROM
			proposal_detail_audiences pda (NOLOCK)
			JOIN proposal_details pd (NOLOCK) ON pd.id=pda.proposal_detail_id
			JOIN spot_lengths sl (NOLOCK) ON sl.id=pd.spot_length_id
		WHERE
			pda.audience_id=@audience_id
			AND pda.proposal_detail_id=@proposal_detail_id
	)
	
	RETURN @return
END