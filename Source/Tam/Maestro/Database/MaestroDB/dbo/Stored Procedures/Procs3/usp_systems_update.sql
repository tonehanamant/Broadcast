﻿
-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/11/2017 11:10:53 AM
-- Description:	Auto-generated method to update a systems record.
-- =============================================
CREATE PROCEDURE usp_systems_update
	@id INT,
	@code VARCHAR(15),
	@name VARCHAR(63),
	@location VARCHAR(63),
	@spot_yield_weight FLOAT,
	@traffic_order_format INT,
	@flag TINYINT,
	@active BIT,
	@effective_date DATETIME,
	@generate_traffic_alert_excel BIT,
	@one_advertiser_per_traffic_alert BIT,
	@cancel_recreate_order_traffic_alert BIT,
	@order_regeneration_traffic_alert BIT,
	@custom_traffic_system BIT
AS
BEGIN
	UPDATE
		[dbo].[systems]
	SET
		[code]=@code,
		[name]=@name,
		[location]=@location,
		[spot_yield_weight]=@spot_yield_weight,
		[traffic_order_format]=@traffic_order_format,
		[flag]=@flag,
		[active]=@active,
		[effective_date]=@effective_date,
		[generate_traffic_alert_excel]=@generate_traffic_alert_excel,
		[one_advertiser_per_traffic_alert]=@one_advertiser_per_traffic_alert,
		[cancel_recreate_order_traffic_alert]=@cancel_recreate_order_traffic_alert,
		[order_regeneration_traffic_alert]=@order_regeneration_traffic_alert,
		[custom_traffic_system]=@custom_traffic_system
	WHERE
		[id]=@id
END

