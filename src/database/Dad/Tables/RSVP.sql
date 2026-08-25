CREATE TABLE [Dad].[RSVP](
    [RsvpId] [int] IDENTITY(1,1) NOT NULL,
    [EventId] [int] NOT NULL,
    [CircleId] [int] NOT NULL,
    [UserId] [int] NOT NULL,
    [Status] [nvarchar](32) NOT NULL,
    [Notes] [nvarchar](1000) NULL,
    [RespondedUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_RSVP] PRIMARY KEY CLUSTERED ([RsvpId] ASC),
    CONSTRAINT [FK_RSVP_Event] FOREIGN KEY ([EventId], [CircleId]) REFERENCES [Dad].[Event] ([EventId], [CircleId]) ON DELETE CASCADE,
    CONSTRAINT [FK_RSVP_User] FOREIGN KEY ([UserId]) REFERENCES [Dad].[User] ([UserId]),
    CONSTRAINT [FK_RSVP_CircleMembership] FOREIGN KEY ([CircleId], [UserId]) REFERENCES [Dad].[CircleMembership] ([CircleId], [UserId])
)
GO

ALTER TABLE [Dad].[RSVP] ADD CONSTRAINT [DF_RSVP_Status] DEFAULT (N'Pending') FOR [Status]
GO
ALTER TABLE [Dad].[RSVP] ADD CONSTRAINT [DF_RSVP_RespondedUtc] DEFAULT (getutcdate()) FOR [RespondedUtc]
GO

CREATE UNIQUE INDEX [IX_RSVP_EventId_UserId] ON [Dad].[RSVP] ([EventId], [UserId])
GO