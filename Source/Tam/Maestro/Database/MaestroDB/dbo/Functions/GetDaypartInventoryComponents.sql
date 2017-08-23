
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/10/2009
-- Description:	Looks up the component inventory dayparts which would make up the daypart defined by the parameters.
-- =============================================
CREATE FUNCTION [dbo].[GetDaypartInventoryComponents]
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
				SELECT 
					id,code,name,tier,start_time,end_time,mon,tue,wed,thu,fri,sat,sun 
				FROM 
					vw_ccc_daypart (NOLOCK)
				WHERE 
					id IN (SELECT daypart_id FROM daypart_maps (NOLOCK) WHERE map_set='Inventories')
					AND ((start_time < 86400) and (end_time > @start_time))
					AND ((mon=@weekdays OR tue=@weekdays OR wed=@weekdays OR thu=@weekdays OR fri=@weekdays) OR (sat=@weekends OR sun=@weekends))
				ORDER BY name

			INSERT INTO @dayparts
				SELECT 
					id,code,name,tier,start_time,end_time,mon,tue,wed,thu,fri,sat,sun 
				FROM 
					vw_ccc_daypart (NOLOCK)
				WHERE 
					id IN (SELECT daypart_id FROM daypart_maps (NOLOCK) WHERE map_set='Inventories')
					AND ((start_time < @end_time) and (end_time > 0))
					AND ((mon=@weekdays OR tue=@weekdays OR wed=@weekdays OR thu=@weekdays OR fri=@weekdays) OR (sat=@weekends OR sun=@weekends))
				ORDER BY name
		END
	ELSE
		BEGIN
			INSERT INTO @dayparts
				SELECT 
					id,code,name,tier,start_time,end_time,mon,tue,wed,thu,fri,sat,sun 
				FROM 
					vw_ccc_daypart (NOLOCK)
				WHERE 
					id IN (SELECT daypart_id FROM daypart_maps (NOLOCK) WHERE map_set='Inventories')
					AND ((start_time < @end_time) and (end_time > @start_time))
					AND ((mon=@weekdays OR tue=@weekdays OR wed=@weekdays OR thu=@weekdays OR fri=@weekdays) OR (sat=@weekends OR sun=@weekends))
				ORDER BY name
		END
	RETURN;
END
