
-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/28/2016 03:33:43 PM
-- Description:	Auto-generated method to update a zones record.
-- =============================================
CREATE PROCEDURE usp_zones_update
	@id INT,
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
	UPDATE
		[dbo].[zones]
	SET
		[code]=@code,
		[name]=@name,
		[type]=@type,
		[primary]=@primary,
		[traffic]=@traffic,
		[dma]=@dma,
		[flag]=@flag,
		[active]=@active,
		[effective_date]=@effective_date,
		[time_zone_id]=@time_zone_id,
		[observe_daylight_savings_time]=@observe_daylight_savings_time
	WHERE
		[id]=@id
END
