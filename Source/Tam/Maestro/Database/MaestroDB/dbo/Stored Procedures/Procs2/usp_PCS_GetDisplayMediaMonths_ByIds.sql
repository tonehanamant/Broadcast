
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetDisplayMediaMonths_ByIds '64,65'
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayMediaMonths_ByIds]
	@ids VARCHAR(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		id,
		year,
		month,
		media_month,
		start_date,
		end_date,
		(SELECT COUNT(*) FROM media_weeks (NOLOCK) WHERE media_weeks.media_month_id=media_months.id) 'weeks_in_month'
	FROM 
		media_months (NOLOCK)
	WHERE 
		id IN (SELECT id FROM dbo.SplitIntegers(@ids))
	ORDER BY
		year ASC,
		month ASC

	SELECT
		id,
		media_month_id
	FROM
		media_weeks (NOLOCK) 
	WHERE
		media_month_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END
