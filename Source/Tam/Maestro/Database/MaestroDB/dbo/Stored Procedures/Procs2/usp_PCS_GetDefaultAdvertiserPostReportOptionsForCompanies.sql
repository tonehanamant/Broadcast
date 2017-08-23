-- =============================================
-- Author:		Stephen DeFusco
-- Create date:	7/5/2011
-- Description:	Gets the default advertiser post report options for the specified companies.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDefaultAdvertiserPostReportOptionsForCompanies]
	@company_ids VARCHAR(MAX)
AS
BEGIN
	SELECT
		dcpro.*
	FROM
		default_company_post_report_options dcpro
	WHERE
		dcpro.company_id IN (
			SELECT id FROM dbo.SplitIntegers(@company_ids)
		)
END
