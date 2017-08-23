-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_TCS_LookupTrafficId]
    @revision INT,
	@original_traffic_id INT
AS
BEGIN
	SELECT 
		t.id 
	FROM 
		traffic t (NOLOCK) 
	WHERE 
		t.revision=@revision 
		AND original_traffic_id=@original_traffic_id
END
