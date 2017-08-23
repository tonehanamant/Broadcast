


CREATE PROCEDURE [dbo].[usp_BRS_GetStatuses]
	@status_set varchar(15)

AS
BEGIN
	SELECT 
		id,
		status_set,
		[name],
		description
	FROM
		statuses	(nolock)
	WHERE
		status_set = @status_set
	ORDER BY [name] ASC
END


