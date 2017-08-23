CREATE PROCEDURE usp_ARS_GetIncompleteRatingsLoadJobs
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT id, value 
	FROM jobs j 
	JOIN job_parameters jp 
		ON jp.job_id = j.id 
	WHERE date_completed IS NULL 
		AND type = 'RLJ'
END
