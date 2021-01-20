CREATE TABLE [dbo].[detection_map_types] (
    [id]            INT           IDENTITY (1, 1) NOT NULL,
    [display]       NVARCHAR (16) NOT NULL,
    [version]       INT           NOT NULL,
    [modified_by]   VARCHAR (63)  NOT NULL,
    [modified_date] DATETIME      NOT NULL,
    CONSTRAINT [PK_detection_map_types] PRIMARY KEY CLUSTERED ([id] ASC)
);

