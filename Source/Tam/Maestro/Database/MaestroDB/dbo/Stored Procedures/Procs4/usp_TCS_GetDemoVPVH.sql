
 

-- =============================================

-- Author:        <Author,,Name>

-- Create date: <Create Date,,>

-- Description:   <Description,,>

-- =============================================

CREATE PROCEDURE [dbo].[usp_TCS_GetDemoVPVH]

(

      @proposal_id as int,

      @audience_id as int

)

 

AS

BEGIN

      select network_id, dbo.GetProposalDetailVPVH(id, @audience_id) from proposal_details where proposal_id = @proposal_id

END

 

