-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/25/2013
-- Description:	Checks to see if there exists a parent child relationship.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_LookupAgencyAdvertiserRelationship]
	@agency_company_id INT,
	@advertiser_company_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		cc.*
	FROM
		dbo.company_companies cc (NOLOCK)
	WHERE
		cc.parent_company_id=@agency_company_id
		AND cc.company_id=@advertiser_company_id
END
