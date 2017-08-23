-- =============================================
-- Author:		Nicholas Kheynis
-- Create date: 3/25/2014
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetAudiencesByCodeForMsa 'F25-54'
CREATE PROCEDURE [dbo].[usp_PCS_GetAudiencesByCodeForMsa]
	@audience_code VARCHAR(31)
AS
BEGIN
	DECLARE @count INT;
	SELECT @count = COUNT(1) FROM audiences a (NOLOCK) WHERE a.code = @audience_code;
	
	IF @count = 1
	BEGIN
		SELECT a.* FROM audiences a (NOLOCK) WHERE a.code = @audience_code;
	END
	ELSE
	BEGIN
		IF (SELECT COUNT(1) FROM audiences a (NOLOCK) WHERE a.code = REPLACE(REPLACE(@audience_code,'F','W'),'P','A')) > 0
			SELECT
				a.*
			FROM
				audiences a (NOLOCK)
			WHERE
				a.code = REPLACE(REPLACE(@audience_code,'F','W'),'P','A')
		ELSE IF (SELECT COUNT(1) FROM audiences a (NOLOCK) WHERE a.code = REPLACE(REPLACE(@audience_code,'F','W'),'P','K')) > 0
			SELECT
				a.*
			FROM
				audiences a (NOLOCK)
			WHERE
				a.code = REPLACE(REPLACE(@audience_code,'F','W'),'P','K')
		ELSE 
			SELECT
				a.*
			FROM
				audiences a (NOLOCK)
			WHERE
				a.code = REPLACE(REPLACE(@audience_code,'F','W'),'P','T')
	END
END
