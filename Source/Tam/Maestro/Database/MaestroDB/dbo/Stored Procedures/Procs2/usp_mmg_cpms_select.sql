CREATE PROCEDURE usp_mmg_cpms_select
(
	@daypart_id		Int,
	@network_group_type		TinyInt
)
AS
SELECT
	*
FROM
	mmg_cpms WITH(NOLOCK)
WHERE
	daypart_id=@daypart_id
	AND
	network_group_type=@network_group_type

