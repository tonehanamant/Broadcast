-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/15/2015
-- Description:	Used to populate MSA Export file "spots_YYYY-MM.txt"
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_MsaExport_Spots]
	@media_month_id INT,
	@msa_tam_post_proposal_ids UniqueIdTable READONLY
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	DECLARE @media_month_start_date DATETIME;
	DECLARE @media_month VARCHAR(10);

	DECLARE @tam_post_proposals TABLE (tam_post_id INT, msa_tam_post_proposal_id INT, post_tam_post_proposal_id INT)
	INSERT INTO @tam_post_proposals
		SELECT
			msa_tpp.tam_post_id,
			msa_tpp.id,
			post_tpp.id
		FROM
			@msa_tam_post_proposal_ids m
			JOIN tam_post_proposals msa_tpp ON msa_tpp.id=m.id
			JOIN tam_post_proposals post_tpp ON post_tpp.tam_post_id=msa_tpp.tam_post_id
				AND post_tpp.posting_plan_proposal_id=msa_tpp.posting_plan_proposal_id
				AND post_tpp.post_source_code=0

	SELECT
		@media_month_start_date = mm.start_date,
		@media_month = mm.media_month
	FROM
		media_months mm
	WHERE
		mm.id=@media_month_id;
	
	SELECT
		tpa.id 'id',
		tpa.zone_id 'zone_id',
		tpa.posted_network_id 'net_id',
		0 'adv_id',
		tpp.tam_post_id 'con_id',
		tpa.proposal_detail_id 'line',
		s.code 'sbt',
		mmsa.code 'copy',
		CONVERT(VARCHAR(10), tpa.air_date, 120) 'air_date',
		tpa.air_time 'air_time',
		CONVERT(VARCHAR(10), ISNULL(a.adjusted_air_date, a.air_date), 120) 'adjusted_air_date',
		ISNULL(a.adjusted_air_time, a.air_time) 'adjusted_air_time',
		sl.length 'length',
		@media_month 'period',
		0 'hh_imp',
		0 'hh_count',
		0 'post_exc'
	FROM
		@tam_post_proposals tpp
		JOIN maestro_analysis.dbo.tam_post_affidavits tpa (NOLOCK) ON tpa.media_month_id=@media_month_id
			AND tpa.tam_post_proposal_id=tpp.post_tam_post_proposal_id
			AND tpa.enabled=1
		JOIN affidavits a (NOLOCK) ON a.media_month_id=@media_month_id
			AND a.id=tpa.affidavit_id
		JOIN uvw_system_universe s (NOLOCK) ON s.system_id=tpa.system_id
			AND (s.start_date<=@media_month_start_date AND (s.end_date>=@media_month_start_date OR s.end_date IS NULL))  
		-- this reprsents the posted copy (the :15 for example, this should never be a married ISCI)
		JOIN materials m (NOLOCK) ON m.id=tpa.posted_material_id
		-- this reprsents client ISCI (if any)
		LEFT JOIN materials mc (NOLOCK) ON mc.id=CASE WHEN m.real_material_id IS NOT NULL THEN m.real_material_id ELSE m.id END
		-- check to see if there is an HD to SD substitution defined
		LEFT JOIN materials msd (NOLOCK) ON msd.code=CASE WHEN mc.code IS NOT NULL THEN SUBSTRING(mc.code, 1, LEN(mc.code)-1) ELSE SUBSTRING(m.code, 1, LEN(m.code)-1) END
		-- final msa determination
		LEFT JOIN materials mmsa (NOLOCK) ON mmsa.id=CASE WHEN msd.id IS NOT NULL THEN msd.id WHEN mc.id IS NOT NULL THEN mc.id ELSE m.id END
		LEFT JOIN spot_lengths sl (NOLOCK) ON sl.id=mmsa.spot_length_id		
END