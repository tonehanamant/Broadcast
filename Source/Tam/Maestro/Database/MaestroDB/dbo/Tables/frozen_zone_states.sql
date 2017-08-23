CREATE TABLE [dbo].[frozen_zone_states] (
    [media_month_id] SMALLINT   NOT NULL,
    [zone_id]        INT        NOT NULL,
    [state_id]       INT        NOT NULL,
    [weight]         FLOAT (53) NOT NULL,
    CONSTRAINT [PK_frozen_zone_states] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [zone_id] ASC, [state_id] ASC)
);

