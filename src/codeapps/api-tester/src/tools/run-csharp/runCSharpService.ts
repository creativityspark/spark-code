import { Csp_Other_RunCSharpService } from '../../generated/services/Csp_Other_RunCSharpService.ts'

export interface RunCSharpRequest {
  Code: string
}

export interface RunCSharpResponse {
  Outputs: string | null
  Logs: string | null
  CompilerErrors: string | null
  RuntimeErrors: string | null
}

export function getRunCSharpErrorMessage(error: unknown): string {
  if (typeof error === 'string' && error.trim()) return error
  if (typeof error === 'object' && error !== null && 'message' in error
    && typeof error.message === 'string' && error.message.trim()) {
    if ('status' in error && typeof error.status === 'number' && error.status > 0) {
      return `HTTP ${error.status}: ${error.message}`
    }
    return error.message
  }
  return 'The API request failed without a readable error message. See the browser console for details.'
}

function nullableString(data: Record<string, unknown>, property: keyof RunCSharpResponse): string | null {
  const value = data[property]
  // Dataverse can omit null response properties on the wire.
  if (value === undefined || value === null) return null
  if (typeof value !== 'string') {
    throw new Error(`RunCSharp returned an invalid ${property} response property.`)
  }
  return value
}

export async function runCSharp(request: RunCSharpRequest): Promise<RunCSharpResponse> {
  const result = await Csp_Other_RunCSharpService.csp_Other_RunCSharp(request.Code)
  if (!result.success) {
    throw result.error ?? new Error('The RunCSharp API request failed without error details.')
  }
  if (!result.data || typeof result.data !== 'object' || Array.isArray(result.data)) {
    throw new Error('RunCSharp returned an invalid response body.')
  }
  return {
    Outputs: nullableString(result.data, 'Outputs'),
    Logs: nullableString(result.data, 'Logs'),
    CompilerErrors: nullableString(result.data, 'CompilerErrors'),
    RuntimeErrors: nullableString(result.data, 'RuntimeErrors'),
  }
}
