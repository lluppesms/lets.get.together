CREATE TABLE [Dad].[InvitationCode](
    [InvitationCodeId] [int] IDENTITY(1,1) NOT NULL,
    [CircleId] [int] NOT NULL,
    [Code] [nvarchar](64) NOT NULL,
    [CreatedByUserId] [int] NOT NULL,
    [CreatedUtc] [datetime2](7) NOT NULL,
    [ExpiresUtc] [datetime2](7) NULL,
    [RedeemedByUserId] [int] NULL,
    [RedeemedUtc] [datetime2](7) NULL,
    [RevokedUtc] [datetime2](7) NULL,
    CONSTRAINT [PK_InvitationCode] PRIMARY KEY CLUSTERED ([InvitationCodeId] ASC),
    CONSTRAINT [FK_InvitationCode_Circle] FOREIGN KEY ([CircleId]) REFERENCES [Dad].[Circle] ([CircleId]) ON DELETE CASCADE,
    CONSTRAINT [FK_InvitationCode_User_CreatedBy] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Dad].[User] ([UserId]),
    CONSTRAINT [FK_InvitationCode_User_RedeemedBy] FOREIGN KEY ([RedeemedByUserId]) REFERENCES [Dad].[User] ([UserId]) ON DELETE SET NULL
)
GO

ALTER TABLE [Dad].[InvitationCode] ADD CONSTRAINT [DF_InvitationCode_CreatedUtc] DEFAULT (getutcdate()) FOR [CreatedUtc]
GO

CREATE UNIQUE INDEX [IX_InvitationCode_Code] ON [Dad].[InvitationCode] ([Code])
GO
CREATE INDEX [IX_InvitationCode_CircleId_CreatedUtc] ON [Dad].[InvitationCode] ([CircleId], [CreatedUtc])
GO