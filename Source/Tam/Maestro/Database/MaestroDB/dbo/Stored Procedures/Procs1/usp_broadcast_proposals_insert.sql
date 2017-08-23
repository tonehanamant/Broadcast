CREATE PROCEDURE [dbo].[usp_broadcast_proposals_insert]
(
	@id		int		OUTPUT,
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
INSERT INTO broadcast_proposals
(
	original_proposal_id,
	version_number,
	name,
	product_id,
	agency_company_id,
	advertiser_company_id,
	start_date,
	end_date,
	budget,
	share_book_id,
	HUTPUT_book_id,
	rating_adjustment,
	salesperson_employee_id,
	date_created,
	date_last_modified,
	buyer_note,
	buyer_contact_id,
	proposal_status_id
)
VALUES
(
	@original_proposal_id,
	@version_number,
	@name,
	@product_id,
	@agency_company_id,
	@advertiser_company_id,
	@start_date,
	@end_date,
	@budget,
	@share_book_id,
	@HUTPUT_book_id,
	@rating_adjustment,
	@salesperson_employee_id,
	@date_created,
	@date_last_modified,
	@buyer_note,
	@buyer_contact_id,
	@proposal_status_id
)

SELECT
	@id = SCOPE_IDENTITY()


