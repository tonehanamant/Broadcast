
CREATE VIEW uvw_release_media_months
	AS
	select distinct
		mm.id,
		t.release_id
	from traffic t
	join media_months mm
		on t.start_date >= mm.start_date and t.start_date <= mm.end_date