	CREATE PROCEDURE [dbo].[usp_PCS_SaveMsaLock]
		@media_month_id INT,
		@is_locked BIT
	AS
	BEGIN
		SET NOCOUNT ON;
	
		DELETE FROM msa_locks WHERE media_month_id=@media_month_id;
		INSERT INTO msa_locks (media_month_id,is_locked) VALUES (@media_month_id,@is_locked);
	END
