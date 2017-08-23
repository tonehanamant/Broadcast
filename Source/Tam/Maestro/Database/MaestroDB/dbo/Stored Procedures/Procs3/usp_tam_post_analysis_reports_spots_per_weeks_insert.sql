﻿CREATE PROCEDURE usp_tam_post_analysis_reports_spots_per_weeks_insert
(
	@tam_post_proposal_id		Int,
	@audience_id		Int,
	@network_id		Int,
	@media_week_id		Int,
	@enabled		Bit,
	@subscribers		BigInt,
	@delivery		Float,
	@eq_delivery		Float,
	@units		Float,
	@dr_delivery		Float,
	@dr_eq_delivery		Float
)
AS
INSERT INTO tam_post_analysis_reports_spots_per_weeks
(
	tam_post_proposal_id,
	audience_id,
	network_id,
	media_week_id,
	enabled,
	subscribers,
	delivery,
	eq_delivery,
	units,
	dr_delivery,
	dr_eq_delivery
)
VALUES
(
	@tam_post_proposal_id,
	@audience_id,
	@network_id,
	@media_week_id,
	@enabled,
	@subscribers,
	@delivery,
	@eq_delivery,
	@units,
	@dr_delivery,
	@dr_eq_delivery
)

