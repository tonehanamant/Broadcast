CREATE PROCEDURE [dbo].[usp_languages_select]
(
	@id		TinyInt
)
AS
SELECT
	*
FROM
	dbo.languages WITH(NOLOCK)
WHERE
	id=@id
