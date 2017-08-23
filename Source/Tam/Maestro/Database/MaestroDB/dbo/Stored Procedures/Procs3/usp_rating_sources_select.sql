
CREATE PROCEDURE [dbo].[usp_rating_sources_select]
(
	@id		TinyInt
)
AS
SELECT
	*
FROM
	rating_sources WITH(NOLOCK)
WHERE
	id=@id


