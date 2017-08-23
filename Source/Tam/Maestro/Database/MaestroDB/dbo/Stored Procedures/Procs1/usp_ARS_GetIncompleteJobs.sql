
-- =============================================
-- Author:		Mike Deaven
-- Create date: 9/10/2012
-- Description:	Fetches incomplete jobs for the given job type
-- =============================================
CREATE PROCEDURE usp_ARS_GetIncompleteJobs
    @jobType VARCHAR(3)
AS
BEGIN
        -- SET NOCOUNT ON added to prevent extra result sets from
        -- interfering with SELECT statements.
    SET NOCOUNT ON;
        
    SELECT * 
    FROM jobs j 
    JOIN job_parameters jp 
                    ON jp.job_id = j.id 
    WHERE date_completed IS NULL 
                            AND type = @jobType
END
