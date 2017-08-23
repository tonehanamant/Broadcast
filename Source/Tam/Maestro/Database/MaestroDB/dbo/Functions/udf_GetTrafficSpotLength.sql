-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/15/2014
-- Description:	Given a traffic_id gets the most occurring spot length in traffic_details
-- =============================================
CREATE FUNCTION udf_GetTrafficSpotLength
(
	@traffic_id INT
)
RETURNS VARCHAR(MAX)
AS
BEGIN
	DECLARE @return VARCHAR(MAX);
	
	SELECT TOP 1 WITH ties 
		@return = CAST(sl.length AS VARCHAR)
	FROM 
		traffic_details td (NOLOCK)
		JOIN spot_lengths sl (NOLOCK) ON sl.id=td.spot_length_id
	WHERE 
		td.traffic_id=@traffic_id
	GROUP BY 
		sl.length
	ORDER BY 
		COUNT(1) DESC;

	RETURN @return;
END
