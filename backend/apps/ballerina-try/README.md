
# Try Ballerina

Note 1: The project name is temporary.

Note 2: For now, treat this as a prototype - basically a collection of notes from my onboarding to the Ballerina ecosystem, loosely jumping between existing codebases. I’m developing it in spare time.

Note 3: Everything here is my own idea. I haven’t used it in any commercial project - just played with it from time to time between 2022 and 2023, mostly in the evenings. I was also an early adopter of Polyglot Notebooks and had many chats with the team from Microsoft.

## The Goal

Try Ballerina aims to support exploratory and explanatory learning - to better understand Ballerina concepts using interactive snippets, tutorials, and playgrounds. 

And, importantly, to quickly test out new ideas.

Think of it like [Try .NET](https://dotnet.microsoft.com/en-us/platform/try-dotnet), but more focused on the Ballerina ecosystem - and occasionally “cheating” or bending language constraints when needed.

That’s the long-term goal. For now, it’s just a first step - a small toolbox for Ballerina developers to run quick, interactive experiments with a visual outcome.

## Why

The Ballerina ecosystem and its data-driven approach are highly polyglot by nature.

Core ideas like coroutines, coproducts, and state processing are implemented similarly in our F# and TypeScript libraries. That makes it a natural fit to use F# for the backend and TypeScript for the frontend.

However, this brings some challenges - like duplicated logic between frontend and backend - which need special handling (more on that later).

It’s worth emphasizing the differences and subtleties, and trying to keep them as coherent as possible.

In the AI/agentic world, some elements will never be fully automated - especially gathering and validating requirements, with a robust architecture.

And one of the most underestimated parts of validation is… having an easy, instant visualization of concepts and ideas, starting from short code snippets.

REPL tools often fall short here. There’s a lack of tooling that makes this kind of work more interactive, more visual, and more exploratory.

It’s not easy - it requires not only deep understanding of the business domain but also serious full-stack skills (e.g., mixing F# with D3.js).

All of this can become an unfair advantage when building complex, data-driven full-stack apps.

## How

This is where it gets interesting.

We want a good balance and a deep understanding of the subtleties between backend (F#) and frontend (TS) in a single tooling project.

- Running real F# code in the browser would be ideal, but it’s not fully possible yet:

  - Microsoft supports running .NET in the browser via Blazor, but:

    - true multithreading with Blazor WASM was delayed from .NET 9 to .NET 10 - and might be delayed again

  - we’d love to write F# code in an online editor, compile and run it in the browser - but:

    - fsi (F# Interactive) isn’t WebAssembly-friendly, and according to the F# team (in Prague), there are no current plans to port it

    - we could do it ourselves (a huge benefit for the F# community), but it would take a month or more

  - it would require loading many F# libraries in the browser - especially compiler-related ones - which could be a performance hit (though only on initial load)

  - NuGet in the browser doesn’t work, but we could still load regular .dll files

**So, is there an alternative?**
Yes - and it’s realistic.

Visual Studio Code (or forks like Cursor) is one of the most widely used IDEs. 

It has an excellent extension called Polyglot Notebooks that lets you run multiple languages in the same notebook, supports AI kernels, and allows for custom visualizations.

💡 Note: If the word notebook reminds you of Jupyter and makes you reluctant, don’t worry. Polyglot Notebooks is similar in concept, but with a different implementation, architecture, and design goals.

You can even combine it with regular web apps (with some limitations due to sandboxing), using any tech stack that compiles to HTML & JS - running side-by-side with backend code.

It might sound like a typical API client/server model, but with shared variables and types across frontend and backend - enabling unmatched productivity, interactivity, and “hacky” experimentation.

In some cases, you can even write code once in F# and reuse it on both sides. That’s a big boost for exploratory or documentation-driven workflows - though the core implementation stays in Ballerina.

For example, it might be worth having a shared codebase for the **_Operations_** section of Ballerina repos. 

While some operations are frontend-only, many things - like validation, parsing, or logic that doesn’t involve state/coroutine handling - could be unified.

**T o be continued...**


