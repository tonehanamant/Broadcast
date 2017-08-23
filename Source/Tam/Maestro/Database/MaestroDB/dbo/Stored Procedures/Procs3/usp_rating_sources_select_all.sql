
CREATE PROCEDURE [dbo].[usp_rating_sources_select_all]
AS
SELECT
	*
FROM
	rating_sources WITH(NOLOCK)

