
CREATE PROCEDURE [dbo].[usp_proposal_audiences_update]
(
	@proposal_id		Int,
	@audience_id		Int,
	@ordinal		Int,
	@universe		Float,
	@post_for_client		Bit
)
AS
UPDATE proposal_audiences SET
	ordinal = @ordinal,
	universe = @universe,
	post_for_client = @post_for_client
WHERE
	proposal_id = @proposal_id AND
	audience_id = @audience_id

