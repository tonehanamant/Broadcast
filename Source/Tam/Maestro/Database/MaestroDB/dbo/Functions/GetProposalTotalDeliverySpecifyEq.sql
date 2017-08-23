-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 1/3/2012
-- Description:	<Description, ,>
-- =============================================
-- SELECT dbo.GetProposalTotalDeliverySpecifyEq(34866,31,0)
CREATE FUNCTION [dbo].[GetProposalTotalDeliverySpecifyEq]
(
	@proposal_id INT,
	@audience_id INT,
	@is_equivalized BIT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return AS FLOAT
	
	SET @return = (
		SELECT
			SUM(
				CAST(pd.num_spots AS FLOAT)
				*
				CASE @is_equivalized
					WHEN 1 THEN
						((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0) * sl.delivery_multiplier
					ELSE
						(pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0
				END
			)
		FROM
			proposal_detail_audiences pda (NOLOCK)
			JOIN proposal_details pd (NOLOCK) ON pd.id=pda.proposal_detail_id
			JOIN spot_lengths sl (NOLOCK) ON sl.id=pd.spot_length_id
		WHERE
			pda.audience_id=@audience_id
			AND pd.proposal_id=@proposal_id
	)
	
	RETURN @return
END
