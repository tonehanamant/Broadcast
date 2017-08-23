CREATE PROCEDURE usp_cmw_network_commissions_update
(
	@network_id		Int,
	@commission		Decimal(18,2)
)
AS
UPDATE cmw_network_commissions SET
	commission = @commission
WHERE
	network_id = @network_id
