CREATE PROCEDURE [dbo].[usp_PCS_GetProposalsForCompanies]
	@ids VARCHAR(MAX)
AS
BEGIN
	SELECT 
		*
	FROM 
		proposals (NOLOCK)
	WHERE 
		advertiser_company_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
		OR
		agency_company_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END

