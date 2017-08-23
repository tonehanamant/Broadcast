

 

-- =============================================

-- Author:        <Author,,Name>

-- Create date: <Create Date,,>

-- Description:   <Description,,>

-- =============================================

CREATE PROCEDURE [dbo].[usp_TCS_GetPrimaryProposalNetworks]

(

      @proposal_id as int

)

 

AS

BEGIN

      select network_id, daypart_id from proposal_details (NOLOCK) where proposal_id = @proposal_id

END

 


