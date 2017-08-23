CREATE PROCEDURE usp_mmg_cpms_delete
(
	@daypart_id		Int,
	@network_group_type		TinyInt)
AS
DELETE FROM
	mmg_cpms
WHERE
	daypart_id = @daypart_id
 AND
	network_group_type = @network_group_type
