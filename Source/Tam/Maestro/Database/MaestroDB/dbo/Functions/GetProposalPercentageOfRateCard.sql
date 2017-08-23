-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProposalPercentageOfRateCard]
(
	@proposal_id INT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @percentage FLOAT
	DECLARE @rate_card_total MONEY
	DECLARE @propsoal_total MONEY

	SELECT
		@rate_card_total = SUM(pd.num_spots * pd.rate_card_rate),
		@propsoal_total = SUM(pd.num_spots * pd.proposal_rate)
	FROM
		proposal_details pd (NOLOCK)
	WHERE
		pd.proposal_id=@proposal_id

	SET @percentage = 0
	
	IF (@rate_card_total > 0)
		SET @percentage = ROUND((@propsoal_total / @rate_card_total) * 100.0, 0)

	RETURN @percentage
END
