
-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/28/2016 03:33:42 PM
-- Description:	Auto-generated method to update a zone_histories record.
-- =============================================
CREATE PROCEDURE usp_zone_histories_update
	@zone_id INT,
	@start_date DATETIME,
	@code VARCHAR(15),
	@name VARCHAR(63),
	@type VARCHAR(63),
	@primary BIT,
	@traffic BIT,
	@dma BIT,
	@flag TINYINT,
	@active BIT,
	@end_date DATETIME,
	@time_zone_id INT,
	@observe_daylight_savings_time BIT
AS
BEGIN
	UPDATE
		[dbo].[zone_histories]
	SET
		[code]=@code,
		[name]=@name,
		[type]=@type,
		[primary]=@primary,
		[traffic]=@traffic,
		[dma]=@dma,
		[flag]=@flag,
		[active]=@active,
		[end_date]=@end_date,
		[time_zone_id]=@time_zone_id,
		[observe_daylight_savings_time]=@observe_daylight_savings_time
	WHERE
		[zone_id]=@zone_id
		AND [start_date]=@start_date
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_zone_network_histories_insert]') AND type in (N'P', N'PC'))
BEGIN
EXEC sp_executesql @statement = N'CREATE PROCEDURE [dbo].[usp_zone_network_histories_insert] AS'
END
