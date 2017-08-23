CREATE PROCEDURE [dbo].[usp_ACS_GetNetworks]
AS
SELECT
	n.*
FROM
	networks n (NOLOCK)
ORDER BY
	n.code