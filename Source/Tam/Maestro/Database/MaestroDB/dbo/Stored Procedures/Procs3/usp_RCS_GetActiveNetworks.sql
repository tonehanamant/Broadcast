-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetActiveNetworks]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		id,
		code,
		name,
		active,
		flag,
		effective_date,
		language_id,
		affiliated_network_id,
		network_type_id
	FROM
		networks (NOLOCK)
	WHERE
		active=1
	ORDER BY
		code
END
