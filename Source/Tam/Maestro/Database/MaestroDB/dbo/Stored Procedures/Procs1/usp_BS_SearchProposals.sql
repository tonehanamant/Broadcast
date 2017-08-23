
CREATE PROCEDURE [dbo].[usp_BS_SearchProposals]
(
	@proposal_id int,
	@search_string varchar(255),
	@company_ids VARCHAR(MAX)
)
AS
BEGIN
DECLARE @company_ids2 TABLE (id int)
INSERT INTO @company_ids2 select * from dbo.SplitIntegers(@company_ids);

declare @query varchar(max);

CREATE TABLE #temp_versions(proposal_id int, revision int);
	
set @query = 'INSERT INTO #temp_versions(proposal_id, revision) 
	SELECT 
		bp.original_proposal_id, 
		MAX(bp.version_number) 
	FROM broadcast_proposals bp with (NOLOCK) 
	left join products p with (NOLOCK) on p.id = bp.product_id
	WHERE ';
IF @proposal_id IS NULL
BEGIN
	if @company_ids IS NOT NULL
	set @query = @query + ' bp.advertiser_company_id in ''('+@company_ids+')'' or bp.agency_company_id in ''('+@company_ids+')''
	or'
	ELSE
	set @query = @query +'p.name like ''%'+@search_string+'%''
	or
	bp.name like ''%'+@search_string+'%''';
END
ELSE
BEGIN
	set @query = @query + ' bp.id = ' + cast(@proposal_id as varchar(8));
END

set @query = @query + 'GROUP BY bp.original_proposal_id';

exec(@query);

set @query = '
select 
	bp.advertiser_company_id,
	bp.agency_company_id,
	p.name,
	bp.*
from broadcast_proposals bp with (NOLOCK)
join #temp_versions on #temp_versions.proposal_id = bp.original_proposal_id and #temp_versions.revision = bp.version_number
left join products p with (NOLOCK) on p.id = bp.product_id
where ';

IF @proposal_id IS NULL
BEGIN
	if @company_ids IS NOT NULL
	set @query = @query + ' bp.advertiser_company_id in ''('+@company_ids+')'' or bp.agency_company_id in ''('+@company_ids+')''
	or'
	ELSE
	set @query = @query + 'p.name like ''%'+@search_string+'%''
	or
	bp.name like ''%'+@search_string+'%''';
END
ELSE
BEGIN
	set @query = @query + ' bp.id = ' + cast(@proposal_id as varchar(8));
END

exec(@query);

DROP TABLE #temp_versions;
END
