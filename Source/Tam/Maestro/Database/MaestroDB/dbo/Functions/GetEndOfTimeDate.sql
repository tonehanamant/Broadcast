-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetEndOfTimeDate]
(
	-- No parameters
)
RETURNS DATETIME
AS
BEGIN
	-- Declare the return variable here
	DECLARE @dateEndOfTime AS DATETIME;

	SELECT 
		@dateEndOfTime = value
	FROM
		properties
	WHERE
		name = 'end_of_time';

	-- Return the result of the function
	RETURN @dateEndOfTime
END
