-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/14/2016
-- Description:	Calculates GRP for a proposal detail line.
-- =============================================
-- SELECT pd.id,pd.num_spots,dbo.GetProposalDetailGrp(pd.id) FROM proposal_details pd WHERE pd.proposal_id=66725
-- SELECT dbo.GetProposalAudienceNEQRating(66725,31)
CREATE FUNCTION [dbo].[GetProposalDetailGrp]
(
	@proposal_detail_id INT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return AS FLOAT
	SET @return = 0.0

	DECLARE @hh_eq_delivery AS FLOAT;
	DECLARE @audience_universe AS FLOAT;
	DECLARE @proposal_id AS INT;

	SELECT @proposal_id = pd.proposal_id FROM proposal_details pd (NOLOCK) WHERE pd.id=@proposal_detail_id;

	SELECT @audience_universe = pa.universe FROM proposal_audiences pa (NOLOCK) WHERE pa.proposal_id=@proposal_id AND pa.audience_id=31;

	SET @hh_eq_delivery = dbo.GetProposalDetailTotalDeliverySpecifyEq(@proposal_detail_id,31, 1)
	
	IF @audience_universe > 0
		SET @return = (@hh_eq_delivery / (@audience_universe / 1000.0)) * 100.0;
	
	RETURN @return
END