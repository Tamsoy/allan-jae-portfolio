'use client'

import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState } from 'react'

type SoundContextValue = {
  enabled: boolean
  toggleSound: () => void
  playClick: () => void
  playHover: () => void
}

const SoundContext = createContext<SoundContextValue | null>(null)

function readPreference() {
  if (typeof document === 'undefined') return false
  return document.cookie.match(/(?:^|; )sound=([^;]*)/)?.[1] === 'on'
}

function writePreference(enabled: boolean) {
  document.cookie = `sound=${enabled ? 'on' : 'off'}; path=/; max-age=31536000; samesite=lax`
}

export function SoundFeedbackProvider({ children }: Readonly<{ children: React.ReactNode }>) {
  const [enabled, setEnabled] = useState(false)
  const contextRef = useRef<AudioContext | null>(null)

  useEffect(() => {
    setEnabled(readPreference())
  }, [])

  const getContext = useCallback(() => {
    if (!contextRef.current) {
      const AudioContextClass = window.AudioContext || (window as typeof window & { webkitAudioContext?: typeof AudioContext }).webkitAudioContext
      if (!AudioContextClass) return null
      contextRef.current = new AudioContextClass()
    }
    if (contextRef.current.state === 'suspended') void contextRef.current.resume()
    return contextRef.current
  }, [])

  const playTone = useCallback((frequency: number, duration: number, volume: number) => {
    if (!enabled) return
    const context = getContext()
    if (!context) return
    const oscillator = context.createOscillator()
    const gain = context.createGain()
    oscillator.type = 'sine'
    oscillator.frequency.setValueAtTime(frequency, context.currentTime)
    gain.gain.setValueAtTime(0.0001, context.currentTime)
    gain.gain.exponentialRampToValueAtTime(volume, context.currentTime + 0.008)
    gain.gain.exponentialRampToValueAtTime(0.0001, context.currentTime + duration)
    oscillator.connect(gain).connect(context.destination)
    oscillator.start()
    oscillator.stop(context.currentTime + duration + 0.02)
  }, [enabled, getContext])

  const toggleSound = useCallback(() => {
    const next = !enabled
    setEnabled(next)
    writePreference(next)
    if (next) {
      const context = getContext()
      if (context) playTone(520, 0.1, 0.025)
    }
  }, [enabled, getContext, playTone])

  useEffect(() => {
    const handlePointerOver = (event: PointerEvent) => {
      if (!enabled || event.pointerType === 'touch') return
      const target = event.target as HTMLElement | null
      if (target?.closest('a, button, [data-sound-hover]')) playTone(330, 0.045, 0.012)
    }
    const handleClick = (event: MouseEvent) => {
      if (!enabled) return
      const target = event.target as HTMLElement | null
      if (target?.closest('a, button, [data-sound-click]')) playTone(520, 0.07, 0.018)
    }
    document.addEventListener('pointerover', handlePointerOver, { passive: true })
    document.addEventListener('click', handleClick, { passive: true })
    return () => {
      document.removeEventListener('pointerover', handlePointerOver)
      document.removeEventListener('click', handleClick)
      contextRef.current?.close()
      contextRef.current = null
    }
  }, [enabled, playTone])

  const value = useMemo(() => ({ enabled, toggleSound, playClick: () => playTone(520, 0.07, 0.018), playHover: () => playTone(330, 0.045, 0.012) }), [enabled, playTone, toggleSound])
  return <SoundContext.Provider value={value}>{children}</SoundContext.Provider>
}

export function useSoundFeedback() {
  const context = useContext(SoundContext)
  if (!context) throw new Error('useSoundFeedback must be used inside SoundFeedbackProvider')
  return context
}
