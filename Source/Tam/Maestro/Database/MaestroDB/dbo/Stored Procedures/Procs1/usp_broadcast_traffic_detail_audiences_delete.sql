CREATE PROCEDURE [dbo].[usp_broadcast_traffic_detail_audiences_delete]
(
	@broadcast_proposal_detail_id		Int,
	@audience_id		Int,
	@market_code		SmallInt)
AS
DELETE FROM
	broadcast_traffic_detail_audiences
WHERE
	broadcast_proposal_detail_id = @broadcast_proposal_detail_id
 AND
	audience_id = @audience_id
 AND
	market_code = @market_code

