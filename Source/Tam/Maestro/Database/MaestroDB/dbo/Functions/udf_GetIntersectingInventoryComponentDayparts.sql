-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/3/2015
-- Description:	Looks up the component inventory dayparts which would make up the daypart defined by the parameters.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetIntersectingInventoryComponentDayparts]
(	
	@start_time INT,
	@end_time INT,
	@mon BIT,
	@tue BIT,
	@wed BIT,
	@thu BIT,
	@fri BIT,
	@sat BIT,
	@sun BIT
)
RETURNS @dayparts TABLE
(
	id INT,
	start_time INT,
	end_time INT,
	mon BIT,
	tue BIT,
	wed BIT,
	thu BIT,
	fri BIT,
	sat BIT,
	sun BIT,
	total_hours INT
)
AS
BEGIN
	DECLARE @weekdays BIT
	DECLARE @weekends BIT
	SET @weekdays = (@mon | @tue | @wed | @thu | @fri)
	SET @weekends = (@sat| @sun)
	
	DECLARE @component_dayparts TABLE (daypart_id INT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, mon BIT NOT NULL, tue BIT NOT NULL, wed BIT NOT NULL, thu BIT NOT NULL, fri BIT NOT NULL, sat BIT NOT NULL, sun BIT NOT NULL, total_hours INT NOT NULL);
	INSERT INTO @component_dayparts
		SELECT * FROM dbo.GetInventoryComponentDayparts();

	IF @end_time < @start_time
		BEGIN
			INSERT INTO @dayparts
				SELECT 
					cd.daypart_id,cd.start_time,cd.end_time,cd.mon,cd.tue,cd.wed,cd.thu,cd.fri,cd.sat,cd.sun,cd.total_hours
				FROM 
					@component_dayparts cd
				WHERE 
					((cd.start_time<86400) AND (cd.end_time>@start_time))
					AND ((cd.mon=@weekdays OR cd.tue=@weekdays OR cd.wed=@weekdays OR cd.thu=@weekdays OR cd.fri=@weekdays) OR (cd.sat=@weekends OR cd.sun=@weekends))

			INSERT INTO @dayparts
				SELECT 
					cd.daypart_id,cd.start_time,cd.end_time,cd.mon,cd.tue,cd.wed,cd.thu,cd.fri,cd.sat,cd.sun,cd.total_hours
				FROM 
					@component_dayparts cd
				WHERE 
					((start_time<@end_time) AND (end_time>0))
					AND ((cd.mon=@weekdays OR cd.tue=@weekdays OR cd.wed=@weekdays OR cd.thu=@weekdays OR cd.fri=@weekdays) OR (cd.sat=@weekends OR cd.sun=@weekends))
		END
	ELSE
		BEGIN
			INSERT INTO @dayparts
				SELECT 
					cd.daypart_id,cd.start_time,cd.end_time,cd.mon,cd.tue,cd.wed,cd.thu,cd.fri,cd.sat,cd.sun,cd.total_hours
				FROM 
					@component_dayparts cd
				WHERE 
					((cd.start_time <@end_time) and (cd.end_time>@start_time))
					AND ((cd.mon=@weekdays OR cd.tue=@weekdays OR cd.wed=@weekdays OR cd.thu=@weekdays OR cd.fri=@weekdays) OR (cd.sat=@weekends OR cd.sun=@weekends))
		END
	RETURN;
END