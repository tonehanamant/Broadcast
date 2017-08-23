-- =============================================
-- Author:		John Carsley
-- Create date: 10/24/2011
-- Description:	Gets the billing terms record for a proposal
-- Usage : exec usp_ACCT_GetBillingTermsForProposal 32093
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACCT_GetBillingTermsForProposal]
	@proposal_id int
AS 
BEGIN
	SET NOCOUNT ON;

	SELECT
		bt.*
	FROM 
		dbo.billing_terms bt WITH(NOLOCK)
	WHERE EXISTS
		(SELECT 1
			FROM [dbo].[proposals] p WITH (NOLOCK)
			WHERE  p.billing_terms_id = bt.id 
			and p.id = @proposal_id)
END
