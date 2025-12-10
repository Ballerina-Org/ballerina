# Custom Entity Domain (tbc)

The **Custom Entity domain** is responsible for transforming user-facing Ballerina code and associated files into:

```
Value<'typeValue, 'valueExt>
```

This `Value` is ultimately sent Form Engine UI (by leveraging to toApi/field converter v2) and to storage.  
Although currently located under the IDE `phases/` directory, the domain is intentionally **isolated, modular, and reusable**. It depends only on a small data-provider API and can be moved into another service or module with minimal effort.

At a high level, the system forms a simple logical flow:

```
Workspace ──► Combined Ballerina Input ──► TypeChecking ──► Value
```

This README explains the structure, the processing pipeline, the job orchestration model (Coroutine), and the vocabulary used throughout the system.

---

# Table of Contents

1. [Conceptual Model](#conceptual-model)
2. [Workspace](#workspace)
3. [Data Provider: collect() and prompt()](#data-provider)
4. [Pipeline: Workspace → Value](#pipeline)
5. [Coroutine Model](#coroutine-model)
6. [Job Lifecycle](#job-lifecycle)
7. [Diagrams](#diagrams)
8. [Glossary](#glossary)

---

# Conceptual Model

The Custom Entity domain can be understood as a **compiler front-end for user-defined Ballerina entities**, producing a runtime structure (`Value`) that the UI uses to render forms and drive updates.

The domain consumes:

- A **Workspace** of files
- A **data-provider** (collect & prompt)
- A **backend job pipeline** (TypeChecking → subsequent phases)

And produces:

- A fully typed `Value` tree matching the Spec JSON definition.

```
Workspace  ──►  Value<'typeValue, 'valueExt>
```

---

# Workspace

A **Workspace** is a collection of files uploaded  (or int the future created) by the user during working on a SPEC

It contains:

- **Spec JSON files** — define schema and types
- **Ballerina `.bl` files** — constructors, updater and domain logic
- **Prompt `.txt` file** *(temporary)* — simulates external document content

### Workspace Characteristics

- Represented as a **tree** (`Node`) of folders and files
- Only one file is "active" at any moment
- Siblings of the active file provide contextual knowledge
- Used by the Custom Entity domain to build the Ballerina input needed for TypeChecking

---

# Data Provider

The domain does not directly access the filesystem.  
Instead, it interacts with **data-provider**, which supplies two functions:

---

## `collect()`

Collects all relevant `.bl` files:

- merges constructors
- merges helper code
- merges domain logic

Result: a **single Ballerina code container** required by the TypeChecking job.

---

## `prompt()`

Provides:

- Fake or temporary **prompt text** (until backend document ingestion is finished)
- A synthetic **document structure**
- A generated line:

```ballerina
let prompt = "<content>"
```

This code is injected **before** `constructor.bl`.

Later, the backend will replace this with a **curated document context** built from real files that will be injected via the constructor parameter.

---

# Pipeline

This is the **Workspace → Value** pipeline used by the IDE:

```
Workspace
   │
   ├── collect()  →  Ballerina code bundle needed for typechecking
   ├── prompt()   →  initial context + synthetic document
   │
   ▼
Combined Ballerina Input
   │
   ▼
TypeChecking Job (backend)
   │
   ▼
Value<'typeValue, 'valueExt>
   │
   ▼
Form Engine Rendering
```

As the particular jobs (typechecking and evaluation) could take, the job orchestration coroutine is also visualization friendly,
so the dump react component is minimal.

---

# Coroutine Model

The IDE uses a **Coroutine-driven job manager**.  
Each backend job has:

- **State** (running, completed, failed)
- **Result** (value or description id needed for- **Continuation** (the next phase to run)


---

# Glossary

**Workspace** — A tree of user-uploaded files: JSON specs, Ballerina code, and temporary prompts.

**Spec JSON** — Defines the structure and schema used to generate the Value model.

**Ballerina Code (`.bl`)** — Contains constructors and domain logic.

**Prompt** — Temporary mechanism for simulating document context.

**Value** — The fully typed structure produced by TypeChecking, powering the form engine.

**Data Provider** — Supplies `collect()` and `prompt()` for assembling Ballerina input.

**TypeChecking Job** — Backend job that merges workspace code, runs type inference, and produces a Value.

**Coroutine** — Job orchestration mechanism.

**Curated Context** — Future backend-produced real document structure replacing the prompt.

---

# End
