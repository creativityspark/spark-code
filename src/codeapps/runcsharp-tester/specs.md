# RunCSharp Tester – Specifications

## 1. Overview

**RunCSharp Tester** is a Power Apps Code App that lets makers and developers write, run and debug C# scripts
against the `csp_Other_RunCSharp` Dataverse custom API without leaving the browser.

- **Custom API:** `csp_Other_RunCSharp` (unbound action, `isfunction = 0`)
- **Plugin:** [RunCSharp.cs](../../assemblies/SparkCode.API/Other/RunCSharp.cs)
- **Design reference:** [mockup.html](./mockup.html). Open it in a browser; it is interactive and uses mock data.

### Goals
- Make it fast to iterate on RunCSharp scripts: edit, run, inspect output, repeat.
- Expose every API parameter in the UI, so testers don't have to build request payloads by hand.
- Make failures easy to understand: compiler errors, runtime errors and log messages.
- Let users inspect structured output (XML/JSON) without external tools.

### Non-goals
- Local C# compilation or IntelliSense backed by a real compiler; all compilation happens in Dataverse.
- Managing, sharing or storing scripts in Dataverse tables. Script persistence uses browser storage only.
- Calling custom APIs other than `csp_Other_RunCSharp`.

## 2. Users

| Persona | Need |
|---|---|
| Power Platform developer | Prototype a RunCSharp script before wiring it into a flow or plugin. |
| Maker / low-code author | Try samples, tweak values and see results without Visual Studio. |
| Support engineer | Reproduce a failing RunCSharp call and inspect its logs and errors. |

## 3. API contract

### 3.1 Request parameters

| Parameter | Type | Required | UI source | Default sent |
|---|---|---|---|---|
| `Code` | string | Yes | Editor → **Code** tab | – (Run is blocked when empty) |
| `InputParameters` | string (JSON) | No | Editor → **InputParameters** tab | omitted when empty |
| `Types` | string (C#) | No | Editor → **Types** tab | omitted when empty |
| `InputTypeName` | string | No | Options → InputTypeName | `string` |
| `OutputTypeName` | string | No | Options → OutputTypeName | `string` |
| `ReferencedAssemblies` | string (comma-separated) | No | Options → ReferencedAssemblies (tags) | omitted when empty |
| `UsingStatements` | string | No | Options → UsingStatements | omitted when empty |

Optional parameters with empty values are **omitted** from the request, not sent as empty strings, so that
the plugin defaults in `ctx.GetInputParameter(..., defaultValue)` apply.

### 3.2 Response properties

| Property | Type | Status | Description |
|---|---|---|---|
| `Outputs` | string | Existing | Result of the script. Native output types use `Convert.ToString`; custom types are serialized to JSON. |
| `Logs` | string \| null | Existing in code (response property not yet registered in the solution) | Messages written with `Log(message)`, joined with `\n` in the order they were written. `null` when nothing was logged. |
| `CompileErrors` | string (JSON array) \| null | **Planned API change** | Compiler errors, or `null` when compilation succeeded. Schema below. |
| `RuntimeErrors` | string (JSON array) \| null | **Planned API change** | Script execution or input-conversion errors, or `null` when neither occurred. Schema below. |

```jsonc
// CompileErrors
[{ "line": 3, "column": 1, "code": "CS0103", "message": "The name 'ouput' does not exist in the current context",
   "section": "Code" }]                    // section: "Code" | "Types" | "UsingStatements"
// RuntimeErrors
[{ "line": 5, "message": "Object reference not set to an instance of an object.",
   "section": "Code", "exceptionType": "System.NullReferenceException",
   "stackTrace": "   at SparkCode.CSharpRunner.Run(String input)" }]
```

- `line` (and `column`) are **relative to the section** the user typed them in (Code, Types or UsingStatements). The API
  maps generated-source positions back to sections, so the client does no offset mapping.
- Input-conversion failures when deserializing `InputParameters` into `InputTypeName` are **runtime errors** with
  `section: "InputParameters"`, not HTTP faults. Return exception details in `RuntimeErrors`, with `Outputs = null`;
  script execution does not start. Use `line = null` when no reliable input location is available.
- With the planned change, the API **doesn't throw** for script errors (compile errors, runtime exceptions or input-conversion failures). It returns a
  normal response in which `Outputs` is `null` and `CompileErrors` or `RuntimeErrors` is populated. `Logs` is always
  complete, including the messages written before a runtime exception.
- Faults that aren't script errors (HTTP 403, timeout, invalid request, an older API version that still throws) still
  arrive as an HTTP error; see FR-P4.

> **Current API behavior** (until the planned change ships): compile errors throw `Compiler Errors:` followed by one
> line per error, and runtime errors throw `Runtime Error: <message>` followed by the logs collected so far. The tester
> shows these as a single `HTTP <status>` problem with the full message (FR-P4).

### 3.3 Logging
The generated code exposes a static `Log(string message)` function on `SparkCode.CSharpRunner`:

```csharp
Log("Total computed: " + output.Total);              // in Code
CSharpRunner.Log("Items generated: " + items.Count); // from classes declared in Types
```

- `Log` takes a single string; there are no format arguments. Use concatenation or interpolation.
- Code runs inside `CSharpRunner.Run`, so it calls `Log(...)` directly. Types are declared in the same `SparkCode`
  namespace and call `CSharpRunner.Log(...)`.
- Messages are collected in memory and returned in the `Logs` output, joined with `\n`. If nothing was logged, `Logs` is `null`.

### 3.4 Invocation
- `POST [org]/api/data/v9.2/csp_Other_RunCSharp` with a JSON body containing the parameters in §3.1.
- The call is made in the signed-in user's context through the Code App's Dataverse connection.
- The client-side timeout aligns with the Dataverse plugin execution limit of 2 minutes; see NFR-3.

## 4. Layout

```
┌──────────────────────────────────────────────────────────────────────────────────┐
│ Logo · Env badge                 Samples ▾ │ ⚙ Options (n) │ ☾ │ ▶ Run Ctrl+↵  │  Header
├────────────┬───────────────────────────────────────────────┬─────────────────────┤
│ Run        │ [Code C#] [InputParameters JSON] [Types C#]   │ Outputs explorer    │
│ history    │ tab hint                                      │ [String|XML|JSON]   │
│            │                                               │ meta: time · size   │
│ (collaps-  │            Monaco editor                      │                     │
│  ible)     │                                               │   viewer            │
│            ├───────────────────────────────────────────────┤                     │
│            │ [Problems n] [Logs n]         filter · copy ▾ │                     │
│            │ list                                          │                     │
├────────────┴───────────────────────────────────────────────┴─────────────────────┤
│ ● Status │ Duration │ Output size │ InputType → OutputType        API name       │  Status bar
└──────────────────────────────────────────────────────────────────────────────────┘
```

## 5. Functional requirements

### 5.1 Header
- **FR-H1:** Show the app name and a badge with the current environment name and the custom API name.
- **FR-H2:** A **Samples** dropdown loads a sample template; see §5.9.
- **FR-H3:** An **⚙ Options** button opens the Options dialog (§5.3). A badge shows how many options differ from
  their defaults; the badge is hidden when the count is 0.
- **FR-H4:** A theme toggle switches between light and dark themes (§5.10).
- **FR-H5:** A primary **▶ Run** button executes the request (§5.4). **Ctrl+Enter** (⌘+Enter on macOS) also triggers Run,
  including when focus is inside the editor.

### 5.2 Editor
- **FR-E1:** A single Monaco editor instance with three tabs, each backed by its own model:

  | Tab | Language | Maps to |
  |---|---|---|
  | Code | C# | `Code` |
  | InputParameters | JSON | `InputParameters` |
  | Types | C# | `Types` |

- **FR-E2:** Switching tabs keeps each model's content, cursor and undo history.
- **FR-E3:** A one-line hint under the tabs explains what the active tab is for and which parameter it maps to.
  The Code hint mentions `input`, `output` and `Log("message")` (returned in the `Logs` output).
- **FR-E4:** Each tab shows a dirty dot when its content differs from the last executed or restored content.
- **FR-E5:** The tab bar shows the cursor position (`Ln x, Col y`) and a **Format** button. Format pretty-prints JSON on the
  InputParameters tab and runs Monaco's format action on the C# tabs.
- **FR-E6:** Problems with a line are shown as Monaco error markers (squiggles) on the model of their section
  (Code, Types or InputParameters); see §5.6.
- **FR-E7:** The editor follows the app theme (`vs` / `vs-dark`).

### 5.3 Options dialog
- **FR-O1:** A modal dialog with these fields:
  - **InputTypeName:** text input with suggestions (§FR-O2), placeholder `string`.
  - **OutputTypeName:** text input with suggestions, placeholder `string`.
  - **ReferencedAssemblies:** tag input. Enter or `,` adds a tag, Backspace on an empty input removes the last tag,
    and pasting a comma-separated list adds multiple tags. Duplicates are ignored.
  - **UsingStatements:** multi-line monospace text area.
- **FR-O2:** Type suggestions include `string`, `int`, `decimal`, `bool`, `DateTime`, plus every `class` / `struct` / `enum`
  name found in the Types tab.
- **FR-O3:** A live summary shows the exact values that will be sent.
- **FR-O4:** Buttons: **Reset to defaults**, **Cancel** (discard changes), **Save** (apply). Esc behaves like Cancel.
- **FR-O5:** Blank type names are saved as `string`.

### 5.4 Run
- **FR-R1:** Run is disabled while a request is in flight. The button shows a spinner and "Running…".
- **FR-R2:** If `Code` is empty or whitespace, Run doesn't call the API and shows a validation message instead.
- **FR-R3:** If InputTypeName isn't `string` and InputParameters isn't valid JSON, Run shows a problem pointing to the
  InputParameters tab and doesn't call the API.
- **FR-R4:** On completion, the app records the duration (client round-trip), updates the status bar, adds a history entry,
  and resets every tab's dirty state.
- **FR-R5:** The run result is derived from the response (§3.2):
  - `CompileErrors` present → **compile failed**.
  - Otherwise `RuntimeErrors` present → **runtime error**.
  - Otherwise → **succeeded**.
- **FR-R6:** On success, the app populates Outputs (§5.5) and Logs (§5.7) and clears Problems.
- **FR-R7:** On a compile or runtime failure, the app populates Problems (§5.6) and Logs (complete, possibly empty),
  clears Outputs and focuses the Problems tab. On an HTTP fault, Logs is empty.

### 5.5 Outputs explorer
- **FR-X1:** The right-hand panel shows the `Outputs` value of the selected run.
- **FR-X2:** A view switcher offers three views:
  - **String:** raw text in monospace.
  - **XML:** parsed with `DOMParser` and rendered as a collapsible element tree with colored tags and attributes.
  - **JSON:** parsed and rendered as a collapsible tree with colored keys and values, labelled `Object{n}` / `Array[n]`.
- **FR-X3:** After a run, the view is chosen automatically: XML if the output starts with `<`, JSON if it parses as an
  object or array, otherwise String. The user can switch views at any time.
- **FR-X4:** If the selected view can't parse the output, show an inline parse error followed by the raw text.
- **FR-X5:** Toolbar actions: expand all, collapse all (keeps the root open), word-wrap toggle (String view), and copy the raw
  output to the clipboard.
- **FR-X6:** A metadata line shows run time, duration, character count and detected format, or "Compilation failed – no output"
  / "Runtime error – no output".
- **FR-X7:** Empty states:
  - Before any run: "Run your code…".
  - Compile failed: "The code did not compile. See the Problems panel."
  - Runtime error: "The code threw an exception at runtime. See the Problems panel; logs written before it are in the Logs tab."
- **FR-X8:** Outputs larger than 1 MB are shown in String view only, with a message and a download option.

### 5.6 Problems panel
- **FR-P1:** The bottom panel's **Problems** tab lists errors from the last or selected run. Its badge shows the count and is red when greater than 0.
- **FR-P2:** Each entry shows severity, code, a kind tag (`compile`, `runtime`, `validation` or `http`), the message and its
  location as `Section (line,col)`, for example `Types (3,24)`.
- **FR-P3:** Clicking an entry navigates to its source:
  - Code, Types or InputParameters → switch to that tab, reveal the line and move the cursor there.
  - UsingStatements → open the Options dialog with the UsingStatements field focused.
- **FR-P4:** Problem sources (no regex parsing of messages):
  - **compile:** one entry per `CompileErrors` item. Code = the `CS` code; section, line and column come from the item.
  - **runtime:** one entry per `RuntimeErrors` item. Code = the short exception type name (for example
    `NullReferenceException`); section and line come from the item. Input-conversion errors point to InputParameters,
    while script execution errors point to Code or Types as mapped by the API. When line is null, focus the section
    without inventing a source position. The full type and stack trace appear in an expandable **stack** section.
  - **validation:** client-side checks done before calling the API (FR-R2, FR-R3).
  - **http:** any fault response (403, timeout, invalid request, or the current API that still throws) becomes a single entry
    with code `HTTP <status>` (or `TIMEOUT`) and the full error message.
- **FR-P5:** Empty state: "No problems. Compilation and execution succeeded." after a successful run, or "Press Run…" before the first run.

### 5.7 Logs panel
- **FR-L1:** A **Logs** tab sits next to Problems in the bottom panel. Its badge shows the message count and is accent-colored when greater than 0.
- **FR-L2:** The `Logs` output is split on `\n`, and each message is shown with its 1-based order in monospace.
- **FR-L3:** A filter box, visible only on the Logs tab, filters messages case-insensitively and highlights matches.
- **FR-L4:** **Copy** copies the raw `Logs` string.
- **FR-L5:** Bottom tab chosen automatically after a run:
  - Failure → Problems.
  - Success with logs → Logs.
  - Success without logs → the tab that was already selected.
- **FR-L6:** After a runtime error, the list starts with a note: "The run failed at runtime – these are all messages
  logged before the error."
- **FR-L7:** Empty states:
  - Before the first run: a hint showing how to call `Log("message")` in Code and `CSharpRunner.Log("message")` in Types.
  - Compile failed: "No logs – the code did not compile."
  - Run completed (successfully or with a runtime error) with `Logs = null`: "The run completed without writing any log messages."
- **FR-L8:** The bottom panel can be collapsed and expanded with a chevron. Clicking a tab expands it. Its height can be resized
  with a drag handle (minimum 80 px, maximum 60% of the editor area).

### 5.8 Run history
- **FR-HI1:** The left panel lists previous runs, newest first. Each entry shows a status dot (green or red), the first
  meaningful line of code (skipping blank lines,   `//` comments and `Log(` / `CSharpRunner.Log(` calls), the time, the duration and `InputType→OutputType`.
  - **FR-HI2:** Clicking an entry restores the editor tabs, options, outputs, problems and logs for that run, and highlights it.
- **FR-HI3:** **Clear** removes every history entry after a confirmation.
- **FR-HI4:** The panel can collapse to a narrow rail.
- **FR-HI5:** History is saved in `localStorage` for each environment, keeping at most 50 entries (oldest removed first).
  Outputs larger than 256 KB are cut off in saved entries, and the entry is marked as truncated.
- **FR-HI6:** Scripts and history use browser storage only. Do not create a Dataverse table or server-side
  persistence/sharing feature; stored entries are local to the browser and are not shared across users or devices.

### 5.9 Sample templates
Each sample sets all three tabs and all options, then resets problems, logs and outputs.

| Sample | Demonstrates | Options |
|---|---|---|
| Untyped – hello | `input`/`output` as strings, two `Log` calls | defaults |
| Typed input/output (JSON) | `InputType`/`OutputType` classes, LINQ, `Log` in Code and `CSharpRunner.Log` in Types (4 messages) | `InputType`, `OutputType`, `System.Core.dll`, `using System.Linq;` |
| XML output | LINQ to XML, XML output, `Log` calls | `System.Xml.dll`, `System.Xml.Linq.dll`, `using System.Linq; using System.Xml.Linq;` |
| Compile error example | `CS0103` and `CS0029` in Code plus `CS1002` in Types (section mapping) | defaults |
| Runtime error example | `NullReferenceException` at Code line 5; Logs keeps the 3 messages written before it, including one from Types | defaults |

If any tab has unsaved edits, loading a sample asks for confirmation first.

### 5.10 Theme and status bar
- **FR-S1:** The theme is light or dark. Its initial value follows `prefers-color-scheme`, and the user's choice is saved in `localStorage`.
- **FR-S2:** The status bar shows:
  - Status: Ready, Running…, ✔ Succeeded, ✖ Compile failed, ✖ Runtime error or ✖ Failed (HTTP fault). The bar is accent-colored normally, amber while running and red on failure.
  - Duration.
  - Output size in bytes.
  - `InputTypeName → OutputTypeName`.
  - The custom API name.

## 6. Non-functional requirements

| ID | Requirement |
|---|---|
| NFR-1 | **Platform:** Power Apps Code App (React + TypeScript + Vite), deployed with `pac code push`. |
| NFR-2 | **Security:** all calls run under the signed-in user; no secrets in the client. Running arbitrary code is gated by the API's privileges and security roles. The app shows a clear message on HTTP 403. |
| NFR-3 | **Timeouts:** the client stops waiting after 130 s and reports `TIMEOUT`. The UI stays responsive while a run is in progress. |
| NFR-4 | **Performance:** first meaningful paint under 2 s on broadband. Monaco is loaded lazily; until it is ready, a plain-text fallback is shown and its content is carried over. |
| NFR-5 | **Accessibility:** WCAG 2.1 AA contrast in both themes, all actions reachable by keyboard, ARIA roles for tabs, tree views and the dialog, and a visible focus indicator. |
| NFR-6 | **Responsiveness:** minimum supported width 1024 px. Below 1280 px the history panel starts collapsed. |
| NFR-7 | **Safety:** output, log and error text is always HTML-escaped before rendering, and XML/JSON are parsed without executing anything. |
| NFR-8 | **Browsers:** current Edge, Chrome and Firefox. |

## 7. Proposed architecture

```
src/
  App.tsx                    layout shell
  components/
    Header.tsx               samples, options, theme, run
    HistoryPanel.tsx
    EditorTabs.tsx           Monaco + 3 models
    OptionsDialog.tsx
    BottomPanel.tsx          Problems | Logs tabs
    ProblemsList.tsx
    LogsList.tsx
    OutputsExplorer.tsx      String | Xml | Json viewers
    StatusBar.tsx
  services/
    runCSharpService.ts      builds request, calls csp_Other_RunCSharp, maps HTTP faults
    responseParser.ts        Outputs/Logs/CompileErrors/RuntimeErrors → RunResult
  state/
    store.ts                 editor values, options, runs, ui prefs (React context / Zustand)
    historyStorage.ts        localStorage persistence (per environment)
  samples/templates.ts
  types.ts                   RunRequest, RunResult, Problem, HistoryEntry
```

```ts
interface RunRequest {
  Code: string; InputParameters?: string; Types?: string;
  InputTypeName?: string; OutputTypeName?: string;
  ReferencedAssemblies?: string; UsingStatements?: string;
}
// Raw response: every property is a string or null
interface RunResponse {
  Outputs: string | null; Logs: string | null;
  CompileErrors: string | null;   // JSON: CompileError[]
  RuntimeErrors: string | null;   // JSON: RuntimeError[]
}
type Section = "Code" | "Types" | "UsingStatements" | "InputParameters";
interface CompileError { line: number; column: number; code: string; message: string; section: "Code" | "Types" | "UsingStatements"; }
interface RuntimeError { line: number | null; section: "Code" | "Types" | "InputParameters"; message: string; exceptionType: string; stackTrace: string; }

interface RunResult {
  status: "ok" | "compile" | "runtime" | "http"; durationMs: number;
  outputs: string | null; logs: string | null;
  problems: Problem[];
}
interface Problem {
  kind: "compile" | "runtime" | "validation" | "http";
  code: string; message: string;
  section?: Section; line?: number; column?: number;
  exceptionType?: string; stackTrace?: string;
}
```

## 8. Acceptance criteria
1. Running the **Untyped – hello** sample returns `Hello, Dataverse!` in String view and shows 2 logs.
2. Running the **Typed** sample selects the JSON view automatically; the tree shows `Total: 246`, and the Logs tab shows 4 messages,
   including `Items generated: 3` written from Types.
3. Running the **XML output** sample selects the XML view and renders an `accounts` tree with 2 `account` children.
4. Running the **Compile error** sample shows "✖ Compile failed" and 3 problems: two in Code and one in Types. Clicking the Types
   problem switches to the Types tab at line 3. Logs shows "No logs – the code did not compile."
5. Running the **Runtime error** sample shows "✖ Runtime error", selects Problems, and shows one `NullReferenceException` problem at
   Code line 5 with an expandable stack trace. The Logs tab shows all 3 messages written before the exception.
6. A compile error with section UsingStatements opens the Options dialog when clicked.
7. Saving the Options dialog sends the configured values in the request, and the badge count matches the number of non-default options.
8. A history entry restores the exact editor content, options, output, problems and logs of that run, including after a page reload.
9. Theme switching updates the app and the editor, and the choice persists across reloads.
10. No HTML injection: an output or log message of `<img src=x onerror=alert(1)>` is shown as text.

## 9. Delivery plan
1. **API changes** (owned by the API work, not by this app):
   - Register the `Logs` response property in the solution (`customapiresponseproperties/Logs`); the plugin already sets it.
   - Return `CompileErrors` and `RuntimeErrors` (§3.2) instead of throwing for script errors, with lines mapped to sections,
     and register both response properties.
   - Redeploy with the `sparkcode-api-package-deploy` skill.
2. **Scaffold** the Code App in `src/codeapps/runcsharp-tester/` and add the Dataverse data source.
3. **Build functionality incrementally.** Each substep extends the previous working version; verify its checkpoint before
   moving on. Until step 1 ships, script errors surface as `HTTP` problems (FR-P4), not parsed message strings.
   1. **Minimal end-to-end run:** show a plain-text Code editor, Run button and read-only string output. Send only
      `Code` through `runCSharpService`; block empty code, prevent overlapping requests and show HTTP failures.
      **Checkpoint:** `output = "hello";` calls the real API and displays `hello`; a failed call shows its error.
   2. **Run lifecycle and status:** add ready/running/success/failure state, client timeout, round-trip duration and
      output byte size. Clear stale output when starting a run and keep the UI responsive.
      **Checkpoint:** repeated clicks cannot start concurrent calls; timeout and failure never show old output as success.
   3. **Input payload:** add the InputParameters editor tab and send nonempty input while retaining the default string
      input type. Preserve both tabs' contents when switching.
      **Checkpoint:** a script reads the supplied string input; empty input is omitted from the request.
   4. **Types and options:** add the Types tab and Options dialog for input/output type names, referenced assemblies
      and using statements. Implement defaults, omission rules, non-default option count and typed-input JSON validation.
      **Checkpoint:** a typed input/output script works, and invalid JSON is reported before the API call.
   5. **Structured problems:** implement `responseParser`, the Problems panel and compile/runtime/validation/HTTP
      classification. Preserve section-relative locations and navigate to the appropriate editor tab or option field;
      input-conversion failures target InputParameters. Clear outputs on script failure.
      **Checkpoint:** compile and runtime responses produce the correct status and locations; errors without a line
      focus their section without inventing a position.
   6. **Logs:** add the Logs panel, message count, automatic panel selection, filter and copy. Keep complete logs on
      runtime failure and distinguish compile failure, no logs and not-yet-run states.
      **Checkpoint:** a script that logs and then throws shows every preceding message alongside its runtime problem.
   7. **Monaco editing:** replace the plain-text editors with lazy-loaded Monaco models for Code, InputParameters and
      Types. Add language modes, formatting, error markers and Ctrl+Enter, retaining the fallback while loading.
      **Checkpoint:** switching tabs or loading Monaco loses no text; problem navigation reveals the correct location.
   8. **Output inspection:** extend the string viewer with JSON/XML detection, structured explorers, format selection
      and copy. Keep all output/error/log rendering escaped and parsing non-executable.
      **Checkpoint:** JSON and XML can be inspected, ordinary strings remain readable, and HTML-like output is inert.
   9. **In-session history:** record completed runs with editor values, options, results, problems, logs and timing.
      Add selection/restoration, current-entry highlighting, collapse and confirmed Clear.
      **Checkpoint:** selecting an earlier run restores its exact request and result without invoking the API again.
   10. **Browser persistence:** persist history per environment in `localStorage`, enforce the 50-entry limit and
       256 KB saved-output truncation marker, and report storage failures visibly. No Dataverse persistence or sharing.
       **Checkpoint:** reload restores history, environments stay isolated, and truncated entries are identified.
   11. **Samples:** add the sample dropdown and templates from §5.9, replacing all editor/options values together and
       resetting prior results. Confirm before replacing unsaved edits.
       **Checkpoint:** each sample loads a complete reproducible request; cancelling replacement preserves current edits.
   12. **Complete the shell and accessibility:** finish header/environment metadata, theme persistence, resizable and
       collapsible panels, responsive defaults, keyboard navigation, ARIA semantics and focus/contrast behavior.
       **Checkpoint:** verify the complete workflow in both themes and supported viewport sizes without losing state.
4. Check the app against §8 in a dev environment, then deploy.

## 10. Decisions and open questions

### Confirmed decisions
- Input-conversion failures are returned in `RuntimeErrors` with section `InputParameters`; they do not throw as HTTP faults.
- Scripts and history use browser storage only, not Dataverse tables or server-side sharing.

### Open questions
- Can Code Apps call an unbound custom API directly through the generated Dataverse client, or is a raw Web API call needed?
