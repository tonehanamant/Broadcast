
-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/28/2016 03:33:42 PM
-- Description:	Auto-generated method to insert a zones record.
-- =============================================
CREATE PROCEDURE usp_zones_insert
	@id INT OUTPUT,
	@code VARCHAR(15),
	@name VARCHAR(63),
	@type VARCHAR(63),
	@primary BIT,
	@traffic BIT,
	@dma BIT,
	@flag TINYINT,
	@active BIT,
	@effective_date DATETIME,
	@time_zone_id INT,
	@observe_daylight_savings_time BIT
AS
BEGIN
	INSERT INTO [dbo].[zones]
	(
		[code],
		[name],
		[type],
		[primary],
		[traffic],
		[dma],
		[flag],
		[active],
		[effective_date],
		[time_zone_id],
		[observe_daylight_savings_time]
	)
	VALUES
	(
		@code,
		@name,
		@type,
		@primary,
		@traffic,
		@dma,
		@flag,
		@active,
		@effective_date,
		@time_zone_id,
		@observe_daylight_savings_time
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
