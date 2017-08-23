CREATE PROCEDURE [dbo].[usp_audiences_update]
(
	@id		Int,
	@category_code		TinyInt,
	@sub_category_code		Char(1),
	@range_start		Int,
	@range_end		Int,
	@custom		Bit,
	@code VARCHAR(15),
	@name		VarChar(127)
)
AS
UPDATE dbo.audiences SET
	category_code = @category_code,
	sub_category_code = @sub_category_code,
	range_start = @range_start,
	range_end = @range_end,
	custom = @custom,
	code = @code,
	name = @name
WHERE
	id = @id
