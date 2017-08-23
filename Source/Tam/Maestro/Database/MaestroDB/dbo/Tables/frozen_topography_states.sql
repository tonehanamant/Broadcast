CREATE TABLE [dbo].[frozen_topography_states] (
    [media_month_id] SMALLINT NOT NULL,
    [topography_id]  INT      NOT NULL,
    [state_id]       INT      NOT NULL,
    [include]        BIT      NOT NULL,
    CONSTRAINT [PK_frozen_topography_states] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [topography_id] ASC, [state_id] ASC)
);

