

CREATE FUNCTION [dbo].[GetProposalDetailTotalDeliveryUnEQ]
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
			CAST(pd.num_spots AS FLOAT) * dbo.GetProposalDetailDeliveryUnEQ(@proposal_detail_id,@audience_id)		
		FROM
			proposal_details pd (NOLOCK)
		WHERE
			pd.id=@proposal_detail_id
	)
	
	RETURN @return
END
