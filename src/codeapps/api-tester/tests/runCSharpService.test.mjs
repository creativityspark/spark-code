import assert from 'node:assert/strict'
import { mock, test } from 'node:test'

const execute = mock.fn()
mock.module('../src/generated/services/Csp_Other_RunCSharpService.ts', {
  namedExports: {
    Csp_Other_RunCSharpService: { csp_Other_RunCSharp: execute },
  },
})
const { getRunCSharpErrorMessage, runCSharp } = await import('../src/tools/run-csharp/runCSharpService.ts')

test('sends only Code and returns validated response fields', async () => {
  execute.mock.resetCalls()
  execute.mock.mockImplementation(async () => ({
    success: true,
    data: { Outputs: 'hello', Logs: 'message', CompilerErrors: null },
  }))
  assert.deepEqual(await runCSharp({ Code: 'output = "hello";' }), {
    Outputs: 'hello',
    Logs: 'message',
    CompilerErrors: null,
    RuntimeErrors: null,
  })
  assert.deepEqual(execute.mock.calls[0].arguments, ['output = "hello";'])
})

test('preserves script diagnostic strings as normal API responses', async () => {
  const diagnostic = '[{"message":"Script failed","section":"Code","line":null}]'
  execute.mock.mockImplementation(async () => ({
    success: true,
    data: { Outputs: null, Logs: 'before failure', RuntimeErrors: diagnostic },
  }))
  const result = await runCSharp({ Code: 'script' })
  assert.equal(result.RuntimeErrors, diagnostic)
  assert.equal(result.Logs, 'before failure')
  assert.equal(result.Outputs, null)
})

test('propagates SDK failures and rejected requests without a success fallback', async () => {
  const fault = new Error('Access denied')
  execute.mock.mockImplementation(async () => ({ success: false, error: fault }))
  await assert.rejects(runCSharp({ Code: 'script' }), error => error === fault)
  execute.mock.mockImplementation(async () => { throw fault })
  await assert.rejects(runCSharp({ Code: 'script' }), error => error === fault)
  execute.mock.mockImplementation(async () => ({ success: false }))
  await assert.rejects(runCSharp({ Code: 'script' }), /failed without error details/)
})

test('rejects malformed response bodies and non-string response properties', async () => {
  for (const data of [null, [], 'hello', { Outputs: 123 }, { RuntimeErrors: [] }]) {
    execute.mock.mockImplementation(async () => ({ success: true, data }))
    await assert.rejects(runCSharp({ Code: 'script' }), /invalid/)
  }
})

test('displays the message and HTTP status from a plain SDK error object', async () => {
  const fault = { message: 'Access denied', status: 403, requestId: 'private-request-id' }
  execute.mock.mockImplementation(async () => ({ success: false, error: fault }))
  await assert.rejects(runCSharp({ Code: 'script' }), error => {
    assert.equal(getRunCSharpErrorMessage(error), 'HTTP 403: Access denied')
    return true
  })
  execute.mock.mockImplementation(async () => { throw { message: 'Host connection unavailable' } })
  await assert.rejects(runCSharp({ Code: 'script' }), error => {
    assert.equal(getRunCSharpErrorMessage(error), 'Host connection unavailable')
    return true
  })
})

test('handles Error/string/unknown failures without object coercion or dumping private fields', () => {
  assert.equal(getRunCSharpErrorMessage(new Error('Timed out')), 'Timed out')
  assert.equal(getRunCSharpErrorMessage('Request failed'), 'Request failed')
  for (const error of [null, undefined, {}, { message: {}, requestId: 'private-request-id' }, '']) {
    assert.equal(
      getRunCSharpErrorMessage(error),
      'The API request failed without a readable error message. See the browser console for details.',
    )
  }
})
