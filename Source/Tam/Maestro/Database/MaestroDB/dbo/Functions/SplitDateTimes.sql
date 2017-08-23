-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 12/10/2014
-- Description:	Accepts a string based comma separated list of start/end date combos outputs a table of media week ids
-- Sample:		SELECT * FROM dbo.SplitDateTimes('12/15/2014-12/24/2014', '-')
-- =============================================
CREATE FUNCTION [dbo].[SplitDateTimes]
(
	@values VARCHAR(MAX),
	@delimiter VARCHAR(1)
)
RETURNS 
@return TABLE
(
	id DATETIME
)
AS
BEGIN
	DECLARE @id VARCHAR(MAX), @pos INT

	SET @values = LTRIM(RTRIM(@values)) + @delimiter
	SET @pos = CHARINDEX(@delimiter, @values, 1)

	IF REPLACE(@values, @delimiter, '') <> ''
		BEGIN
			WHILE @pos > 0
				BEGIN
					SET @id = LTRIM(RTRIM(LEFT(@values, @pos - 1)))
					IF @id <> ''
						BEGIN
							INSERT INTO @return (id) VALUES (CAST(@id AS DATETIME))
						END
					SET @values = RIGHT(@values, LEN(@values) - @pos)
					SET @pos = CHARINDEX(@delimiter, @values, 1)
				END
		END	
	RETURN
END
