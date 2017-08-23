-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 12/16/2013
-- Description:	Get's all NON-MARRIED trafficked ISCI's in a media month.
-- =============================================
-- usp_PCS_GetTraffickedDisplayMaterialsAssociatedWithProposals 397
CREATE PROCEDURE [dbo].[usp_PCS_GetTraffickedDisplayMaterialsAssociatedWithProposals]
	@media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @start_date DATETIME
	DECLARE @end_date DATETIME

	SELECT
		@start_date=mm.start_date,
		@end_date=mm.end_date
	FROM
		dbo.media_months mm (NOLOCK)
	WHERE
		mm.id=@media_month_id

	-- all non-married isci's trafficked where the ordered plan which the posting plans were created from was directly trafficked (i.e. no traffic plan)
	SELECT DISTINCT
		p.id 'proposal_id',
		dm.*
	FROM 
		dbo.traffic_materials tm (NOLOCK)
		JOIN dbo.reel_materials rm (NOLOCK) ON rm.id=tm.reel_material_id
		JOIN dbo.traffic_proposals tp (NOLOCK) ON tp.traffic_id=tm.traffic_id
		JOIN dbo.proposals p (NOLOCK) ON p.original_proposal_id=tp.proposal_id
			AND p.proposal_status_id=7
		JOIN dbo.uvw_display_materials dm (NOLOCK) ON dm.material_id=rm.material_id
			AND dm.type='Original'
	WHERE
		(tm.start_date <= @end_date AND tm.end_date >= @start_date)

	UNION

	-- all married isci's (married proposals) trafficked where the ordered plan which the posting plans were created from was directly trafficked (i.e. no traffic plan)
	SELECT DISTINCT
		p.id 'proposal_id',
		dm.*
	FROM 
		dbo.traffic_materials tm (NOLOCK)
		JOIN dbo.reel_materials rm (NOLOCK) ON rm.id=tm.reel_material_id
		JOIN dbo.traffic_proposals tp (NOLOCK) ON tp.traffic_id=tm.traffic_id
		JOIN dbo.proposal_proposals pp (NOLOCK) ON pp.parent_proposal_id=tp.proposal_id
		JOIN dbo.proposals p (NOLOCK) ON p.original_proposal_id=pp.child_proposal_id
			AND p.proposal_status_id=7
		JOIN dbo.uvw_display_materials dmm (NOLOCK) ON dmm.material_id=rm.material_id
			AND dmm.type='MARRIED'
		JOIN dbo.material_revisions mr (NOLOCK) ON mr.original_material_id=dmm.material_id
		JOIN dbo.uvw_display_materials dm (NOLOCK) ON dm.material_id=mr.revised_material_id
			AND dm.type='ORIGINAL'
			AND dm.product_id=p.product_id
	WHERE
		(tm.start_date <= @end_date AND tm.end_date >= @start_date)

	UNION
	
	-- all married isci's (non-married proposals) trafficked where the ordered plan which the posting plans were created from was directly trafficked (i.e. no traffic plan)
	SELECT DISTINCT
		p.id 'proposal_id',
		dm.*
	FROM 
		dbo.traffic_materials tm (NOLOCK)
		JOIN dbo.reel_materials rm (NOLOCK) ON rm.id=tm.reel_material_id
		JOIN dbo.traffic_proposals tp (NOLOCK) ON tp.traffic_id=tm.traffic_id
		JOIN dbo.proposals p (NOLOCK) ON p.original_proposal_id=tp.proposal_id
			AND p.proposal_status_id=7
		JOIN dbo.uvw_display_materials dmm (NOLOCK) ON dmm.material_id=rm.material_id
			AND dmm.type='MARRIED'
		JOIN dbo.material_revisions mr (NOLOCK) ON mr.original_material_id=dmm.material_id
		JOIN dbo.uvw_display_materials dm (NOLOCK) ON dm.material_id=mr.revised_material_id
			AND dm.type='ORIGINAL'
			AND dm.product_id=p.product_id
	WHERE
		(tm.start_date <= @end_date AND tm.end_date >= @start_date)




	UNION




	-- all non-married isci's trafficked where the trafficked plan was not the ordered plan in which the posting plans where created from (i.e. traffic plan)
	SELECT DISTINCT
		p.id 'proposal_id',
		dm.*
	FROM 
		dbo.traffic_materials tm (NOLOCK)
		JOIN dbo.reel_materials rm (NOLOCK) ON rm.id=tm.reel_material_id
		JOIN dbo.traffic_proposals tp (NOLOCK) ON tp.traffic_id=tm.traffic_id
		JOIN dbo.proposal_linkages pl (NOLOCK) ON pl.primary_proposal_id=tp.proposal_id
			AND pl.proposal_linkage_type=0 -- Traffic Plan
		JOIN dbo.proposals p (NOLOCK) ON p.original_proposal_id=pl.linked_proposal_id
			AND p.proposal_status_id=7
		JOIN dbo.uvw_display_materials dm (NOLOCK) ON dm.material_id=rm.material_id
			AND dm.type='Original'
	WHERE
		(tm.start_date <= @end_date AND tm.end_date >= @start_date)

	UNION

	-- all married isci's (married proposals) trafficked where the ordered plan which the posting plans were created from was directly trafficked (i.e. no traffic plan)
	-- possibly a rare occurrence
	SELECT DISTINCT
		p.id 'proposal_id',
		dm.*
	FROM 
		dbo.traffic_materials tm (NOLOCK)
		JOIN dbo.reel_materials rm (NOLOCK) ON rm.id=tm.reel_material_id
		JOIN dbo.traffic_proposals tp (NOLOCK) ON tp.traffic_id=tm.traffic_id
		JOIN dbo.proposal_proposals pp (NOLOCK) ON pp.parent_proposal_id=tp.proposal_id
		JOIN dbo.proposal_linkages pl (NOLOCK) ON pl.primary_proposal_id=pp.child_proposal_id
			AND pl.proposal_linkage_type=0 -- Traffic Plan
		JOIN dbo.proposals p (NOLOCK) ON p.original_proposal_id=pl.linked_proposal_id
			AND p.proposal_status_id=7
		JOIN dbo.uvw_display_materials dmm (NOLOCK) ON dmm.material_id=rm.material_id
			AND dmm.type='MARRIED'
		JOIN dbo.material_revisions mr (NOLOCK) ON mr.original_material_id=dmm.material_id
		JOIN dbo.uvw_display_materials dm (NOLOCK) ON dm.material_id=mr.revised_material_id
			AND dm.type='ORIGINAL'
			AND dm.product_id=p.product_id
	WHERE
		(tm.start_date <= @end_date AND tm.end_date >= @start_date)
		
	UNION
	
	-- all married isci's (non-married proposals) trafficked where the ordered plan which the posting plans were created from was directly trafficked (i.e. no traffic plan)
	-- possibly a rare occurrence
	SELECT DISTINCT
		p.id 'proposal_id',
		dm.*
	FROM 
		dbo.traffic_materials tm (NOLOCK)
		JOIN dbo.reel_materials rm (NOLOCK) ON rm.id=tm.reel_material_id
		JOIN dbo.traffic_proposals tp (NOLOCK) ON tp.traffic_id=tm.traffic_id
		JOIN dbo.proposal_linkages pl (NOLOCK) ON pl.primary_proposal_id=tp.proposal_id
			AND pl.proposal_linkage_type=0 -- Traffic Plan
		JOIN dbo.proposals p (NOLOCK) ON p.original_proposal_id=pl.linked_proposal_id
			AND p.proposal_status_id=7
		JOIN dbo.uvw_display_materials dmm (NOLOCK) ON dmm.material_id=rm.material_id
			AND dmm.type='MARRIED'
		JOIN dbo.material_revisions mr (NOLOCK) ON mr.original_material_id=dmm.material_id
		JOIN dbo.uvw_display_materials dm (NOLOCK) ON dm.material_id=mr.revised_material_id
			AND dm.type='ORIGINAL'
			AND dm.product_id=p.product_id
	WHERE
		(tm.start_date <= @end_date AND tm.end_date >= @start_date)
END
