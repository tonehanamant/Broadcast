
CREATE FUNCTION [dbo].[GetTrafficDetailCoverageUniverse]
(
	@traffic_detail_id INT,
	@audience_id INT,
	@topography_id INT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return AS FLOAT
	
	SET @return = 0.0
	
	SET @return = (
		select 
			  avg(tdt.universe) * dbo.GetTrafficDetailScalingFactor(@traffic_detail_id, @audience_id)	
		from 
			traffic_detail_topographies (NOLOCK) as tdt 
			join traffic_detail_weeks (NOLOCK) as tdw on tdw.id = tdt.traffic_detail_week_id
		WHERE
			tdt.topography_id = @topography_id
			and
			tdw.traffic_detail_id = @traffic_detail_id
	)
	
	RETURN @return
END
