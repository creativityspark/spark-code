import { useRef, useState } from 'react'
import { getRunCSharpErrorMessage, runCSharp } from './runCSharpService'

type RunState =
  | { status: 'idle' | 'running' }
  | { status: 'success'; output: string | null }
  | { status: 'error'; message: string }

export function RunCSharpTool() {
  const [code, setCode] = useState('output = "hello";')
  const [run, setRun] = useState<RunState>({ status: 'idle' })
  const inFlight = useRef(false)

  async function executeCode() {
    if (inFlight.current) return
    if (!code.trim()) {
      setRun({ status: 'error', message: 'Enter Code before running.' })
      return
    }

    inFlight.current = true
    setRun({ status: 'running' })
    try {
      const result = await runCSharp({ Code: code })
      if (result.CompilerErrors || result.RuntimeErrors) {
        const diagnostics = [
          result.CompilerErrors && `Compiler errors: ${result.CompilerErrors}`,
          result.RuntimeErrors && `Runtime errors: ${result.RuntimeErrors}`,
        ].filter(Boolean).join('\n')
        setRun({ status: 'error', message: diagnostics })
      } else {
        setRun({ status: 'success', output: result.Outputs })
      }
    } catch (error) {
      console.error('RunCSharp API request failed.', error)
      setRun({
        status: 'error',
        message: getRunCSharpErrorMessage(error),
      })
    } finally {
      inFlight.current = false
    }
  }

  return (
    <div className="run-csharp-tool">
      <div className="run-csharp-toolbar">
        <button
          type="button"
          className="button run-csharp-run"
          disabled={run.status === 'running'}
          onClick={() => { void executeCode() }}
        >
          {run.status === 'running' ? 'Running...' : 'Run'}
        </button>
        <p role="status" aria-atomic="true">
          {run.status === 'idle' && 'Edit Code, then select Run. No code runs automatically.'}
          {run.status === 'running' && 'Running Code through the Power Apps connection...'}
          {run.status === 'success' && (run.output === null
            ? 'Run completed. No output was returned.'
            : run.output === '' ? 'Run completed. Output is an empty string.' : 'Run completed.')}
        </p>
      </div>
      <div className="run-csharp-workspace">
        <div className="run-csharp-field">
          <label htmlFor="run-csharp-code">Code</label>
          <textarea
            id="run-csharp-code"
            value={code}
            onChange={(event) => { setCode(event.target.value) }}
            disabled={run.status === 'running'}
            spellCheck={false}
            autoCapitalize="off"
            autoCorrect="off"
          />
        </div>
        <div className="run-csharp-field">
          <label htmlFor="run-csharp-output">String output</label>
          <textarea
            id="run-csharp-output"
            value={run.status === 'success' ? run.output ?? '' : ''}
            readOnly
            spellCheck={false}
          />
        </div>
      </div>
      {run.status === 'error' && <p className="run-csharp-error" role="alert">{run.message}</p>}
    </div>
  )
}
