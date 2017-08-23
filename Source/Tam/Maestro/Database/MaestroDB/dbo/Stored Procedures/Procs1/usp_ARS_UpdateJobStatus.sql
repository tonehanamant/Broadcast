CREATE PROCEDURE usp_ARS_UpdateJobStatus
	@job_id INT,
	@status VARCHAR(15),
	@current_date DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE jobs
	SET status = @status
	WHERE id=@job_id

	IF @status = 'Processing'
	BEGIN
		UPDATE jobs 
		SET date_started = @current_date
		WHERE id = @job_id
	END

	IF (@status = 'Done' OR @status = 'Error')
	BEGIN
		UPDATE jobs 
		SET date_completed = @current_date
		WHERE id = @job_id
	END

END
