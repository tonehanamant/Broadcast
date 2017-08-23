CREATE PROCEDURE usp_material_revisions_select
(
	@original_material_id		Int,
	@revised_material_id		Int,
	@ordinal		TinyInt
)
AS
SELECT
	*
FROM
	material_revisions WITH(NOLOCK)
WHERE
	original_material_id=@original_material_id
	AND
	revised_material_id=@revised_material_id
	AND
	ordinal=@ordinal

