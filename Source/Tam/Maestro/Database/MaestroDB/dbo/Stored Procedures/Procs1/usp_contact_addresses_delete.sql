CREATE PROCEDURE usp_contact_addresses_delete
(
	@contact_id		Int,
	@address_id		Int)
AS
DELETE FROM
	contact_addresses
WHERE
	contact_id = @contact_id
 AND
	address_id = @address_id
