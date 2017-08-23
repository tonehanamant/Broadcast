
CREATE PROCEDURE [dbo].[usp_PCS_AttachProposalAsMarryCandidate]
(
	@id		int		OUTPUT,
	@proposal_id int,
	@spot_length_id int,
	@media_month_id int,
	@base_index DECIMAL(19,2)
)
AS
	INSERT INTO proposal_marry_candidates(proposal_id, media_month_id, spot_length_id,base_index)
	VALUES (@proposal_id, @media_month_id, @spot_length_id, @base_index);

SELECT
@id = SCOPE_IDENTITY()
