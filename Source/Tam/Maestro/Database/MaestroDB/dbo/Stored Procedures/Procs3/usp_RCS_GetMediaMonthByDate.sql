-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 8/16/2013
-- Description:	
-- =============================================
CREATE PROCEDURE usp_RCS_GetMediaMonthByDate
	@date DATE
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		mm.*
	FROM
		dbo.media_months mm (NOLOCK)
	WHERE
		@date BETWEEN mm.start_date AND mm.end_date
END
