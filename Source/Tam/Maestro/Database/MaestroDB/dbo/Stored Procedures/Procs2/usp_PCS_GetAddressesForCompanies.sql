
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_PCS_GetAddressesForCompanies
	@ids VARCHAR(MAX)
AS
BEGIN
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
			SELECT address_id FROM company_addresses (NOLOCK) WHERE company_id IN (
				SELECT id FROM dbo.SplitIntegers(@ids)
			)
		)
END

