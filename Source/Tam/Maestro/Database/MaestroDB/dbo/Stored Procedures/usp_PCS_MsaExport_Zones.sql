-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/15/2015
-- Description:	Used to populate MSA Export file "zones.txt"
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_MsaExport_Zones]
	@msa_tam_post_proposal_ids UniqueIdTable READONLY
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	DECLARE @media_month_start_date DATETIME;
	DECLARE @zone_ids AS TABLE (zone_id INT);
	
	-- should always be in the context of 1 media month (but just in case we use MIN with current date fallback)
	SELECT
		@media_month_start_date = ISNULL(MIN(mm.start_date),CAST(GETDATE() AS DATE))
	FROM
		@msa_tam_post_proposal_ids mttp
		JOIN tam_post_proposals tppm ON tppm.id=mttp.id
		JOIN proposals p ON p.id=tppm.posting_plan_proposal_id
		JOIN media_months mm ON mm.id=p.posting_media_month_id
	
	CREATE TABLE #market_breaks_counties (market_break VARCHAR(63), dma_id INT, dma_name VARCHAR(63), total_counties INT); 
	CREATE TABLE #market_breaks (market_break VARCHAR(63) NOT NULL, dma_id INT NOT NULL, dma_name VARCHAR(63) NOT NULL, total_counties INT, PRIMARY KEY (dma_id), UNIQUE (dma_id)); 
	
	INSERT INTO #market_breaks_counties 
		SELECT  
			RIGHT(nmbc.market_break,LEN(nmbc.market_break)- 12) 'market_break', 
			ncue.dma_id, 
			ncue.dma_name, 
			COUNT(1) 'total_counties' 
		FROM 
			dbo.nielsen_market_break_counties nmbc 
			JOIN dbo.nielsen_county_universe_estimates ncue ON ncue.nmr_county_code=nmbc.nmr_county_code 
		GROUP BY 
			nmbc.market_break, ncue.dma_id, ncue.dma_name 
		ORDER BY 
			dma_name; 
			
	INSERT INTO #market_breaks 
		SELECT 
			mb.* 
		FROM ( 
			SELECT 
				dma_id, dma_name, MAX(total_counties) 'max_counties' 
			FROM 
				#market_breaks_counties 
			GROUP BY 
				dma_id, dma_name
		) tmp 
		JOIN #market_breaks_counties mb ON mb.dma_id=tmp.dma_id AND tmp.max_counties=mb.total_counties; 	

	INSERT INTO @zone_ids
		SELECT
			tpz.zone_id
		FROM
			@msa_tam_post_proposal_ids mttp
			JOIN tam_post_proposals tppm ON tppm.id=mttp.id
			JOIN tam_post_proposals tpp ON tpp.tam_post_id=tppm.tam_post_id
				AND tpp.post_source_code=0 -- POST
			JOIN tam_post_zones tpz ON tpz.tam_post_proposal_id=tpp.id
		GROUP BY
			tpz.zone_id
	
	SELECT
		z.zone_id 'id', 
		z.name 'name', 
		CASE z.[primary] WHEN 1 THEN 'P' ELSE 'S' END 'type', 
		z.code 'sys_code', 
		s.code 'sbt', 
		b.name 'mso', 
		d.name 'dma', 
		mb.market_break 'break'
	FROM
		@zone_ids zids
		JOIN uvw_zone_universe z ON zids.zone_id=z.zone_id
			AND (z.start_date<=@media_month_start_date AND (z.end_date>=@media_month_start_date OR z.end_date IS NULL)) 
		JOIN uvw_systemzone_universe sz ON sz.zone_id=z.zone_id
			AND (sz.start_date<=@media_month_start_date AND (sz.end_date>=@media_month_start_date OR sz.end_date IS NULL)) 
			AND sz.type='BILLING' 
		JOIN uvw_system_universe s ON s.system_id=sz.system_id
			AND (s.start_date<=@media_month_start_date AND (s.end_date>=@media_month_start_date OR s.end_date IS NULL))  
		JOIN uvw_zonedma_universe zd ON zd.zone_id=z.zone_id
			AND (zd.start_date<=@media_month_start_date AND (zd.end_date>=@media_month_start_date OR zd.end_date IS NULL))  
		JOIN uvw_dma_universe d ON d.dma_id=zd.dma_id
			AND (d.start_date<=@media_month_start_date AND (d.end_date>=@media_month_start_date OR d.end_date IS NULL))  
		JOIN uvw_zonebusiness_universe zb ON zb.zone_id=z.zone_id
			AND (zb.start_date<=@media_month_start_date AND (zb.end_date>=@media_month_start_date OR zb.end_date IS NULL))
			AND zb.type='OWNEDBY' 
		JOIN uvw_business_universe b ON b.business_id=zb.business_id
			AND (b.start_date<=@media_month_start_date AND (b.end_date>=@media_month_start_date OR b.end_date IS NULL))  
		LEFT JOIN #market_breaks mb ON mb.dma_id=zd.dma_id
END