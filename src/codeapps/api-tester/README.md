# Spark Code Tester

Power Apps Code App scaffold for testing SparkCode APIs. This increment implements only the shared shell:
an accessible tool dropdown, selected API title, configured environment badge and persistent light/dark theme.
RunCSharp and RenderTemplate both have entirely empty workspaces. No tool API is called.

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
when working on future SDK/connector integrations; standalone localhost is not proof of authenticated
Dataverse connectivity.

```powershell
npm run build
npm run lint
npm run preview
```

Build includes strict TypeScript checking and produces `dist`. Preview serves the production bundle locally.
There is no deployment step in this increment.

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

The official template includes the Power Apps SDK and Vite plugin. This version of the template has no local
`pa`/`power-apps` CLI binary; the installed PAC CLI provided supported local initialization instead.
No app has been pushed (`appId` is null), no tables or API data sources have been added, and no records or
metadata have been changed. Connection/database references are intentionally empty. Hosted SDK authentication
and custom action invocation remain unverified until tool integration is authorized.

The environment badge reports whether local configuration selects an environment, not live API health.
The source contains no hardcoded organization URL or environment name. Do not add client secrets or
custom authentication; future tool integrations must use the signed-in Power Apps connection.

## Shell behavior

- RunCSharp is selected initially. The small typed [tool registry](./src/tools/registry.ts) supplies both navigation
  and the host's component mapping. Both empty components remain mounted when switching.
- The Tools menu supports mouse selection, Enter/Space and arrow-key opening, Arrow Up/Down wrapping,
  Home/End, Escape with focus restoration, outside-click dismissal and dismissal when focus leaves.
- Theme initially follows the OS. An explicit choice is stored as `spark-code-tester:v1:theme`.
  Invalid values and unavailable browser storage produce visible warnings; a failed write still changes the
  in-session theme without claiming persistence.
- No toolbars, editors, footers, history, results, execution shortcuts or simulated tool functionality are included.

## Validation

Production build and lint passed. Headless Edge checks covered both selections, harmless reselection,
keyboard/focus behavior, blank workspaces, OS theme changes, persistence across reloads, invalid/blocked
storage, both themes at 1024/1280 px, no horizontal overflow and no custom API requests/browser errors.
The host-integrated authentication workflow was not exercised; nothing was deployed.

The official template's dependency audit reports zero production vulnerabilities. Its development dependency
tree reports 12 moderate advisories associated with `minimatch` and ESLint tooling; no forced breaking dependency
upgrades were applied as part of this shell increment.

## Specifications

- [General specification and delivery status](./specs.md)
- [RunCSharp specification and incremental delivery plan](./run-csharp-specs.md)
- [RenderTemplate design-only specification](./render-template-specs.md)
- [Interactive design mockup](./mockup.html): simulated/layout-only tool content is a design reference, not app functionality.
