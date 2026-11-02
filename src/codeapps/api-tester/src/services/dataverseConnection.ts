const configurations = import.meta.glob<{ environmentId: string }>(
  '../../power.config.json',
  { eager: true, import: 'default' },
)
const environmentId = configurations['../../power.config.json']?.environmentId.trim() || null

// Configuration identifies the target; it does not assert a live SDK connection.
export const dataverseEnvironment = {
  id: environmentId,
  displayName: environmentId ? 'Environment configured' : 'Environment not configured',
}
