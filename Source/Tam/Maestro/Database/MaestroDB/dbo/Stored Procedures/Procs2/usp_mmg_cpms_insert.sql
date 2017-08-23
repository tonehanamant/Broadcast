CREATE PROCEDURE usp_mmg_cpms_insert
(
	@daypart_id		Int,
	@network_group_type		TinyInt,
	@cpm		Money
)
AS
INSERT INTO mmg_cpms
(
	daypart_id,
	network_group_type,
	cpm
)
VALUES
(
	@daypart_id,
	@network_group_type,
	@cpm
)

