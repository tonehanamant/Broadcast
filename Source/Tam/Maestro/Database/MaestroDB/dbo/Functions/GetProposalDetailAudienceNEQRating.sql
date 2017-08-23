-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProposalDetailAudienceNEQRating]
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
			CASE 
				WHEN pa.universe > 0.0 THEN
					(dbo.GetProposalDetailTotalDelivery(@proposal_detail_id,@audience_id) / (pa.universe / 1000.0)) * 100.0
				ELSE
					0.0
			END
		FROM
			proposal_audiences pa (NOLOCK)
		WHERE
			proposal_id=(SELECT proposal_id FROM proposal_details (NOLOCK) WHERE id=@proposal_detail_id)
			AND audience_id=@audience_id
	)
	
	RETURN @return
END
