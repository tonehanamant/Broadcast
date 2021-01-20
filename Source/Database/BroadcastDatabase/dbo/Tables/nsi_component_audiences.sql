CREATE TABLE [dbo].[nsi_component_audiences] (
    [audience_id]       INT           NOT NULL,
    [category_code]     TINYINT       NOT NULL,
    [sub_category_code] CHAR (1)      NOT NULL,
    [range_start]       INT           NULL,
    [range_end]         INT           NULL,
    [custom]            BIT           NOT NULL,
    [code]              VARCHAR (15)  NOT NULL,
    [name]              VARCHAR (127) NOT NULL,
    CONSTRAINT [PK_nsi_component_audiences] PRIMARY KEY CLUSTERED ([audience_id] ASC),
    CONSTRAINT [FK_nsi_component_audiences_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]) ON DELETE CASCADE
);

