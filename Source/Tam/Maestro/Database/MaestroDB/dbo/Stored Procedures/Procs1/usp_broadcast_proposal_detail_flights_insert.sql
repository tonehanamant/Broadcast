CREATE PROCEDURE [dbo].[usp_broadcast_proposal_detail_flights_insert]
(
	@broadcast_proposal_detail_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@selected		Bit
)
AS
INSERT INTO broadcast_proposal_detail_flights
(
	broadcast_proposal_detail_id,
	start_date,
	end_date,
	selected
)
VALUES
(
	@broadcast_proposal_detail_id,
	@start_date,
	@end_date,
	@selected
)

