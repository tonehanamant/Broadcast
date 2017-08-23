CREATE PROCEDURE [dbo].[usp_affidavit_delivery_runs_insert]
(
	@id		Int		OUTPUT,
	@media_month_id		Int,
	@rating_source_id		TinyInt,
	@started_by_employee_id		Int,
	@status_code		TinyInt,
	@total_needed_affidavits		Int,
	@total_done_affidavits		Int,
	@total_remaining_affidavits		Int,
	@time_to_get_needed_affidavits		Int,
	@time_to_get_done_affidavits		Int,
	@time_to_get_remaining_affidavits		Int,
	@time_to_calculate_deliveries		Int,
	@num_processed		Int,
	@num_remaining		Int,
	@date_queued		DateTime,
	@date_started		DateTime,
	@date_last_updated		DateTime,
	@date_completed		DateTime
)
AS
INSERT INTO dbo.affidavit_delivery_runs
(
	media_month_id,
	rating_source_id,
	started_by_employee_id,
	status_code,
	total_needed_affidavits,
	total_done_affidavits,
	total_remaining_affidavits,
	time_to_get_needed_affidavits,
	time_to_get_done_affidavits,
	time_to_get_remaining_affidavits,
	time_to_calculate_deliveries,
	num_processed,
	num_remaining,
	date_queued,
	date_started,
	date_last_updated,
	date_completed
)
VALUES
(
	@media_month_id,
	@rating_source_id,
	@started_by_employee_id,
	@status_code,
	@total_needed_affidavits,
	@total_done_affidavits,
	@total_remaining_affidavits,
	@time_to_get_needed_affidavits,
	@time_to_get_done_affidavits,
	@time_to_get_remaining_affidavits,
	@time_to_calculate_deliveries,
	@num_processed,
	@num_remaining,
	@date_queued,
	@date_started,
	@date_last_updated,
	@date_completed
)

SELECT
	@id = SCOPE_IDENTITY()
