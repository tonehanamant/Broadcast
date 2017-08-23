CREATE procedure [dbo].[usp_ARSLoader_CheckReadyToPreCrunch]
as
BEGIN
	declare @lWeeksAmount int,
			@lWeekNumber int,
			@lReturn bit,
			@lMedia_month_id int

	select @lWeeksAmount= count(*) from media_weeks where media_month_id
	in (
	select
		id
	from
		media_months mm
	where
		(
			select
				max(end_date)
			from
				mit_universes
		) between mm.start_date and mm.end_date
	)

	select @lWeekNumber= week_number,
			@lMedia_month_id= media_month_id
	from media_weeks where end_date=(select
				max(end_date)
			from
				mit_universes)

	if @lWeeksAmount = @lWeekNumber
		set @lReturn = 1
	else
		set @lReturn = 0

	select @lReturn, @lMedia_month_id
END
