
-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/18/2015 02:08:59 PM
-- Description:	Auto-generated method to update a traffic_detail_audiences record.
-- =============================================
CREATE PROCEDURE usp_traffic_detail_audiences_update
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
	UPDATE
		[dbo].[traffic_detail_audiences]
	SET
		[traffic_rating]=@traffic_rating,
		[vpvh]=@vpvh,
		[proposal_rating]=@proposal_rating,
		[us_universe]=@us_universe,
		[scaling_factor]=@scaling_factor,
		[coverage_universe]=@coverage_universe
	WHERE
		[traffic_detail_id]=@traffic_detail_id
		AND [audience_id]=@audience_id
END
