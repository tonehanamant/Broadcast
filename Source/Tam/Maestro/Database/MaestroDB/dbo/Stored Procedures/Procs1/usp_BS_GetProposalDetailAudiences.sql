CREATE procedure [dbo].[usp_BS_GetProposalDetailAudiences]
	@proposal_detail_id int
AS
BEGIN
	SELECT 
		a.name,
		a.range_start,
		a.range_end,
		am.map_value,
		bpda.*
	FROM
		broadcast_proposal_detail_audiences bpda WITH(NOLOCK) 
		JOIN audiences a WITH(NOLOCK) ON a.id = bpda.audience_id
		LEFT JOIN audience_maps am WITH(NOLOCK) ON am.audience_id = a.id 
			AND am.map_set = 'NSI'
	WHERE
		bpda.broadcast_proposal_detail_id = @proposal_detail_id
	ORDER BY
		bpda.ordinal
END
