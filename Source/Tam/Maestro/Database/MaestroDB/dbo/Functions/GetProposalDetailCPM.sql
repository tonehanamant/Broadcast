-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProposalDetailCPM]
(
	@proposal_detail_id INT,
	@audience_id INT
)
RETURNS MONEY
AS
BEGIN
	DECLARE @return AS MONEY
	DECLARE @delivery AS FLOAT

	SET @delivery = dbo.GetProposalDetailDelivery(@proposal_detail_id,@audience_id)
	
	SET @return = (
		SELECT
			CASE 
				WHEN @delivery > 0.0 THEN
					CAST(pd.proposal_rate / @delivery AS MONEY)
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
