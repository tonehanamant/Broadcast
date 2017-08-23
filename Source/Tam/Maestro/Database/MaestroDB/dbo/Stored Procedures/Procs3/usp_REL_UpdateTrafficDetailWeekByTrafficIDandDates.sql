
/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** XXXXXX	    XXXXXXXX
** 04/17/2017	Abdul Sukkur	Added suspended_by_traffic_alert_id
*****************************************************************************************************/
CREATE Procedure [dbo].[usp_REL_UpdateTrafficDetailWeekByTrafficIDandDates]
    @traffic_id int,
	@start_date datetime,
	@end_date datetime,
	@suspend bit,
	@suspended_by_traffic_alert_id int = NULL
AS
BEGIN
	declare @otid as int;
	select 
		  @otid = case when t.original_traffic_id is null then t.id else t.original_traffic_id end
	from traffic t WITH (NOLOCK)
	where
		  t.id = @traffic_id

	IF(@suspend = 0)
	BEGIN
		SET @suspended_by_traffic_alert_id = NULL
	END

	update tdw 
		set tdw.suspended = @suspend,
		suspended_by_traffic_alert_id = @suspended_by_traffic_alert_id
	from 
		traffic_detail_weeks tdw WITH (NOLOCK)
		join traffic_details td WITH (NOLOCK) on tdw.traffic_detail_id = td.id
		join traffic t WITH (NOLOCK) on t.id = td.traffic_id
	where 
		(t.id = @otid or t.original_traffic_id = @otid)
		and tdw.start_date = @start_date 
		and tdw.end_date = @end_date
		and suspended = ~@suspend
END
