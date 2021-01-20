CREATE TABLE [dbo].[audiences] (
    [id]                INT           IDENTITY (1, 1) NOT NULL,
    [category_code]     TINYINT       NOT NULL,
    [sub_category_code] CHAR (1)      NOT NULL,
    [range_start]       INT           NULL,
    [range_end]         INT           NULL,
    [custom]            BIT           NOT NULL,
    [code]              VARCHAR (15)  NOT NULL,
    [name]              VARCHAR (127) NOT NULL,
    CONSTRAINT [PK_audiences] PRIMARY KEY CLUSTERED ([id] ASC)
);

