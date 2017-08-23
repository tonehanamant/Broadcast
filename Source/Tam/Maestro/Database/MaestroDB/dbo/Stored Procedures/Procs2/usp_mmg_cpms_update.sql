CREATE PROCEDURE usp_mmg_cpms_update
(
	@daypart_id		Int,
	@network_group_type		TinyInt,
	@cpm		Money
)
AS
UPDATE mmg_cpms SET
	cpm = @cpm
WHERE
	daypart_id = @daypart_id AND
	network_group_type = @network_group_type
