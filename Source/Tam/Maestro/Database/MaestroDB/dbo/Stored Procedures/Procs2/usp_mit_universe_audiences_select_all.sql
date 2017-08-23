CREATE PROCEDURE [dbo].[usp_mit_universe_audiences_select_all]
AS
SELECT
	*
FROM
	dbo.mit_universe_audiences WITH(NOLOCK)
