CREATE TABLE [dbo].[network_types] (
    [id]   TINYINT      IDENTITY (1, 1) NOT NULL,
    [name] VARCHAR (31) NOT NULL,
    CONSTRAINT [PK_network_types] PRIMARY KEY CLUSTERED ([id] ASC)
);

