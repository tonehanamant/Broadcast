-- =============================================
-- Author:		John Carsley
-- Create date: 10/13/2011
-- Description:	Returns the records of active employees.
-- =============================================
CREATE PROCEDURE [dbo].[usp_MAS_GetActiveEmployees]
AS
SELECT
	id,
	username,
	accountdomainsid,
	firstname,
	lastname,
	mi,
	email,
	phone,
	internal_extension,
	[status],
	datecreated,
	datelastlogin,
	datelastmodified,
	hitcount
FROM
	employees WITH (NOLOCK)
WHERE 
	status = 0 -- Active = 0, Disabled = 1
	