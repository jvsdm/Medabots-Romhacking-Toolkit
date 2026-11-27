# Medabots Romhacking Toolkit: Rokusho Expansion fork

This is a fork of **[Simon Tulling's Medabots Romhacking Toolkit](https://github.com/STulling/Medabots-Romhacking-Toolkit)**. The branch `decomp-project` holds the WPF editor used to make the **[Medabots Rokusho Expansion](https://github.com/jvsdm/medabots-expansion)**.

The original toolkit is the base of everything here. This fork adds a second way of working (Decomp mode) and a set of new editors on top of it.

## Download

The Windows builds (x64 and ARM64, no install needed) are in the expansion's release: [medabots-expansion/releases/latest](https://github.com/jvsdm/medabots-expansion/releases/latest).

The editor contains no game data. You need your own ROMs.

## Two ways to use it

**ROM mode** (as in the original toolkit). Open a `.gba` file and edit it directly: Medabots, parts, text, maps, sprites and events. The editor finds the game's tables by signature, so it reads the European and US versions and ROMs that were already expanded.

**Decomp mode** (new). Open a folder of the [medabots-expansion](https://github.com/jvsdm/medabots-expansion) project. You edit Medabots, maps, event scripts and dialog as project files, and **Build ROM** runs the project's build on the game's C decompilation. Setup of that folder (Python, ROMs, decomp) is in its [INSTALL.md](https://github.com/jvsdm/medabots-expansion/blob/main/INSTALL.md).

## What this fork adds

- **Decomp mode**: project window for Medabots (stats, parts, graphics source), art templates (export the parts as PNG to draw on, then check and clean them), Build ROM, and the event editor that reads and writes the project's `.evt` scripts with a scene preview.
- **Events**: one line per action, named arguments, the project's text banks, add and delete steps.
- **Maps**: warp connections shown on the map, new maps, tileset editing, map expansion.
- **Sprites**: pixel editor (pencil, eraser, line, rectangles, bucket, picker, undo), and **Export PNG / Import PNG** for overworld sprites and portraits. Import matches the colours to the sprite's 16-colour palette.
- **Portraits and roster**: more portrait characters, more Medabots (bot adder, importers for Medarot Navi and Shingata Medarot graphics).
- **Animation**: pose viewer, move editor and skeleton editor for the battle animations.
- **Font**: extended font with extra glyphs, and TrueType import.
- Light and dark themes.

## Building from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download) on Windows.

```sh
dotnet build src/Medabots.Rom.WPFEditor/Medabots.Rom.WPFEditor.csproj
```

The release builds come from `.github/workflows/release.yml` (self-contained single file, no debug symbols).

## Credits

- **Simon Tulling ([STulling](https://github.com/STulling))**: the original Medabots Romhacking Toolkit and editor. This fork would not exist without it.
- **rommer-gba contributors**: the C decompilation of Medabots - Rokusho Version (USA), [rommerhacks/medabots-rokusho](https://github.com/rommerhacks/medabots-rokusho) (MIT), which Decomp mode builds on.

## Reporting bugs

Open an issue on [medabots-expansion](https://github.com/jvsdm/medabots-expansion/issues) and say it is about the editor.

## AI disclaimer

> **AI disclaimer: this project was made with heavy AI use.** Most of the code of this fork was written by an AI assistant, directed and reviewed by jvsdm. The original toolkit is Simon Tulling's work.
>
> No art was made with AI.
>
> If that is a deal-breaker for you, this release is not for you, and that's fine. Take inspiration on what makes sense for you and build something fully human.
