-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetProductsForCompanyIds]
	@ids VARCHAR(MAX)
AS
BEGIN
    SELECT 
		id,
		name,
		description,
		company_id,
		default_rate_card_type_id,
		geo_sensitive_comment,
		pol_sensitive_comment,
		date_created,
		date_last_modified,
		display_name
	FROM 
		products (NOLOCK) 
	WHERE 
		company_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END
