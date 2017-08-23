-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_STS2_GetSystemStatementContactInfosBySystemStatementGroup
	@system_statement_group_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		id,
		system_statement_group_id,
		system_id,
		first_name,
		last_name,
		email_address
	FROM
		system_statement_contact_infos (NOLOCK)
	WHERE
		system_statement_group_id=@system_statement_group_id
END
