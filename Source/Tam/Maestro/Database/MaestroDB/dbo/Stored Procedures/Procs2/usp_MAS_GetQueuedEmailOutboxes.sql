-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MAS_GetQueuedEmailOutboxes]
	@max_num_attempts INT,
	@minutes_between_attempts INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		id, 
		email_profile_id,
		subject, 
		body, 
		is_html, 
		mail_priority,
		reply_to_email_address, 
		date_created
	FROM
		email_outboxes
	WHERE
		id IN (
			SELECT	
				email_outbox_id 
			FROM 
				email_outbox_details
			WHERE 
				status_code NOT IN (1,2) -- see TAMBusinessEntities.Enums.EmailOutboxDetailStatusCode Enumeration
				AND num_attempts < @max_num_attempts
				AND (date_last_attempt IS NULL OR DATEDIFF(minute, date_last_attempt, getdate()) > @minutes_between_attempts)
		)
END
