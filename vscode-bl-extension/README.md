# BL Language Tools (VS Code)

Features:

- Basic syntax highlighting for `.bl` files (keywords/operators from the lexer/parser).
- `.blproj` discovery for the active `.bl` file.
- Recursive build-order resolution (`inputProjects` first, then `sources`).
- Build through the persistent `bise-sql server` mode and publish diagnostics to the Problems pane.

## Commands

- `BL: Build Active Project`
- `BL: Show Active Project Build Order`

## Notes

- Default build command executed (persistent process):
  `dotnet run --project <workspace>/src/playgrounds/bise-sql/bise-sql.fsproj -- server`
- For each build, the extension writes `<project.blproj>` to server stdin and reads a JSON result from stdout.
