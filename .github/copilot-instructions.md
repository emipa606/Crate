# GitHub Copilot Instructions for Crate (Continued) RimWorld Mod

## Mod Overview and Purpose
The Crate (Continued) mod is an update to the original Crate mod by Vendan. It introduces a crate system to help players organize their items by packing them into a single container. The main goal of this mod is to provide a convenient way to transport and manage items on caravans to camps or colonies. Players can mass-pack and unpack items to designated areas, or pack and unpack specific items to specific locations. Colonists cannot directly access the contents of the crates, so players will have to remember to unpack items if colonists need them.

## Key Features and Systems
- **Crate Construction**: Crates can now be constructed at the smithing bench instead of the tailoring table.
- **Packing and Unpacking**: Players can pack multiple items into crates and unpack them to designated areas or specific locations.
- **Colonist Management**: Colonists cannot access the contents of the crates, so players must manage unpacking items when needed.
- **Transport Efficiency**: Players can quickly move a large number of items by packing them into crates and then transferring them to herd animals or transport pods.

## Coding Patterns and Conventions
- **C# Files**: The mod contains 16 C# files that define various classes and methods for managing crates and their interactions with the game world.
- **XML Files**: The mod includes 4 XML files that provide definitions for job, thing, and work giver elements used in the game.
- **Namespacing**: All C# classes are organized within appropriate namespaces to avoid naming conflicts and improve code organization.
- **Encapsulation**: Classes are designed to encapsulate their data and provide public methods for interacting with the game world.
- **Error Handling**: The mod includes error handling mechanisms to ensure stability and provide meaningful error messages to users.

## XML Integration
The mod integrates XML files to define various game elements. These files include:
- `About.xml`: Contains metadata about the mod, such as the author and package ID.
- `Jobs_Misc.xml`: Defines job definitions used in the game.
- `Items_Usable.xml`: Defines thing definitions for items that can be used in the game.
- `WorkGivers.xml`: Defines work giver definitions used in the game.

## Harmony Patching
The mod uses Harmony patching to extend the functionality of the base game. Harmony is a popular library for modding RimWorld and other games. The mod likely uses Harmony patches to:
- Modify existing game mechanics to integrate with the crate system.
- Add new features, such as the ability to pack and unpack items from crates.

## Suggestions for Copilot
To ensure that Copilot provides relevant and useful suggestions, consider the following:
- **Documentation**: Provide detailed documentation for the mod's codebase, including class descriptions, method explanations, and usage examples.
- **Code Comments**: Add comments to the codebase to explain complex logic and provide guidance for future developers.
- **Consistent Naming**: Use consistent naming conventions for classes, methods, and variables to make the codebase easier to understand.
- **Testing**: Regularly test the mod to ensure that Copilot-generated code does not introduce bugs or compatibility issues.
- **Feedback Loop**: Encourage users to provide feedback on Copilot-generated code and use this feedback to improve the mod.

## Example Copilot Instructions
md
# GitHub Copilot Instructions for Crate (Continued) RimWorld Mod

## Mod Overview and Purpose
The Crate (Continued) mod is an update to the original Crate mod by Vendan. It introduces a crate system to help players organize their items by packing them into a single container. The main goal of this mod is to provide a convenient way to transport and manage items on caravans to camps or colonies. Players can mass-pack and unpack items to designated areas, or pack and unpack specific items to specific locations. Colonists cannot directly access the contents of the crates, so players will have to remember to unpack items if colonists need them.

## Key Features and Systems
- **Crate Construction**: Crates can now be constructed at the smithing bench instead of the tailoring table.
- **Packing and Unpacking**: Players can pack multiple items into crates and unpack them to designated areas or specific locations.
- **Colonist Management**: Colonists cannot access the contents of the crates, so players must manage unpacking items when needed.
- **Transport Efficiency**: Players can quickly move a large number of items by packing them into crates and then transferring them to herd animals or transport pods.

## Coding Patterns and Conventions
- **C# Files**: The mod contains 16 C# files that define various classes and methods for managing crates and their interactions with the game world.
- **XML Files**: The mod includes 4 XML files that provide definitions for job, thing, and work giver elements used in the game.
- **Namespacing**: All C# classes are organized within appropriate namespaces to avoid naming conflicts and improve code organization.
- **Encapsulation**: Classes are designed to encapsulate their data and provide public methods for interacting with the game world.
- **Error Handling**: The mod includes error handling mechanisms to ensure stability and provide meaningful error messages to users.

## XML Integration
The mod integrates XML files to define various game elements. These files include:
- `About.xml`: Contains metadata about the mod, such as the author and package ID.
- `Jobs_Misc.xml`: Defines job definitions used in the game.
- `Items_Usable.xml`: Defines thing definitions for items that can be used in the game.
- `WorkGivers.xml`: Defines work giver definitions used in the game.

## Harmony Patching
The mod uses Harmony patching to extend the functionality of the base game. Harmony is a popular library for modding RimWorld and other games. The mod likely uses Harmony patches to:
- Modify existing game mechanics to integrate with the crate system.
- Add new features, such as the ability to pack and unpack items from crates.

## Suggestions for Copilot
To ensure that Copilot provides relevant and useful suggestions, consider the following:
- **Documentation**: Provide detailed documentation for the mod's codebase, including class descriptions, method explanations, and usage examples.
- **Code Comments**: Add comments to the codebase to explain complex logic and provide guidance for future developers.
- **Consistent Naming**: Use consistent naming conventions for classes, methods, and variables to make the codebase easier to understand.
- **Testing**: Regularly test the mod to ensure that Copilot-generated code does not introduce bugs or compatibility issues.
- **Feedback Loop**: Encourage users to provide feedback on Copilot-generated code and use this feedback to improve the mod.


This .github/copilot-instructions.md file provides a comprehensive overview of the Crate (Continued) mod, including its purpose, key features, coding patterns, XML integration, and Harmony patching. It also includes suggestions for improving the mod's compatibility and usability with GitHub Copilot.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

