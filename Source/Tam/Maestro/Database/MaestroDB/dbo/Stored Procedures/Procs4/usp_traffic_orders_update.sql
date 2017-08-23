
-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/25/2016 04:41:41 PM
-- Description:	Auto-generated method to update a traffic_orders record.
-- =============================================
CREATE PROCEDURE usp_traffic_orders_update
	@id INT,
	@system_id INT,
	@zone_id INT,
	@traffic_detail_id INT,
	@daypart_id INT,
	@ordered_spots INT,
	@ordered_spot_rate MONEY,
	@start_date DATETIME,
	@end_date DATETIME,
	@release_id INT,
	@subscribers INT,
	@display_network_id INT,
	@on_financial_reports BIT,
	@active BIT,
	@topography_id INT,
	@proposal1_rating FLOAT,
	@proposal2_rating FLOAT,
	@proposal1_guaranteed_audience_id INT,
	@proposal2_guaranteed_audience_id INT,
	@proposal1_3net_cpm MONEY,
	@proposal2_3net_cpm MONEY,
	@traffic1_rating FLOAT,
	@traffic2_rating FLOAT,
	@proposal1_id INT,
	@proposal2_id INT,
	@discount_factor FLOAT,
	@original_rate MONEY,
	@calculated_rate MONEY,
	@rate1 MONEY,
	@rate2 MONEY,
	@traffic_spot_target_id INT,
	@media_month_id SMALLINT,
	@media_week_id INT,
	@traffic_id INT
AS
BEGIN
	UPDATE
		[dbo].[traffic_orders]
	SET
		[system_id]=@system_id,
		[zone_id]=@zone_id,
		[traffic_detail_id]=@traffic_detail_id,
		[daypart_id]=@daypart_id,
		[ordered_spots]=@ordered_spots,
		[ordered_spot_rate]=@ordered_spot_rate,
		[start_date]=@start_date,
		[end_date]=@end_date,
		[release_id]=@release_id,
		[subscribers]=@subscribers,
		[display_network_id]=@display_network_id,
		[on_financial_reports]=@on_financial_reports,
		[active]=@active,
		[topography_id]=@topography_id,
		[proposal1_rating]=@proposal1_rating,
		[proposal2_rating]=@proposal2_rating,
		[proposal1_guaranteed_audience_id]=@proposal1_guaranteed_audience_id,
		[proposal2_guaranteed_audience_id]=@proposal2_guaranteed_audience_id,
		[proposal1_3net_cpm]=@proposal1_3net_cpm,
		[proposal2_3net_cpm]=@proposal2_3net_cpm,
		[traffic1_rating]=@traffic1_rating,
		[traffic2_rating]=@traffic2_rating,
		[proposal1_id]=@proposal1_id,
		[proposal2_id]=@proposal2_id,
		[discount_factor]=@discount_factor,
		[original_rate]=@original_rate,
		[calculated_rate]=@calculated_rate,
		[rate1]=@rate1,
		[rate2]=@rate2,
		[traffic_spot_target_id]=@traffic_spot_target_id,
		[media_month_id]=@media_month_id,
		[media_week_id]=@media_week_id,
		[traffic_id]=@traffic_id
	WHERE
		[id]=@id
END
