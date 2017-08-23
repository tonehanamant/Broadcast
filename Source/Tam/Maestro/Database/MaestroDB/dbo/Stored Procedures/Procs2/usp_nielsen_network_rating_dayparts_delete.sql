CREATE PROCEDURE usp_nielsen_network_rating_dayparts_delete
(
	@nielsen_network_id		Int,
	@daypart_id		Int
)
AS
DELETE FROM nielsen_network_rating_dayparts WHERE nielsen_network_id=@nielsen_network_id AND daypart_id=@daypart_id
