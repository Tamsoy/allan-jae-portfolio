'use client'

import { useCallback, useEffect, useState } from 'react'

export function useThemeTransition() {
  const [isDark, setIsDark] = useState(false)

  useEffect(() => {
    setIsDark(document.documentElement.classList.contains('dark'))
  }, [])

  const toggleTheme = useCallback((origin?: { clientX: number; clientY: number }) => {
    const root = document.documentElement
    const nextIsDark = !root.classList.contains('dark')
    const applyTheme = () => {
      root.classList.toggle('dark', nextIsDark)
      root.classList.toggle('light', !nextIsDark)
      document.cookie = `theme=${nextIsDark ? 'dark' : 'light'}; path=/; max-age=31536000; samesite=lax`
      setIsDark(nextIsDark)
    }
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches || !document.startViewTransition) {
      applyTheme()
      return
    }
    const x = origin?.clientX ?? window.innerWidth - 32
    const y = origin?.clientY ?? 32
    const transition = document.startViewTransition(applyTheme)
    transition.ready.then(() => {
      root.animate({ clipPath: [`circle(0px at ${x}px ${y}px)`, `circle(${Math.hypot(Math.max(x, window.innerWidth - x), Math.max(y, window.innerHeight - y))}px at ${x}px ${y}px)`] }, { duration: 480, easing: 'cubic-bezier(.22,.8,.32,1)', pseudoElement: '::view-transition-new(root)' })
    }).catch(() => undefined)
  }, [])

  return { isDark, toggleTheme }
}
