-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/8/2014
-- Description:	Given a media month, returns the next month incremented by the parameter.
--				Procedure can accept negative months to add as well as positive, and also accepts 0 (reflective).
--				Returns media_months.id
--				Given 0114 and num months to add equal to 12, it returns 0115
--				Given 0114 and num months to add equal to -12, it returns 0113
-- =============================================
-- SELECT dbo.udf_CalculateFutureMediaMonthId(388,12) -- given 0114 (id=388) returns 0115 (id=400)
-- SELECT dbo.udf_CalculateFutureMediaMonthId(388,-12) -- given 0114 (id=388) returns 0113 (id=376)
CREATE FUNCTION [dbo].[udf_CalculateFutureMediaMonthId]
(
	@start_media_month_id INT,
	@num_months_to_add INT
)
RETURNS INT
AS
BEGIN
	DECLARE @return INT;
	DECLARE @start_month INT;
	DECLARE @start_year INT;
	DECLARE @pivot_rownum INT;

	IF @num_months_to_add = 0
		RETURN @start_media_month_id;
		
	SELECT
		@start_month = mm.month,
		@start_year = mm.year
	FROM
		dbo.media_months mm (NOLOCK)
	WHERE
		mm.id=@start_media_month_id;
		
	SELECT
		@pivot_rownum = tmp.rownum
	FROM (
		SELECT
			mm.id,
			ROW_NUMBER() OVER(ORDER BY start_date) 'rownum',
			CASE WHEN mm.month = @start_month AND mm.year=@start_year THEN 1 ELSE 0 END 'pivot_month'
		FROM
			dbo.media_months mm
	) tmp
	WHERE
		tmp.pivot_month=1;
		
	SELECT
		@return = id
	FROM (
		SELECT
			mm.id,
			ROW_NUMBER() OVER(ORDER BY start_date) 'rownum'
		FROM
			dbo.media_months mm
	) tmp
	WHERE
		tmp.rownum=@pivot_rownum+@num_months_to_add;
	
	RETURN @return;
END
