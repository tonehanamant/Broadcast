
CREATE PROCEDURE [dbo].[usp_cmw_divisions_select_all]
AS
SELECT
	*
FROM
	cmw_divisions WITH(NOLOCK)

