CREATE PROCEDURE usp_cmw_network_commissions_delete
(
	@network_id		Int)
AS
DELETE FROM
	cmw_network_commissions
WHERE
	network_id = @network_id
