-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_ACS_GetMediaMonthByMediaMonth
	@media_month VARCHAR(15)
AS
BEGIN
	SELECT
		mm.*
	FROM
		media_months mm (NOLOCK)
	WHERE
		mm.media_month = @media_month
END
