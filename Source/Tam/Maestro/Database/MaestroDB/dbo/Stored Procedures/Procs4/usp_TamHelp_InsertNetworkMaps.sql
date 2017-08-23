CREATE PROC usp_TamHelp_InsertNetworkMaps 
(
	@internal_network_code varchar(15),
	@external_network_code varchar(15),
	@effective_date datetime
)
AS
BEGIN
BEGIN TRY

declare @network_id int
SELECT @network_id = n.id
FROM networks n
WHERE n.code = @internal_network_code;

IF @network_id IS NULL
	RAISERROR('Could not find a maestro network with a code of %s', --message
		 16, --Severity
		 1,
		 @internal_network_code) --State
		 
	INSERT INTO [dbo].[network_maps]
           ([network_id]
           ,[map_set]
           ,[map_value]
           ,[active]
           ,[flag]
           ,[effective_date])
     VALUES
           (@network_id
           ,'excel'
           ,@external_network_code
           ,1
           ,NULL
           ,@effective_date)
END TRY
BEGIN CATCH
	DECLARE @ErrorMessage NVARCHAR(4000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    SELECT 
        @ErrorMessage = ERROR_MESSAGE(),
        @ErrorSeverity = ERROR_SEVERITY(),
        @ErrorState = ERROR_STATE();

    --need to RAISERROR AGAIN to report it back to the user
    RAISERROR (@ErrorMessage, -- Message text.
               @ErrorSeverity, -- Severity.
               @ErrorState -- State.
               );
END CATCH;

END