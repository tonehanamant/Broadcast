CREATE PROCEDURE [dbo].[usp_audiences_select_all]
AS
SELECT
	*
FROM
	dbo.audiences WITH(NOLOCK)
