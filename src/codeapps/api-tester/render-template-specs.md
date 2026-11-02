# RenderTemplate Testing Tool - Specifications

## 1. Overview and scope

**RenderTemplate** is the second example testing tool in **Spark Code Tester**.
The [general specification](./specs.md) owns the shared header, selection, environment, theme and lifecycle.
See [mockup.html](./mockup.html) and the first [RunCSharp tool](./run-csharp-specs.md).

**This iteration is a layout-only placeholder.** It does not render Liquid, simulate a successful request or call
Dataverse. Its disabled Run action and placeholder notice explicitly disclose that limitation.

## 2. Verified API contract

- **API:** `csp_Templates_RenderTemplate` (unbound action).
- **Plugin:** [RenderTemplate.cs](../../assemblies/SparkCode.API/Templates/RenderTemplate.cs).

| Parameter | Direction | Type | Required | Intended region |
|---|---|---|---|---|
| `Template` | Input | string (Liquid) | Yes | Template editor |
| `Context` | Input | string (JSON) | Yes | Context editor |
| `Results` | Output | string | N/A | Rendered text viewer |

Example for a future real invocation: Template `Hello {{name}}`, Context `{"name":"Cris"}`, Results `Hello Cris`.
Do not present that expected result as an executed result in the placeholder.
This API does not expose RunCSharp's `Outputs`, `Logs`, `CompilerErrors`, `RuntimeErrors`, Types or execution options.

## 3. Placeholder layout and behavior

```
Shared header: Tools | Spark Code Tester · RenderTemplate | Environment | Dark Mode
Tool action row: Render a Liquid template | Layout preview | Run (disabled)
Tool workspace:
  Template (Liquid)                Results (rendered text)
  Context (JSON)                   Not executed - layout preview only
Tool footer: Layout preview - execution not implemented | csp_Templates_RenderTemplate
```

- Show labelled, read-only representative Template and Context fields. They communicate request structure,
  not fully implemented editing/validation behavior.
- Show an empty Results region with an explicit not-executed message, not RunCSharp's output explorer.
- Do not include RunCSharp Samples, Options, history, Problems, Logs, type names or status.
- Run remains disabled. Ctrl/Cmd+Enter does not execute this or another tool.
- The shared theme applies to all placeholder surfaces. Switching preserves each tool's in-memory state,
  including the RunCSharp state, without copying results or statuses between them.
- No history or browser-storage payload is created for this layout-only placeholder.

## 4. Acceptance criteria

1. Selecting RenderTemplate updates the shared title and shows its own action row, workspace and footer.
2. Template/Context/Results labels match the verified contract and required input types.
3. The layout preview notice, disabled Run and not-executed output make its nonfunctional status clear.
4. No RunCSharp-specific controls or footer are visible; keyboard run cannot trigger the hidden RunCSharp tool.
5. Shared theme switching works, and returning to RunCSharp does not reset that tool.

## 5. Delivery boundary

Deliver the layout example and this specification only. Actual execution, validation, sample loading, output
inspection, failure UX and any history/storage schema need a subsequent approved functional specification.
Use the shared Dataverse connection and verified action contract when that work is authorized; never reuse the
RunCSharp response parser for RenderTemplate.
