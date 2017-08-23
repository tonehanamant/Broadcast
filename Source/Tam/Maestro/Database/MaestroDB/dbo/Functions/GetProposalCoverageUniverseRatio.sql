
-- =============================================
-- Author:                            <Author,,Name>
-- Create date: <Create Date, ,>
-- Description:   <Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProposalCoverageUniverseRatio]
(
                @proposal_details_id INT,
                @traffic_id INT 
)
RETURNS FLOAT
AS
BEGIN
                DECLARE @return AS FLOAT
                DECLARE @MAXUNIVERSE as float;
                DECLARE @p1 as FLOAT;
                DECLARE @p2 as FLOAT;               
                DECLARE @match_detail_id as int;

                SET @return = 0.0
                set @p1 = (select dbo.GetProposalDetailCoverageUniverse(@proposal_details_id, 31));

                set @match_detail_id = 
                (
                                select 
                                                distinct top 1 pd2.id
                                from 
                                                proposal_details (NOLOCK) pd join
                                                release_cpmlink (NOLOCK) rc on pd.proposal_id = rc.proposal_id
                                                join release_cpmlink (NOLOCK) rc2 on rc2.traffic_id = rc.traffic_id and rc2.proposal_id <> rc.proposal_id
                                                join proposal_details (NOLOCK) pd2 on pd2.proposal_id = rc2.proposal_id and pd2.network_id = pd.network_id 
                                where
                                                pd.id = @proposal_details_id and rc.traffic_id = @traffic_id
                );

                set @p2 = (select dbo.GetProposalDetailCoverageUniverse(@match_detail_id, 31));
                
                IF(@p1 > @p2)
                                set @MAXUNIVERSE = @p1;
                ELSE
                                set @MAXUNIVERSE = @p2;

                set @return = @p1 / @MAXUNIVERSE; 
                                
                RETURN @return;

END
