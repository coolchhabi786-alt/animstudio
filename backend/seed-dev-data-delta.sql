USE [AnimStudio];
GO

/* =============================================================================
   AnimStudio — Delta Seed Script (Phase 4 + Phase 5 new tables)
   =============================================================================
   Prerequisite : scripts\seed-dev-data.sql must have been run first, OR
                  the identity/content base tables must already contain data
                  for the dev team.

   Dev identity
   ------------
   UserId  : 4327FA31-FF93-4B68-AF4D-1FC56AFA33C5
   TeamId  : C0000001-0000-0000-0000-000000000001

   What this inserts
   -----------------
   §1  content.Characters       — 3 characters at different training stages
   §2  content.EpisodeCharacters— character cast links for 5 active episodes
   §3  content.Episodes UPDATE  — keeps CharacterIds JSON in sync with join table
   §4  content.Scripts          — scripts for every episode past the Script stage

   Idempotent — every INSERT is guarded with IF NOT EXISTS.
   Wrapped in a transaction — rolls back entirely on any error.
   ============================================================================= */

-- ── Stable GUIDs ─────────────────────────────────────────────────────────────
-- Characters
DECLARE @Char1 UNIQUEIDENTIFIER = 'EEEEEEEE-0001-0000-0000-000000000000'; -- Prof. Whiskerbolt (Ready)
DECLARE @Char2 UNIQUEIDENTIFIER = 'EEEEEEEE-0002-0000-0000-000000000000'; -- Zara the Stargazer (Training)
DECLARE @Char3 UNIQUEIDENTIFIER = 'EEEEEEEE-0003-0000-0000-000000000000'; -- Commander Vex (Draft)

-- Team
DECLARE @TeamId UNIQUEIDENTIFIER = 'C0000001-0000-0000-0000-000000000001';

-- Episodes confirmed in DB for this team
DECLARE @Ep_Awakening    UNIQUEIDENTIFIER = 'F0000001-0000-0000-0000-000000000001'; -- The Awakening     (Completed)
DECLARE @Ep_FirstFlight  UNIQUEIDENTIFIER = 'F0000001-0000-0000-0000-000000000002'; -- First Flight       (Draft)
DECLARE @Ep_Forest       UNIQUEIDENTIFIER = 'DDDDDDDD-0001-0000-0000-000000000000'; -- The Forest Guardian(Script)
DECLARE @Ep_Picnic       UNIQUEIDENTIFIER = 'DDDDDDDD-0002-0000-0000-000000000000'; -- Picnic Disaster    (Done)
DECLARE @Ep_DarkNeon     UNIQUEIDENTIFIER = 'DDDDDDDD-0003-0000-0000-000000000000'; -- Dark Neon Rising   (Idle)
DECLARE @Ep_Electric     UNIQUEIDENTIFIER = 'DDDDDDDD-0004-0000-0000-000000000000'; -- Electric Storm     (Animation)
DECLARE @Ep_Launch       UNIQUEIDENTIFIER = 'F0000001-0000-0000-0000-000000000003'; -- Launch Day         (Rendering)

-- Script GUIDs (one per episode that has reached Script stage or beyond)
DECLARE @Scr_Awakening UNIQUEIDENTIFIER = 'AABB0003-0000-0000-0000-000000000000';
DECLARE @Scr_Forest    UNIQUEIDENTIFIER = 'AABB0001-0000-0000-0000-000000000000';
DECLARE @Scr_Picnic    UNIQUEIDENTIFIER = 'AABB0002-0000-0000-0000-000000000000';
DECLARE @Scr_Electric  UNIQUEIDENTIFIER = 'AABB0004-0000-0000-0000-000000000000';
DECLARE @Scr_Launch    UNIQUEIDENTIFIER = 'AABB0005-0000-0000-0000-000000000000';

BEGIN TRANSACTION;
BEGIN TRY

-- =============================================================================
PRINT '── §1  content.Characters ───────────────────────────────────────────────';
-- =============================================================================
-- RowVersion is a true SQL Server rowversion — omit; auto-assigned.
-- TrainingStatus stored as string (HasConversion<string>()).

-- ── Character 1: Professor Whiskerbolt — Ready (100%) ─────────────────────────
IF NOT EXISTS (SELECT 1 FROM [content].[Characters] WHERE [Id] = @Char1)
BEGIN
    INSERT INTO [content].[Characters]
        ([Id], [TeamId], [Name], [Description], [StyleDna],
         [ImageUrl], [LoraWeightsUrl], [TriggerWord],
         [TrainingStatus], [TrainingProgressPercent], [CreditsCost],
         [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
        (@Char1, @TeamId,
         'Professor Whiskerbolt',
         'A wise and slightly eccentric elderly professor with a magnificent silver moustache. Wears a tweed jacket with elbow patches and round spectacles.',
         'elderly professor, tweed jacket, elbow patches, round spectacles, silver moustache, expressive eyes, warm smile, slightly dishevelled hair',
         'https://cdn.animstudio.local/characters/whiskerbolt/reference.png',
         'https://blob.animstudio.local/lora/whiskerbolt.safetensors',
         'PROF_WHISKERBOLT',
         'Ready', 100, 50,
         '2026-04-05T10:00:00.0000000+00:00',
         '2026-04-07T14:30:00.0000000+00:00', 0);
    PRINT '  + Character: Professor Whiskerbolt (Ready, 100%)';
END ELSE PRINT '  ✓ Prof. Whiskerbolt already exists';

-- ── Character 2: Zara the Stargazer — Training (60%) ──────────────────────────
IF NOT EXISTS (SELECT 1 FROM [content].[Characters] WHERE [Id] = @Char2)
BEGIN
    INSERT INTO [content].[Characters]
        ([Id], [TeamId], [Name], [Description], [StyleDna],
         [ImageUrl], [LoraWeightsUrl], [TriggerWord],
         [TrainingStatus], [TrainingProgressPercent], [CreditsCost],
         [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
        (@Char2, @TeamId,
         'Zara the Stargazer',
         'A teenage astronomer with a passion for the cosmos. Has silver-streaked hair, freckles, and always carries a battered telescope.',
         'teenage girl astronomer, silver-streaked hair, freckles, bright curious eyes, oversized hoodie, battered telescope prop, cosmic accessories',
         'https://cdn.animstudio.local/characters/zara/reference.png',
         NULL, 'ZARA_STARGAZER',
         'Training', 60, 50,
         '2026-04-10T09:00:00.0000000+00:00',
         '2026-04-12T08:45:00.0000000+00:00', 0);
    PRINT '  + Character: Zara the Stargazer (Training, 60%)';
END ELSE PRINT '  ✓ Zara the Stargazer already exists';

-- ── Character 3: Commander Vex — Draft (0%) ───────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [content].[Characters] WHERE [Id] = @Char3)
BEGIN
    INSERT INTO [content].[Characters]
        ([Id], [TeamId], [Name], [Description], [StyleDna],
         [ImageUrl], [LoraWeightsUrl], [TriggerWord],
         [TrainingStatus], [TrainingProgressPercent], [CreditsCost],
         [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
        (@Char3, @TeamId,
         'Commander Vex',
         'An imposing galactic commander with chrome battle armour and piercing blue eyes. Commands authority but hides a troubled past.',
         'galactic commander, chrome battle armour, piercing blue eyes, stern expression, imposing stature, futuristic military uniform',
         NULL, NULL, NULL,
         'Draft', 0, 50,
         '2026-04-12T10:00:00.0000000+00:00',
         '2026-04-12T10:00:00.0000000+00:00', 0);
    PRINT '  + Character: Commander Vex (Draft, 0%)';
END ELSE PRINT '  ✓ Commander Vex already exists';


-- =============================================================================
PRINT '── §2  content.EpisodeCharacters ────────────────────────────────────────';
-- =============================================================================
-- Only Ready characters may be attached (Prof. Whiskerbolt + Zara on eligible episodes).
-- Draft (Commander Vex) and Idle/Draft episodes are excluded.

-- The Awakening (Completed) ← Prof. Whiskerbolt
IF EXISTS (SELECT 1 FROM [content].[Episodes] WHERE [Id] = @Ep_Awakening)
    AND NOT EXISTS (SELECT 1 FROM [content].[EpisodeCharacters] WHERE [EpisodeId] = @Ep_Awakening AND [CharacterId] = @Char1)
BEGIN
    INSERT INTO [content].[EpisodeCharacters] ([EpisodeId], [CharacterId], [AttachedAt])
    VALUES (@Ep_Awakening, @Char1, '2026-04-03T12:00:00.0000000+00:00');
    PRINT '  + The Awakening ← Prof. Whiskerbolt';
END

-- The Forest Guardian (Script) ← Prof. Whiskerbolt
IF EXISTS (SELECT 1 FROM [content].[Episodes] WHERE [Id] = @Ep_Forest)
    AND NOT EXISTS (SELECT 1 FROM [content].[EpisodeCharacters] WHERE [EpisodeId] = @Ep_Forest AND [CharacterId] = @Char1)
BEGIN
    INSERT INTO [content].[EpisodeCharacters] ([EpisodeId], [CharacterId], [AttachedAt])
    VALUES (@Ep_Forest, @Char1, '2026-04-06T11:00:00.0000000+00:00');
    PRINT '  + The Forest Guardian ← Prof. Whiskerbolt';
END

-- The Great Picnic Disaster (Done) ← Prof. Whiskerbolt
IF EXISTS (SELECT 1 FROM [content].[Episodes] WHERE [Id] = @Ep_Picnic)
    AND NOT EXISTS (SELECT 1 FROM [content].[EpisodeCharacters] WHERE [EpisodeId] = @Ep_Picnic AND [CharacterId] = @Char1)
BEGIN
    INSERT INTO [content].[EpisodeCharacters] ([EpisodeId], [CharacterId], [AttachedAt])
    VALUES (@Ep_Picnic, @Char1, '2026-04-06T12:00:00.0000000+00:00');
    PRINT '  + The Great Picnic Disaster ← Prof. Whiskerbolt';
END

-- The Great Picnic Disaster (Done) ← Zara the Stargazer
IF EXISTS (SELECT 1 FROM [content].[Episodes] WHERE [Id] = @Ep_Picnic)
    AND NOT EXISTS (SELECT 1 FROM [content].[EpisodeCharacters] WHERE [EpisodeId] = @Ep_Picnic AND [CharacterId] = @Char2)
BEGIN
    INSERT INTO [content].[EpisodeCharacters] ([EpisodeId], [CharacterId], [AttachedAt])
    VALUES (@Ep_Picnic, @Char2, '2026-04-06T12:05:00.0000000+00:00');
    PRINT '  + The Great Picnic Disaster ← Zara the Stargazer';
END

-- Electric Storm (Animation) ← Prof. Whiskerbolt
IF EXISTS (SELECT 1 FROM [content].[Episodes] WHERE [Id] = @Ep_Electric)
    AND NOT EXISTS (SELECT 1 FROM [content].[EpisodeCharacters] WHERE [EpisodeId] = @Ep_Electric AND [CharacterId] = @Char1)
BEGIN
    INSERT INTO [content].[EpisodeCharacters] ([EpisodeId], [CharacterId], [AttachedAt])
    VALUES (@Ep_Electric, @Char1, '2026-04-09T09:00:00.0000000+00:00');
    PRINT '  + Electric Storm ← Prof. Whiskerbolt';
END

-- Launch Day (Rendering) ← Prof. Whiskerbolt
IF EXISTS (SELECT 1 FROM [content].[Episodes] WHERE [Id] = @Ep_Launch)
    AND NOT EXISTS (SELECT 1 FROM [content].[EpisodeCharacters] WHERE [EpisodeId] = @Ep_Launch AND [CharacterId] = @Char1)
BEGIN
    INSERT INTO [content].[EpisodeCharacters] ([EpisodeId], [CharacterId], [AttachedAt])
    VALUES (@Ep_Launch, @Char1, '2026-04-09T10:00:00.0000000+00:00');
    PRINT '  + Launch Day ← Prof. Whiskerbolt';
END

-- Launch Day (Rendering) ← Zara the Stargazer
IF EXISTS (SELECT 1 FROM [content].[Episodes] WHERE [Id] = @Ep_Launch)
    AND NOT EXISTS (SELECT 1 FROM [content].[EpisodeCharacters] WHERE [EpisodeId] = @Ep_Launch AND [CharacterId] = @Char2)
BEGIN
    INSERT INTO [content].[EpisodeCharacters] ([EpisodeId], [CharacterId], [AttachedAt])
    VALUES (@Ep_Launch, @Char2, '2026-04-09T10:05:00.0000000+00:00');
    PRINT '  + Launch Day ← Zara the Stargazer';
END


-- =============================================================================
PRINT '── §3  content.Episodes — sync CharacterIds JSON ────────────────────────';
-- =============================================================================
-- Episodes.CharacterIds (nvarchar JSON array) must stay in sync with the
-- EpisodeCharacters join table so both the Phase-2 and Phase-4 read paths agree.

UPDATE [content].[Episodes]
SET    [CharacterIds] = '["EEEEEEEE-0001-0000-0000-000000000000"]',
       [UpdatedAt]    = '2026-04-12T13:00:00.0000000+00:00'
WHERE  [Id] = @Ep_Awakening AND [CharacterIds] = '[]';
IF @@ROWCOUNT > 0 PRINT '  + CharacterIds synced: The Awakening';

UPDATE [content].[Episodes]
SET    [CharacterIds] = '["EEEEEEEE-0001-0000-0000-000000000000"]',
       [UpdatedAt]    = '2026-04-12T13:00:00.0000000+00:00'
WHERE  [Id] = @Ep_Forest AND [CharacterIds] NOT LIKE '%EEEEEEEE-0001%';
IF @@ROWCOUNT > 0 PRINT '  + CharacterIds synced: The Forest Guardian';

UPDATE [content].[Episodes]
SET    [CharacterIds] = '["EEEEEEEE-0001-0000-0000-000000000000","EEEEEEEE-0002-0000-0000-000000000000"]',
       [UpdatedAt]    = '2026-04-12T13:00:00.0000000+00:00'
WHERE  [Id] = @Ep_Picnic AND [CharacterIds] NOT LIKE '%EEEEEEEE-0001%';
IF @@ROWCOUNT > 0 PRINT '  + CharacterIds synced: The Great Picnic Disaster';

UPDATE [content].[Episodes]
SET    [CharacterIds] = '["EEEEEEEE-0001-0000-0000-000000000000"]',
       [UpdatedAt]    = '2026-04-12T13:00:00.0000000+00:00'
WHERE  [Id] = @Ep_Electric AND [CharacterIds] NOT LIKE '%EEEEEEEE-0001%';
IF @@ROWCOUNT > 0 PRINT '  + CharacterIds synced: Electric Storm';

UPDATE [content].[Episodes]
SET    [CharacterIds] = '["EEEEEEEE-0001-0000-0000-000000000000","EEEEEEEE-0002-0000-0000-000000000000"]',
       [UpdatedAt]    = '2026-04-12T13:00:00.0000000+00:00'
WHERE  [Id] = @Ep_Launch AND [CharacterIds] NOT LIKE '%EEEEEEEE-0001%';
IF @@ROWCOUNT > 0 PRINT '  + CharacterIds synced: Launch Day';


-- =============================================================================
PRINT '── §4  content.Scripts ──────────────────────────────────────────────────';
-- =============================================================================
-- RowVersion is a true SQL Server rowversion — omit; auto-assigned.
-- One script per episode (unique index on EpisodeId).
-- Covers every episode that has progressed to Script stage or beyond.

-- ── Script: The Awakening (Completed) ─────────────────────────────────────────
IF EXISTS (SELECT 1 FROM [content].[Episodes] WHERE [Id] = @Ep_Awakening)
    AND NOT EXISTS (SELECT 1 FROM [content].[Scripts] WHERE [EpisodeId] = @Ep_Awakening)
BEGIN
    INSERT INTO [content].[Scripts]
        ([Id], [EpisodeId], [Title], [RawJson], [IsManuallyEdited], [DirectorNotes],
         [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
        (@Scr_Awakening, @Ep_Awakening,
         'The Awakening — Final Script',
         '{"title":"The Awakening","scenes":[{"scene_number":1,"visual_description":"A vast cave lit by bioluminescent fungi. EMBER the dragon hatchling cracks through an enormous obsidian egg, shaking crystals from the walls. Ancient runes on the cave walls begin to glow.","emotional_tone":"wonder, birth","dialogue":[{"character":"Elder Dragon (V.O.)","text":"The first breath is the hardest. But you were born for this.","start_time":0.0,"end_time":4.5}]},{"scene_number":2,"visual_description":"Ember steps into sunlight for the first time, pupils adjusting. A mountain range stretches endlessly to the horizon. She spreads her wings — they are still damp but enormous.","emotional_tone":"awe, possibility","dialogue":[{"character":"Ember","text":"It is... so big.","start_time":0.0,"end_time":2.0},{"character":"Elder Dragon (V.O.)","text":"Yes. And all of it is waiting for you.","start_time":2.5,"end_time":5.5}]}]}',
         1,
         'Opening rune glow should pulse on the music beat. Sunlight scene — lens flare effect.',
         '2026-04-03T16:00:00.0000000+00:00',
         '2026-04-04T09:30:00.0000000+00:00', 0);
    PRINT '  + Script: The Awakening (manually edited)';
END ELSE PRINT '  ✓ Script: The Awakening already exists';

-- ── Script: The Forest Guardian (Script stage) ────────────────────────────────
IF EXISTS (SELECT 1 FROM [content].[Episodes] WHERE [Id] = @Ep_Forest)
    AND NOT EXISTS (SELECT 1 FROM [content].[Scripts] WHERE [EpisodeId] = @Ep_Forest)
BEGIN
    INSERT INTO [content].[Scripts]
        ([Id], [EpisodeId], [Title], [RawJson], [IsManuallyEdited], [DirectorNotes],
         [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
        (@Scr_Forest, @Ep_Forest,
         'The Forest Guardian — Episode Script',
         '{"title":"The Forest Guardian","scenes":[{"scene_number":1,"visual_description":"Wide shot of a sunlit forest clearing. PIP, a small red fox with oversized ears, presses his paw against the bark of an ancient oak. Soft golden light filters through the canopy.","emotional_tone":"wonder, discovery","dialogue":[{"character":"Pip","text":"Hello? Is... is someone there?","start_time":0.0,"end_time":2.5},{"character":"Ancient Oak","text":"Finally. We have been waiting a long time for you, young one.","start_time":3.0,"end_time":6.5}]},{"scene_number":2,"visual_description":"A logging truck rumbles along a dirt road at the forest edge. Chainsaws echo in the distance. Pip watches from behind a bush, eyes wide with alarm.","emotional_tone":"tension, urgency","dialogue":[{"character":"Pip","text":"They are going to cut down the whole western grove!","start_time":0.0,"end_time":3.0},{"character":"Ancient Oak","text":"Then you must call the Forest Council. Only together can you stop them.","start_time":3.5,"end_time":7.0}]},{"scene_number":3,"visual_description":"A grand mossy amphitheatre carved from living trees. Dozens of woodland animals sit in tiered rows of roots and branches. Pip stands nervously at the centre podium, lit by shafts of golden light.","emotional_tone":"determination, hope","dialogue":[{"character":"Pip","text":"I know I am just a fox. But those trees have stood for a thousand years. We cannot let them fall without a fight.","start_time":0.0,"end_time":6.0},{"character":"Owl Elder","text":"The young fox speaks truth. All those in favour of the defence pact, raise a wing.","start_time":6.5,"end_time":10.0}]}]}',
         0, NULL,
         '2026-04-10T14:08:00.0000000+00:00',
         '2026-04-10T14:08:00.0000000+00:00', 0);
    PRINT '  + Script: The Forest Guardian (AI-generated)';
END ELSE PRINT '  ✓ Script: The Forest Guardian already exists';

-- ── Script: The Great Picnic Disaster (Done) ──────────────────────────────────
IF EXISTS (SELECT 1 FROM [content].[Episodes] WHERE [Id] = @Ep_Picnic)
    AND NOT EXISTS (SELECT 1 FROM [content].[Scripts] WHERE [EpisodeId] = @Ep_Picnic)
BEGIN
    INSERT INTO [content].[Scripts]
        ([Id], [EpisodeId], [Title], [RawJson], [IsManuallyEdited], [DirectorNotes],
         [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
        (@Scr_Picnic, @Ep_Picnic,
         'The Great Picnic Disaster — Final Script',
         '{"title":"The Great Picnic Disaster","scenes":[{"scene_number":1,"visual_description":"A sunny forest glade decorated with bunting and checked tablecloths. PROFESSOR WHISKERBOLT adjusts a towering three-tiered cake, humming contentedly. ZARA arranges telescope-printed paper plates nearby.","emotional_tone":"cheerful, anticipatory","dialogue":[{"character":"Professor Whiskerbolt","text":"Ah, everything is perfectly perfect! Nothing could possibly go wrong this year.","start_time":0.0,"end_time":4.0},{"character":"Zara","text":"Professor, you say that every single year.","start_time":4.5,"end_time":6.5}]},{"scene_number":2,"visual_description":"Close-up on a crack in the earth. A thousand ants emerge in military formation, marching toward the dessert table. Their tiny general raises a bread crumb like a sword and points at the cake.","emotional_tone":"comedy, rising tension","dialogue":[{"character":"Ant General","text":"TROOPS — THE CAKE IS OURS. ADVANCE!","start_time":0.0,"end_time":2.5},{"character":"Professor Whiskerbolt","text":"Great galaxies! They have a GENERAL!","start_time":3.0,"end_time":5.0}]},{"scene_number":3,"visual_description":"Slow-motion: a cream pie sails through the air. The Professor ducks. The pie hits the Ant General square in the face. A beat of silence. Then absolute chaos.","emotional_tone":"peak comedy chaos","dialogue":[{"character":"Zara","text":"I am going to need a significantly bigger telescope to document all of this.","start_time":0.0,"end_time":3.5}]}]}',
         1,
         'Ant general scene: added extra beat before cream pie lands for comedic timing.',
         '2026-04-08T10:00:00.0000000+00:00',
         '2026-04-09T15:30:00.0000000+00:00', 0);
    PRINT '  + Script: The Great Picnic Disaster (manually edited)';
END ELSE PRINT '  ✓ Script: The Great Picnic Disaster already exists';

-- ── Script: Electric Storm (Animation stage) ──────────────────────────────────
IF EXISTS (SELECT 1 FROM [content].[Episodes] WHERE [Id] = @Ep_Electric)
    AND NOT EXISTS (SELECT 1 FROM [content].[Scripts] WHERE [EpisodeId] = @Ep_Electric)
BEGIN
    INSERT INTO [content].[Scripts]
        ([Id], [EpisodeId], [Title], [RawJson], [IsManuallyEdited], [DirectorNotes],
         [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
        (@Scr_Electric, @Ep_Electric,
         'Electric Storm — Production Script',
         '{"title":"Electric Storm","scenes":[{"scene_number":1,"visual_description":"Neon City from above at night. Every light goes out simultaneously — a perfect blackout rolls across the skyline like a wave. Emergency sirens wail in the dark below.","emotional_tone":"ominous, tension","dialogue":[{"character":"News Reporter (Radio)","text":"Authorities are confirming a total grid failure across all twelve districts. Cause unknown.","start_time":0.0,"end_time":5.0}]},{"scene_number":2,"visual_description":"A rooftop command post. COMMANDER VEX stands silhouetted against a holographic display showing the city power map, one grid sector at a time going dark.","emotional_tone":"menace, control","dialogue":[{"character":"Commander Vex","text":"Tell the AI council they have four hours to agree to my terms. After that, I drain the backup reserves too.","start_time":0.0,"end_time":6.0}]},{"scene_number":3,"visual_description":"DETECTIVE UNIT 7 and MARCUS sprint across rooftops, the city dark beneath them. They are running out of time.","emotional_tone":"urgency, determination","dialogue":[{"character":"Marcus","text":"We are not going to make it.","start_time":0.0,"end_time":1.5},{"character":"Detective Unit 7","text":"We have to.","start_time":2.0,"end_time":3.0}]}]}',
         0, NULL,
         '2026-04-11T10:00:00.0000000+00:00',
         '2026-04-11T10:00:00.0000000+00:00', 0);
    PRINT '  + Script: Electric Storm (AI-generated)';
END ELSE PRINT '  ✓ Script: Electric Storm already exists';

-- ── Script: Launch Day (Rendering stage) ─────────────────────────────────────
IF EXISTS (SELECT 1 FROM [content].[Episodes] WHERE [Id] = @Ep_Launch)
    AND NOT EXISTS (SELECT 1 FROM [content].[Scripts] WHERE [EpisodeId] = @Ep_Launch)
BEGIN
    INSERT INTO [content].[Scripts]
        ([Id], [EpisodeId], [Title], [RawJson], [IsManuallyEdited], [DirectorNotes],
         [CreatedAt], [UpdatedAt], [IsDeleted])
    VALUES
        (@Scr_Launch, @Ep_Launch,
         'Launch Day — Final Script',
         '{"title":"Launch Day","scenes":[{"scene_number":1,"visual_description":"Mission control. Banks of screens, scientists in white coats. ZARA stands at the central console, her silver-streaked hair tied back, telescope pin on her jacket. Countdown clock reads T-minus 00:05:00.","emotional_tone":"tension, excitement","dialogue":[{"character":"Flight Director","text":"All systems nominal. Dr. Zara, your call.","start_time":0.0,"end_time":3.0},{"character":"Zara","text":"I have waited my whole life for this moment. Initiate final sequence.","start_time":3.5,"end_time":7.0}]},{"scene_number":2,"visual_description":"Exterior: the launch pad. Steam vents billow. The rocket — a sleek silver needle with the constellation Orion painted on its side — rises slowly, then faster, then blazes into the stratosphere.","emotional_tone":"awe, triumph","dialogue":[{"character":"Zara (V.O.)","text":"Go. Fly. See what I could not.","start_time":0.0,"end_time":4.0}]},{"scene_number":3,"visual_description":"The control room erupts in cheers. Zara stands at the window, watching the contrail fade. She holds her telescope up to the sky — the one she carried as a child.","emotional_tone":"bittersweet, wonder","dialogue":[{"character":"Zara","text":"See you out there someday.","start_time":0.0,"end_time":2.5}]}]}',
         1,
         'Final scene: hold on Zara and the telescope for 3 seconds before cut to black. No music — just ambient silence.',
         '2026-04-10T08:00:00.0000000+00:00',
         '2026-04-11T11:00:00.0000000+00:00', 0);
    PRINT '  + Script: Launch Day (manually edited)';
END ELSE PRINT '  ✓ Script: Launch Day already exists';


-- =============================================================================
    COMMIT TRANSACTION;
    PRINT '';
    PRINT '════════════════════════════════════════════════════════════════════════';
    PRINT '  ✅  Delta seed completed.';
    PRINT '';
    PRINT '  content.Characters        3 rows  (Ready / Training / Draft)';
    PRINT '  content.EpisodeCharacters 7 links (5 episodes × 1-2 chars each)';
    PRINT '  content.Episodes          CharacterIds JSON synced';
    PRINT '  content.Scripts           5 scripts (Awakening / Forest / Picnic /';
    PRINT '                                        Electric Storm / Launch Day)';
    PRINT '════════════════════════════════════════════════════════════════════════';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '';
    PRINT '════════════════════════════════════════════════════════════════════════';
    PRINT '  ❌  Delta seed FAILED — transaction rolled back.';
    PRINT '  Error ' + CAST(ERROR_NUMBER() AS NVARCHAR) + ': ' + ERROR_MESSAGE();
    PRINT '  Line: ' + CAST(ERROR_LINE() AS NVARCHAR);
    PRINT '════════════════════════════════════════════════════════════════════════';
    THROW;
END CATCH;
