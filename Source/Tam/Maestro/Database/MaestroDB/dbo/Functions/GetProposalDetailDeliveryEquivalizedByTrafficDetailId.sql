-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION GetProposalDetailDeliveryEquivalizedByTrafficDetailId
(
	@traffic_detail_id INT,
	@audience_id INT
)
RETURNS FLOAT
AS
BEGIN
	DECLARE @return AS FLOAT
	DECLARE @num_rows FLOAT
	DECLARE @proposal_detail_id INT

	SET @return = 0.0
	SET @num_rows = 0.0

	DECLARE PROPOSAL_DETAIL_CURSOR CURSOR FOR SELECT proposal_detail_id FROM traffic_details_proposal_details_map tdpd (NOLOCK) WHERE traffic_detail_id=@traffic_detail_id
	OPEN PROPOSAL_DETAIL_CURSOR
		FETCH NEXT FROM PROPOSAL_DETAIL_CURSOR INTO @proposal_detail_id
		WHILE @@FETCH_STATUS = 0
			BEGIN
				SET @num_rows = @num_rows + 1.0
				SET @return = @return + dbo.GetProposalDetailDeliveryEquivalized(@proposal_detail_id,@audience_id)
				FETCH NEXT FROM PROPOSAL_DETAIL_CURSOR INTO @proposal_detail_id
			END
	CLOSE PROPOSAL_DETAIL_CURSOR
	DEALLOCATE PROPOSAL_DETAIL_CURSOR

	SET @return = @return / @num_rows

	RETURN @return
END
