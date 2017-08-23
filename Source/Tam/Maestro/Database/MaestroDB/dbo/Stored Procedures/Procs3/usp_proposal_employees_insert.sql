CREATE PROCEDURE usp_proposal_employees_insert
(
	@proposal_id		Int,
	@employee_id		Int,
	@effective_date		DateTime,
	@flight_text		VARCHAR(MAX),
	@original_flight_text		VARCHAR(MAX),
	@total_gross_cost		Money,
	@total_rate_card_cost		Money,
	@total_units		Int,
	@total_detail_lines		SmallInt,
	@total_demographics		TinyInt,
	@total_hh_delivery		Float,
	@total_hh_cpm		Money,
	@primary_audience_id		Int,
	@total_demo_delivery		Float,
	@total_demo_cpm		Money
)
AS
INSERT INTO proposal_employees
(
	proposal_id,
	employee_id,
	effective_date,
	flight_text,
	original_flight_text,
	total_gross_cost,
	total_rate_card_cost,
	total_units,
	total_detail_lines,
	total_demographics,
	total_hh_delivery,
	total_hh_cpm,
	primary_audience_id,
	total_demo_delivery,
	total_demo_cpm
)
VALUES
(
	@proposal_id,
	@employee_id,
	@effective_date,
	@flight_text,
	@original_flight_text,
	@total_gross_cost,
	@total_rate_card_cost,
	@total_units,
	@total_detail_lines,
	@total_demographics,
	@total_hh_delivery,
	@total_hh_cpm,
	@primary_audience_id,
	@total_demo_delivery,
	@total_demo_cpm
)

