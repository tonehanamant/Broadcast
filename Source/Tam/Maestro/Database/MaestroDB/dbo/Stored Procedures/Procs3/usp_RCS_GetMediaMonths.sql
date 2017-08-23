
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetMediaMonths]
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
		end_date
	FROM 
		media_months (NOLOCK)
	ORDER BY
		year DESC,
		month ASC
END

