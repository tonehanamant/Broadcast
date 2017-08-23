-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetPhoneNumbersForContact]
	@contact_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

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
			SELECT phone_number_id FROM contact_phone_numbers (NOLOCK) WHERE contact_id=@contact_id
		) 
	ORDER BY 
		date_last_modified
END
