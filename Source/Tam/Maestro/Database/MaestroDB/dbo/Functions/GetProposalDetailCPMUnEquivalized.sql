/****** Object:  Function [dbo].[GetProposalDetailCPMUnEquivalized]    Script Date: 02/12/2014 10:16:38 ******/
CREATE FUNCTION [dbo].[GetProposalDetailCPMUnEquivalized]
(
    @proposal_detail_id INT,
    @audience_id INT
)
RETURNS MONEY
AS
BEGIN
    DECLARE @return MONEY
    DECLARE @delivery FLOAT

    SET @delivery = dbo.GetProposalDetailDeliveryUnEQ(@proposal_detail_id,@audience_id)
    
    SET @return = (
        SELECT
            CASE 
                WHEN @delivery > 0.0 THEN
					CAST(pd.proposal_rate / @delivery AS MONEY)
                ELSE
                    0.0
            END
        FROM
			proposal_details pd (NOLOCK)
        WHERE
            pd.id=@proposal_detail_id
    )
    RETURN @return
END
