-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/07/2014 09:10:36 AM
-- Description:	Auto-generated method to insert a nielsen_nads record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_nielsen_nads_insert]
	@start_date DATETIME,
	@end_date DATETIME,
	@market_break VARCHAR(63),
	@audience_id INT,
	@network_id INT,
	@daypart_id INT,
	@document_id INT,
	@line_number INT,
	@coverage_area VARCHAR(63),
	@demographic VARCHAR(63),
	@viewing_source VARCHAR(127),
	@daypart VARCHAR(63),
	@feed_pattern VARCHAR(31),
	@playback_period VARCHAR(31),
	@total_duration INT,
	@mc_us_aa_perc FLOAT,
	@mc_us_aa_proj_delivery FLOAT,
	@pess INT,
	@avg_intab INT,
	@avg_scaled_intab INT,
	@avg_scaled_installed INT,
	@ue INT,
	@ue_type VARCHAR(3),
	@vpvh FLOAT
AS
BEGIN
	INSERT INTO [dbo].[nielsen_nads]
	(
		[start_date],
		[end_date],
		[market_break],
		[audience_id],
		[network_id],
		[daypart_id],
		[document_id],
		[line_number],
		[coverage_area],
		[demographic],
		[viewing_source],
		[daypart],
		[feed_pattern],
		[playback_period],
		[total_duration],
		[mc_us_aa_perc],
		[mc_us_aa_proj_delivery],
		[pess],
		[avg_intab],
		[avg_scaled_intab],
		[avg_scaled_installed],
		[ue],
		[ue_type],
		[vpvh]
	)
	VALUES
	(
		@start_date,
		@end_date,
		@market_break,
		@audience_id,
		@network_id,
		@daypart_id,
		@document_id,
		@line_number,
		@coverage_area,
		@demographic,
		@viewing_source,
		@daypart,
		@feed_pattern,
		@playback_period,
		@total_duration,
		@mc_us_aa_perc,
		@mc_us_aa_proj_delivery,
		@pess,
		@avg_intab,
		@avg_scaled_intab,
		@avg_scaled_installed,
		@ue,
		@ue_type,
		@vpvh
	)
END
