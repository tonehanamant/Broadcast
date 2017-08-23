-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/15/2015
-- Description:	Used to populate MSA Export file "zone_subscribers_YYYY-MM.txt"
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_MsaExport_ZoneSubscribers]
	@msa_tam_post_proposal_ids UniqueIdTable READONLY
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	
	DECLARE @media_month_start_date DATETIME;
	DECLARE @media_month_end_date DATETIME;
	DECLARE @zone_ids AS TABLE (zone_id INT);
	
	-- should always be in the context of 1 media month (but just in case we use MIN/MAX with current date fallback)
	SELECT
		@media_month_start_date = ISNULL(MIN(mm.start_date),CAST(GETDATE() AS DATE)),
		@media_month_end_date = ISNULL(MAX(mm.end_date),CAST(GETDATE() AS DATE))
	FROM
		@msa_tam_post_proposal_ids mttp
		JOIN tam_post_proposals tppm ON tppm.id=mttp.id
		JOIN proposals p ON p.id=tppm.posting_plan_proposal_id
		JOIN media_months mm ON mm.id=p.posting_media_month_id
	
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
		b.name 'video_op_name',
		zn.zone_id 'zone_id',
		zn.network_id 'net_id',
		MAX(zn.subscribers) 'max_tier_subs',
		CONVERT(VARCHAR(10), MIN(zn.start_date), 120) 'load_date'
	FROM
		@zone_ids zids
		JOIN uvw_zonenetwork_universe zn ON zn.zone_id=zids.zone_id
			AND (zn.start_date <= @media_month_end_date AND zn.end_date >= @media_month_start_date)
		JOIN uvw_zonebusiness_universe zb ON zb.zone_id=zn .zone_id
			AND zb.type='OWNEDBY' 
			AND (zb.start_date<=@media_month_start_date AND (zb.end_date>=@media_month_start_date OR zb.end_date IS NULL))
		JOIN uvw_business_universe b ON b.business_id=zb.business_id
			AND (b.start_date<=@media_month_start_date AND (b.end_date>=@media_month_start_date OR b.end_date IS NULL))  
	GROUP BY
		b.name,
		zn.zone_id,
		zn.network_id
	ORDER BY
		b.name,
		zn.zone_id,
		zn.network_id
END