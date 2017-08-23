-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetPhoneNumberForContactByProposal]
(
	@proposal_id INT,
	@phone_number_type_id INT
)
RETURNS VARCHAR(15)
AS
BEGIN
	DECLARE @return VARCHAR(15)
	DECLARE @contact_id INT

	SET @contact_id = (
		SELECT TOP 1 contact_id FROM proposal_contacts (NOLOCK) WHERE proposal_id=@proposal_id ORDER BY date_created DESC
	)

	SET @return = (
		SELECT
			TOP 1 phone_number + (CASE WHEN LEN(extension)>0 THEN 'x' + extension ELSE '' END)
		FROM
			phone_numbers (NOLOCK)
		WHERE
			id IN (
				SELECT phone_number_id FROM contact_phone_numbers (NOLOCK) WHERE contact_id=@contact_id
			)
			AND phone_number_type_id=@phone_number_type_id
		ORDER BY
			date_created DESC
	)

	RETURN @return
END
