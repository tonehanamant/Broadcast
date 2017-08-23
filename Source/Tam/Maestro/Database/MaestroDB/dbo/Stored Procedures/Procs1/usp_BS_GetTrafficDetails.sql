
CREATE PROCEDURE [dbo].[usp_BS_GetTrafficDetails]
(
                @proposal_detail_id int
)
AS
BEGIN

DECLARE @start_date datetime;
declare @media_month_id int;

SELECT @start_date = min(start_date) from broadcast_proposal_detail_flights bpdf with (NOLOCK) where bpdf.selected = 1 and bpdf.broadcast_proposal_detail_id = @proposal_detail_id;

select @media_month_id = max(distinct mh.media_month_id) FROM external_rating.nsi.market_headers mh WITH (NOLOCK)
      join media_months mm WITH (NOLOCK) on mm.id = mh.media_month_id
where mm.month = 11;
      

CREATE TABLE #temp_sz (proposal_detail_id int, system_id int, zone_id int, revision int);

INSERT INTO #temp_sz (proposal_detail_id, system_id, zone_id, revision)
SELECT 
                btd.broadcast_proposal_detail_id, 
                btd.system_id, 
                btd.zone_id, 
                max(btd.revision) 
from broadcast_traffic_details btd with (NOLOCK) 
where
                btd.broadcast_proposal_detail_id = @proposal_detail_id
group by
                btd.broadcast_proposal_detail_id, 
                btd.system_id, 
                btd.zone_id;

SELECT 
                distinct 
                pmc.market_rank, 
                pmc.market_code, 
                max(pmc.station_code), 
                pmc.broadcast_channel_number, 
                pmc.call_letters, 
                tz.name, 
                tz.id, 
                d.name, 
                pmc.affiliation,
                bptd.id,
                bptd.broadcast_proposal_detail_id,
                bptd.revision,
                bptd.system_id,
                bptd.zone_id,
                bptd.accepted,
                bptd.ordered_dollars,
                bptd.impressions_spots,
                bptd.market_percentage,
                bptd.employee_id,
                bptd.notes
from 
                                external_rating.dbo.program_monthly_schedule pmc WITH (NOLOCK)
                                join zone_maps zm1 with (NOLOCK) on zm1.map_value = pmc.station_code and zm1.map_set = 'Brdcst Callltr'    
                                join uvw_zone_universe z WITH (NOLOCK) on z.zone_id = zm1.zone_id and (z.start_date<= @start_date AND (z.end_date>=@start_date OR z.end_date IS NULL))
                                join uvw_systemzone_universe sz with (NOLOCK) on sz.zone_id = z.zone_id and (sz.start_date<=@start_date AND (sz.end_date>=@start_date OR sz.end_date IS NULL))
                                join zone_maps zm with (NOLOCK) on zm.zone_id = z.zone_id and zm.map_set = 'Brdcst Timezone'
                                join dmas d with (NOLOCK) on (cast(d.code as int) - 400) = pmc.market_code 
                                join broadcast_traffic_details bptd with (NOLOCK) on bptd.zone_id = z.zone_id and bptd.system_id = sz.system_id
                                join #temp_sz on #temp_sz.proposal_detail_id = bptd.broadcast_proposal_detail_id and  #temp_sz.zone_id = bptd.zone_id and #temp_sz.system_id = bptd.system_id and #temp_sz.revision = bptd.revision
                                join time_zones tz on tz.id = cast(zm.map_value as int) 
WHERE
                bptd.broadcast_proposal_detail_id = @proposal_detail_id
				 and pmc.media_month_id = @media_month_id
GROUP BY
pmc.market_rank, 
                pmc.market_code, 
                pmc.broadcast_channel_number, 
                pmc.call_letters, 
                tz.name, 
                tz.id, 
                d.name, 
                pmc.affiliation,
                bptd.id,
                bptd.broadcast_proposal_detail_id,
                bptd.revision,
                bptd.system_id,
                bptd.zone_id,
                bptd.accepted,
                bptd.ordered_dollars,
                bptd.impressions_spots,
                bptd.market_percentage,
                bptd.employee_id,
                bptd.notes;

DROP TABLE #temp_sz;

END

