CREATE PROCEDURE [dbo].[usp_broadcast_proposal_detail_audiences_update]
(
	@broadcast_proposal_detail_id		Int,
	@audience_id		Int,
	@ordinal		SmallInt
)
AS
UPDATE broadcast_proposal_detail_audiences SET
	ordinal = @ordinal
WHERE
	broadcast_proposal_detail_id = @broadcast_proposal_detail_id AND
	audience_id = @audience_id

