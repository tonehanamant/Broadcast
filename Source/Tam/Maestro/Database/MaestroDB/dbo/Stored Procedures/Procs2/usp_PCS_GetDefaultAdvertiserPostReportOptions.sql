-- =============================================
-- Author:		Stephen DeFusco
-- Create date:	7/5/2011
-- Description:	Gets the default advertiser post report options for the specified company.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDefaultAdvertiserPostReportOptions]
	@company_id INT
AS
BEGIN
	SELECT
		dcpro.*
	FROM
		default_company_post_report_options dcpro
	WHERE
		dcpro.company_id=@company_id
END
