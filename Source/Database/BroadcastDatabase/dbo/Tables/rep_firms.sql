CREATE TABLE [dbo].[rep_firms] (
    [id]          INT           IDENTITY (1, 1) NOT NULL,
    [parent_name] VARCHAR (127) NOT NULL,
    [team_name]   VARCHAR (127) NOT NULL,
    CONSTRAINT [PK_rep_firms] PRIMARY KEY CLUSTERED ([id] ASC)
);

