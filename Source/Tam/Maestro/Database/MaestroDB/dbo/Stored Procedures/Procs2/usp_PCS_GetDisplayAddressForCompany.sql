-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/16/2011
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayAddressForCompany]
	@company_id INT
AS
BEGIN
	SELECT
		s.name,
		s.code,
		at.name,
		a.*
	FROM
		company_addresses ca (NOLOCK)
		JOIN addresses a (NOLOCK) ON a.id=ca.address_id
		JOIN states s (NOLOCK) ON s.id=a.state_id
		JOIN address_types at (NOLOCK) ON at.id=a.address_type_id
	WHERE
		ca.company_id=@company_id
END
