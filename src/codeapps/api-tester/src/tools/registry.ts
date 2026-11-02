import type { ComponentType } from 'react'
import { RunCSharpTool } from './run-csharp/RunCSharpTool'
import { RenderTemplateTool } from './render-template/RenderTemplateTool'

export type ToolId = 'run-csharp' | 'render-template'

interface ToolDefinition {
  id: ToolId
  label: string
  Component: ComponentType
}

export const tools: readonly ToolDefinition[] = [
  { id: 'run-csharp', label: 'RunCSharp', Component: RunCSharpTool },
  { id: 'render-template', label: 'RenderTemplate', Component: RenderTemplateTool },
]
