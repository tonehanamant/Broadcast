-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/25/2014
-- Description:	Returns the number of intersecting days.
-- =============================================
-- SELECT dbo.GetIntersectingDaypartDays(0,0,1,1,1,1,1,1,0,1,0,1,0,1)
CREATE FUNCTION GetIntersectingDaypartDays
(
	@mon1 BIT,
	@tue1 BIT,
	@wed1 BIT,
	@thu1 BIT,
	@fri1 BIT,
	@sat1 BIT,
	@sun1 BIT,
	@mon2 BIT,
	@tue2 BIT,
	@wed2 BIT,
	@thu2 BIT,
	@fri2 BIT,
	@sat2 BIT,
	@sun2 BIT
)
RETURNS INT
AS
BEGIN
	DECLARE @return INT;
	SELECT 
		@return =
			CASE WHEN @mon1 & @mon2 = 1 THEN 1 ELSE 0 END +
			CASE WHEN @tue1 & @tue2 = 1 THEN 1 ELSE 0 END + 
			CASE WHEN @wed1 & @wed2 = 1 THEN 1 ELSE 0 END + 
			CASE WHEN @thu1 & @thu2 = 1 THEN 1 ELSE 0 END + 
			CASE WHEN @fri1 & @fri2 = 1 THEN 1 ELSE 0 END + 
			CASE WHEN @sat1 & @sat2 = 1 THEN 1 ELSE 0 END + 
			CASE WHEN @sun1 & @sun2 = 1 THEN 1 ELSE 0 END;
			
	RETURN @return;
END
