# Spark Code Tester

Power Apps Code App for testing SparkCode APIs. The shared shell provides an accessible tool dropdown,
selected API title, configured environment badge and persistent light/dark theme.
RunCSharp provides a plain-text Code editor, Run action and read-only string output using its generated service
and typed adapter. RenderTemplate remains empty; later RunCSharp features are not implemented.

## Local development

Prerequisites: Node.js 22+ and, for Power Apps environment configuration, authenticated PAC CLI access.
Copy the example on a fresh checkout only; do not overwrite an existing local configuration.

```powershell
# From the repository root:
Set-Location .\src\codeapps\api-tester
npm ci
Copy-Item power.config.example.json power.config.json
npm run dev
```

The example configuration has no environment selected and supports standalone shell development.
After initializing an environment as described below, the official Power Apps Vite plugin prints both a
localhost URL and a **Local Play** URL for running the local app inside the authenticated Power Apps host. Open Local Play
when running the API; standalone localhost is not proof of authenticated
Dataverse connectivity.

```powershell
npm run build
npm run lint
npm test
npm run preview
```

Build includes strict TypeScript checking and produces `dist`. Preview serves the production bundle locally.
To publish to your configured environment, confirm the local app/environment selection and run
`npx --no-install pa app push` after a successful build. This updates the configured live app.

## Environment and connection boundary

The published [example configuration](./power.config.example.json) contains no environment, app, tenant,
connection or database identifiers. The real `power.config.json` is local-only and ignored by Git.
To configure your own environment, verify your authenticated PAC selection using `pac org who` and
`pac env list`, then initialize locally:

```powershell
pac code init --displayName "Spark Code Tester" --environment "<your-environment-id>"
```

Never commit the resulting configuration, organization URLs, tenant/user identifiers or credentials.
Generated build assets can embed local environment configuration; `dist` is also ignored and must not be
published as generic source or documentation. Review staged files before publishing. Previously published
credentials require revocation/rotation and repository-history cleanup; deleting them from current files alone is insufficient.

The official template includes the Power Apps SDK and Vite plugin. Initial setup used PAC; adding Dataverse actions
requires the newer [Power Apps Code App CLI](https://learn.microsoft.com/power-apps/developer/code-apps/reference/cli),
not `pac code`. With that CLI installed and authenticated against your own environment, generate the operation:

```powershell
pa app find-dataverse-api --search csp_Other_RunCSharp
pa app add dataverse-api --api-name csp_Other_RunCSharp
```

Generation writes the environment-neutral API metadata under `.power/schemas` and the service under
`src/generated`, and updates the ignored local `power.config.json`. Review generated files before publishing;
never commit local environment/app identifiers or connection details. Regenerate rather than manually editing
the generated service. No tables, records, or API metadata are created by adding the operation.

The typed [RunCSharp adapter](./src/tools/run-csharp/runCSharpService.ts) currently accepts Code only and uses the
generated service. It validates nullable string response fields without parsing script diagnostics yet.
SDK failures are thrown, while compiler/runtime diagnostic strings remain normal API response fields.
The minimal UI displays these diagnostics as plain error text, not structured Problems entries.

Authenticated metadata discovery and service generation succeeded. The user confirmed that the signed-in browser
connection check succeeds with the fixed hello script. Local CLI authentication does not authenticate the browser.
No deployment was performed as part of service generation.

### Run Code and verify the API connection

1. Run `npm run dev` and open the printed **Local Play** URL using your signed-in Power Apps browser profile.
2. Select RunCSharp. The **Code** editor initially contains `output = "hello";`.
3. Click **Run**. Only the Code parameter is sent; **String output** should display `hello`.
   Edit Code to run another script. Whitespace-only Code is rejected without an API call.
4. SDK/authentication failures and script diagnostics show an explicit error instead.
   Plain SDK error objects show their message and HTTP status when supplied, rather than `[object Object]`.
   The Run button and Code editor are disabled during execution; later runs replace the previous output/error.
5. A null output is explicitly described as no output; an empty string is described as an empty string.
   Returned markup remains inert plain text in the read-only output area.

Execution never starts automatically. Code, results and messages belong to RunCSharp and survive switching tools.
If a request finishes while RenderTemplate is selected, it updates only RunCSharp.
Standalone localhost is useful for developing the UI but cannot establish authenticated Dataverse connectivity.

The environment badge reports whether local configuration selects an environment, not live API health.
The source contains no hardcoded organization URL or environment name. Do not add client secrets or
custom authentication; future tool integrations must use the signed-in Power Apps connection.

## Shell behavior

- RunCSharp is selected initially. The small typed [tool registry](./src/tools/registry.ts) supplies both navigation
  and the host's component mapping. Both tool components remain mounted when switching.
- The Tools menu supports mouse selection, Enter/Space and arrow-key opening, Arrow Up/Down wrapping,
  Home/End, Escape with focus restoration, outside-click dismissal and dismissal when focus leaves.
- Theme initially follows the OS. An explicit choice is stored as `spark-code-tester:v1:theme`.
  Invalid values and unavailable browser storage produce visible warnings; a failed write still changes the
  in-session theme without claiming persistence.
- RunCSharp includes only the Code editor, Run action, plain string output and minimal busy/error feedback.
  No detailed footer, history, samples, options, extra input tabs, logs/structured Problems panels,
  Monaco, output-format detection, timeouts, timing/size metrics or execution shortcuts are included.

## Validation

Production build and lint passed. Headless Edge checks covered both selections, harmless reselection,
keyboard/focus behavior, blank workspaces, OS theme changes, persistence across reloads, invalid/blocked
storage, both themes at 1024/1280 px, no horizontal overflow and no custom API requests/browser errors.
The user separately verified the original live host-integrated hello connection check for item 9.2.

Six adapter tests cover Code-only invocation/response mapping, script diagnostics, SDK fault propagation, malformed
responses and readable error-object messages. Run `npm test` with Node.js 22.22+; this uses Node's built-in test runner with experimental TypeScript stripping
and module mocking, not an additional test dependency. These isolated tests do not replace the required authenticated
Local Play smoke call. The user's successful live check completes item 9.2.

For item 9.3.1, production build/type-check, lint and all six adapter tests pass. Isolated Edge checks with a
mocked generated service cover edited Code-only payloads, blank-code rejection, overlap prevention, SDK/compiler/runtime
failures, retry, stale-output clearing, null/empty/inert string output, retained tool state, hidden-request completion
isolation and both themes at 1024/1280 px. No uncaught browser exceptions occurred.
These checks execute the real UI and adapter but do not prove a live Dataverse call.
The editable workflow's authenticated hello checkpoint and version 0.1.1 deployment still require verification.

The official template's dependency audit reports zero production vulnerabilities. Its development dependency
tree reports 12 moderate advisories associated with `minimatch` and ESLint tooling; no forced breaking dependency
upgrades were applied as part of this shell increment.

## Specifications

- [General specification and delivery status](./specs.md)
- [RunCSharp specification and incremental delivery plan](./run-csharp-specs.md)
- [RenderTemplate design-only specification](./render-template-specs.md)
- [Interactive design mockup](./mockup.html): simulated/layout-only tool content is a design reference, not app functionality.
