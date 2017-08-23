-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Modified:	11/8/2013
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetAudienceStringFromID]
(
	@audienceID INT
)
RETURNS VARCHAR(31)
AS
BEGIN
	DECLARE @return VARCHAR(31)
	SELECT @return = a.name FROM dbo.audiences a (NOLOCK) WHERE a.id=@audienceID
	RETURN @return;
END
