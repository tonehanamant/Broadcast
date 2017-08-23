CREATE PROCEDURE [dbo].[usp_materials_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	dbo.materials WITH(NOLOCK)
WHERE
	id = @id
