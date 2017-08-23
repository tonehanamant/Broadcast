CREATE PROCEDURE [dbo].[usp_broadcast_traffic_detail_audiences_insert]
(
	@broadcast_proposal_detail_id		Int,
	@audience_id		Int,
	@market_code		SmallInt,
	@subscribers		Int
)
AS
INSERT INTO broadcast_traffic_detail_audiences
(
	broadcast_proposal_detail_id,
	audience_id,
	market_code,
	subscribers
)
VALUES
(
	@broadcast_proposal_detail_id,
	@audience_id,
	@market_code,
	@subscribers
)


