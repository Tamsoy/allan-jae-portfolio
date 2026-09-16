'use client'

import { Moon, Sun, Volume2, VolumeX } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { useSoundFeedback } from '@/components/sound-feedback'
import { useThemeTransition } from '@/components/theme-transition'

export function ThemeToggle() {
  const { isDark, toggleTheme } = useThemeTransition()
  const { enabled, toggleSound } = useSoundFeedback()

  return (
    <div className="flex items-center gap-2">
      <Button
        type="button"
        variant="outline"
        size="icon"
        onClick={(event) => toggleTheme({ clientX: event.clientX, clientY: event.clientY })}
        aria-label={isDark ? 'Switch to light theme' : 'Switch to dark theme'}
        title={isDark ? 'Switch to light theme' : 'Switch to dark theme'}
        className="theme-control rounded-full"
      >
        {isDark ? <Sun className="size-4" aria-hidden="true" /> : <Moon className="size-4" aria-hidden="true" />}
      </Button>
      <Button
        type="button"
        variant="outline"
        size="icon"
        onClick={toggleSound}
        aria-label={enabled ? 'Mute interface sounds' : 'Enable interface sounds'}
        title={enabled ? 'Mute interface sounds' : 'Enable interface sounds'}
        className="theme-control rounded-full"
      >
        {enabled ? <Volume2 className="size-4" aria-hidden="true" /> : <VolumeX className="size-4" aria-hidden="true" />}
      </Button>
    </div>
  )
}
