-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/4/2012
-- Description:	<Description,,>
-- =============================================
-- usp_STS2_IsPartOfLocalOrNationalPods 696
CREATE PROCEDURE [dbo].[usp_STS2_IsPartOfLocalOrNationalPods]
	@system_id INT,
	@system_statement_group_id INT
AS
BEGIN
	DECLARE @count AS INT
	CREATE TABLE #system_ids (system_id INT)
	
	IF @system_id IS NOT NULL
		INSERT INTO #system_ids SELECT @system_id
	ELSE IF @system_statement_group_id IS NOT NULL
		INSERT INTO #system_ids SELECT ssgs.system_id FROM dbo.system_statement_group_systems ssgs (NOLOCK) WHERE ssgs.system_statement_group_id=@system_statement_group_id

	SELECT
		@count = COUNT(*)
	FROM
		dbo.system_zones sz (NOLOCK)
	WHERE
		sz.zone_id IN (
			SELECT
				DISTINCT zone_id
			FROM
				dbo.system_zones sz (NOLOCK)
			WHERE
				sz.system_id IN (
					SELECT system_id FROM #system_ids
				)
		)
		AND sz.system_id IN (698,749)

	IF @count > 0
		SELECT CAST(1 AS BIT)
	ELSE
		SELECT CAST(0 AS BIT)
END
