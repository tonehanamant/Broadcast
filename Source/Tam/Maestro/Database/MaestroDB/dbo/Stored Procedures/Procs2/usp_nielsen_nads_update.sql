-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/07/2014 09:10:36 AM
-- Description:	Auto-generated method to update a nielsen_nads record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_nielsen_nads_update]
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
	UPDATE
		[dbo].[nielsen_nads]
	SET
		[document_id]=@document_id,
		[line_number]=@line_number,
		[coverage_area]=@coverage_area,
		[demographic]=@demographic,
		[viewing_source]=@viewing_source,
		[daypart]=@daypart,
		[feed_pattern]=@feed_pattern,
		[playback_period]=@playback_period,
		[total_duration]=@total_duration,
		[mc_us_aa_perc]=@mc_us_aa_perc,
		[mc_us_aa_proj_delivery]=@mc_us_aa_proj_delivery,
		[pess]=@pess,
		[avg_intab]=@avg_intab,
		[avg_scaled_intab]=@avg_scaled_intab,
		[avg_scaled_installed]=@avg_scaled_installed,
		[ue]=@ue,
		[ue_type]=@ue_type,
		[vpvh]=@vpvh
	WHERE
		[start_date]=@start_date
		AND [end_date]=@end_date
		AND [market_break]=@market_break
		AND [audience_id]=@audience_id
		AND [network_id]=@network_id
		AND [daypart_id]=@daypart_id
END
