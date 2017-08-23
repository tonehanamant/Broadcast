-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_GetRemainingMediaMonthsForSystemStatements]
	@statement_type TINYINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT	
		id,
		[year],
		[month],
		media_month, 
		start_date, 
		end_date 
	FROM
		media_months (NOLOCK) 
	WHERE
		id NOT IN (
			SELECT media_month_id FROM statements (NOLOCK) WHERE statement_type = @statement_type
		)
END
