-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProposalDetailVPVH]
(
	@proposal_detail_id INT,
	@audience_id INT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return AS FLOAT

	DECLARE @hh_delivery AS FLOAT
	DECLARE @demo_delivery AS FLOAT

	SET @return = 0.0

	SET @hh_delivery	= dbo.GetProposalDetailDelivery(@proposal_detail_id,31)
	SET @demo_delivery	= dbo.GetProposalDetailDelivery(@proposal_detail_id,@audience_id)
	
	IF (@hh_delivery > 0.0)
		SET @return = (@demo_delivery / @hh_delivery)
	
	RETURN @return
END
