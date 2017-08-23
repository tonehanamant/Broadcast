-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProposalAudienceTotalDelivery]
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
			SUM(dbo.GetProposalDetailTotalDelivery(pd.id,@audience_id))
		FROM
			proposal_details pd (NOLOCK)
		WHERE
			pd.proposal_id=@proposal_id
	)

	RETURN @return
END
