CREATE TABLE [dbo].[sequence_number] (
    [Id]                 INT          NOT NULL,
    [name]               VARCHAR (50) NOT NULL,
    [current_value]      BIGINT       NOT NULL,
    [date_last_modified] DATETIME     NOT NULL,
    CONSTRAINT [PK_sequence_number] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 90)
);

