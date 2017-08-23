
CREATE PROCEDURE [dbo].[usp_SRS_GetTrafficOrderByRelease]
(
      @release_id Int
)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

select DISTINCT 
            trd.traffic_id pid,
            case
            when len(rtrim(ltrim(tr.display_name))) = 0 then tr.name +  ', Id: ' + cast(trd.traffic_id as varchar) 
            else tr.display_name +  ', Id: ' + cast(trd.traffic_id as varchar) 
            end [TrafficOrders]
    from
            releases r
            join traffic tr on      
                  tr.release_id = r.id
            join traffic_details trd on 
                  tr.id = trd.traffic_id
            join traffic_orders tro on
                  trd.id = tro.traffic_detail_id
      where
            r.id = @release_id
            and tro.on_financial_reports = 1
            and tro.active = 1
      order by
            [TrafficOrders]


END
