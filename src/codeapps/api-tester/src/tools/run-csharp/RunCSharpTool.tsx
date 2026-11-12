import { useState } from 'react'
import { getRunCSharpErrorMessage, runCSharp } from './runCSharpService'

type ConnectionCheck =
  | { status: 'idle' | 'checking' | 'success' }
  | { status: 'error'; message: string }

export function RunCSharpTool() {
  const [check, setCheck] = useState<ConnectionCheck>({ status: 'idle' })

  async function checkConnection() {
    if (check.status === 'checking') return
    setCheck({ status: 'checking' })
    try {
      const result = await runCSharp({ Code: 'output = "hello";' })
      if (result.CompilerErrors || result.RuntimeErrors) {
        throw new Error(`The API responded with script diagnostics: ${result.CompilerErrors ?? result.RuntimeErrors}`)
      }
      if (result.Outputs !== 'hello') {
        throw new Error(`The API responded, but expected "hello" and received ${JSON.stringify(result.Outputs)}.`)
      }
      setCheck({ status: 'success' })
    } catch (error) {
      console.error('RunCSharp API connection check failed.', error)
      setCheck({
        status: 'error',
        message: getRunCSharpErrorMessage(error),
      })
    }
  }

  return (
    <div className="connection-check">
      <h2>API connection check</h2>
      <p>Run a fixed hello script through the signed-in Power Apps connection. No code runs until you click.</p>
      <button
        type="button"
        className="button"
        disabled={check.status === 'checking'}
        onClick={() => { void checkConnection() }}
      >
        {check.status === 'checking' ? 'Checking API connection...' : 'Check API connection'}
      </button>
      <p role="status" aria-atomic="true">
        {check.status === 'idle' && 'API connection has not been checked. Open this app in Power Apps Local Play to verify it.'}
        {check.status === 'checking' && 'Running the hello verification script...'}
        {check.status === 'success' && 'API connection works. RunCSharp returned "hello".'}
      </p>
      {check.status === 'error' && <p role="alert">API connection check failed: {check.message}</p>}
    </div>
  )
}
