CREATE PROCEDURE [dbo].[usp_audience_audiences_select_all]
AS
SELECT
	*
FROM
	dbo.audience_audiences WITH(NOLOCK)

