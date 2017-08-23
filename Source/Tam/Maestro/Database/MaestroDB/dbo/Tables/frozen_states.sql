CREATE TABLE [dbo].[frozen_states] (
    [media_month_id] SMALLINT     NOT NULL,
    [state_id]       INT          NOT NULL,
    [code]           VARCHAR (15) NOT NULL,
    [name]           VARCHAR (63) NOT NULL,
    [flag]           TINYINT      NULL,
    CONSTRAINT [PK_frozen_states] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [state_id] ASC) WITH (FILLFACTOR = 90)
);

