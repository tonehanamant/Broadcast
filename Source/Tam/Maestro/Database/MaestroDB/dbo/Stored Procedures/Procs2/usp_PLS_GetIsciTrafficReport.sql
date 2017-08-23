
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/4/2011
-- Description:	Report all post log ISCI's which shouldn't be running according to traffic instructions.
-- =============================================
-- usp_PLS_GetIsciTrafficReport '8/1/2011','8/7/2011'
CREATE PROCEDURE [dbo].[usp_PLS_GetIsciTrafficReport]
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	-- first figure out everything that's valid
	CREATE TABLE #valid_post_logs (post_log_id INT)
	INSERT INTO #valid_post_logs
		SELECT DISTINCT
			pl.id
		FROM
			postlog_staging.dbo.post_logs pl (NOLOCK)
			JOIN traffic_materials tm (NOLOCK) ON pl.air_date BETWEEN tm.start_date AND tm.end_date
			JOIN reel_materials rm (NOLOCK) ON rm.id=tm.reel_material_id AND rm.material_id=pl.material_id
		WHERE
			pl.status_code=1
			AND pl.air_date BETWEEN @start_date AND @end_date
						
	-- second figure out everything else that's invalid
	SELECT
		z.code 'zone',
		s.code 'system',
		b.name 'mso',
		m.code 'isci',
		MIN(pl.air_date) 'min_date',
		MAX(pl.air_date) 'max_date',
		0
	FROM
		postlog_staging.dbo.post_logs pl (NOLOCK)
		LEFT JOIN traffic_materials tm (NOLOCK) ON pl.air_date BETWEEN tm.start_date AND tm.end_date
		LEFT JOIN reel_materials rm (NOLOCK) ON rm.id=tm.reel_material_id AND rm.material_id=pl.material_id
		JOIN postlog_staging.dbo.post_log_files plf (NOLOCK) ON plf.id=pl.post_log_file_id
		JOIN businesses b (NOLOCK) ON b.id=plf.business_id
		JOIN systems s (NOLOCK) ON s.id=pl.system_id
		JOIN zones z (NOLOCK) ON z.id=pl.zone_id
		JOIN materials m (NOLOCK) ON m.id=pl.material_id
	WHERE
		pl.status_code=1
		AND pl.air_date BETWEEN @start_date AND @end_date
		AND pl.id NOT IN (
			SELECT post_log_id FROM #valid_post_logs
		)
		AND (tm.id IS NULL OR rm.id IS NULL)
	GROUP BY
		z.code,
		s.code,
		b.name,
		m.code,
		pl.zone_id,
		pl.system_id,
		pl.material_id,
		plf.business_id
	ORDER BY
		m.code,
		b.name,
		s.code,
		z.code

	DROP TABLE #valid_post_logs;
END

