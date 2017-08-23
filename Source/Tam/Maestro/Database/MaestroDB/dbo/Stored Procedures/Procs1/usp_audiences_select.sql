CREATE PROCEDURE [dbo].[usp_audiences_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	dbo.audiences WITH(NOLOCK)
WHERE
	id = @id
