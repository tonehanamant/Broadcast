-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MAS_GetQueuedEmailOutboxDetails]
	@email_outbox_id INT,
	@max_num_attempts INT,
	@minutes_between_attempts INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		id, 
		email_outbox_id, 
		email_address, 
		display_name, 
		num_attempts,
		status_code,
		date_sent, 
		date_last_attempt
	FROM
		email_outbox_details
	WHERE
		email_outbox_id = @email_outbox_id
		AND status_code NOT IN (1,2) -- see TAMBusinessEntities.Enums.EmailOutboxDetailStatusCode Enumeration
		AND num_attempts < @max_num_attempts
		AND (date_last_attempt IS NULL OR DATEDIFF(minute, date_last_attempt, getdate()) > @minutes_between_attempts)
END
