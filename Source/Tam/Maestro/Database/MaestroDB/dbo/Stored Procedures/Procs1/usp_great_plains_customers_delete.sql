
CREATE PROCEDURE [dbo].[usp_great_plains_customers_delete]
(
	@customer_number		Char(8))
AS
DELETE FROM
	great_plains_customers
WHERE
	customer_number = @customer_number

