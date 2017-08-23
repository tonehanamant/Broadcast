CREATE PROCEDURE [wb].[usp_WB_GetScheduleItemsForAgencyAndDate]
(
	@agency_name varchar(127),
	@order_title varchar(127),
	@start_date datetime,
	@region_code VARCHAR(127)
)
AS
BEGIN
select 
	wbs.id, 
	wbs.region_code,
	mw.start_date [week date],
	mw.start_date + (d.ordinal - 1) [run date],
	wbs.start_time,
	wbs.end_time,
	d.code_3,
	wba.name,
	wbs.order_title,
	wbs.status,
	wbs.net_rate,
	wbs.agency_percent,
	wbs.gross_rate,
	wbs.tape_id,
	wbs.phone_number,
	wbs.sales_person,
	wbs.rate_card_rate,
	wbs.wb_agency_id
from 
	wb.wb_schedules (NOLOCK) wbs
	join dbo.media_weeks (NOLOCK) mw on wbs.media_week_id = mw.id 
	join dbo.days (NOLOCK) d on d.id = wbs.day_id
	join wb.wb_agencies (NOLOCK) wba on wba.id = wbs.wb_agency_id 
where
	wbs.order_title = @order_title and
	wba.name = @agency_name and
	mw.start_date = @start_date and
	wbs.region_code = @region_code
order by
	d.ordinal, 
	wbs.start_time
END
