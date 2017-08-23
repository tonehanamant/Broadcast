
CREATE PROCEDURE [dbo].[usp_great_plains_customers_insert]
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
INSERT INTO great_plains_customers
(
	customer_number,
	customer_name,
	Address1,
	Address2,
	City,
	State,
	Zip,
	email,
	phone,
	date_modified,
	modified_by,
	include_ISCI,
	cmw_division_id
)
VALUES
(
	@customer_number,
	@customer_name,
	@Address1,
	@Address2,
	@City,
	@State,
	@Zip,
	@email,
	@phone,
	@date_modified,
	@modified_by,
	@include_ISCI,
	@cmw_division_id
)


