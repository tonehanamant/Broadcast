CREATE TABLE [dbo].[maestro_global_audit_trail] (
    [id]            BIGINT        IDENTITY (1, 1) NOT NULL,
    [entity_id]     VARCHAR (100) NOT NULL,
    [entity_name]   VARCHAR (255) NOT NULL,
    [username]      VARCHAR (127) NOT NULL,
    [time_stamp]    DATETIME      CONSTRAINT [DF_maestro_global_audit_trail_time_stamp] DEFAULT (getdate()) NOT NULL,
    [audit_type]    TINYINT       NOT NULL,
    [property_name] VARCHAR (255) NOT NULL,
    [old_value]     VARCHAR (511) NULL,
    [new_value]     VARCHAR (511) NULL,
    CONSTRAINT [PK_maestro_global_audit_trail] PRIMARY KEY CLUSTERED ([id] ASC)
);




GO
CREATE NONCLUSTERED INDEX IX_maestro_global_audit_trail_entity_id_entity_name 
	ON maestro_global_audit_trail (entity_id, entity_name)

