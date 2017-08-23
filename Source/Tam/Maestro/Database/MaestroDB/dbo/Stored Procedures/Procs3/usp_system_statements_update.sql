-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/30/2015 02:31:02 PM
-- Description:	Auto-generated method to update a system_statements record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_system_statements_update]
	@id INT,
	@statement_id INT,
	@system_statement_group_id INT,
	@system_id INT,
	@name VARCHAR(63),
	@status TINYINT,
	@enabled BIT,
	@confirmed_amount MONEY
AS
BEGIN
	UPDATE
		[dbo].[system_statements]
	SET
		[statement_id]=@statement_id,
		[system_statement_group_id]=@system_statement_group_id,
		[system_id]=@system_id,
		[name]=@name,
		[status]=@status,
		[enabled]=@enabled,
		[confirmed_amount]=@confirmed_amount
	WHERE
		[id]=@id
END

