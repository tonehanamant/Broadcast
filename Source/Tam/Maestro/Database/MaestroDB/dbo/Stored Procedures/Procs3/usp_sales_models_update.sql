CREATE PROCEDURE [dbo].[usp_sales_models_update]
(
	@id		Int,
	@name		VarChar(63),
	@code		VarChar(15),
	@display_name		VarChar(31),
	@scx_name		VarChar(127),
	@scx_office		VarChar(127),
	@scx_ncc_universal_agency_office_id		VarChar(15),
	@scx_street		VarChar(255),
	@scx_country		VarChar(63),
	@scx_city		VarChar(63),
	@scx_state		VarChar(63),
	@scx_zip		VarChar(15),
	@scx_contact_first_name		VarChar(63),
	@scx_contact_last_name		VarChar(63),
	@scx_contact_email		VarChar(255),
	@scx_contact_phone		VarChar(31),
	@scx_contact_fax		VarChar(31)
)
AS
UPDATE dbo.sales_models SET
	name = @name,
	code = @code,
	display_name = @display_name,
	scx_name = @scx_name,
	scx_office = @scx_office,
	scx_ncc_universal_agency_office_id = @scx_ncc_universal_agency_office_id,
	scx_street = @scx_street,
	scx_country = @scx_country,
	scx_city = @scx_city,
	scx_state = @scx_state,
	scx_zip = @scx_zip,
	scx_contact_first_name = @scx_contact_first_name,
	scx_contact_last_name = @scx_contact_last_name,
	scx_contact_email = @scx_contact_email,
	scx_contact_phone = @scx_contact_phone,
	scx_contact_fax = @scx_contact_fax
WHERE
	id = @id
