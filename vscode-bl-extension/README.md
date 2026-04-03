# BL Language Tools (VS Code)

Features:

- Basic syntax highlighting for `.bl` files (keywords/operators from the lexer/parser).
- `.blproj` discovery for the active `.bl` file.
- Recursive build-order resolution (`inputProjects` first, then `sources`).
- Build with the BL compiler from PATH (default command: `ballerina`) and publish diagnostics to the Problems pane.

## Commands

- `BL: Build Active Project`
- `BL: Show Active Project Build Order`

## Notes

- Default build command executed:
  `ballerina -f <project.blproj>`
- Compiler command is configurable via VS Code setting:
  `bl.compilerCommand`
