CREATE PROCEDURE [dbo].[usp_languages_select_all]
AS
SELECT
	*
FROM
	dbo.languages WITH(NOLOCK)
