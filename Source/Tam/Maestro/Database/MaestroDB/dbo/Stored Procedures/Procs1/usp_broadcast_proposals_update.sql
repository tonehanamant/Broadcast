CREATE PROCEDURE [dbo].[usp_broadcast_proposals_update]
(
	@id		Int,
	@original_proposal_id		Int,
	@version_number		Int,
	@name		VarChar(127),
	@product_id		Int,
	@agency_company_id		Int,
	@advertiser_company_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@budget		Money,
	@share_book_id		Int,
	@HUTPUT_book_id		Int,
	@rating_adjustment		Decimal(18,0),
	@salesperson_employee_id		Int,
	@date_created		DateTime,
	@date_last_modified		DateTime,
	@buyer_note		VarChar(127),
	@buyer_contact_id		Int,
	@proposal_status_id		TinyInt
)
AS
UPDATE broadcast_proposals SET
	original_proposal_id = @original_proposal_id,
	version_number = @version_number,
	name = @name,
	product_id = @product_id,
	agency_company_id = @agency_company_id,
	advertiser_company_id = @advertiser_company_id,
	start_date = @start_date,
	end_date = @end_date,
	budget = @budget,
	share_book_id = @share_book_id,
	HUTPUT_book_id = @HUTPUT_book_id,
	rating_adjustment = @rating_adjustment,
	salesperson_employee_id = @salesperson_employee_id,
	date_created = @date_created,
	date_last_modified = @date_last_modified,
	buyer_note = @buyer_note,
	buyer_contact_id = @buyer_contact_id,
	proposal_status_id = @proposal_status_id
WHERE
	id = @id


