-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/16/2008
-- Updated:		7/15/2015 - fixed boundary issue, not all dayparts were being included that should have been.
-- Description:	Looks up the component dayparts which would make up the daypart defined by the parameters.
-- =============================================
-- SELECT * FROM dbo.GetDaypartComponents(86399,0,1,1,1,1,1,0,0) ORDER BY start_time
CREATE FUNCTION [dbo].[GetDaypartComponents]
(
	@start_time INT,
	@end_time INT,
	@monday BIT,
	@tuesday BIT,
	@wednesday BIT,
	@thursday BIT,
	@friday BIT,
	@saturday BIT,
	@sunday BIT
)
RETURNS @dayparts TABLE
(
	id INT,
	code VARCHAR(15),
	name VARCHAR(63),
	tier INT,
	start_time INT,
	end_time INT,
	mon INT,
	tue INT,
	wed INT,
	thu INT,
	fri INT,
	sat INT,
	sun INT
)
AS
BEGIN
	DECLARE @weekdays BIT
	DECLARE @weekends BIT
	SET @weekdays = (@monday | @tuesday | @wednesday | @thursday | @friday)
	SET @weekends = (@saturday | @sunday)

	IF @end_time < @start_time
		BEGIN
			INSERT INTO @dayparts
				SELECT DISTINCT
					id,code,name,tier,start_time,end_time,mon,tue,wed,thu,fri,sat,sun 
				FROM (
					SELECT
						id,code,name,tier,start_time,end_time,mon,tue,wed,thu,fri,sat,sun 
					FROM 
						vw_ccc_daypart (NOLOCK)
					WHERE 
						id IN (SELECT daypart_id FROM daypart_maps (NOLOCK) WHERE map_set='Ratings')
						AND (start_time <= 86399 and end_time >= @start_time)
						AND ((mon=@weekdays OR tue=@weekdays OR wed=@weekdays OR thu=@weekdays OR fri=@weekdays) OR (sat=@weekends OR sun=@weekends))
					
					UNION ALL
					
					SELECT 
						id,code,name,tier,start_time,end_time,mon,tue,wed,thu,fri,sat,sun 
					FROM 
						vw_ccc_daypart (NOLOCK)
					WHERE 
						id IN (SELECT daypart_id FROM daypart_maps (NOLOCK) WHERE map_set='Ratings')
						AND (start_time <= @end_time and end_time >= 0)
						AND ((mon=@weekdays OR tue=@weekdays OR wed=@weekdays OR thu=@weekdays OR fri=@weekdays) OR (sat=@weekends OR sun=@weekends))
				) tmp
		END
	ELSE
		BEGIN
			INSERT INTO @dayparts
				SELECT DISTINCT
					id,code,name,tier,start_time,end_time,mon,tue,wed,thu,fri,sat,sun 
				FROM 
					vw_ccc_daypart (NOLOCK)
				WHERE 
					id IN (SELECT daypart_id FROM daypart_maps (NOLOCK) WHERE map_set='Ratings')
					AND (start_time <= @end_time and end_time >= @start_time)
					AND ((mon=@weekdays OR tue=@weekdays OR wed=@weekdays OR thu=@weekdays OR fri=@weekdays) OR (sat=@weekends OR sun=@weekends))
				ORDER BY name
		END
	RETURN;
END
