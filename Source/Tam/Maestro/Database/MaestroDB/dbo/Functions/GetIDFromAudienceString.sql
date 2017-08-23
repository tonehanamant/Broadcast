-- =============================================
-- Author:		David Sisson
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetIDFromAudienceString] 
(
	-- Add the parameters for the function here
	@audienceString as varchar(6)
)
RETURNS int
AS
BEGIN
	-- Declare the return variable here
	DECLARE 
		@audienceID as int,
		@originalString as varchar(6),
		@workingString as varchar(6),
		@audienceTarget as varchar(4),
		@audienceStartAge as int,
		@audienceEndAge as int;
	
	SET @originalString = @audienceString;
	
	SET @workingString = LTRIM(RTRIM(@originalString));
	SET @audienceTarget = LEFT( @workingString, 1 );
	
	IF NOT @audienceTarget IN('H', 'A', 'T', 'K', 'F', 'W', 'M', 'P')
		BEGIN
		RETURN NULL;
		END

	IF @audienceTarget = 'H'
		BEGIN
		IF @originalString = 'HH'
			BEGIN
			select
				@audienceID = a.id
			from
				audiences a (NOLOCK)
			where
				a.category_code = 0
				AND a.sub_category_code = @audienceTarget
				AND a.custom = 0;
			END
		END
	ELSE
		BEGIN
		WHILE ISNUMERIC(LEFT( @workingString, 1 )) <> 1
		BEGIN
			IF LEN(@workingString) > 1 
				BEGIN
				SET @workingString = RIGHT(@workingString, LEN(@workingString) - 1)
				END
			ELSE
				BEGIN
				RETURN NULL;
				END
		END;
		
		IF RIGHT(@workingString, 1) = '+'
			BEGIN
				IF (LEN(@workingString) < 2) AND (ISNUMERIC(LEFT(@workingString, LEN(@workingString) - 1)) <> 1)
					BEGIN
					RETURN NULL;
					END
				SET @audienceStartAge = CAST(LEFT(@workingString, LEN(@workingString) - 1) AS INT);
				SET @audienceEndAge = 99
			END
		ELSE
			BEGIN
			IF ISNUMERIC(@workingString) <> 1
				BEGIN
				RETURN NULL;
				END
			IF LEN(@workingString) = 1
				BEGIN
				RETURN NULL;
				END
			ELSE IF LEN(@workingString) = 2 
				BEGIN
					SET @audienceStartAge = CAST(LEFT(@workingString,1) AS INT);
					SET @audienceEndAge = CAST(RIGHT(@workingString,1) AS INT);
				END
			ELSE IF LEN(@workingString) = 3 
				BEGIN
					SET @audienceStartAge = CAST(LEFT(@workingString,1) AS INT);
					SET @audienceEndAge = CAST(RIGHT(@workingString,2) AS INT);
				END
			ELSE IF LEN(@workingString) = 4 
				BEGIN
					SET @audienceStartAge = CAST(LEFT(@workingString,2) AS INT);
					SET @audienceEndAge = CAST(RIGHT(@workingString,2) AS INT);
				END
			ELSE
				BEGIN
				RETURN NULL;
				END
			END

		-- Add the T-SQL statements to compute the return value here
		SELECT
			@audienceID = a.id
		FROM
			audiences a (NOLOCK)
		WHERE
			a.category_code = 0
			AND a.sub_category_code = @audienceTarget
			AND a.range_start = @audienceStartAge
			AND a.range_end = @audienceEndAge;
	END
	
	RETURN @audienceID;
END
