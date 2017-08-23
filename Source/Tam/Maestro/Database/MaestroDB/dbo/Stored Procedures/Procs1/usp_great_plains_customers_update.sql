
CREATE PROCEDURE [dbo].[usp_great_plains_customers_update]
(
	@customer_number		Char(8),
	@customer_name		VarChar(65),
	@Address1		VarChar(128),
	@Address2		VarChar(64),
	@City		VarChar(50),
	@State		Char(2),
	@Zip		VarChar(10),
	@email		VarChar(50),
	@phone		VarChar(25),
	@date_modified		SmallDateTime,
	@modified_by		VarChar(20),
	@include_ISCI		Bit,
	@cmw_division_id		Int
)
AS
UPDATE great_plains_customers SET
	customer_name = @customer_name,
	Address1 = @Address1,
	Address2 = @Address2,
	City = @City,
	State = @State,
	Zip = @Zip,
	email = @email,
	phone = @phone,
	date_modified = @date_modified,
	modified_by = @modified_by,
	include_ISCI = @include_ISCI,
	cmw_division_id = @cmw_division_id
WHERE
	customer_number = @customer_number

