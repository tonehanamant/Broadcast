
-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/28/2016 03:33:41 PM
-- Description:	Auto-generated method to insert a zone_histories record.
-- =============================================
CREATE PROCEDURE usp_zone_histories_insert
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
	INSERT INTO [dbo].[zone_histories]
	(
		[zone_id],
		[start_date],
		[code],
		[name],
		[type],
		[primary],
		[traffic],
		[dma],
		[flag],
		[active],
		[end_date],
		[time_zone_id],
		[observe_daylight_savings_time]
	)
	VALUES
	(
		@zone_id,
		@start_date,
		@code,
		@name,
		@type,
		@primary,
		@traffic,
		@dma,
		@flag,
		@active,
		@end_date,
		@time_zone_id,
		@observe_daylight_savings_time
	)
END
