
-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/11/2017 11:10:52 AM
-- Description:	Auto-generated method to update a system_histories record.
-- =============================================
CREATE PROCEDURE usp_system_histories_update
	@system_id INT,
	@start_date DATETIME,
	@code VARCHAR(15),
	@name VARCHAR(63),
	@location VARCHAR(63),
	@spot_yield_weight FLOAT,
	@traffic_order_format INT,
	@flag TINYINT,
	@active BIT,
	@end_date DATETIME,
	@generate_traffic_alert_excel BIT,
	@one_advertiser_per_traffic_alert BIT,
	@cancel_recreate_order_traffic_alert BIT,
	@order_regeneration_traffic_alert BIT,
	@custom_traffic_system BIT
AS
BEGIN
	UPDATE
		[dbo].[system_histories]
	SET
		[code]=@code,
		[name]=@name,
		[location]=@location,
		[spot_yield_weight]=@spot_yield_weight,
		[traffic_order_format]=@traffic_order_format,
		[flag]=@flag,
		[active]=@active,
		[end_date]=@end_date,
		[generate_traffic_alert_excel]=@generate_traffic_alert_excel,
		[one_advertiser_per_traffic_alert]=@one_advertiser_per_traffic_alert,
		[cancel_recreate_order_traffic_alert]=@cancel_recreate_order_traffic_alert,
		[order_regeneration_traffic_alert]=@order_regeneration_traffic_alert,
		[custom_traffic_system]=@custom_traffic_system
	WHERE
		[system_id]=@system_id
		AND [start_date]=@start_date
END
