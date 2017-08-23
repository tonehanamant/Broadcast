CREATE PROCEDURE [dbo].[usp_BRS_GetStates]

AS
BEGIN

      SET NOCOUNT ON;

      SELECT
            id,
            code,
            upper(left([name], 1)) + lower(right([name], len([name]) - 1)),
            active,
            flag,
            effective_date
      FROM
            states (nolock)
      WHERE
            id not in (3,11,14,24,44,54,60)
      ORDER BY
            [name] asc
END