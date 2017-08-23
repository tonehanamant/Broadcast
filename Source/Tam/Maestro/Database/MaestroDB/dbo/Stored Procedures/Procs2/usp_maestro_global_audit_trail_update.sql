-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/07/2015 12:54:02 PM
-- Description:	Auto-generated method to update a maestro_global_audit_trail record.
-- =============================================
CREATE PROCEDURE usp_maestro_global_audit_trail_update
	@id BIGINT,
	@entity_id VARCHAR(36),
	@entity_name VARCHAR(255),
	@username VARCHAR(127),
	@time_stamp DATETIME,
	@audit_type TINYINT,
	@property_name VARCHAR(255),
	@old_value VARCHAR(511),
	@new_value VARCHAR(511)
AS
BEGIN
	UPDATE
		[dbo].[maestro_global_audit_trail]
	SET
		[entity_id]=@entity_id,
		[entity_name]=@entity_name,
		[username]=@username,
		[time_stamp]=@time_stamp,
		[audit_type]=@audit_type,
		[property_name]=@property_name,
		[old_value]=@old_value,
		[new_value]=@new_value
	WHERE
		[id]=@id
END
