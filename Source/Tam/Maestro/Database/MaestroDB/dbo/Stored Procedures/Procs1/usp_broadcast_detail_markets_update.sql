CREATE PROCEDURE [dbo].[usp_broadcast_detail_markets_update]
(
	@id		Int,
	@market_code		SmallInt,
	@martket_rank		SmallInt,
	@broadcast_proposal_detail_id		Int,
	@include		Bit
)
AS
UPDATE broadcast_detail_markets SET
	market_code = @market_code,
	martket_rank = @martket_rank,
	broadcast_proposal_detail_id = @broadcast_proposal_detail_id,
	include = @include
WHERE
	id = @id

