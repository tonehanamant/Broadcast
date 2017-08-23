
CREATE PROCEDURE [dbo].[usp_proposal_audiences_insert]
(
	@proposal_id		Int,
	@audience_id		Int,
	@ordinal		Int,
	@universe		Float,
	@post_for_client		Bit
)
AS
INSERT INTO proposal_audiences
(
	proposal_id,
	audience_id,
	ordinal,
	universe,
	post_for_client
)
VALUES
(
	@proposal_id,
	@audience_id,
	@ordinal,
	@universe,
	@post_for_client
)


