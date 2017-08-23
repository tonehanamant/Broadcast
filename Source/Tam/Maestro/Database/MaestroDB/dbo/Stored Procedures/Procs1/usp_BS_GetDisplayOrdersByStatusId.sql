
CREATE PROCEDURE [dbo].[usp_BS_GetDisplayOrdersByStatusId]
@status_id int

AS
BEGIN

	SET NOCOUNT ON;

	CREATE TABLE #temp_versions(proposal_id int, revision int);
	
	INSERT INTO #temp_versions(proposal_id, revision) 
	SELECT 
		bp.original_proposal_id, 
		MAX(bp.version_number) 
	FROM broadcast_proposals bp with (NOLOCK) 
	GROUP BY
		bp.original_proposal_id;
	
	select 
		bp.advertiser_company_id,
		bp.agency_company_id,
		p.name ,
		bp.*
	from broadcast_proposals bp with (NOLOCK)
	join #temp_versions on #temp_versions.proposal_id = bp.original_proposal_id and #temp_versions.revision = bp.version_number
	left join products p with (NOLOCK) on p.id = bp.product_id
	WHERE
		bp.proposal_status_id = @status_id
	ORDER BY 
		bp.original_proposal_id DESC, bp.version_number;
	
	DROP TABLE #temp_versions;
END
