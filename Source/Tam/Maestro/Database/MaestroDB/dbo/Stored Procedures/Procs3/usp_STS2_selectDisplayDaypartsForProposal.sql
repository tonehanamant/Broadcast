-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDisplayDaypartsForProposal]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		id,code,name,start_time,end_time,mon,tue,wed,thu,fri,sat,sun 
	FROM 
		vw_ccc_daypart (NOLOCK) 
	WHERE 
		id IN (SELECT daypart_id FROM daypart_maps (NOLOCK) WHERE map_set='Proposals')
		OR id IN (58,97)
END
