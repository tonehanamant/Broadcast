CREATE FUNCTION [dbo].[GetProposalVersionIdentifier]  
(  
                @proposal_id INT  
)  
RETURNS VARCHAR(127)  
AS  
BEGIN  
                -- Declare the return variable here  
                DECLARE @return VARCHAR(127) = ''
  
                -- Add the T-SQL statements to compute the return value here  
                DECLARE @original_proposal_id AS INT  
                DECLARE @percentage AS INT  
                DECLARE @version AS INT  
                DECLARE @status AS VARCHAR(1)  
                DECLARE @brand_or_direct_response AS VARCHAR(1)  
                DECLARE @is_media_ocean_plan AS BIT;  
  
                SET @percentage = 0  
                  
                SELECT   
                                @original_proposal_id=original_proposal_id,  
                                @version=version_number,  
                                @brand_or_direct_response=CASE rate_card_type_id WHEN 1 THEN 'B' WHEN 2 THEN 'D' ELSE '' END,  
                                @status=CASE proposal_status_id WHEN 4 THEN 'C' ELSE 'U' END,  
                                @is_media_ocean_plan=is_media_ocean_plan  
                FROM   
                                proposals (NOLOCK)   
                WHERE   
                                id=@proposal_id  
  
                IF (SELECT SUM(rate_card_rate * num_spots) FROM proposal_details (NOLOCK) WHERE proposal_id=@proposal_id) > 0  
                                BEGIN SET @percentage = ROUND((SELECT SUM(proposal_rate * num_spots) / SUM(rate_card_rate * num_spots) FROM proposal_details (NOLOCK) WHERE proposal_id=@proposal_id) * 100.0, 0) END  
                IF(@is_media_ocean_plan = 1)  
                BEGIN  
                                SET @return = '(MO) ';  
                END  
                SET @return = @return + CAST(@proposal_id AS VARCHAR(24))  
                IF @original_proposal_id IS NOT NULL  
                                BEGIN SET @return = @return + ' (' + CAST(@original_proposal_id AS VARCHAR(24)) + '-R' + CAST(@version AS VARCHAR(24)) + ')' END  
                SET @return = @return + '(' + @status + ')-' + (CASE WHEN @percentage < 10 THEN '00' + CAST(@percentage AS VARCHAR(1)) WHEN @percentage < 100 THEN '0' + CAST(@percentage AS VARCHAR(2)) ELSE CAST(@percentage AS VARCHAR(3)) END)  
                SET @return = @return + @brand_or_direct_response  
  
                -- Return the result of the function  
                RETURN @return;  
END  
  
