CREATE PROCEDURE [dbo].[usp_broadcast_proposal_detail_audiences_insert]
(
	@broadcast_proposal_detail_id		Int,
	@audience_id		Int,
	@ordinal		SmallInt
)
AS
INSERT INTO broadcast_proposal_detail_audiences
(
	broadcast_proposal_detail_id,
	audience_id,
	ordinal
)
VALUES
(
	@broadcast_proposal_detail_id,
	@audience_id,
	@ordinal
)



