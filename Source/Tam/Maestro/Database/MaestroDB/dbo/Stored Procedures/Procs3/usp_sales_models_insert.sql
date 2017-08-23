-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/09/2013 08:04:15 AM
-- Description:	Auto-generated method to insert a sales_models record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_sales_models_insert]
	@id INT OUTPUT,
	@name VARCHAR(63),
	@code VARCHAR(15),
	@display_name VARCHAR(31),
	@scx_name VARCHAR(127),
	@scx_office VARCHAR(127),
	@scx_ncc_universal_agency_office_id VARCHAR(15),
	@scx_street VARCHAR(255),
	@scx_country VARCHAR(63),
	@scx_city VARCHAR(63),
	@scx_state VARCHAR(63),
	@scx_zip VARCHAR(15),
	@scx_contact_first_name VARCHAR(63),
	@scx_contact_last_name VARCHAR(63),
	@scx_contact_email VARCHAR(255),
	@scx_contact_phone VARCHAR(31),
	@scx_contact_fax VARCHAR(31)
AS
BEGIN
	INSERT INTO [dbo].[sales_models]
	(
		[name],
		[code],
		[display_name],
		[scx_name],
		[scx_office],
		[scx_ncc_universal_agency_office_id],
		[scx_street],
		[scx_country],
		[scx_city],
		[scx_state],
		[scx_zip],
		[scx_contact_first_name],
		[scx_contact_last_name],
		[scx_contact_email],
		[scx_contact_phone],
		[scx_contact_fax]
	)
	VALUES
	(
		@name,
		@code,
		@display_name,
		@scx_name,
		@scx_office,
		@scx_ncc_universal_agency_office_id,
		@scx_street,
		@scx_country,
		@scx_city,
		@scx_state,
		@scx_zip,
		@scx_contact_first_name,
		@scx_contact_last_name,
		@scx_contact_email,
		@scx_contact_phone,
		@scx_contact_fax
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
