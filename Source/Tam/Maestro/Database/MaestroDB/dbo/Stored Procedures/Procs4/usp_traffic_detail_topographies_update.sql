
-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/18/2015 02:09:00 PM
-- Description:	Auto-generated method to update a traffic_detail_topographies record.
-- =============================================
CREATE PROCEDURE usp_traffic_detail_topographies_update
	@traffic_detail_week_id INT,
	@topography_id INT,
	@daypart_id INT,
	@spots FLOAT,
	@universe FLOAT,
	@rate MONEY,
	@lookup_rate MONEY,
	@ordered_spot_cost MONEY,
	@calculated_spot_cost MONEY,
	@fixed_spot_cost MONEY,
	@spot_cost1 MONEY,
	@spot_cost2 MONEY
AS
BEGIN
	UPDATE
		[dbo].[traffic_detail_topographies]
	SET
		[daypart_id]=@daypart_id,
		[spots]=@spots,
		[universe]=@universe,
		[rate]=@rate,
		[lookup_rate]=@lookup_rate,
		[ordered_spot_cost]=@ordered_spot_cost,
		[calculated_spot_cost]=@calculated_spot_cost,
		[fixed_spot_cost]=@fixed_spot_cost,
		[spot_cost1]=@spot_cost1,
		[spot_cost2]=@spot_cost2
	WHERE
		[traffic_detail_week_id]=@traffic_detail_week_id
		AND [topography_id]=@topography_id
END
