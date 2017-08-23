-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/28/2012
-- Description:	Used to get subscribers by system of active zones regardless if zone is primary or not AND excludes FXSP.
--				For use in Posting section for display purposes only!
-- =============================================
-- usp_STS2_GetSubscribersOfSystems '1,2,4,5,6,7,8,9,10,12,14,15,16,18,19,21,22,23,24,25,26,28,29,30,32,33,34,35,36,37,38,39,41,42,43,45,46,47,48,49,50,51,52,53,54,55,56,57,58,60,61,62,63,64,65,66,68,69,71,72,73,74,75,76,77,79,80,81,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,102,103,104,105,106,108,109,110,112,113,114,115,118,119,120,121,122,123,124,125,126,127,128,129,130,131,132,133,136,137,138,140,141,142,143,145,146,147,148,149,154,155,156,157,160,162,165,167,211,217,218,219,220,221,223,224,225,226,241,242,244,245,250,257,293,296,370,393,411,415,429,453,474,481,486,487,489,497,602,632,754,815','3/28/2012'
CREATE PROCEDURE [dbo].[usp_STS2_GetSubscribersOfSystems]
	@system_ids VARCHAR(MAX),
	@effective_date DATETIME
AS
BEGIN
	CREATE TABLE #system_zones (system_id INT, zone_id INT)
	INSERT INTO #system_zones
		SELECT
			DISTINCT sz.system_id,sz.zone_id
		FROM
			uvw_systemzone_universe sz (NOLOCK)
			INNER JOIN uvw_system_universe s	(NOLOCK) ON s.system_id=sz.system_id
			INNER JOIN uvw_zone_universe z		(NOLOCK)ON z.zone_id=sz.zone_id
		WHERE
			sz.system_id IN (SELECT id FROM dbo.SplitIntegers(@system_ids))
			AND z.active=1
			AND ((@effective_date IS NULL AND sz.end_date IS NULL) OR (sz.start_date<=@effective_date AND (sz.end_date>=@effective_date OR sz.end_date IS NULL)))
			AND ((@effective_date IS NULL AND  s.end_date IS NULL) OR ( s.start_date<=@effective_date AND ( s.end_date>=@effective_date OR  s.end_date IS NULL)))
			AND ((@effective_date IS NULL AND  z.end_date IS NULL) OR ( z.start_date<=@effective_date AND ( z.end_date>=@effective_date OR  z.end_date IS NULL)))

	CREATE TABLE #system_networks (system_id INT, network_id INT, subscribers BIGINT)
	INSERT INTO #system_networks
		SELECT
			sz.system_id,
			zn.network_id,
			SUM(zn.subscribers) AS subscribers
		FROM
			#system_zones sz
			JOIN uvw_zonenetwork_universe zn (NOLOCK) ON zn.zone_id=sz.zone_id
				AND ((@effective_date IS NULL AND zn.end_date IS NULL) OR (zn.start_date<=@effective_date AND (zn.end_date>=@effective_date OR zn.end_date IS NULL)))	
				AND zn.network_id <> 24 -- FXSP
		GROUP BY
			sz.system_id,
			zn.network_id

	SELECT
		system_id,
		MAX(subscribers) 
	FROM 
		#system_networks 
	GROUP BY 
		system_id

	DROP TABLE #system_zones
	DROP TABLE #system_networks
END
