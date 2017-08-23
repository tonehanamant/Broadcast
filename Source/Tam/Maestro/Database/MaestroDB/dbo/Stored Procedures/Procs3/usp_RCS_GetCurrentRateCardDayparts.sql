-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/21/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetCurrentRateCardDayparts]
	@sales_model_id INT,
	@effective_date DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		d.id,
		d.code,
		d.name,
		d.start_time,
		d.end_time,
		d.mon,
		d.tue,
		d.wed,
		d.thu,
		d.fri,
		d.sat,
		d.sun 
	FROM 
		vw_ccc_daypart d
		JOIN sales_model_rate_card_dayparts smrcd (NOLOCK) ON smrcd.sales_model_id=@sales_model_id
			AND smrcd.rate_card_daypart_id=d.id
			AND smrcd.start_date<=@effective_date AND (smrcd.end_date>=@effective_date OR smrcd.end_date IS NULL)
END
