

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
-- SELECT n.code, dbo.GetTrafficDetailCPMEquivalized(td.id,31) FROM traffic_details td JOIN networks n ON n.id=td.network_id WHERE traffic_id=2028 ORDER BY n.code
CREATE FUNCTION [dbo].[GetTrafficDetailCPMEquivalized]
(
	@traffic_detail_id INT,
	@audience_id INT
)
RETURNS MONEY
AS
BEGIN
	DECLARE @return AS MONEY
	DECLARE @delivery AS FLOAT
	DECLARE @rate MONEY	

	SET @delivery = dbo.GetProposalDetailDeliveryEquivalizedByTrafficDetailId(@traffic_detail_id,@audience_id)
	SET @rate = dbo.GetTrafficDetailRate(@traffic_detail_id)
	
	SET @return = (
		CASE WHEN @delivery > 0.0 THEN CAST(@rate / @delivery AS MONEY) ELSE 0.0 END
	)

	RETURN @return
END


