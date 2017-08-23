-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/9/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetMediaMonthsByYear]
	@year INT
AS
BEGIN
	SELECT mm.* FROM media_months mm (NOLOCK) WHERE mm.[year]=@year ORDER BY mm.start_date
END
