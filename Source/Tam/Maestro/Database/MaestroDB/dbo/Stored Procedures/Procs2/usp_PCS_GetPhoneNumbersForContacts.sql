
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_PCS_GetPhoneNumbersForContacts
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT 
		id,
		phone_number_type_id,
		phone_number,
		extension,
		date_created,
		date_last_modified
	FROM 
		phone_numbers (NOLOCK)
	WHERE 
		id IN (
			SELECT phone_number_id FROM contact_phone_numbers (NOLOCK) WHERE contact_id IN (
				SELECT id FROM dbo.SplitIntegers(@ids)
			)
		)
END
