-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProposalAudienceNEQRating]
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
			CASE 
				WHEN pa.universe > 0.0 THEN
					(dbo.GetProposalAudienceTotalDelivery(@proposal_id,@audience_id) / (pa.universe / 1000.0)) * 100.0
				ELSE
					0.0
			END
		FROM
			proposal_audiences pa (NOLOCK)
		WHERE
			proposal_id=@proposal_id
			AND audience_id=@audience_id
	)

	RETURN @return
END
