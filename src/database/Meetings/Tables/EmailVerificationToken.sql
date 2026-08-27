CREATE TABLE [Meetings].[EmailVerificationToken](
    [EmailVerificationTokenId] [int] IDENTITY(1,1) NOT NULL,
    [TokenHash] [nvarchar](64) NOT NULL,
    [NormalizedEmailAddress] [nvarchar](320) NOT NULL,
    [InvitationCodeId] [int] NULL,
    [UserEmailAliasId] [int] NULL,
    [ExpiresUtc] [datetime2](7) NOT NULL,
    [UsedUtc] [datetime2](7) NULL,
    [CreatedUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_EmailVerificationToken] PRIMARY KEY CLUSTERED ([EmailVerificationTokenId] ASC),
    CONSTRAINT [CK_EmailVerificationToken_Target] CHECK (([InvitationCodeId] IS NOT NULL AND [UserEmailAliasId] IS NULL) OR ([InvitationCodeId] IS NULL AND [UserEmailAliasId] IS NOT NULL)),
    CONSTRAINT [FK_EmailVerificationToken_InvitationCode] FOREIGN KEY ([InvitationCodeId]) REFERENCES [Meetings].[InvitationCode] ([InvitationCodeId]) ON DELETE CASCADE,
    CONSTRAINT [FK_EmailVerificationToken_UserEmailAlias] FOREIGN KEY ([UserEmailAliasId]) REFERENCES [Meetings].[UserEmailAlias] ([UserEmailAliasId]) ON DELETE CASCADE
)
GO

ALTER TABLE [Meetings].[EmailVerificationToken] ADD CONSTRAINT [DF_EmailVerificationToken_CreatedUtc] DEFAULT (getutcdate()) FOR [CreatedUtc]
GO

CREATE UNIQUE INDEX [UX_EmailVerificationToken_TokenHash] ON [Meetings].[EmailVerificationToken] ([TokenHash])
GO
CREATE INDEX [IX_EmailVerificationToken_InvitationCodeId_UsedUtc] ON [Meetings].[EmailVerificationToken] ([InvitationCodeId], [UsedUtc])
GO