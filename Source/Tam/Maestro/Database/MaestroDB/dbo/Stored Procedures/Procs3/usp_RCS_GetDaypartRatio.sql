-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/21/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetDaypartRatio]
	@sales_model_id INT,
	@daypart_id INT,
	@effective_date DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		smrcd.ratio
	FROM
		sales_model_rate_card_dayparts smrcd (NOLOCK)
	WHERE
		smrcd.sales_model_id=@sales_model_id
		AND smrcd.rate_card_daypart_id=@daypart_id
		AND smrcd.start_date<=@effective_date AND (smrcd.end_date>=@effective_date OR smrcd.end_date IS NULL)
END
