CREATE PROCEDURE usp_outlook_contact_addresses_delete
(
	@outlook_contact_id		Int,
	@outlook_address_id		Int)
AS
DELETE FROM
	outlook_contact_addresses
WHERE
	outlook_contact_id = @outlook_contact_id
 AND
	outlook_address_id = @outlook_address_id
