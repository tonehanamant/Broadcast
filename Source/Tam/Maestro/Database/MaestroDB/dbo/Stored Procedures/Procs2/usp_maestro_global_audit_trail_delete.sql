-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/07/2015 08:44:33 AM
-- Description:	Auto-generated method to delete a single maestro_global_audit_trail record.
-- =============================================
CREATE PROCEDURE usp_maestro_global_audit_trail_delete
	@id BIGINT
AS
BEGIN
	DELETE FROM
		[dbo].[maestro_global_audit_trail]
	WHERE
		[id]=@id
END
