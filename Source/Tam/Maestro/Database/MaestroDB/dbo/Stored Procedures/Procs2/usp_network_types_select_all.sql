CREATE PROCEDURE usp_network_types_select_all
AS
BEGIN
SELECT
	*
FROM
	dbo.network_types WITH(NOLOCK)
END
