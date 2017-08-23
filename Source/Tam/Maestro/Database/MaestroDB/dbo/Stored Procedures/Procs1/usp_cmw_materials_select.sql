CREATE PROCEDURE usp_cmw_materials_select
(
	@id Int
)
AS
SELECT
	*
FROM
	cmw_materials WITH(NOLOCK)
WHERE
	id = @id
