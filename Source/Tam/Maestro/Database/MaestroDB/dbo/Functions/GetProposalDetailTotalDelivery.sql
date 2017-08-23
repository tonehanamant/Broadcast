-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProposalDetailTotalDelivery]
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
			CAST(pd.num_spots AS FLOAT) * dbo.GetProposalDetailDelivery(@proposal_detail_id,@audience_id)		
		FROM
			proposal_details pd (NOLOCK)
		WHERE
			pd.id=@proposal_detail_id
	)
	
	RETURN @return
END
