CREATE PROCEDURE usp_company_addresses_insert
(
	@company_id		Int,
	@address_id		Int
)
AS
INSERT INTO company_addresses
(
	company_id,
	address_id
)
VALUES
(
	@company_id,
	@address_id
)

