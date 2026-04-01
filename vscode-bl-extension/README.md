# BL Language Tools (VS Code)

Features:

- Basic syntax highlighting for `.bl` files (keywords/operators from the lexer/parser).
- `.blproj` discovery for the active `.bl` file.
- Recursive build-order resolution (`inputProjects` first, then `sources`).
- Build with `bise-sql` and publish diagnostics to the Problems pane.

## Commands

- `BL: Build Active Project`
- `BL: Show Active Project Build Order`

## Notes

- This extension expects to run inside the BISE repo and uses:
  `src/playgrounds/bise-sql/bise-sql.fsproj`
- Build command executed:
  `dotnet run --project <bise-sql.fsproj> -- --file <project.blproj>`
