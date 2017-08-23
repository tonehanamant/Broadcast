
CREATE FUNCTION [dbo].[GetOptimizationRulesForTraffic]
(	
	@traffic_id INT
)

RETURNS @rules TABLE
(
	traffic_detail_id INT,	
	topography_id INT,
	target_topography_id INT,
	percentage FLOAT
)
AS
BEGIN
		
	INSERT INTO @rules
		SELECT 
				distinct traffic_details.id,
				topography_optimization_rules.topography_id, 
				topography_optimization_rules.target_topography_id, 
				topography_optimization_rules.optimization_ratio
			FROM 
				topography_optimization_rules (NOLOCK) join 
				traffic_detail_topographies (NOLOCK) on 1=1 
				join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.id = traffic_detail_topographies.traffic_detail_week_id
				join topographies (NOLOCK) on topographies.id = traffic_detail_topographies.topography_id and topographies.code = 'NFC'
				join traffic_details (NOLOCK) on traffic_details.id = traffic_detail_weeks.traffic_detail_id
			WHERE 
				traffic_details.traffic_id = @traffic_id
				and topography_optimization_rules.rule_set = 
				case when traffic_detail_topographies.universe > 0 then 'nfc' else 'nonfc' end
RETURN;
END

