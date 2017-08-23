-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_STS2_GetStatementBusinessObject 3
CREATE PROCEDURE [dbo].[usp_STS2_GetStatementBusinessObject]
	@statement_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		s.id,
		s.media_month_id,
		s.statement_type,
		mm.media_month
	FROM
		statements s (NOLOCK)
		JOIN media_months mm (NOLOCK) ON mm.id=s.media_month_id
	WHERE
		s.id = @statement_id


	SELECT
		ss.*,
		notes.id,
		notes.comment
	FROM
		system_statements ss (NOLOCK)
		LEFT JOIN (
			SELECT 
				MAX(id) AS [note_id],
				CAST(reference_id AS INT) AS [reference_id]
			FROM
				notes (NOLOCK)
			WHERE
				notes.note_type='system_statements'
			GROUP BY
				reference_id
		) tmp ON tmp.reference_id=ss.id
		LEFT JOIN notes (NOLOCK) ON notes.id=tmp.note_id
	WHERE
		ss.statement_id = @statement_id
	ORDER BY
		ss.[name]


	SELECT
		ssd.system_statement_id, 
		ssd.date_sent, 
		ssd.email_outbox_id,
		dbo.GetEmailOutboxStatusCode(ssd.email_outbox_id) 'email_outbox_status_code'
	FROM
		system_statement_details ssd (NOLOCK)
		JOIN system_statements ss (NOLOCK) ON ss.id=ssd.system_statement_id
	WHERE
		ss.statement_id = @statement_id


	SELECT
		sss.system_statement_id, 
		sss.system_id
	FROM
		system_statement_systems sss (NOLOCK)
		JOIN system_statements ss (NOLOCK) ON ss.id=sss.system_statement_id
	WHERE
		ss.statement_id = @statement_id
END
