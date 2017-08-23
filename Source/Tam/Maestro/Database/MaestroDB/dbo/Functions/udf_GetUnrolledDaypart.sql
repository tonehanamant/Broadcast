
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetUnrolledDaypart]
(	
	@idDaypart int
)
RETURNS TABLE
AS
RETURN 
(
	with
	--tsx unrolls timespans that cross midnight, i.e., end_time < start_time, 
	--into two timespans
	--	A. start_time through midnight
	--	B. midnight through start_time
	--Both unrolled timespans share the same timespan_id.
	--
	--tsx adds a day_offset field. day_offset is the number of calendar days 
	--between the start_time of the original timespan, which is identified by 
	--timespan_id,and the timespan specified by the current row. In practice, 
	--day_offset has two potential values, 0 or 1.
	--	For all timespans that do not cross midnight, day_offest = 0.
	--	For all timespans that do cross midnight, 
	--		For unrolled timespan A, day_offset = 0.
	--		For unrolled timespan B, day_offset = 1.
	tsx(
		timespan_id,
		start_time,
		end_time,
		day_offset
	) as (
		select
			timespans.id timespan_id,
			timespans.start_time start_time,
			timespans.end_time end_time,
			0 day_offset
		from
			timespans with (nolock)
		where
			start_time <= end_time
		union all
		select
			timespans.id timespan_id,
			timespans.start_time start_time,
			86399 end_time,
			0 day_offset
		from
			timespans with (nolock)
		where
			start_time > end_time
		union all
		select
			timespans.id timespan_id,
			0 start_time,
			timespans.end_time end_time,
			1 day_offset
		from
			timespans with (nolock)
		where
			start_time > end_time
	)
	--Return a denormalized view of dayparts that uses the unrolled timespans 
	--in tsx. The join to days applies tsx.day_offset to determine the correct 
	--calendar day for the row, which is contained in the actual_day_* fields. 
	--
	--adp adds two fields, offset_start_time and offset_end_time, which add
	--one day, 86,400 seconds, to unrolled start_time and end_times in timespan B 
	--rows.
	select
		dayparts.id daypart_id,
		dayparts.daypart_text daypart_text,
		timespans.timespan_id timespan_id,
		timespans.start_time,
		timespans.end_time,
		timespans.day_offset,
		timespans.start_time + (86400 * timespans.day_offset) offset_start_time,
		timespans.end_time + (86400 * timespans.day_offset) offset_end_time,
		actual_days.id actual_day_id,
		actual_days.code_2 actual_day_code,
		actual_days.name actual_day_name,
		actual_days.ordinal actual_day_ordinal
	from
		dayparts with (nolock)
		join tsx timespans on
			timespans.timespan_id = dayparts.timespan_id
		join daypart_days with (nolock) on
			dayparts.id = daypart_days.daypart_id
		join days with (nolock) on
			days.id = daypart_days.day_id
		join days actual_days on
			actual_days.ordinal = 1 + ((days.ordinal - 1) + timespans.day_offset) % 7
	where
		@idDaypart = dayparts.id
);

