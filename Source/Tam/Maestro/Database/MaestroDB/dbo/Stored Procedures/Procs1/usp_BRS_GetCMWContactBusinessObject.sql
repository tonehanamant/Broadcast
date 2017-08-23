

CREATE PROCEDURE [dbo].[usp_BRS_GetCMWContactBusinessObject]
@contactID int

AS
BEGIN

	SET NOCOUNT ON;

	exec dbo.usp_cmw_contacts_select @contactID
	SELECT
		id,
		cmw_contact_id,
		contact_method_id,
		value,
		ordinal,
		date_last_modified,
		date_created,
		name
	FROM
		cmw_contact_infos (NOLOCK)
	WHERE
		cmw_contact_id = @contactID
			
END




