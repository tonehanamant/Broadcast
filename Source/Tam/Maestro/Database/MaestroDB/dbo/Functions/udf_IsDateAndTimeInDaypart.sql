
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/9/2010
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[udf_IsDateAndTimeInDaypart]
(	
	@date datetime,
	@secondsAfterMidnight int,
	@idDaypart int
)
RETURNS TABLE 
AS
RETURN (
	with
	instant(
		day_id,
		next_day_id,
		seconds_past_midnight
	) as (
		select
			days.id day_id,
			next_days.id next_day_id,
			wd.seconds_past_midnight
		from
			(
				select
					datename(weekday, @date) day_name,
					datename(weekday, dateadd(day, 1, @date)) next_day_name,
					datediff(second, cast(convert(varchar, @date, 111) as datetime), @date) seconds_past_midnight
			) wd
			join days on
				wd.day_name = days.name
			join days next_days on
				wd.next_day_name = next_days.name
	)
	select  
		cast(convert(varchar, @date, 111) as datetime) date_part,
		@secondsAfterMidnight seconds_past_midnight,
		@idDaypart daypart_id,
		count(*) in_daypart
	from
		dayparts
		join timespans on
			timespans.id = dayparts.timespan_id
		join daypart_days on
			dayparts.id = daypart_days.daypart_id
		join days on
			days.id = daypart_days.day_id
		join instant on
			(
				timespans.start_time < timespans.end_time
				and
				instant.seconds_past_midnight between timespans.start_time and timespans.end_time
				and
				days.id = instant.day_id
			)
			or
			(
				timespans.start_time > timespans.end_time
				and
				instant.seconds_past_midnight between timespans.start_time and 86399
				and
				days.id = instant.day_id
			)
			or
			(
				timespans.start_time > timespans.end_time
				and
				instant.seconds_past_midnight between 0 and timespans.end_time
				and
				days.id = instant.next_day_id
			)
	where
		dayparts.id = @idDaypart
);
