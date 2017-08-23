-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 8/20/2010
-- Description: Gets a list of quarters corresponding to all created proposals.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayQuartersForProposals]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @start_date DATETIME
	DECLARE @end_date DATETIME

	SELECT @start_date = MIN(start_date), @end_date = MAX(end_date) FROM proposals p (NOLOCK)

    SELECT DISTINCT
		CASE mm.[month] WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END 'quarter',
		mm.[year]
	FROM
		media_months mm (NOLOCK)
	WHERE
		(mm.start_date <= @end_date AND mm.end_date >= @start_date)
	ORDER BY
		mm.[year] DESC,
		[quarter] DESC
END
