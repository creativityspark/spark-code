import { useEffect, useState } from 'react'
import { AppHeader } from './components/AppHeader'
import { tools } from './tools/registry'
import type { ToolId } from './tools/registry'
import { useAppPreferences } from './state/appPreferences'
import './App.css'

function App() {
  const [activeToolId, setActiveToolId] = useState<ToolId>('run-csharp')
  const activeTool = tools.find((tool) => tool.id === activeToolId)!
  const { theme, toggleTheme, storageWarning } = useAppPreferences()

  useEffect(() => {
    document.title = `Spark Code Tester · ${activeTool.label}`
  }, [activeTool.label])

  return (
    <div className="app-shell">
      <AppHeader
        activeToolId={activeToolId}
        onSelectTool={setActiveToolId}
        theme={theme}
        onToggleTheme={toggleTheme}
        storageWarning={storageWarning}
      />
      <main className="tool-host" aria-label={`${activeTool.label} workspace`}>
        {tools.map(({ id, Component }) => (
          <section key={id} hidden={id !== activeToolId}>
            <Component />
          </section>
        ))}
      </main>
    </div>
  )
}

export default App
