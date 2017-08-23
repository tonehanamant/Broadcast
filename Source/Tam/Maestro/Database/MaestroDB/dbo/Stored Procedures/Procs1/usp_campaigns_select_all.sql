CREATE PROCEDURE usp_campaigns_select_all
AS
SELECT
	*
FROM
	campaigns WITH(NOLOCK)
