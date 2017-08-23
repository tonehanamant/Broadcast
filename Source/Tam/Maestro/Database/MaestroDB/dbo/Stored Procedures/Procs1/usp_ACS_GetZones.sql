CREATE PROCEDURE [dbo].[usp_ACS_GetZones]
AS
SELECT
	z.*
FROM
	zones z (NOLOCK)
ORDER BY
	z.code