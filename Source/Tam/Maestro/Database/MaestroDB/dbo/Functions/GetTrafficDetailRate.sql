-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION GetTrafficDetailRate
(
	@traffic_detail_id INT
)
RETURNS MONEY
AS
BEGIN
	DECLARE @return AS MONEY
	SET @return = (
		SELECT SUM(proposal_rate) FROM traffic_details_proposal_details_map tdpd (NOLOCK)
			WHERE traffic_detail_id=@traffic_detail_id
	)
	RETURN @return
END
