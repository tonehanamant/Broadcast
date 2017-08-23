-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/14/2014
-- Description:	
-- =============================================
CREATE FUNCTION udf_IsFixed
(
	@traffic_id INT,
	@topography_id INT
)
RETURNS BIT
AS
BEGIN
	DECLARE @is_fixed INT;
	SELECT 
		@is_fixed = CASE WHEN COUNT(1)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
	FROM 
		topography_maps tm (NOLOCK) 
	WHERE 
		(tm.map_value = 'fxd' OR tm.map_value = 'hyb') 
		AND tm.map_set = 'rate_type' 
		AND tm.topography_id = @topography_id;
	RETURN @is_fixed;
END
