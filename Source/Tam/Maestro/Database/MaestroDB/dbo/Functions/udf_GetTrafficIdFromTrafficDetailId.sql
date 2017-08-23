-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/14/2014
-- Description:	
-- =============================================
CREATE FUNCTION udf_GetTrafficIdFromTrafficDetailId
(
	@traffic_detail_id INT
)
RETURNS INT
AS
BEGIN
	DECLARE @traffic_id INT;
	SELECT 
		@traffic_id = td.traffic_id
	FROM
		dbo.traffic_details td (NOLOCK)
	WHERE
		td.id=@traffic_detail_id;
	RETURN @traffic_id;
END
