CREATE PROCEDURE [dbo].[usp_REL_GetProposalAudiences]
	@proposal_id int
AS
BEGIN
	select 
		a.name, 
		pa.ordinal 
	from 
		proposal_audiences pa (NOLOCK)
		JOIN audiences a (NOLOCK) ON a.id=pa.audience_id
	where 
		pa.proposal_id=@proposal_id 
	order by 
		pa.ordinal
END
