
CREATE VIEW dbo.uvw_proposal_flights
AS
	select 
		pf.*,
		mw.id as media_week_id
	from proposal_flights pf
		join media_weeks mw
			on pf.start_date >= mw.start_date
			and pf.end_date <= mw.end_date