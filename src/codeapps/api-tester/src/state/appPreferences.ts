import { useEffect, useState } from 'react'

export type Theme = 'light' | 'dark'
const themeKey = 'spark-code-tester:v1:theme'
const systemThemeQuery = '(prefers-color-scheme: dark)'

function readPreference(): { preference: Theme | null; warning: string | null } {
  try {
    const stored = localStorage.getItem(themeKey)
    if (stored === null || stored === 'light' || stored === 'dark') {
      return { preference: stored, warning: null }
    }
    return {
      preference: null,
      warning: 'The saved theme preference is invalid. Using the system theme until you choose a theme.',
    }
  } catch (error) {
    console.error('Unable to read the theme preference.', error)
    return {
      preference: null,
      warning: 'Browser storage is unavailable. Theme changes apply only to this session.',
    }
  }
}

export function useAppPreferences() {
  const [settings, setSettings] = useState(readPreference)
  const [systemTheme, setSystemTheme] = useState<Theme>(
    () => window.matchMedia(systemThemeQuery).matches ? 'dark' : 'light',
  )
  const theme = settings.preference ?? systemTheme

  useEffect(() => {
    const query = window.matchMedia(systemThemeQuery)
    const updateSystemTheme = (event: MediaQueryListEvent) => {
      setSystemTheme(event.matches ? 'dark' : 'light')
    }
    query.addEventListener('change', updateSystemTheme)
    return () => query.removeEventListener('change', updateSystemTheme)
  }, [])

  useEffect(() => {
    document.documentElement.dataset.theme = theme
    document.documentElement.style.colorScheme = theme
  }, [theme])

  function toggleTheme() {
    const preference: Theme = theme === 'dark' ? 'light' : 'dark'
    try {
      localStorage.setItem(themeKey, preference)
      setSettings({ preference, warning: null })
    } catch (error) {
      console.error('Unable to save the theme preference.', error)
      setSettings({
        preference,
        warning: 'Theme preference could not be saved. This change applies only to this session.',
      })
    }
  }

  return { theme, toggleTheme, storageWarning: settings.warning }
}
