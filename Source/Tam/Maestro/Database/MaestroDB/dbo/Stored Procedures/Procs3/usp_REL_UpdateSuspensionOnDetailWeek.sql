
/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** XXXXXX	    XXXXXXXX
** 04/17/2017	Abdul Sukkur	Added suspended_by_traffic_alert_id
*****************************************************************************************************/
CREATE PROCEDURE usp_REL_UpdateSuspensionOnDetailWeek
	@traffic_detail_week_id int,
	@suspend bit,
	@suspended_by_traffic_alert_id int = NULL
AS
BEGIN
	--set @traffic_detail_week_id = 3455;
	--set @suspend = 1;

	declare @otid int;
	declare @networkid int;
	declare @startdate datetime;
	declare @enddate datetime;
	declare @daypartid int;

	select
		  @networkid = td.network_id,
		  @startdate = tdw.start_date,
		  @enddate = tdw.end_date,
		  @daypartid = td.daypart_id
	FROM
		  traffic_details td WITH (NOLOCK) 
		  JOIN traffic_detail_weeks tdw WITH (NOLOCK) on tdw.traffic_detail_id = td.id
	WHERE
		  tdw.id = @traffic_detail_week_id;

	select 
		  @otid = case when t.original_traffic_id is null then t.id else t.original_traffic_id end
	from traffic_detail_weeks tdw WITH (NOLOCK)
		  join traffic_details td WITH (NOLOCK) on td.id = tdw.traffic_detail_id
		  join traffic t WITH (NOLOCK) on t.id = td.traffic_id
	where
		  tdw.id = @traffic_detail_week_id

	IF(@suspend = 0)
	BEGIN
		SET @suspended_by_traffic_alert_id = NULL
	END

	update
		 tdw set suspended = @suspend ,
		 suspended_by_traffic_alert_id = @suspended_by_traffic_alert_id
	from traffic_detail_weeks tdw WITH (NOLOCK)
	join traffic_details td WITH (NOLOCK) on td.id = tdw.traffic_detail_id 
	join traffic t WITH (NOLOCK) on t.id = td.traffic_id
	WHERE
		  tdw.start_date = @startdate
		  and tdw.end_date = @enddate
		  and td.network_id = @networkid
		  and td.daypart_id = @daypartid
		  and (t.id = @otid or t.original_traffic_id = @otid)
		  and suspended = ~@suspend ;
END
