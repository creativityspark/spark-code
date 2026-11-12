# Liquid Templates

## What is a Liquid template?

[Liquid](https://shopify.github.io/liquid/) is a simple, safe templating
language that mixes plain text with `{{ expression }}` output tags and
`{% tag %}` logic tags (conditionals, loops, assignments, etc.). This
repository renders Liquid templates using [Fluid](https://github.com/sebastienros/fluid),
a .NET implementation of the Liquid language.

A basic template looks like this:

```liquid
Hello {{ name }},
{% if orders.size > 0 %}
You have {{ orders.size }} order(s).
{% else %}
You have no orders yet.
{% endif %}
```

When rendered with a context such as `{"name": "Cris", "orders": [1, 2]}`,
the output is:

```
Hello Cris,

You have 2 order(s).
```

## Using Liquid templates with our custom APIs

Three custom APIs in the **Templates** group render Liquid templates. They
differ in where the template text comes from and where the context values
come from:

| API | Template source | Context source |
|---|---|---|
| `RenderTemplate` | `Template` input parameter | `Context` JSON input parameter |
| `RenderDataverseTemplate` | `Template` input parameter | Optional Dataverse record + optional additional JSON context |
| `RenderWebResourceTemplate` | Dataverse web resource content | Optional Dataverse record + optional additional JSON context |

All three return the rendered text in a `Results` output parameter, and can
be called the same way as any other custom API (Power Automate, Canvas
Apps/Pages, or classic workflows — see the main [README](./README.md)).

> `GetFrontMatter` is a related, but separate, helper API: it does not render
> Liquid, it only extracts a YAML front matter block (delimited by `---`)
> from a text input and returns it alongside the remaining body. It is used
> internally by `RenderWebResourceTemplate` to separate metadata from the
> template body, and can also be called directly.

### RenderTemplate

Renders a Liquid template using a plain JSON object as the context. This is
the simplest option when you already have all the data you need as JSON and
don't need Dataverse-aware special tags (see below — they are **not**
available in this API).

**Example**

- `Template`: `"Hello {{ name }}"`
- `Context`: `"{\"name\":\"Cris\"}"`
- `Results` (output): `"Hello Cris"`

### RenderDataverseTemplate

Renders a Liquid template, automatically building the context from an
optional Dataverse record (by `RecordId` + `RecordType`) plus an optional
`AdditionalContext` JSON object merged on top. Only the columns referenced
by identifiers used in the template are retrieved from the record. This API
also registers all the [special tags](#special-tags) below.

**Example**

- `Template`: `"Hello {{ firstname }} {{ lastname }}!"`
- `RecordId`: `"3f1b2c...": contact GUID`
- `RecordType`: `"contact"`
- `Results` (output): `"Hello Jane Doe!"`

### RenderWebResourceTemplate

Same context model as `RenderDataverseTemplate`, but the template text is
loaded from a Dataverse web resource instead of being passed inline. Useful
for larger or reusable templates stored/managed in Dataverse. If the web
resource content starts with a YAML front matter block, it is parsed and
rendered separately (its string values are also run through the Liquid
renderer), and returned in the `FrontMatter` / `FrontMatterJson` outputs.
This API also registers all the [special tags](#special-tags) below.

**Example**

- `WebResourceName`: `"csp_/templates/welcome.liquid"` containing:

  ```liquid
  ---
  subject: Welcome {{ firstname }}
  ---
  Hello {{ firstname }} {{ lastname }}!
  ```

- `RecordId`: contact GUID
- `RecordType`: `"contact"`
- `Results` (output): `"Hello Jane Doe!"`
- `FrontMatter` / `FrontMatterJson` (output): `{ "subject": "Welcome Jane" }`

## Date filters

All three rendering APIs support the Power Pages date filters. The `date`
filter formats a date/time value with a .NET date and time format string.
Date-add filters accept positive or negative values; days, hours, minutes,
and seconds also accept fractional values. Months and years require whole
numbers. The date format uses the template culture, which defaults to the
invariant culture.

```liquid
{{ createdon | date: 'MMMM dd, yyyy' }}
{{ createdon | date_add_days: -2.5 | date: 'g' }}
{{ createdon | date_add_months: 1 }}
{{ createdon | date_to_iso8601 }}
{{ createdon | date_to_rfc822 }}
```

The supported filters are:

| Filter | Behavior |
|---|---|
| `date` | Formats a date/time value using a .NET date/time format string. |
| `date_add_days` | Adds whole or fractional days. |
| `date_add_hours` | Adds whole or fractional hours. |
| `date_add_minutes` | Adds whole or fractional minutes. |
| `date_add_months` | Adds a whole number of months. |
| `date_add_seconds` | Adds whole or fractional seconds. |
| `date_add_years` | Adds a whole number of years. |
| `date_to_iso8601` | Formats the value as ISO 8601 in UTC, for example `2024-01-15T12:30:00Z`. |
| `date_to_rfc822` | Formats the value as RFC 822 in UTC, for example `Mon, 07 May 2018 07:20:46 Z`. |

The ISO 8601 and RFC 822 output examples match the formats shown in the
[Microsoft Learn date-filter reference](https://learn.microsoft.com/en-us/power-pages/configure/liquid/liquid-filters#date-filters).

## Special tags

`RenderDataverseTemplate` and `RenderWebResourceTemplate` register a set of
custom Liquid tags that give templates access to Dataverse-specific data.
These tags are **not** available in `RenderTemplate`, since it has no
Dataverse service context.

### `_envVar`

Outputs the value of a Dataverse environment variable, given its schema
name.

```liquid
{% _envVar new_MyEnvironmentVariable %}
```

### `_orgDetails`

Outputs an attribute of the current organization's details (e.g.
`FriendlyName`, `OrganizationId`).

```liquid
{% _orgDetails FriendlyName %}
```

### `_orgUrl`

Outputs the base URL of the current Dataverse organization. Takes no
arguments.

```liquid
{% _orgUrl %}
```

### `_appUrl`

Outputs the URL to open a canvas app or model-driven app, given its name
(canvas app) or unique name (model-driven app).

```liquid
{% _appUrl my_canvas_app_name %}
```

### `_appName`

Outputs the display name of a canvas app or model-driven app, given its name
or unique name.

```liquid
{% _appName my_canvas_app_name %}
```

### `_webResource`

Inlines the (decoded) content of another Dataverse web resource, given its
name as a string expression. Useful for shared partials (headers, footers,
styles).

```liquid
{% _webResource 'csp_/templates/header.liquid' %}
```

### `fetchxml` ... `endfetchxml`

A block tag that mirrors the Power Pages `fetchxml` Liquid tag. The body is
first rendered as Liquid (so `{{ }}` expressions inside the query, like a
record id, are resolved), then executed as a FetchXML query against
Dataverse. The result is assigned to the given variable name and can be used
later in the template. The result object exposes `xml` (the executed query)
and `results` (with `entities`, `entityName`, `totalRecordCount`,
`totalRecordCountLimitExceeded`, `moreRecords`, `pagingCookie`, and
`minActiveRowVersion`).

```liquid
{%- fetchxml accounts -%}
<fetch>
  <entity name="account">
    <attribute name="name" />
    <filter>
      <condition attribute="primarycontactid" operator="eq" value="{{ RecordId }}" />
    </filter>
  </entity>
</fetch>
{%- endfetchxml -%}

{% for account in accounts.results.entities %}
- {{ account.name }}
{% endfor %}
```
