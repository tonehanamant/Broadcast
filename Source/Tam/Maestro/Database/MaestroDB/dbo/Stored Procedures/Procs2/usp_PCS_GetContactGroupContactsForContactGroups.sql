-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetContactGroupContactsForContactGroups]
	@ids VARCHAR(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		cgc.*
	FROM
		contact_group_contacts cgc (NOLOCK)
	WHERE
		cgc.contact_group_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END
