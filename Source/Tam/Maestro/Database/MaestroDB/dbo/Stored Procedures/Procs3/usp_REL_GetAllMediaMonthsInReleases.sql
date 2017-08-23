
CREATE PROCEDURE [dbo].[usp_REL_GetAllMediaMonthsInReleases] (
	@return_only_months_orders bit = 0
)
AS
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED  -- same as NOLOCK

	if(@return_only_months_orders = 0)
	BEGIN
		--return all media months from 2 years ago until 2 months from now.
		select 
			media_months.id, 
			media_months.month, 
			media_months.year,
			media_months.media_month,
			media_months.start_date,
			media_months.end_date
		from 
			media_months
		WHERE 
			media_months.start_date >= DATEADD(year, -1, GETDATE())
			and
			media_months.end_date <= DATEADD(month, 2, GETDATE())
		group by media_months.id, 
			media_months.month, 
			media_months.year,
			media_months.media_month,
			media_months.start_date,
			media_months.end_date
		order by
			media_months.id desc;
	END
	ELSE
	BEGIN
	--query the release and traffic_orders table to get only months that return valid data
	CREATE TABLE #temp_release_dates (release_id int,start_date datetime);
	INSERT INTO #temp_release_dates (release_id,start_date)
		   SELECT releases.id,
				 case when min(traffic_orders.start_date) is null then releases.release_date else min(traffic_orders.start_date) end
			FROM releases
			left join traffic_orders on traffic_orders.release_id = releases.id
	GROUP BY
		releases.id,
		releases.release_date;

	select 
		distinct media_months.id, 
		media_months.month, 
		media_months.year,
		media_months.media_month,
		media_months.start_date,
		media_months.end_date
	from 
		media_months
		cross join releases
		join #temp_release_dates on #temp_release_dates.release_id = releases.id
	WHERE 
		#temp_release_dates.start_date >= media_months.start_date
		and
		#temp_release_dates.start_date <= media_months.end_date
	order by
		media_months.id;

	--drop table #temp_release_dates;  it's faster to let SQL SERVER remove the temp table
END
