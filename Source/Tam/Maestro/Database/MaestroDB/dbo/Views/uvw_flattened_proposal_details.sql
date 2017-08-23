-- SELECT * FROM uvw_posting_plan_proposal_details
CREATE VIEW [dbo].[uvw_flattened_proposal_details]
AS
	SELECT
		pm.material_id,
		pd.*,
		d.start_time,
		d.end_time,
		d.mon,
		d.tue,
		d.wed,
		d.thu,
		d.fri,
		d.sat,
		d.sun,
		d.daypart_text,
		pf.start_date [flight_start_date],
		pf.end_date [flight_end_date]
	FROM
		proposal_details			pd	(NOLOCK)
		JOIN proposals				p	(NOLOCK) ON p.id=pd.proposal_id
		JOIN vw_ccc_daypart			d	(NOLOCK) ON d.id=pd.daypart_id
		JOIN proposal_flights		pf	(NOLOCK) ON pf.proposal_id=pd.proposal_id AND pf.selected=1
		JOIN proposal_materials		pm	(NOLOCK) ON pm.proposal_id=pd.proposal_id
	WHERE
		p.proposal_status_id=7
--	ORDER BY
--		pd.id,
--		pm.material_id,
--		pf.start_date