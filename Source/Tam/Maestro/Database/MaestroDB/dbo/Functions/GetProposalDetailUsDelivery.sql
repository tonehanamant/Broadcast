-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProposalDetailUsDelivery]
(
	@proposal_detail_id INT,
	@audience_id INT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return AS FLOAT
	
	SET @return = (
		SELECT
			CASE p.is_equivalized
				WHEN 1 THEN
					((pda.us_universe * pda.rating) / 1000.0) * sl.delivery_multiplier
				ELSE
					(pda.us_universe * pda.rating) / 1000.0
			END					
		FROM
			proposal_detail_audiences pda (NOLOCK)
			JOIN proposal_details pd (NOLOCK) ON pd.id=pda.proposal_detail_id
			JOIN proposals p (NOLOCK) ON p.id=pd.proposal_id
			JOIN spot_lengths sl (NOLOCK) ON sl.id=pd.spot_length_id
		WHERE
			pda.audience_id=@audience_id
			AND pda.proposal_detail_id=@proposal_detail_id
	)
	
	RETURN @return
END
