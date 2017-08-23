CREATE PROCEDURE [dbo].[usp_TCS_get_proposal_details_by_id]
(
	@proposal_id Int
)
AS
select proposal_details.id, spot_lengths.length, network_id, 
networks.code 'network_code', networks.name 'network_name', num_spots,
proposal_details.start_date, proposal_details.end_date, proposal_details.proposal_rate,
dayparts.id as dayparts_id, 
dayparts.code as dayparts_code, dayparts.name as dayparts_name,
timespans.start_time, timespans.end_time, dayparts.tier,
(select count(*) from daypart_days where daypart_days.daypart_id=dayparts.id and daypart_days.day_id=1) as Mon,
(select count(*) from daypart_days where daypart_days.daypart_id=dayparts.id and daypart_days.day_id=2) as Tue,
(select count(*) from daypart_days where daypart_days.daypart_id=dayparts.id and daypart_days.day_id=3) as Wed,
(select count(*) from daypart_days where daypart_days.daypart_id=dayparts.id and daypart_days.day_id=4) as Thu,
(select count(*) from daypart_days where daypart_days.daypart_id=dayparts.id and daypart_days.day_id=5) as Fri,
(select count(*) from daypart_days where daypart_days.daypart_id=dayparts.id and daypart_days.day_id=6) as Sat,
(select count(*) from daypart_days where daypart_days.daypart_id=dayparts.id and daypart_days.day_id=7) as Sun
from proposal_details
INNER JOIN spot_lengths on spot_lengths.id = proposal_details.spot_length_id 
INNER join dayparts on dayparts.id = proposal_details.daypart_id
left join timespans on dayparts.timespan_id = timespans.id
left join networks on networks.id = proposal_details.network_id
WHERE proposal_details.proposal_id = @proposal_id
