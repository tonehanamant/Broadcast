-- =============================================
-- Author:		<Nicholas Kheynis>
-- Create date: <10-6-2014>
-- Description:	<Get All Files From MSA Delivery Files By Id>
-- =============================================
-- SELECT dbo.usp_PCS_GetMSADataIntegraterCompletedFilesById
CREATE PROCEDURE [dbo].[usp_PCS_GetMSADataIntegraterCompletedFilesById]
	@msa_file_id INT
AS
BEGIN
	SELECT
		mdf.*,
		d.file_name,
		e.firstname,
		e.lastname
		
	FROM
		msa_delivery_files mdf
		JOIN documents d (NOLOCK)ON d.id = mdf.document_id
		JOIN dbo.employees e (NOLOCK) ON e.id = mdf.employee_id
	WHERE
		mdf.id = @msa_file_id
END
