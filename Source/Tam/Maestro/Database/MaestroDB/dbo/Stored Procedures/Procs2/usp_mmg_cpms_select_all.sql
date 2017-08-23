CREATE PROCEDURE usp_mmg_cpms_select_all
AS
SELECT
	*
FROM
	mmg_cpms WITH(NOLOCK)
