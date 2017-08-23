
-- =============================================
-- Author:		<Author,,David Sisson>
-- Create date: <Create Date,,2008-08-11>
-- Description:	<Description,,>
-- =============================================
CREATE PROC [dbo].[usp_calculate_average_actual_ratings]
(
	@mediaMonth DateTime
)
AS
	declare
		@baseMediaMonth as datetime,
		@baseMediaMonthID as int;

	select
		@baseMediaMonth = dbo.Period2FirstOfMonth(bmm.media_month),
		@baseMediaMonthID = bmm.id
	from
		media_months bmm
	where
		@mediaMonth between bmm.start_date and bmm.end_date;
	
	select
		@baseMediaMonth base_month,
		@baseMediaMonthID base_media_month_id,
		ar.nielsen_network_id,
		ar.daypart_id,
		ar.audience_id,
		avg(ar.audience_usage) audience_usage,
		avg(ar.tv_usage) tv_usage,
		count(*) months_averaged
	from
		uvw_actual_ratings ar
		join media_months mm on
			mm.id = ar.media_month_id
	where
		@baseMediaMonth between mm.start_date and mm.end_date
		or
		dateadd(month,-1,@baseMediaMonth) between mm.start_date and mm.end_date
		or
		dateadd(month,-2,@baseMediaMonth) between mm.start_date and mm.end_date
	group by
		nielsen_network_id,
		daypart_id,
		audience_id;
