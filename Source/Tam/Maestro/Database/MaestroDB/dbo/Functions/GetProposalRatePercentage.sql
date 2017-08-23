-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION GetProposalRatePercentage
(
	@proposal_id INT
)
RETURNS DECIMAL
AS
BEGIN
	DECLARE @return DECIMAL

	SET @return = 0

	IF (SELECT SUM(rate_card_rate * num_spots) FROM proposal_details (NOLOCK) WHERE proposal_id=@proposal_id) > 0
		BEGIN SET @return = ROUND((SELECT SUM(proposal_rate * num_spots) / SUM(rate_card_rate * num_spots) FROM proposal_details (NOLOCK) WHERE proposal_id=@proposal_id) * 100.0, 0) END

	RETURN @return
END
