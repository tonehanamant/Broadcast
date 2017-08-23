CREATE PROCEDURE usp_cmw_network_commissions_select_all
AS
SELECT
	*
FROM
	cmw_network_commissions WITH(NOLOCK)
