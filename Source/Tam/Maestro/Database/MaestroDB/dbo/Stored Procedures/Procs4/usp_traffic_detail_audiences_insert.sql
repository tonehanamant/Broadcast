
-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/18/2015 02:08:59 PM
-- Description:	Auto-generated method to insert a traffic_detail_audiences record.
-- =============================================
CREATE PROCEDURE usp_traffic_detail_audiences_insert
	@traffic_detail_id INT,
	@audience_id INT,
	@traffic_rating FLOAT,
	@vpvh FLOAT,
	@proposal_rating FLOAT,
	@us_universe FLOAT,
	@scaling_factor FLOAT,
	@coverage_universe FLOAT
AS
BEGIN
	INSERT INTO [dbo].[traffic_detail_audiences]
	(
		[traffic_detail_id],
		[audience_id],
		[traffic_rating],
		[vpvh],
		[proposal_rating],
		[us_universe],
		[scaling_factor],
		[coverage_universe]
	)
	VALUES
	(
		@traffic_detail_id,
		@audience_id,
		@traffic_rating,
		@vpvh,
		@proposal_rating,
		@us_universe,
		@scaling_factor,
		@coverage_universe
	)
END

