CREATE PROCEDURE usp_company_addresses_delete
(
	@company_id		Int,
	@address_id		Int)
AS
DELETE FROM
	company_addresses
WHERE
	company_id = @company_id
 AND
	address_id = @address_id
