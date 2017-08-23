CREATE PROCEDURE [dbo].[usp_mit_universes_select_all]
AS
SELECT
	*
FROM
	dbo.mit_universes WITH(NOLOCK)
