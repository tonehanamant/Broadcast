
-- =============================================
-- Author:		CRUD Creator
-- Create date: 01/30/2017 12:09:59 PM
-- Description:	Auto-generated method to insert a proposal_marry_candidates record.
-- =============================================
CREATE PROCEDURE usp_proposal_marry_candidates_insert
	@id INT OUTPUT,
	@proposal_id INT,
	@media_month_id INT,
	@spot_length_id INT,
	@base_index DECIMAL(19,2)
AS
BEGIN
	INSERT INTO [dbo].[proposal_marry_candidates]
	(
		[proposal_id],
		[media_month_id],
		[spot_length_id],
		[base_index]
	)
	VALUES
	(
		@proposal_id,
		@media_month_id,
		@spot_length_id,
		@base_index
	)

	SELECT
		@id = SCOPE_IDENTITY()
END

