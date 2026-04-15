using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimStudio.ContentModule.Migrations
{
    /// <inheritdoc />
    public partial class Phase3Templates : Migration
    {
        // ── Stable seed GUIDs (deterministic, never change) ───────────────────
        private static readonly Guid T_KidsSuperhero   = new("11111111-0001-0000-0000-000000000003");
        private static readonly Guid T_FamilyComedy    = new("11111111-0002-0000-0000-000000000003");
        private static readonly Guid T_MysteryThriller = new("11111111-0003-0000-0000-000000000003");
        private static readonly Guid T_RomanceVignette = new("11111111-0004-0000-0000-000000000003");
        private static readonly Guid T_HorrorShort     = new("11111111-0005-0000-0000-000000000003");
        private static readonly Guid T_SciFiAction     = new("11111111-0006-0000-0000-000000000003");
        private static readonly Guid T_ProductAdvert   = new("11111111-0007-0000-0000-000000000003");
        private static readonly Guid T_BrandStory      = new("11111111-0008-0000-0000-000000000003");

        private static readonly Guid S_Pixar3D         = new("22222222-0001-0000-0000-000000000003");
        private static readonly Guid S_Anime           = new("22222222-0002-0000-0000-000000000003");
        private static readonly Guid S_Watercolor      = new("22222222-0003-0000-0000-000000000003");
        private static readonly Guid S_ComicBook       = new("22222222-0004-0000-0000-000000000003");
        private static readonly Guid S_Realistic       = new("22222222-0005-0000-0000-000000000003");
        private static readonly Guid S_PhotoStorybook  = new("22222222-0006-0000-0000-000000000003");
        private static readonly Guid S_RetroCartoon    = new("22222222-0007-0000-0000-000000000003");
        private static readonly Guid S_Cyberpunk       = new("22222222-0008-0000-0000-000000000003");

        private static readonly DateTimeOffset Seed = new(2026, 4, 5, 0, 0, 0, TimeSpan.Zero);

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EpisodeTemplates",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Genre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PlotStructure = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultStyle = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PreviewVideoUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpisodeTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StylePresets",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Style = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SampleImageUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FluxStylePromptSuffix = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StylePresets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EpisodeTemplates_Genre",
                schema: "content",
                table: "EpisodeTemplates",
                column: "Genre");

            migrationBuilder.CreateIndex(
                name: "IX_EpisodeTemplates_SortOrder",
                schema: "content",
                table: "EpisodeTemplates",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_StylePresets_Style",
                schema: "content",
                table: "StylePresets",
                column: "Style",
                unique: true);

            // ── Seed: 8 Episode Templates ──────────────────────────────────────
            migrationBuilder.InsertData(
                schema: "content",
                table: "EpisodeTemplates",
                columns: new[] { "Id", "Title", "Genre", "Description", "PlotStructure", "DefaultStyle", "IsActive", "SortOrder", "CreatedAt", "UpdatedAt", "IsDeleted", "RowVersion" },
                values: new object[,]
                {
                    {
                        T_KidsSuperhero, "Kids Superhero Adventure", "Kids",
                        "A young hero discovers extraordinary powers and must use them to protect their neighbourhood from a quirky villain.",
                        @"{""acts"":[{""name"":""Act 1"",""description"":""Hero discovers powers"",""beats"":2},{""name"":""Act 2"",""description"":""Villain threatens the neighbourhood"",""beats"":4},{""name"":""Act 3"",""description"":""Hero saves the day"",""beats"":2}]}",
                        "Pixar3D", true, 1, Seed, Seed, false, new byte[0]
                    },
                    {
                        T_FamilyComedy, "Family Comedy", "Comedy",
                        "A loveable family gets into a series of hilarious misunderstandings that snowball into chaos before a heartfelt resolution.",
                        @"{""acts"":[{""name"":""Act 1"",""description"":""Setup the misunderstanding"",""beats"":2},{""name"":""Act 2"",""description"":""Escalating chaos"",""beats"":4},{""name"":""Act 3"",""description"":""Heartfelt resolution"",""beats"":2}]}",
                        "RetroCartoon", true, 2, Seed, Seed, false, new byte[0]
                    },
                    {
                        T_MysteryThriller, "Mystery Thriller Short", "Drama",
                        "A detective unravels a web of secrets in a rain-soaked city, facing danger at every turn.",
                        @"{""acts"":[{""name"":""Act 1"",""description"":""Crime discovered"",""beats"":2},{""name"":""Act 2"",""description"":""Investigation deepens with false leads"",""beats"":5},{""name"":""Act 3"",""description"":""Revelation and confrontation"",""beats"":3}]}",
                        "Realistic", true, 3, Seed, Seed, false, new byte[0]
                    },
                    {
                        T_RomanceVignette, "Romance Vignette", "Romance",
                        "Two strangers meet by chance and navigate hesitant feelings through a series of bittersweet encounters.",
                        @"{""acts"":[{""name"":""Act 1"",""description"":""Chance meeting"",""beats"":2},{""name"":""Act 2"",""description"":""Growing connection and obstacles"",""beats"":3},{""name"":""Act 3"",""description"":""Decisive moment"",""beats"":2}]}",
                        "WatercolorIllustration", true, 4, Seed, Seed, false, new byte[0]
                    },
                    {
                        T_HorrorShort, "Horror Short", "Horror",
                        "A group of friends spend the night in an abandoned house and discover something terrifying that defies explanation.",
                        @"{""acts"":[{""name"":""Act 1"",""description"":""Arrival and foreboding"",""beats"":2},{""name"":""Act 2"",""description"":""Escalating dread and isolation"",""beats"":4},{""name"":""Act 3"",""description"":""Shocking climax and ambiguous escape"",""beats"":2}]}",
                        "Cyberpunk", true, 5, Seed, Seed, false, new byte[0]
                    },
                    {
                        T_SciFiAction, "Sci-Fi Action", "SciFi",
                        "A lone pilot discovers a derelict spacecraft that holds the key to saving humanity from an alien threat.",
                        @"{""acts"":[{""name"":""Act 1"",""description"":""Discovery of the derelict ship"",""beats"":2},{""name"":""Act 2"",""description"":""Uncovering the threat and preparing a response"",""beats"":5},{""name"":""Act 3"",""description"":""Epic battle and sacrifice"",""beats"":3}]}",
                        "Cyberpunk", true, 6, Seed, Seed, false, new byte[0]
                    },
                    {
                        T_ProductAdvert, "Product Advert", "Marketing",
                        "A 60-second animated spot that dramatises the hero moment of your product solving a customer's pain point.",
                        @"{""acts"":[{""name"":""Hook"",""description"":""Relatable pain point"",""beats"":1},{""name"":""Solution"",""description"":""Product introduced as hero"",""beats"":2},{""name"":""Proof"",""description"":""Result and delight"",""beats"":1},{""name"":""CTA"",""description"":""Brand moment and call to action"",""beats"":1}]}",
                        "Pixar3D", true, 7, Seed, Seed, false, new byte[0]
                    },
                    {
                        T_BrandStory, "Brand Story", "Marketing",
                        "An origin story that communicates brand values, mission, and the people behind a company.",
                        @"{""acts"":[{""name"":""Origin"",""description"":""Founding moment and challenge"",""beats"":2},{""name"":""Journey"",""description"":""Growth and mission"",""beats"":3},{""name"":""Vision"",""description"":""Future and invitation to join"",""beats"":2}]}",
                        "ComicBook", true, 8, Seed, Seed, false, new byte[0]
                    }
                });

            // ── Seed: 8 Style Presets ──────────────────────────────────────────
            migrationBuilder.InsertData(
                schema: "content",
                table: "StylePresets",
                columns: new[] { "Id", "Style", "DisplayName", "Description", "FluxStylePromptSuffix", "IsActive", "CreatedAt", "UpdatedAt", "IsDeleted", "RowVersion" },
                values: new object[,]
                {
                    {
                        S_Pixar3D, "Pixar3D", "Pixar 3D",
                        "Vibrant 3D animation with Pixar-style characters, smooth subsurface scattering, and cinematic lighting.",
                        "vibrant Pixar-style 3D animation, smooth subsurface scattering, cinematic three-point lighting, shallow depth of field, 4K render quality, expressive character design",
                        true, Seed, Seed, false, new byte[0]
                    },
                    {
                        S_Anime, "Anime", "Anime",
                        "Cel-shaded anime with dynamic action lines, expressive character emotions, and vibrant colour palettes.",
                        "anime art style, cel-shaded illustration, dynamic speed lines, expressive emotive characters, vibrant saturated colors, clean linework, Studio Ghibli inspired",
                        true, Seed, Seed, false, new byte[0]
                    },
                    {
                        S_Watercolor, "WatercolorIllustration", "Watercolor",
                        "Soft painterly textures with gentle colour bleeding and a handcrafted storybook feel.",
                        "soft watercolor illustration, painterly textures, gentle color bleeding, warm pastel tones, handcrafted storybook aesthetic, loose brushwork, visible paper texture",
                        true, Seed, Seed, false, new byte[0]
                    },
                    {
                        S_ComicBook, "ComicBook", "Comic Book",
                        "Bold outlines, halftone dot patterns, flat saturated colours, and dynamic superhero poses.",
                        "comic book art style, bold black outlines, halftone dot shading, flat vibrant colors, dynamic action poses, CMYK print aesthetic, Marvel and DC inspired panel composition",
                        true, Seed, Seed, false, new byte[0]
                    },
                    {
                        S_Realistic, "Realistic", "Realistic CGI",
                        "Photorealistic CGI with physically-based rendering, detailed surface textures, and dramatic cinematic lighting.",
                        "photorealistic CGI, physically-based rendering, detailed surface textures, dramatic cinematic lighting, 8K resolution, ray-traced reflections and shadows, film grain",
                        true, Seed, Seed, false, new byte[0]
                    },
                    {
                        S_PhotoStorybook, "PhotoStorybook", "Photo Storybook",
                        "Photo-real illustration combining photographic detail with painterly finishing touches.",
                        "photo-real storybook illustration, photographic detail with painterly overlay, rich saturated colors, storybook composition, children's picture book aesthetic",
                        true, Seed, Seed, false, new byte[0]
                    },
                    {
                        S_RetroCartoon, "RetroCartoon", "Retro Cartoon",
                        "1950s-1970s television cartoon aesthetic: limited palette, bold outlines, and rubberhose character movement.",
                        "retro 1960s cartoon style, limited color palette, bold flat outlines, rubberhose character animation, Saturday morning cartoon aesthetic, flat background art",
                        true, Seed, Seed, false, new byte[0]
                    },
                    {
                        S_Cyberpunk, "Cyberpunk", "Cyberpunk",
                        "Neon-drenched cityscapes, high-contrast shadows, glitch visual effects, and dark dystopian atmosphere.",
                        "cyberpunk aesthetic, neon-lit rain-soaked cityscape, high contrast shadows, chromatic aberration glitch effect, dark dystopian atmosphere, Blade Runner and Ghost in the Shell inspired",
                        true, Seed, Seed, false, new byte[0]
                    }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EpisodeTemplates",
                schema: "content");

            migrationBuilder.DropTable(
                name: "StylePresets",
                schema: "content");
        }
    }
}
