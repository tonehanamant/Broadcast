
CREATE PROCEDURE [dbo].[usp_mit_tv_audiences_select_all]
AS
SELECT
	*
FROM
	dbo.mit_tv_audiences WITH(NOLOCK)
