
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetContactPhoneNumbersForContacts]
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT 
		cpn.*
	FROM 
		contact_phone_numbers cpn (NOLOCK)
	WHERE 
		cpn.contact_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END