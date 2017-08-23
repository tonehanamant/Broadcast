-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/11/2014 10:31:07 AM
-- Description:	Auto-generated method to update a companies record.
-- =============================================
CREATE PROCEDURE usp_companies_update
	@id INT,
	@name VARCHAR(63),
	@url VARCHAR(127),
	@company_status_id INT,
	@salesperson_employee_id INT,
	@geo_sensitive_comment VARCHAR(2047),
	@pol_sensitive_comment VARCHAR(2047),
	@additional_information TEXT,
	@enabled BIT,
	@account_status_id INT,
	@default_rate_card_type_id INT,
	@default_billing_terms_id INT,
	@date_created DATETIME,
	@date_last_modified DATETIME,
	@display_name VARCHAR(63),
	@default_is_msa BIT,
	@agency_dds_idb_number VARCHAR(31)
AS
BEGIN
	UPDATE
		[dbo].[companies]
	SET
		[name]=@name,
		[url]=@url,
		[company_status_id]=@company_status_id,
		[salesperson_employee_id]=@salesperson_employee_id,
		[geo_sensitive_comment]=@geo_sensitive_comment,
		[pol_sensitive_comment]=@pol_sensitive_comment,
		[additional_information]=@additional_information,
		[enabled]=@enabled,
		[account_status_id]=@account_status_id,
		[default_rate_card_type_id]=@default_rate_card_type_id,
		[default_billing_terms_id]=@default_billing_terms_id,
		[date_created]=@date_created,
		[date_last_modified]=@date_last_modified,
		[display_name]=@display_name,
		[default_is_msa]=@default_is_msa,
		[agency_dds_idb_number]=@agency_dds_idb_number
	WHERE
		[id]=@id
END

