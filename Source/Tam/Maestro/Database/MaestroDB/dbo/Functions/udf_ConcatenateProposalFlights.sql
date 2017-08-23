
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/9/2010
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[udf_ConcatenateProposalFlights]
(	
	@idProposal int
)
RETURNS TABLE 
AS
RETURN (
	with
	flight_set(
		start_date,
		end_date
	) as (
		select
			proposal_flights.start_date,
			proposal_flights.end_date
		from
			proposal_flights
		where
			@idProposal = proposal_flights.proposal_id
			and
			1 = proposal_flights.selected
	),
	min_max_days(
		min_day,
		max_day
	) as (
		select
			MIN(flight_set.start_date) min_day,
			max(flight_set.end_date) max_day
		from
			flight_set
	),
	flight_days(
		flight_day
	) as (
		select
			start_date flight_day
		from
			flight_set
		union all
		select
			dateadd(day, 1, flight_days.flight_day) flight_day
		from
			flight_days
			join flight_set on
				flight_days.flight_day between flight_set.start_date and flight_set.end_date
		where
			flight_days.flight_day < flight_set.end_date
	),
	distinct_flight_days(
		flight_day
	) as (
		select distinct
			flight_day
		from
			flight_days
	),
	flight_day_stats(
		flight_day,
		position,
		ordinal
	) as (
		select
			distinct_flight_days.flight_day,
			datediff(day, min_max_days.min_day, distinct_flight_days.flight_day) + 1 position,
			row_number() over (order by distinct_flight_days.flight_day desc) ordinal
		from
			distinct_flight_days
			cross join min_max_days
	)
	select
		min(flight_day_stats.flight_day) start_date,
		max(flight_day_stats.flight_day) end_date
	from
		flight_day_stats
	group by
		flight_day_stats.ordinal + flight_day_stats.position
);
