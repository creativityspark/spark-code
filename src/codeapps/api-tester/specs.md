# Spark Code Tester - General Specifications

## 1. Overview

**Spark Code Tester** is a Power Apps Code App hosting API-specific testing tools.  Each tool owns its request editors, actions, result inspection and status.
The shared application shell provides navigation, environment identity and a single light/dark theme.

Project folder: `src/codeapps/api-tester/`.
Design reference: [mockup.html](./mockup.html).

### Specification index

| Tool | API | Specification | Design status |
|---|---|---|---|
| RunCSharp | `csp_Other_RunCSharp` | [RunCSharp tool](./run-csharp-specs.md) | Interactive mockup; executions are simulated |
| RenderTemplate | `csp_Templates_RenderTemplate` | [RenderTemplate tool](./render-template-specs.md) | Layout-only placeholder; execution disabled |

### Goals

- Provide one consistent entry point for testing different SparkCode APIs.
- Let each tool expose the parameters and outputs appropriate to its API, without forcing all APIs into a C# editor.
- Preserve a tool's working state when navigating to another tool.
- Keep general application requirements here and API-specific behavior in the corresponding tool specification.

### Non-goals

- Implementing all SparkCode API tools in this iteration.
- A generic schema-generated request editor or shared diagnostics for every API.
- Dataverse storage, server-side sharing, or synchronization of test inputs/history across devices.

## 2. Users

| Persona | Need |
|---|---|
| Power Platform developer | Select an API tool, prepare a request and inspect its response. |
| Maker / low-code author | Try API-specific examples without building request payloads manually. |
| Support engineer | Reproduce API behavior and inspect the selected tool's results and failures. |

## 3. Shared shell and ownership

```
Spark Code Tester header
  Tools dropdown | Spark Code Tester · <selected API> | Environment | Dark Mode
Selected testing tool
  Tool-specific action row
  Tool-specific workspace and overlays
  Tool-specific footer status
```

Only the top header is shared UI. Everything below it, including footer status, belongs to the selected
testing tool. The shared header must not contain Samples, Options, Run, output sizes, type names or run status.
Those belong to a tool, if relevant to that tool's API.

The environment badge represents the application's Dataverse connection. The selected API's technical name
is shown in that tool's workspace/footer rather than being hard-coded in the environment badge.

## 4. Functional requirements

### 4.1 Header and testing-tools menu

- **APP-H1:** Display `Spark Code Tester` followed by the selected API name, initially `RunCSharp`.
- **APP-H2:** A labelled Tools button expands/collapses a dropdown listing RunCSharp and RenderTemplate.
  Show the current selection and expose expanded/selected state to assistive technology.
- **APP-H3:** Selecting an item closes the dropdown, changes the selected API title and shows only that tool.
  Selecting the current tool is harmless and does not reset it.
- **APP-H4:** Escape dismisses the dropdown and restores focus to its trigger. Clicking outside dismisses it.
  Support keyboard opening, arrow-key movement, Home/End, Enter/Space selection and visible focus.
- **APP-H5:** Keep the environment badge and a Dark Mode toggle at the right of the header.
  The toggle's accessible label/state reflects the selected theme.

### 4.2 Tool lifecycle and isolation

- **APP-T1:** Each tool owns its action row, editors, panels, dialogs, notifications, status and result model.
  Do not show RunCSharp controls or its footer while RenderTemplate is selected.
- **APP-T2:** Preserve editor content, cursor/undo state, options, history, results, panel state and status in memory
  when switching. Navigation neither executes an API nor replaces unsaved edits.
- **APP-T3:** Keyboard shortcuts act only on the active tool. Ctrl/Cmd+Enter must not invoke a hidden RunCSharp tool.
- **APP-T4:** Tool-only dialogs cannot remain visible after leaving that tool. Close them without applying unsaved
  dialog changes; preserve the tool's already-applied options and editor edits.
- **APP-T5:** A request started by one tool may finish while another tool is selected. Its completion updates only
  the originating tool's state/status, never the currently selected tool's results, footer or notifications.
  Each tool prevents its own overlapping requests.
- **APP-T6:** Retain tool instances/state while switching, or restore them losslessly. Visible Monaco editors must
  relayout on return. Inactive tools must not remain keyboard-focusable or expose hidden content to screen readers.
- **APP-T7:** Adding a future tool requires its own specification, component/state owner and a menu registration,
  not alterations to RunCSharp's request model or assumptions about its diagnostics.

### 4.3 Theme and browser storage

- **APP-S1:** One light/dark theme covers the header and every tool, including editors. Initially follow
  `prefers-color-scheme`; store an explicit user preference in browser storage.
- **APP-S2:** Theme is app-wide, with a versioned key such as `spark-code-tester:v1:theme`.
- **APP-S3:** Persisted tool data is isolated by stable environment identity and tool ID, not environment display name.
  Use versioned keys such as `spark-code-tester:v1:<environment-id>:<tool-id>:history`.
  Tool specs define their own payloads, limits and truncation rules. Do not create history for a layout-only tool.
- **APP-S4:** Browser-only data is local to the browser/origin and is not shared through Dataverse.
  Storage failures must produce a visible error/warning, not a claim that data was saved.
- **APP-S5:** The current mockup demonstrates in-session state preservation only; persisted theme/history remains
  a production requirement, not an implemented feature of this design update.

## 5. Non-functional requirements

| ID | Requirement |
|---|---|
| APP-N1 | Power Apps Code App: React + TypeScript + Vite, deployed using `pac code push`. |
| APP-N2 | Calls use the signed-in user's Dataverse connection; no client secrets. Tool specs define request/fault handling. |
| APP-N3 | First meaningful paint under 2 s on broadband; load heavyweight editors lazily with a lossless fallback. |
| APP-N4 | WCAG 2.1 AA contrast, keyboard access, visible focus and appropriate ARIA semantics in both themes. |
| APP-N5 | Minimum supported width 1024 px; layouts must remain usable at 1024 and 1280 px and above. |
| APP-N6 | Escape output/errors/logs before rendering; never execute returned markup or scripts. |
| APP-N7 | Current Edge, Chrome and Firefox. Keep navigation responsive during tool execution. |

## 6. Proposed architecture

```
src/
  App.tsx                      shared header and selected tool host
  components/
    AppHeader.tsx              branding, environment, tool navigation, theme
    ToolSelector.tsx           accessible collapsible dropdown
  tools/
    registry.ts                stable tool IDs, labels and components
    run-csharp/                 RunCSharpTool, toolbar, workspace, footer and state
    render-template/            RenderTemplateTool layout-only placeholder
  state/
    appPreferences.ts          shared selection/theme, not API results
  services/
    dataverseConnection.ts     signed-in connection boundary
```

Keep the registry small: ID, label and component are sufficient. Do not introduce a generic API execution
framework or require all tools to share the RunCSharp response type. See each tool spec for its internal architecture.

## 7. Acceptance criteria

1. The shared header shows `Spark Code Tester · RunCSharp` initially and Dark Mode on the right.
2. The dropdown lists both tools, identifies the current selection and supports mouse and keyboard dismissal/selection.
3. RunCSharp Samples, Options and Run are in its second row, not the shared header.
4. RenderTemplate selection changes the title and displays only its toolbar, placeholder workspace and footer.
   Its disabled Run action and placeholder notice cannot be mistaken for a working renderer.
5. Editing/running RunCSharp, switching tools and returning preserves the request, options, history, results and status.
6. Ctrl/Cmd+Enter while RenderTemplate is selected does not trigger RunCSharp.
7. Switching during a simulated RunCSharp execution does not overwrite RenderTemplate's placeholder status.
8. Light/dark styling follows the shared toggle for both tools; navigation works at 1024 and 1280 px.
9. The general/tool specifications link to one another and describe their own responsibilities without a single-API exclusion.

## 8. Delivery plan

1. **Design update (this scope):** split specifications, update the shared mockup header and RunCSharp ownership,
   add the RenderTemplate layout-only example and verify switching/state isolation.
2. **Scaffold shared application:** create the Code App under `src/codeapps/api-tester/`, connect Dataverse,
   add a minimal tool registry/header/host and theme.
3. **Implement RunCSharp incrementally:** follow [RunCSharp delivery plan, section 9](./run-csharp-specs.md#9-delivery-plan).
   Its API prerequisite work is already deployed; preserve the incremental frontend checkpoints.
4. **Complete shared lifecycle/storage/accessibility:** verify isolation during navigation/in-flight calls and
   tool-scoped browser storage. Do not merge tool histories.
5. **RenderTemplate implementation:** not authorized by the layout-only example. Define and approve detailed
   execution behavior before implementing it.
6. Validate shared acceptance criteria and each implemented tool's criteria before deployment.

## 9. Decisions and open questions

### Confirmed

- RunCSharp is the first tool in Spark Code Tester; RenderTemplate is a layout-only second example.
- All content below the top header, including status, belongs to the selected tool.
- Navigation preserves in-memory state; browser storage is tool/environment-scoped.
- Browser storage only; no Dataverse test-history storage or sharing.

### Open

- Verify whether the Code Apps generated Dataverse client supports the unbound actions directly, or whether the
  shared connection boundary must support authenticated Web API calls. Do not guess a connector method signature.
