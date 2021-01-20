
CREATE FUNCTION [dbo].[SplitIntegers]
(
	@values VARCHAR(MAX)
)
RETURNS 
@return TABLE
(
	id INT
)
AS
BEGIN
	DECLARE @id VARCHAR(10), @pos INT

	SET @values = LTRIM(RTRIM(@values)) + ','
	SET @pos = CHARINDEX(',', @values, 1)

	IF REPLACE(@values, ',', '') <> ''
		BEGIN
			WHILE @pos > 0
				BEGIN
					SET @id = LTRIM(RTRIM(LEFT(@values, @pos - 1)))
					IF @id <> ''
						BEGIN
							INSERT INTO @return (id) VALUES (CAST(@id AS INT))
						END
					SET @values = RIGHT(@values, LEN(@values) - @pos)
					SET @pos = CHARINDEX(',', @values, 1)
				END
		END	
	RETURN
END