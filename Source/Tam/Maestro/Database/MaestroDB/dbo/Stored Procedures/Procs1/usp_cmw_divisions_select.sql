
CREATE PROCEDURE [dbo].[usp_cmw_divisions_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	cmw_divisions WITH(NOLOCK)
WHERE
	id = @id

