	CREATE PROCEDURE [dbo].[usp_PCS_IsMediaMonthMsaLocked]
		@media_month_id INT
	AS
	BEGIN
		SET NOCOUNT ON;
	
		DECLARE @is_locked BIT;
		SELECT
			@is_locked = ml.is_locked
		FROM
			msa_locks ml (NOLOCK)
		WHERE
			ml.media_month_id=@media_month_id;
		
		IF @is_locked IS NULL
			SET @is_locked = 0;
		
		SELECT @is_locked;
	END
