﻿-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetAddressesForContact]
	@contact_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		id,
		name,
		address_type_id,
		address_line_1,
		address_line_2,
		address_line_3,
		city,
		state_id,
		zip,
		country_id,
		date_created,
		date_last_modified 
	FROM 
		addresses (NOLOCK) 
	WHERE 
		id IN (
			SELECT address_id FROM contact_addresses (NOLOCK) WHERE contact_id=@contact_id
		) 
	ORDER BY 
		name
END
