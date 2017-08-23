
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/16/2008
-- Description:	Looks up the component dayparts which would make up the daypart defined by the parameters.
-- =============================================
CREATE FUNCTION [dbo].[GetDaypartComponentsEx]
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
	sun INT,
	seconds int,
	out_of_seconds int
)
AS
BEGIN
	DECLARE @secondsPerDay AS INT;
	DECLARE	@secondsPerQuantum AS INT;
	DECLARE @weekdays BIT;
	DECLARE @weekends BIT;
	DECLARE @out_of_days INT;
	DECLARE @out_of_seconds INT;

	SET @secondsPerDay = 60 * 60 * 24;
	SET @secondsPerQuantum = 1;
	SET @weekdays = (@monday | @tuesday | @wednesday | @thursday | @friday)
	SET @weekends = (@saturday | @sunday)

	IF @end_time < @start_time
	BEGIN
		set @out_of_seconds = ((@end_time + @secondsPerQuantum + @secondsPerDay) - @start_time) * @out_of_days;

		INSERT INTO @dayparts
			SELECT 
				id,code,name,tier,start_time,end_time,mon,tue,wed,thu,fri,sat,sun,
				(
					((mon & @monday) + 
					 (tue & @tuesday) + 
					 (wed & @wednesday) + 
					 (thu & @thursday) + 
					 (fri & @friday) + 
					 (sat & @saturday) + 
					 (sun & @sunday)) 
				) * (
					case
						when end_time < @secondsPerDay then end_time
						else @secondsPerDay
					end - 
					case
						when start_time >  @start_time then start_time
						else @start_time
					end + @secondsPerQuantum
				) seconds,
				@out_of_seconds
			FROM 
				vw_ccc_daypart (NOLOCK)
			WHERE 
				id IN (SELECT daypart_id FROM daypart_maps (NOLOCK) WHERE map_set='Ratings')
				AND 
				((start_time < @secondsPerDay) and (end_time > @start_time))
				AND 
				((mon & @monday) + 
				 (tue & @tuesday) + 
				 (wed & @wednesday) + 
				 (thu & @thursday) + 
				 (fri & @friday) + 
				 (sat & @saturday) + 
				 (sun & @sunday)) > 0
			ORDER BY name

		INSERT INTO @dayparts
			SELECT 
				id,code,name,tier,start_time,end_time,mon,tue,wed,thu,fri,sat,sun ,
				(
					((mon & @monday) + 
					 (tue & @tuesday) + 
					 (wed & @wednesday) + 
					 (thu & @thursday) + 
					 (fri & @friday) + 
					 (sat & @saturday) + 
					 (sun & @sunday)) 
				) * (
					case
						when end_time < @end_time then end_time
						else @end_time
					end - 
					case
						when start_time > 0 then start_time
						else 0
					end + @secondsPerQuantum
				) seconds,
				@out_of_seconds
			FROM 
				vw_ccc_daypart (NOLOCK)
			WHERE 
				id IN (SELECT daypart_id FROM daypart_maps (NOLOCK) WHERE map_set='Ratings')
				AND ((start_time < @end_time) and (end_time > 0))
				AND 
				((mon & @monday) + 
				 (tue & @tuesday) + 
				 (wed & @wednesday) + 
				 (thu & @thursday) + 
				 (fri & @friday) + 
				 (sat & @saturday) + 
				 (sun & @sunday)) > 0
			ORDER BY name
	END
	ELSE
	BEGIN
		set @out_of_seconds = ((@end_time + @secondsPerQuantum) - @start_time) * @out_of_days;

		INSERT INTO @dayparts
			SELECT 
				id,code,name,tier,start_time,end_time,mon,tue,wed,thu,fri,sat,sun ,
				(
					((mon & @monday) + 
					 (tue & @tuesday) + 
					 (wed & @wednesday) + 
					 (thu & @thursday) + 
					 (fri & @friday) + 
					 (sat & @saturday) + 
					 (sun & @sunday)) 
				) * (
					case
						when end_time < @end_time then end_time
						else @end_time
					end - 
					case
						when start_time >  @start_time then start_time
						else @start_time
					end + @secondsPerQuantum
				) seconds,
				@out_of_seconds
			FROM 
				vw_ccc_daypart (NOLOCK)
			WHERE 
				id IN (SELECT daypart_id FROM daypart_maps (NOLOCK) WHERE map_set='Ratings')
				AND ((start_time < @end_time) and (end_time > @start_time))
				AND 
				((mon & @monday) + 
				 (tue & @tuesday) + 
				 (wed & @wednesday) + 
				 (thu & @thursday) + 
				 (fri & @friday) + 
				 (sat & @saturday) + 
				 (sun & @sunday)) > 0
			ORDER BY name
	END;
	RETURN;
END
