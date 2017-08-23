CREATE FUNCTION ufn_generate_audit_string(
	@firstname VARCHAR(50)
	,@lastname VARCHAR(50)
	,@action_date DATETIME
)
RETURNS VARCHAR(150)
AS
BEGIN
	DECLARE @return varchar(100)
	
	IF (@firstname is null OR @lastname is null or @action_date is null)
	BEGIN
		SET @return = null
	END
	ELSE
	BEGIN
		SET @return = (@firstname + ' ' + @lastname + ' (On ' + CONVERT(VARCHAR(10), @action_date, 101) + ' ' + CONVERT(VARCHAR(8), @action_date, 108) + ')')
	END
	
	RETURN @return
END
