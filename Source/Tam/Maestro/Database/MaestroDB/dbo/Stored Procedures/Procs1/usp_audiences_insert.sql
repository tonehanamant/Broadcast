-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/14/2013 02:26:30 PM
-- Description:	Auto-generated method to insert a audiences record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_audiences_insert]
	@id INT OUTPUT,
	@category_code TINYINT,
	@sub_category_code CHAR(1),
	@range_start INT,
	@range_end INT,
	@custom BIT,
	@code VARCHAR(15),
	@name VARCHAR(127)
AS
BEGIN
	INSERT INTO [dbo].[audiences]
	(
		[category_code],
		[sub_category_code],
		[range_start],
		[range_end],
		[custom],
		[code],
		[name]
	)
	VALUES
	(
		@category_code,
		@sub_category_code,
		@range_start,
		@range_end,
		@custom,
		@code,
		@name
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
