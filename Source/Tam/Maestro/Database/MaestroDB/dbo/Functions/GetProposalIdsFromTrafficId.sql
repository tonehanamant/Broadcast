-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION GetProposalIdsFromTrafficId
(
	@traffic_id INT
)
RETURNS VARCHAR(MAX)
AS
BEGIN
	DECLARE @return AS VARCHAR(MAX)
	DECLARE @proposal_id AS INT

	SET @return = ''

	DECLARE MYCURSOR CURSOR FOR SELECT proposal_id FROM traffic_proposals tp (NOLOCK) WHERE tp.traffic_id=@traffic_id
	OPEN MYCURSOR
		FETCH NEXT FROM MYCURSOR INTO @proposal_id
		WHILE @@FETCH_STATUS = 0
			BEGIN
				SET @return = @return + CAST(@proposal_id AS VARCHAR(25))
				FETCH NEXT FROM MYCURSOR INTO @proposal_id
				IF @@FETCH_STATUS = 0
					BEGIN
						SET @return = @return + ', '
					END
			END
	CLOSE MYCURSOR
	DEALLOCATE MYCURSOR

	RETURN @return
END
