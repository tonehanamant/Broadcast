CREATE PROCEDURE usp_cmw_network_commissions_select
(
	@network_id		Int
)
AS
SELECT
	*
FROM
	cmw_network_commissions WITH(NOLOCK)
WHERE
	network_id=@network_id

