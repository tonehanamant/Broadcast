

CREATE PROCEDURE [dbo].[usp_TCS_GetAudienceImpressionsForMarried]
(
            @traffic_id as int,
            @audience_id as int
)
AS
with A1 (network_id, audience_id, traffic_rating, proposal_rating, proposal_spots, traffic_detail_id, proposal_universe, start_date, 
hh_universe, us_universe, daypart_id, traffic_rate, proposal_rate, proposal_detail_id, status_id)
as
(
      select distinct 
      traffic_details.network_id, 
      traffic_detail_audiences.audience_id, 
      traffic_detail_audiences.traffic_rating,
      case when pda1.rating is null then 0.0 else pda1.rating end, 
      m1.proposal_spots, 
      traffic_details.id, 
      case when pda1.us_universe * pd1.universal_scaling_factor is null then 0.0 else pda1.us_universe * pd1.universal_scaling_factor end , 
      traffic_detail_weeks.start_date, 
      (select tda2.us_universe from traffic_detail_audiences (NOLOCK) as tda2, traffic_details (NOLOCK) as td2 where td2.id = tda2.traffic_detail_id and tda2.audience_id = 31 and td2.id = traffic_details.id),
      traffic_detail_audiences.us_universe, 
      traffic_details.daypart_id, 
      m1.proposal_rate,
      pd1.proposal_rate,
      m1.proposal_detail_id,
      traffic.status_id
            from traffic_detail_audiences (NOLOCK) 
            join traffic_details (NOLOCK) on traffic_detail_audiences.traffic_detail_id = traffic_details.id 
            join traffic (NOLOCK) on traffic.id = traffic_details.traffic_id
            join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id
            join traffic_detail_topographies (NOLOCK) on traffic_detail_topographies.traffic_detail_week_id = traffic_detail_weeks.id 
            join traffic_audiences (NOLOCK) on traffic_audiences.audience_id = traffic_detail_audiences.audience_id and traffic_audiences.traffic_id = traffic_details.traffic_id
            join traffic_proposals tp1 on tp1.traffic_id = traffic_details.traffic_id and traffic_audiences.traffic_id = tp1.traffic_id and tp1.primary_proposal = 1
            join traffic_details_proposal_details_map m1 (NOLOCK) on m1.traffic_detail_id = traffic_details.id 
            JOIN proposal_details pd1 (NOLOCK) ON pd1.proposal_id = tp1.proposal_id and pd1.id = m1.proposal_detail_id 
            LEFT JOIN proposal_detail_audiences pda1 (NOLOCK) ON pd1.id=pda1.proposal_detail_id and 
                  pda1.audience_id=traffic_detail_audiences.audience_id
      where 
            traffic_details.traffic_id = @traffic_id and traffic_detail_audiences.audience_id = @audience_id
      GROUP BY 
            traffic_details.network_id, 
            pd1.proposal_id, 
            traffic_detail_audiences.audience_id, 
            traffic_detail_audiences.traffic_rating, 
            pda1.rating, 
            m1.proposal_spots, 
            traffic_details.id,     
            pda1.us_universe, 
            pd1.universal_scaling_factor, 
            traffic_detail_weeks.start_date, 
            traffic_audiences.universe, 
            traffic_detail_audiences.us_universe,     
            traffic_details.daypart_id, 
            m1.proposal_rate, 
            pd1.proposal_rate,
            m1.proposal_detail_id,
            traffic.status_id
)
select 
      A1.network_id, 
      A1.audience_id, 
      A1.traffic_rating, 
      A1.proposal_rating, 
      A1.proposal_spots, 
      A1.traffic_detail_id, 
      A1.proposal_universe, 
      A1.start_date, 
      A1.hh_universe, 
      A1.us_universe, 
      A1.daypart_id, 
      A1.traffic_rate, 
      A1.proposal_rate, 
      A1.proposal_detail_id, 
      m2.proposal_rate [traffic_rate], 
      pd2.proposal_rate, 
      pd2.id, 
      case when pda2.rating is null then 0.0 else pda2.rating end, 
      m2.proposal_spots, 
      case when pda2.us_universe * pd2.universal_scaling_factor is null then 0 else pda2.us_universe * pd2.universal_scaling_factor end,
      A1.status_id
from A1
      join traffic_details_proposal_details_map m2 (NOLOCK) on m2.traffic_detail_id = A1.traffic_detail_id 
      join traffic_proposals (NOLOCK) tp2 on tp2.traffic_id = @traffic_id and tp2.primary_proposal = 0
      JOIN proposal_details pd2 (NOLOCK) ON pd2.proposal_id = tp2.proposal_id and pd2.id = m2.proposal_detail_id 
      LEFT JOIN proposal_detail_audiences pda2 (NOLOCK) ON pd2.id=pda2.proposal_detail_id and 
                  pda2.audience_id=A1.audience_id
order by A1.network_id
