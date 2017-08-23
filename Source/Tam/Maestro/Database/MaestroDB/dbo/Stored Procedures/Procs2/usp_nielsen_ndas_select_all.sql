CREATE PROCEDURE [dbo].[usp_nielsen_ndas_select_all]
AS
SELECT
	*
FROM
	nielsen_ndas WITH(NOLOCK)
