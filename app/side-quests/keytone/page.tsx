'use client'

import Image from 'next/image'
import Link from 'next/link'
import { useRef, useState } from 'react'
import { ArrowDown, ArrowLeft, ArrowUpRight, Download, Pause, Play } from 'lucide-react'

const previews = [
  { label: 'Blue Switch', detail: 'Clicky press & release', src: '/keytone/blue-preview.wav' },
  { label: 'Red Switch', detail: 'Smooth linear clack', src: '/keytone/red-preview.wav' },
  { label: 'Brown Switch', detail: 'Tactile, mellow clack', src: '/keytone/brown-preview.wav' },
]

const downloads = [
  { name: 'KeyTone Portable', detail: 'Windows ZIP · 1.2.0', href: '/keytone/KeyTone-Portable.zip' },
  { name: 'KeyTone Source', detail: 'C# / XAML · MIT licensed', href: '/keytone/KeyTone-Source.zip' },
]

export default function KeyTonePage() {
  const [playing, setPlaying] = useState<string | null>(null)
  const audioRef = useRef<HTMLAudioElement | null>(null)

  function togglePreview(preview: (typeof previews)[number]) {
    if (!audioRef.current) audioRef.current = new Audio()
    if (playing === preview.label) {
      audioRef.current.pause()
      setPlaying(null)
      return
    }
    audioRef.current.pause()
    audioRef.current.src = preview.src
    audioRef.current.currentTime = 0
    audioRef.current.onended = () => setPlaying(null)
    void audioRef.current.play()
    setPlaying(preview.label)
  }

  return (
    <main>
      <header className="sticky top-0 z-10 border-b bg-background/90 backdrop-blur-md">
        <div className="mx-auto flex max-w-7xl items-center justify-between px-5 py-4 md:px-8">
          <Link href="/" className="flex items-center gap-2 font-mono text-sm font-semibold tracking-tight"><ArrowLeft aria-hidden="true" className="size-4" /> AJ AE</Link>
          <span className="font-mono text-xs uppercase tracking-widest text-muted-foreground">Side Quest 01</span>
        </div>
      </header>

      <section className="mx-auto max-w-7xl px-5 py-16 md:px-8 md:py-28">
        <div className="grid gap-12 md:grid-cols-12 md:items-end md:gap-8">
          <div className="md:col-span-8"><p className="mb-6 font-mono text-xs font-bold uppercase tracking-widest text-muted-foreground">Windows utility · UI design · Sound</p><h1 className="text-[clamp(4rem,11vw,9rem)] font-semibold leading-[0.82] tracking-[-0.075em]">KeyTone</h1></div>
          <p className="max-w-sm leading-relaxed text-muted-foreground md:col-span-4">A tactile sound utility for people who care about how their keyboard feels, sounds, and works.</p>
        </div>
      </section>

      <section className="border-y bg-muted"><div className="mx-auto max-w-7xl px-5 py-8 md:px-8 md:py-12"><div className="overflow-hidden bg-background"><Image src="/keytone/keytone-screenshot.jpg" alt="KeyTone desktop app showing switch profiles, a keyboard map, and a key inspector" width={1536} height={816} priority className="w-full" /></div><p className="mt-4 font-mono text-xs uppercase tracking-widest text-muted-foreground">KeyTone / desktop interface preview</p></div></section>

      <section className="mx-auto grid max-w-7xl gap-12 px-5 py-20 md:grid-cols-12 md:px-8 md:py-28"><div className="md:col-span-3"><p className="font-mono text-xs font-bold uppercase tracking-widest text-muted-foreground">The idea</p></div><div className="flex flex-col gap-10 md:col-span-8 md:col-start-5"><p className="text-3xl font-medium leading-tight tracking-tight md:text-5xl">Make every key your own, without making the interface feel like work.</p><div className="grid gap-8 text-muted-foreground sm:grid-cols-2"><p className="leading-relaxed">KeyTone maps physical keys to recorded switch sounds, then lets you tune the character of individual keys through a compact inspector.</p><p className="leading-relaxed">The project is a focused Windows utility with scan-code mapping, press and release events, pitch and tone controls, variation, and a lightweight tray workflow.</p></div></div></section>

      <section className="border-t"><div className="mx-auto max-w-7xl px-5 py-20 md:px-8 md:py-28"><div className="mb-12 flex items-end justify-between"><div><p className="mb-4 font-mono text-xs uppercase tracking-widest text-muted-foreground">Recorded previews</p><h2 className="text-3xl font-semibold tracking-tight md:text-5xl">Listen</h2></div><Play aria-hidden="true" className="size-5" /></div><div className="grid gap-4 md:grid-cols-3">{previews.map((preview) => { const isPlaying = playing === preview.label; return <button key={preview.label} type="button" onClick={() => togglePreview(preview)} aria-label={`${isPlaying ? 'Pause' : 'Play'} ${preview.label} preview`} className="flex min-h-40 flex-col justify-between border p-5 text-left transition-colors hover:bg-muted"><span className="font-mono text-xs uppercase tracking-widest text-muted-foreground">{isPlaying ? 'Playing' : 'Preview'}</span><span className="flex items-center justify-between gap-4 font-medium">{preview.label}<span className="flex flex-col items-end gap-1 text-right"><span className="text-sm">{preview.detail}</span>{isPlaying ? <Pause aria-hidden="true" className="size-4" /> : <Play aria-hidden="true" className="size-4" />}</span></span></button> })}</div><p className="mt-5 text-sm text-muted-foreground">Click a card to hear the bundled switch recording. Audio is attributed to MechvibesDX in the source notices.</p></div></section>

      <section className="border-t bg-primary text-primary-foreground"><div className="mx-auto max-w-7xl px-5 py-20 md:px-8 md:py-28"><div className="mb-12 flex items-end justify-between"><div><p className="mb-4 font-mono text-xs uppercase tracking-widest text-primary-foreground/60">Project files</p><h2 className="text-3xl font-semibold tracking-tight md:text-5xl">Download</h2></div><ArrowDown aria-hidden="true" className="size-5" /></div><div className="grid gap-3 md:grid-cols-2">{downloads.map((item) => <a key={item.name} href={item.href} download className="group flex items-center justify-between border border-primary-foreground/25 p-5 transition-colors hover:bg-primary-foreground hover:text-primary"><span className="flex flex-col gap-2"><span className="font-medium">{item.name}</span><span className="font-mono text-xs uppercase tracking-widest opacity-60">{item.detail}</span></span><Download aria-hidden="true" className="size-5 transition-transform group-hover:translate-y-1" /></a>)}</div><p className="mt-5 max-w-2xl text-sm text-primary-foreground/60">KeyTone is unsigned and currently distributed for Windows. Review the included licenses and readme before using or building it.</p></div></section>

      <footer className="border-t"><div className="mx-auto flex max-w-7xl items-center justify-between px-5 py-8 md:px-8"><Link href="/" className="font-mono text-xs uppercase tracking-widest">Back to portfolio</Link><a href="#top" className="font-mono text-xs uppercase tracking-widest">Top <ArrowUpRight aria-hidden="true" className="inline size-3" /></a></div></footer>
    </main>
  )
}
