



/* END MME-1374: Custom Traffic SBTs will not affect MVPD's coverage universe */

/* BEGIN products refactor */
CREATE PROCEDURE [dbo].[usp_PCS_GetPostInformation]
	@tam_post_proposal_id INT
AS
BEGIN
	DECLARE @tam_post_id INT
	DECLARE @posting_plan_proposal_id INT
	
	SELECT
		@tam_post_id = tpp.tam_post_id,
		@posting_plan_proposal_id = tpp.posting_plan_proposal_id
	FROM
		tam_post_proposals tpp (NOLOCK)
	WHERE
		tpp.id = @tam_post_proposal_id

	-- tam_post
    SELECT 
		tp.* 
	FROM 
		tam_posts tp (NOLOCK)
	WHERE 
		tp.id = @tam_post_id

	-- proposal
    SELECT 
		p.*,
		mm.*,
		p.product_id
	FROM 
		proposals p				(NOLOCK)
		JOIN media_months mm	(NOLOCK) ON mm.id=p.posting_media_month_id
	WHERE 
		p.id = @posting_plan_proposal_id

	-- tam_post_proposal
	SELECT
		*
	FROM
		tam_post_proposals tpp (NOLOCK)
	WHERE
		tpp.id = @tam_post_proposal_id

	-- proposal_materials
	CREATE TABLE #tmp_materials (material_id INT, revised_material_id INT, start_date DATETIME, end_date DATETIME)
	CREATE INDEX IX_tmp_materials on #tmp_materials (material_id)

	INSERT INTO #tmp_materials
		SELECT pm.material_id, pm.material_id, pm.start_date, pm.end_date FROM proposal_materials pm (NOLOCK) WHERE proposal_id=@posting_plan_proposal_id
	INSERT INTO #tmp_materials
		SELECT mr.original_material_id, mr.revised_material_id, pm.start_date, pm.end_date FROM proposal_materials pm (NOLOCK) JOIN material_revisions mr (NOLOCK) ON mr.revised_material_id=pm.material_id WHERE pm.proposal_id=@posting_plan_proposal_id

    SELECT
		m.id,
		m_rev.id,
		m.code,
		m_rev.code,
		sl.delivery_multiplier
	FROM
		#tmp_materials tm
		JOIN materials m	 (NOLOCK) ON m.id=tm.material_id
		JOIN materials m_rev (NOLOCK) ON m_rev.id=tm.revised_material_id
		JOIN spot_lengths sl (NOLOCK) ON sl.id=m_rev.spot_length_id

	DROP TABLE #tmp_materials;

	-- proposal_details
	SELECT DISTINCT
		n.code 'network',
		sl.length,
		pd.*,
		d.id,
		d.code,
		d.name,
		d.start_time,
		d.end_time,
		d.mon,
		d.tue,
		d.wed,
		d.thu,
		d.fri,
		d.sat,
		d.sun
	FROM
		proposal_details pd		(NOLOCK)
		JOIN vw_ccc_daypart d	(NOLOCK) ON d.id=pd.daypart_id
		JOIN networks n			(NOLOCK) ON n.id=pd.network_id
		JOIN spot_lengths sl	(NOLOCK) ON sl.id=pd.spot_length_id
	WHERE
		pd.proposal_id = @posting_plan_proposal_id
	
	-- proposal_flights
	SELECT 
		pf.start_date,
		pf.end_date,
		pf.selected,
		mm.month,
		mw.week_number
	FROM 
		proposal_flights pf		(NOLOCK) 
		JOIN media_weeks mw		(NOLOCK) ON pf.start_date BETWEEN mw.start_date AND mw.end_date
		JOIN media_months mm	(NOLOCK) ON mw.media_month_id=mm.id
	WHERE 
		pf.proposal_id = @posting_plan_proposal_id

	-- proposal_audiences
	SELECT
		pa.*
	FROM
		proposal_audiences pa (NOLOCK)
	WHERE
		pa.proposal_id = @posting_plan_proposal_id
END
