-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/6/2014
-- Description:	<Description, ,>
-- =============================================
-- SELECT dbo.udf_GetTrafficClearanceFactor(5428,GETDATE())
CREATE FUNCTION [dbo].[udf_GetTrafficClearanceFactor] 
(
	@traffic_id INT,
	@effective_date DATETIME
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return FLOAT;
	DECLARE @spot_length_id INT;
	
	-- get spot_length_id from traffic order
	SET @spot_length_id = (
		SELECT TOP 1 WITH ties 
			traffic_details.spot_length_id
		FROM 
			traffic_details (NOLOCK)
		WHERE 
			traffic_details.spot_length_id IS NOT NULL
			AND traffic_details.traffic_id = @traffic_id
		GROUP BY 
			traffic_details.spot_length_id
		ORDER  BY 
			COUNT(1) DESC
	);
	
	-- get release clearance factor
	SELECT
		@return = ISNULL(CAST(slm.map_value AS FLOAT), 1.0)
	FROM
		uvw_spot_length_maps slm
	WHERE                 
		slm.spot_length_id=@spot_length_id
		AND slm.map_set='traffic_clearance_factor'
		AND slm.start_date<=@effective_date AND (slm.end_date>=@effective_date OR slm.end_date IS NULL);

	RETURN @return;
END
