CREATE TABLE [dbo].[days] (
    [id]      INT          IDENTITY (1, 1) NOT NULL,
    [code_1]  CHAR (10)    NOT NULL,
    [code_2]  CHAR (10)    NOT NULL,
    [code_3]  CHAR (10)    NOT NULL,
    [name]    VARCHAR (15) NOT NULL,
    [ordinal] INT          NOT NULL,
    CONSTRAINT [PK_days] PRIMARY KEY CLUSTERED ([id] ASC)
);

