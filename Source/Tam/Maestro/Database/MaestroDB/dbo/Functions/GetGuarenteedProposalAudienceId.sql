-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================

CREATE FUNCTION GetGuarenteedProposalAudienceId
(
	@proposal_id INT
)
RETURNS INT
AS
BEGIN
	DECLARE @return AS INT
	DECLARE @guarentee_type AS TINYINT

	SET @guarentee_type = (
		SELECT guarantee_type FROM proposals WHERE id=@proposal_id
	)

	SET @return = (
		SELECT audience_id FROM proposal_audiences WHERE proposal_id=@proposal_id AND ordinal = CASE @guarentee_type WHEN 0 THEN 0 WHEN 1 THEN 1 END
	)

	RETURN @return
END
