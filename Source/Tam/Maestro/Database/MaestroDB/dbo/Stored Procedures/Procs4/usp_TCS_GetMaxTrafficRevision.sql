-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/5/2014
-- Description:	
-- =============================================
CREATE PROCEDURE usp_TCS_GetMaxTrafficRevision
	@original_traffic_id INT
AS
BEGIN
	SELECT 
		MAX(t.revision) 
	FROM 
		traffic t (NOLOCK) 
	WHERE 
		t.original_traffic_id=@original_traffic_id
END
