-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_RCS_GetProperty
	@name VARCHAR(63)
AS
BEGIN
	SELECT
		value
	FROM
		properties (NOLOCK)
	WHERE
		name=@name
END
