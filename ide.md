# Running IDE Locally

> ⚠️ **Note:**  
> Until the IDE is tagged, the backend is undergoing incredibly fast-paced development  
> and may occasionally be unstable due to active collaboration across multiple areas.

### Remarks
- Until deployment (**25th Oct**), the IDE frontend is developed solely on a feature branch.
- Its backend is currently coupled with BLP-specific components that need to be extracted and replaced (along with several other minor TODOs).

---

## Backend

The IDE API routes are integrated into the **`ballerina-runtime`** project.  
You can start the backend using one of the following methods:

- **Command line:**
  ```bash
  dotnet run web -mode server
  ```
- **Rider IDE:**
  Load the configuration from
  ```
  backend/.idea/.idea.unbound/.idea/runConfigurations
  ```
- **VS Code:**  
  *(TODO — configuration to be added)*

### Verifying API Availability

To confirm the backend is running correctly:

- `http://localhost:5021/health` → should return a `"healthy"` status
- `http://localhost:5021/api/preview/specs` → when accessed in a browser, should return
  ```json
  { "errors": ["No IDE request context found"] }
  ```

### Multitenancy Notes

The IDE supports multitenancy, but until full deployment there is a **single development tenant** registered in the backend:

```
c0a8011e-3f7e-4a44-9c3a-97bcb80b10fd
```

The development server automatically injects this tenant as a request header.  
Eventually, tenant validation will be performed entirely in the backend, based on identity claim values.

For manual testing, you can also set the tenant explicitly via the `Tenant-Id` header  
(e.g. in Postman, `.rest` files, or other tools).

---

### Using `.rest` Files

It is often useful to configure or test the IDE using `.rest` files.  
They allow you to:
- Start the IDE with a predefined spec loaded from a JSON file.
- Test various API routes directly from the editor, with all necessary headers included.

*(Details: TODO)*

---

## Frontend

While the IDE remains under active development in a feature branch, it is **enabled by default** using:

```tsx
<App app="ide" />
```

You can start the frontend with:

```bash
yarn dev
```

---

## Project Structure

Below is an overview of the key directories involved in running the IDE locally:

```
ballerina/
├── backend/
│   └── ballerina-runtime/            # currently only in the BLP-specific project
│
├── frontend/
│   ├── apps/
│   │   └── web/
│   │       ├── src/
│   │       │   ├── Ide.tsx
│   │       │   └── domains/
│   │       │       └── ide/
│   │       │           └── views/   # React views
│   │       ├── package.json
│   │       └── ...
│   │
│   ├── libraries/
│   │   └── playground-core/
│   │       └── ide/                 # IDE state and logic
│   │       ...
│
└── ...

```

---
# Dependencies

The IDE currently relies on several external libraries categorized by their importance and replacement difficulty.

---

## Hard Dependencies

These are core libraries integral to the IDE and unlikely to be replaced.

- **React** — core framework for the frontend.
- **Ballerina** — runtime and domain logic integration.
- **Immutable.js** — state immutability and structural sharing.
- **Tailwind CSS** — primary styling framework.
    - **DaisyUI** — Tailwind-based component library.
    - **Planned improvement:** Make the IDE style-flexible (should also support at least one alternative UI kit).

---

## Medium Dependencies

Dependencies that are important but can be replaced or modularized with moderate effort.

- **Monaco Editor** (Microsoft / VS Code)
    - will soon become a *hard dependency* for code editing.
- **Sonner** (toast notifications)
    - Used across the app for notifications, but still loosely coupled and replaceable.
- **React-Accessible-Treeview**
    - Used for demos and MVP; will eventually be replaced by an in-house tree component.

---

## Soft Dependencies

Non-critical dependencies that can be easily removed or swapped.

- **React Icons** — primarily for Visual Studio Code-style icons.
- **JSZip** — ZIP file support (well-maintained: ~12M weekly downloads, ~10k GitHub stars).

---

# TODO

## Features

- ⬜ Add **unit tests for Virtual Folders** — by **25 Oct**

---

## Technical Tasks

- **Use Ballerina templates with isolated state per view** — by **25 Oct**
    - *Reason:* The general IDE layout and component structure are still in flux. Introducing isolated state management earlier would cause unnecessary refactoring.

- **Refactor the Monaco Editor into a proper Ballerina component (with coroutines)** — by **25 Nov**
    - *Reason:* Currently integrated from an example snippet; it works, but must evolve into a production-ready component consistent with Ballerina design patterns.

- **Replace React-Accessible-Treeview with a custom component** — by **25 Nov**
    - *Reason:* The existing library served well for PoC and demos, but the tree view is a core part of the system and must be fully controlled in-house.

---

