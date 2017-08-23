CREATE PROCEDURE usp_zone_states_delete
(
	@zone_id		Int,
	@state_id		Int
)
AS
DELETE FROM zone_states WHERE zone_id=@zone_id AND state_id=@state_id
