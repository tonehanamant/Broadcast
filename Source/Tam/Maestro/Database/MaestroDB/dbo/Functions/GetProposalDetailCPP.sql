-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProposalDetailCPP]
(
	@proposal_detail_id INT,
	@audience_id INT
)
RETURNS MONEY
AS
BEGIN
	DECLARE @return AS MONEY
	DECLARE @neq_rating AS FLOAT

	SET @neq_rating = dbo.GetProposalDetailAudienceNEQRating(@proposal_detail_id,@audience_id)
	
	SET @return = (
		SELECT
			CASE 
				WHEN @neq_rating > 0.0 THEN
					CAST((pd.num_spots * pd.proposal_rate) / @neq_rating AS MONEY)
				ELSE
					0.0
			END
		FROM
			proposal_details pd (NOLOCK)
		WHERE
			pd.id=@proposal_detail_id
	)

	RETURN @return
END
