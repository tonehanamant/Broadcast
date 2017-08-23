-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/28/2012
-- Description:	Get MediaMonth by code.
-- =============================================
CREATE PROCEDURE usp_PCS_GetMediaMonthByCode
	@media_month VARCHAR(15)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		mm.*
	FROM
		dbo.media_months mm (NOLOCK)
	WHERE
		mm.media_month=@media_month
END
