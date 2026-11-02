import { version } from '../../package.json'
import { dataverseEnvironment } from '../services/dataverseConnection'
import type { Theme } from '../state/appPreferences'
import { tools } from '../tools/registry'
import type { ToolId } from '../tools/registry'
import { ToolSelector } from './ToolSelector'

interface AppHeaderProps {
  activeToolId: ToolId
  onSelectTool: (toolId: ToolId) => void
  theme: Theme
  onToggleTheme: () => void
  storageWarning: string | null
}

export function AppHeader({
  activeToolId,
  onSelectTool,
  theme,
  onToggleTheme,
  storageWarning,
}: AppHeaderProps) {
  const selectedTool = tools.find((tool) => tool.id === activeToolId)!

  return (
    <header className="app-header">
      <div className="header-row">
        <ToolSelector activeToolId={activeToolId} onSelectTool={onSelectTool} />
        <h1 className="app-title">
          <span className="app-mark" aria-hidden="true">SC</span>
          Spark Code Tester <span className="api-title">· {selectedTool.label}</span>
        </h1>
        <span className="version">v{version}</span>
        <span
          className="environment"
          title={dataverseEnvironment.id
            ? `Configured environment: ${dataverseEnvironment.id}. Live API connectivity is not tested by this scaffold.`
            : 'Initialize local Power Apps configuration to select your environment. No live API connectivity has been verified.'}
        >
          {dataverseEnvironment.displayName}
        </span>
        <button
          type="button"
          className="button theme-toggle"
          aria-pressed={theme === 'dark'}
          onClick={onToggleTheme}
          title={`Current theme: ${theme}. Switch to ${theme === 'dark' ? 'light' : 'dark'} mode.`}
        >
          Dark Mode
        </button>
      </div>
      {storageWarning && <p className="storage-warning" role="alert">{storageWarning}</p>}
    </header>
  )
}
