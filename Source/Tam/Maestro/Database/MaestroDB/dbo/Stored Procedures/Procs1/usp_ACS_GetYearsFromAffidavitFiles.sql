-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_ACS_GetYearsFromAffidavitFiles
AS
BEGIN
	SELECT 
		DISTINCT media_months.year
	FROM 
		affidavit_file_details 
		JOIN media_months ON media_months.id=affidavit_file_details.media_month_id 
	ORDER BY 
		media_months.year DESC
END
