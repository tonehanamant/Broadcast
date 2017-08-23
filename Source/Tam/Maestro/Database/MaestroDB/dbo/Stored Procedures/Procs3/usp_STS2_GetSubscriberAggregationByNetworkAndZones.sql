-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/3/2011
-- Modified:	1/2/2014 - Stephen DeFusco - Modified to use primary flag of zone and zone_network records.
-- Description:	<Description,,>
-- =============================================
-- usp_STS2_GetSubscriberAggregationByNetworkAndZones 137,'4/28/2014','58,66,91,406,493,495,607,4036,4037,4038,4360,4361'
-- usp_STS2_GetSubscriberAggregationByNetworkAndZones 47,'4/28/2014','58,66,91,406,493,495,607,4036,4037,4038,4360,4361'
-- usp_STS2_GetSubscriberAggregationByNetworkAndZones 60,'4/28/2014','58,66,91,406,493,495,607,4036,4037,4038,4360,4361'
-- usp_STS2_GetSubscriberAggregationByNetworkAndZones 35,'4/28/2014','58,66,91,406,493,495,607,4036,4037,4038,4360,4361'
CREATE PROCEDURE [dbo].[usp_STS2_GetSubscriberAggregationByNetworkAndZones]
	@network_id INT,
	@effective_date DATETIME,
	@zone_ids VARCHAR(MAX)
AS
BEGIN
	DECLARE @night_to_day_daypart_networks TABLE (night_network_id INT NOT NULL, day_network_id INT NOT NULL);
	INSERT INTO @night_to_day_daypart_networks
		SELECT 
			network_id,
			CAST(nm.map_value AS INT) 
		FROM 
			uvw_networkmap_universe nm 
		WHERE 
			nm.map_set='DaypartNetworks' 
			AND nm.flag=3 
			AND (nm.start_date<=@effective_date AND (nm.end_date>=@effective_date OR nm.end_date IS NULL));

	SELECT 
		zn.network_id, 
		SUM(zn.subscribers)
	FROM
		uvw_zonenetwork_universe zn (NOLOCK)
		JOIN uvw_zone_universe z (NOLOCK) ON z.zone_id=zn.zone_id
			AND z.active=1
			AND (z.start_date<=@effective_date AND (z.end_date>=@effective_date OR z.end_date IS NULL))
		LEFT JOIN @night_to_day_daypart_networks dn ON dn.night_network_id=zn.network_id
	WHERE
		(zn.start_date<=@effective_date AND (zn.end_date>=@effective_date OR zn.end_date IS NULL))
		AND zn.zone_id IN (SELECT id FROM dbo.SplitIntegers(@zone_ids))
		AND zn.trafficable=1
		AND zn.network_id=CASE WHEN @network_id=dn.night_network_id THEN dn.day_network_id ELSE @network_id END
	GROUP BY 
		zn.network_id
END

