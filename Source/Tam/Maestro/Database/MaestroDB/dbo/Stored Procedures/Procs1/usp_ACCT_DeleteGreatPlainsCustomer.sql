
-- =============================================
-- Author:		John Carsley
-- Create date: 07/05/2011
-- Description:	Removes a great_plains_customers record
-- Usage: exec usp_ACCT_DeleteGreatPlainsCustomer '6789'
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACCT_DeleteGreatPlainsCustomer]
	 @customer_number varchar(8)
AS
BEGIN
	SET NOCOUNT ON;
		
    DELETE great_plains_customers
    WHERE customer_number = @customer_number
    
END
