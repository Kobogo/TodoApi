IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250919122740_InitialCreate'
)
BEGIN
    CREATE TABLE [StaticTasks] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NULL,
        [IsCompleted] bit NOT NULL,
        [TimeOfDay] time NULL,
        [LastCompletedDate] datetime2 NULL,
        [LastShownDate] datetime2 NULL,
        [RepeatDays] nvarchar(max) NULL,
        CONSTRAINT [PK_StaticTasks] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250919122740_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'IsCompleted', N'LastCompletedDate', N'LastShownDate', N'RepeatDays', N'TimeOfDay', N'Title') AND [object_id] = OBJECT_ID(N'[StaticTasks]'))
        SET IDENTITY_INSERT [StaticTasks] ON;
    EXEC(N'INSERT INTO [StaticTasks] ([Id], [IsCompleted], [LastCompletedDate], [LastShownDate], [RepeatDays], [TimeOfDay], [Title])
    VALUES (1, CAST(0 AS bit), NULL, NULL, NULL, NULL, N''Tømme opvaskemaskine & flyde den igen''),
    (2, CAST(0 AS bit), NULL, NULL, NULL, NULL, N''Tørre støv af''),
    (3, CAST(0 AS bit), NULL, NULL, NULL, NULL, N''Dække bord + tørre bord af''),
    (4, CAST(0 AS bit), NULL, NULL, NULL, NULL, N''Støvsuge hele huset''),
    (5, CAST(0 AS bit), NULL, NULL, NULL, NULL, N''Vaske gulv''),
    (6, CAST(0 AS bit), NULL, NULL, NULL, NULL, N''Hænge vasketøj op med en voksen''),
    (7, CAST(0 AS bit), NULL, NULL, NULL, NULL, N''Skylle af efter aftensmaden''),
    (8, CAST(0 AS bit), NULL, NULL, NULL, NULL, N''Pille tøj ned af tørrestativet''),
    (9, CAST(0 AS bit), NULL, NULL, NULL, NULL, N''Lægge tøj sammen + lægge tøj på plads med en voksen''),
    (10, CAST(0 AS bit), NULL, NULL, NULL, NULL, N''Tømme skraldespande''),
    (11, CAST(0 AS bit), NULL, NULL, NULL, NULL, N''Ordne badeværelser med en voksen''),
    (12, CAST(0 AS bit), NULL, NULL, NULL, NULL, N''Slå græs''),
    (13, CAST(0 AS bit), NULL, NULL, NULL, NULL, N''Fejrne ukrudt (min. 1 spand)''),
    (14, CAST(0 AS bit), NULL, NULL, NULL, NULL, N''Være med til at lave aftensmad''),
    (15, CAST(0 AS bit), NULL, NULL, NULL, NULL, N''Fylde op i køleskab med sodavand''),
    (16, CAST(0 AS bit), NULL, NULL, NULL, NULL, N''Give kattene mad''),
    (17, CAST(0 AS bit), NULL, NULL, NULL, NULL, N''Rede senge (Alle)'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'IsCompleted', N'LastCompletedDate', N'LastShownDate', N'RepeatDays', N'TimeOfDay', N'Title') AND [object_id] = OBJECT_ID(N'[StaticTasks]'))
        SET IDENTITY_INSERT [StaticTasks] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250919122740_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250919122740_InitialCreate', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StaticTasks]') AND [c].[name] = N'Title');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [StaticTasks] DROP CONSTRAINT [' + @var + '];');
    EXEC(N'UPDATE [StaticTasks] SET [Title] = N'''' WHERE [Title] IS NULL');
    ALTER TABLE [StaticTasks] ALTER COLUMN [Title] nvarchar(max) NOT NULL;
    ALTER TABLE [StaticTasks] ADD DEFAULT N'' FOR [Title];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    EXEC(N'UPDATE [StaticTasks] SET [RepeatDays] = N''[]''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    EXEC(N'UPDATE [StaticTasks] SET [RepeatDays] = N''[]''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    EXEC(N'UPDATE [StaticTasks] SET [RepeatDays] = N''[]''
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    EXEC(N'UPDATE [StaticTasks] SET [RepeatDays] = N''[]''
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    EXEC(N'UPDATE [StaticTasks] SET [RepeatDays] = N''[]''
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    EXEC(N'UPDATE [StaticTasks] SET [RepeatDays] = N''[]''
    WHERE [Id] = 6;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    EXEC(N'UPDATE [StaticTasks] SET [RepeatDays] = N''[]''
    WHERE [Id] = 7;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    EXEC(N'UPDATE [StaticTasks] SET [RepeatDays] = N''[]''
    WHERE [Id] = 8;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    EXEC(N'UPDATE [StaticTasks] SET [RepeatDays] = N''[]''
    WHERE [Id] = 9;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    EXEC(N'UPDATE [StaticTasks] SET [RepeatDays] = N''[]''
    WHERE [Id] = 10;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    EXEC(N'UPDATE [StaticTasks] SET [RepeatDays] = N''[]''
    WHERE [Id] = 11;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    EXEC(N'UPDATE [StaticTasks] SET [RepeatDays] = N''[]''
    WHERE [Id] = 12;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    EXEC(N'UPDATE [StaticTasks] SET [RepeatDays] = N''[]''
    WHERE [Id] = 13;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    EXEC(N'UPDATE [StaticTasks] SET [RepeatDays] = N''[]''
    WHERE [Id] = 14;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    EXEC(N'UPDATE [StaticTasks] SET [RepeatDays] = N''[]''
    WHERE [Id] = 15;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    EXEC(N'UPDATE [StaticTasks] SET [RepeatDays] = N''[]''
    WHERE [Id] = 16;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    EXEC(N'UPDATE [StaticTasks] SET [RepeatDays] = N''[]''
    WHERE [Id] = 17;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251022074019_CombinedSeedAndConversion'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251022074019_CombinedSeedAndConversion', N'9.0.8');
END;

COMMIT;
GO

