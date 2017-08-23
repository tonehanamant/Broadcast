CREATE PROCEDURE [dbo].[usp_preferences_columns_select_all]
AS
SELECT
	*
FROM
	dbo.preferences_columns WITH(NOLOCK)
