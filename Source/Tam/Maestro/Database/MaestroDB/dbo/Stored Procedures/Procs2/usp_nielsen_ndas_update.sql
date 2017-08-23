
CREATE PROCEDURE [dbo].[usp_nielsen_ndas_update]
(
	@document_id		Int,
	@line_number		Int,
	@coverage_area		VarChar(63),
	@market_break		VarChar(63),
	@demographic		VarChar(63),
	@audience_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@viewing_source		VarChar(127),
	@network_id		Int,
	@daypart		VarChar(63),
	@daypart_id		Int,
	@feed_pattern		VarChar(31),
	@playback_period		VarChar(31),
	@total_duration		Int,
	@mc_us_aa_perc		Float,
	@mc_us_aa_proj_delivery		Float,
	@pess		Int,
	@avg_intab		Int,
	@avg_scaled_intab		Int,
	@avg_scaled_installed		Int,
	@ue		Int,
	@ue_type		VarChar(3)
)
AS
UPDATE nielsen_ndas SET
	coverage_area = @coverage_area,
	market_break = @market_break,
	demographic = @demographic,
	audience_id = @audience_id,
	start_date = @start_date,
	end_date = @end_date,
	viewing_source = @viewing_source,
	network_id = @network_id,
	daypart = @daypart,
	daypart_id = @daypart_id,
	feed_pattern = @feed_pattern,
	playback_period = @playback_period,
	total_duration = @total_duration,
	mc_us_aa_perc = @mc_us_aa_perc,
	mc_us_aa_proj_delivery = @mc_us_aa_proj_delivery,
	pess = @pess,
	avg_intab = @avg_intab,
	avg_scaled_intab = @avg_scaled_intab,
	avg_scaled_installed = @avg_scaled_installed,
	ue = @ue,
	ue_type = @ue_type
WHERE
	document_id = @document_id AND
	line_number = @line_number

