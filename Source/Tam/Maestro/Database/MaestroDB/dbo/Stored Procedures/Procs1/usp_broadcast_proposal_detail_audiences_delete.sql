CREATE PROCEDURE [dbo].[usp_broadcast_proposal_detail_audiences_delete]
(
	@broadcast_proposal_detail_id		Int,
	@audience_id		Int)
AS
DELETE FROM
	broadcast_proposal_detail_audiences
WHERE
	broadcast_proposal_detail_id = @broadcast_proposal_detail_id
 AND
	audience_id = @audience_id

