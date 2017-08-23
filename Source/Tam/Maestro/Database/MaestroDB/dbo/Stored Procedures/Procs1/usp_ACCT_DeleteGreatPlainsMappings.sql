-- =============================================
-- Author:		John Carsley
-- Create date: 06/27/2014
-- Description:	Deletes all great_plains_mappings records for a customer
-- =============================================
CREATE PROCEDURE usp_ACCT_DeleteGreatPlainsMappings
(
	@customer_number VARCHAR(8)
)
AS
BEGIN
	DELETE dbo.great_plains_mapping
	WHERE great_plains_customer_number = @customer_number
END

