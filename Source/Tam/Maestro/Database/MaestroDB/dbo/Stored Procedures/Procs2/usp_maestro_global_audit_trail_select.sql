-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/07/2015 12:54:02 PM
-- Description:	Auto-generated method to select a single maestro_global_audit_trail record.
-- =============================================
CREATE PROCEDURE usp_maestro_global_audit_trail_select
	@id BigInt
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[maestro_global_audit_trail].*
	FROM
		[dbo].[maestro_global_audit_trail] WITH(NOLOCK)
	WHERE
		[id]=@id
END
