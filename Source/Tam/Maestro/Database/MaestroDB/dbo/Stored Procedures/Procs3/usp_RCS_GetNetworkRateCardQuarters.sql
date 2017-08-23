
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetNetworkRateCardQuarters]
	@sales_model_id INT,
	@year INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		DISTINCT 
			quarter,
			'Q' + CAST(quarter AS VARCHAR(1)) + SUBSTRING(CAST(year AS VARCHAR(4)),3,2)
	FROM 
		network_rate_card_books (NOLOCK)
	WHERE
		sales_model_id=@sales_model_id
		AND year=@year
		AND media_month_id IS NULL
	ORDER BY 
		quarter ASC
END

