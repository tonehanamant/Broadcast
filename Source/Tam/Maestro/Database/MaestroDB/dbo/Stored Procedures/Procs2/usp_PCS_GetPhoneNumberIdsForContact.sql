
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_PCS_GetPhoneNumberIdsForContact
	@contact_id INT
AS
BEGIN
	SELECT 
		phone_number_id 
	FROM 
		contact_phone_numbers (NOLOCK)
	WHERE 
		contact_id=@contact_id
END