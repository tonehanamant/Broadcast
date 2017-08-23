CREATE PROCEDURE usp_networks_select_all
AS
BEGIN
SELECT
	*
FROM
	dbo.networks WITH(NOLOCK)
END
