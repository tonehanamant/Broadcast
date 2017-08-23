create FUNCTION [dbo].[FirstOfMonth2Period] (@FirstOfMonth AS DATETIME)
RETURNS varchar(4)
AS
BEGIN
	DECLARE @Period AS varchar(4)
	
	IF (DATEPART(dd, @FirstOfMonth) <> 1)
		RETURN(NULL)

	SET @Period = CAST(DATEPART(mm, @FirstOfMonth) AS varchar(2)) + 
                      RIGHT(CAST(DATEPART(yy, @FirstOfMonth) AS varchar(10)),2)

	IF (LEN(@Period) = 3)
		SET @Period = '0' + @Period

	RETURN(@Period)
END
