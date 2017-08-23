
-- =============================================
-- Author:		CRUD Creator
-- Create date: 01/30/2017 12:09:59 PM
-- Description:	Auto-generated method to delete a single proposal_marry_candidates record.
-- =============================================
CREATE PROCEDURE usp_proposal_marry_candidates_delete
	@id INT
AS
BEGIN
	DELETE FROM
		[dbo].[proposal_marry_candidates]
	WHERE
		[id]=@id
END
