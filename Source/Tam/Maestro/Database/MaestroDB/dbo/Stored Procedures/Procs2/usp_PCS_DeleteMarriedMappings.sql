CREATE Procedure [dbo].[usp_PCS_DeleteMarriedMappings]
	@proposal_id int
AS

DELETE FROM proposal_proposals where parent_proposal_id = @proposal_id;

