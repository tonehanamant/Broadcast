CREATE PROCEDURE usp_cmw_network_commissions_insert
(
	@network_id		Int,
	@commission		Decimal(18,2)
)
AS
INSERT INTO cmw_network_commissions
(
	network_id,
	commission
)
VALUES
(
	@network_id,
	@commission
)

