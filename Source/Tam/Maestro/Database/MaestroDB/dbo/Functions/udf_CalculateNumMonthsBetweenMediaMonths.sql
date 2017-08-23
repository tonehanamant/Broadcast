-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/27/2017
-- Description:	Given a starting month and ending month, calculate the number of media months between them. Always returns a positive number.
-- =============================================
-- SELECT dbo.udf_CalculateNumMonthsBetweenMediaMonths(388,400) -- given 0114 (id=388) returns 0115 (id=400)
CREATE FUNCTION [dbo].[udf_CalculateNumMonthsBetweenMediaMonths]
(
	@start_media_month_id INT,
	@end_media_month_id INT
)
RETURNS INT
AS
BEGIN
	DECLARE @start_row INT;
	DECLARE @end_row INT;
	
	SELECT
		@start_row = tmp.rownum
	FROM (
		SELECT
			mm.id 'media_month_id',
			ROW_NUMBER() OVER(ORDER BY start_date) 'rownum'
		FROM
			dbo.media_months mm
	) tmp
	WHERE
		tmp.media_month_id=@start_media_month_id

	SELECT
		@end_row = tmp.rownum
	FROM (
		SELECT
			mm.id 'media_month_id',
			ROW_NUMBER() OVER(ORDER BY start_date) 'rownum'
		FROM
			dbo.media_months mm
	) tmp
	WHERE
		tmp.media_month_id=@end_media_month_id
	
	RETURN ABS(@start_row-@end_row);
END