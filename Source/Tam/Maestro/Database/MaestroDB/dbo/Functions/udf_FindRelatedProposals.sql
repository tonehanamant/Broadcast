
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns the proposal_id of all proposals related to the input proposal ID. 
--				Returns relation_level, where 1 = original proposal_id, 2 = revised proposal_id 
--					and 3 = posting plan proposal_id.
-- =============================================
CREATE FUNCTION [dbo].[udf_FindRelatedProposals]
(	
	@idProposal as int
)
RETURNS TABLE
AS
RETURN 
(
	with
	original_proposals(
		target_proposal_id,
		current_proposal_id,
		original_proposal_id,
		relation_level
	) as (
		select
			@idProposal target_proposal_id,
			p.id current_proposal_id,
			p.original_proposal_id original_proposal_id,
			1 relation_level
		from
			proposals p
		where
			p.id = @idProposal

		union all

		select
			@idProposal target_proposal_id,
			p.id related_proposal_id,
			p.original_proposal_id,
			1 + op.relation_level relation_level
		from
			original_proposals op
			join proposals p on
				p.id = op.original_proposal_id
	),
	related_proposals
	(
		proposal_id,
		relation_level
	) as (
		select
			p.id proposal_id,
			1 relation_level
		from
			proposals p with(nolock)
			join original_proposals op on
				p.id = op.current_proposal_id
				and
				op.original_proposal_id is null

		union all

		select
			p.id proposal_id,
			1 + rp.relation_level relation_level
		from
			related_proposals rp
			join proposals p on
				p.original_proposal_id = rp.proposal_id
			
	)
	select
		proposal_id,
		relation_level
	from
		related_proposals rp
);

