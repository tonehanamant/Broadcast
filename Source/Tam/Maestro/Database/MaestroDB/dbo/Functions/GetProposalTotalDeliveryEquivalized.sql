-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProposalTotalDeliveryEquivalized]
(
	@proposal_id INT,
	@audience_id INT
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
				((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0) * sl.delivery_multiplier
			)
		FROM
			proposal_detail_audiences pda (NOLOCK)
			JOIN proposal_details pd (NOLOCK) ON pd.id=pda.proposal_detail_id
			JOIN proposals p (NOLOCK) ON p.id=pd.proposal_id
			JOIN spot_lengths sl (NOLOCK) ON sl.id=pd.spot_length_id
		WHERE
			pda.audience_id=@audience_id
			AND pd.proposal_id=@proposal_id
	)
	
	RETURN @return
END
