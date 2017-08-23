
CREATE procedure usp_BS_GetProposalCreator
(
	@proposal_id int
)
as
select 
	min(bpe.effective_date),
	e.id,
	e.username,
	e.accountdomainsid,
	e.firstname,
	e.lastname,
	e.mi,
	e.email,
	e.phone,
	e.internal_extension,
	e.status,
	e.datecreated,
	e.datelastlogin,
	e.datelastmodified,
	e.hitcount
from broadcast_proposals bp with (NOLOCK)
join broadcast_proposal_employees bpe with (NOLOCK) on bpe.broadcast_proposal_id = bp.id 
join employees e with (NOLOCK) on e.id = bpe.employee_id
where
	bp.id = @proposal_id
group by
	e.id,
	e.username,
	e.accountdomainsid,
	e.firstname,
	e.lastname,
	e.mi,
	e.email,
	e.phone,
	e.internal_extension,
	e.status,
	e.datecreated,
	e.datelastlogin,
	e.datelastmodified,
	e.hitcount
	
