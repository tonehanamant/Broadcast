CREATE PROCEDURE [dbo].[usp_broadcast_detail_markets_insert]
(
	@id		int		OUTPUT,
	@market_code		SmallInt,
	@martket_rank		SmallInt,
	@broadcast_proposal_detail_id		Int,
	@include		Bit
)
AS
INSERT INTO broadcast_detail_markets
(
	market_code,
	martket_rank,
	broadcast_proposal_detail_id,
	include
)
VALUES
(
	@market_code,
	@martket_rank,
	@broadcast_proposal_detail_id,
	@include
)

SELECT
	@id = SCOPE_IDENTITY()


