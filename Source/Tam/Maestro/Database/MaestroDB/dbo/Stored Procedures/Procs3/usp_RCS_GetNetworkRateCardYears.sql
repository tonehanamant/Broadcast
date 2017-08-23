
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetNetworkRateCardYears]
	@sales_model_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		DISTINCT year 
	FROM 
		network_rate_card_books (NOLOCK)
	WHERE
		sales_model_id=@sales_model_id
	ORDER BY 
		year DESC
END

