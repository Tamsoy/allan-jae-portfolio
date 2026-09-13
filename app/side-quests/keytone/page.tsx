import Image from 'next/image'
import Link from 'next/link'
import { ArrowDown, ArrowLeft, ArrowUpRight, Download, Play } from 'lucide-react'

const downloads = [
  { name: 'KeyTone web files', detail: 'HTML · CSS · JavaScript', href: '/downloads/keytone-web-assets.zip' },
  { name: 'Switch sound previews', detail: 'WAV · 48 kHz', href: '/downloads/keytone-audio-pack.zip' },
]

export default function KeyTonePage() {
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
          <div className="md:col-span-8">
            <p className="mb-6 font-mono text-xs font-bold uppercase tracking-widest text-muted-foreground">Web audio · UI design · Sound</p>
            <h1 className="text-[clamp(4rem,11vw,9rem)] font-semibold leading-[0.82] tracking-[-0.075em]">KeyTone</h1>
          </div>
          <p className="max-w-sm leading-relaxed text-muted-foreground md:col-span-4">A tactile sound library for people who care about how their keyboard feels, sounds, and works.</p>
        </div>
      </section>

      <section className="border-y bg-muted">
        <div className="mx-auto max-w-7xl px-5 py-8 md:px-8 md:py-12">
          <div className="overflow-hidden bg-background">
            <Image src="/images/atlas-project.png" alt="KeyTone project interface showing a curated keyboard sound library" width={1600} height={1000} priority className="aspect-video w-full object-cover" />
          </div>
          <p className="mt-4 font-mono text-xs uppercase tracking-widest text-muted-foreground">KeyTone / desktop interface preview</p>
        </div>
      </section>

      <section className="mx-auto grid max-w-7xl gap-12 px-5 py-20 md:grid-cols-12 md:px-8 md:py-28">
        <div className="md:col-span-3"><p className="font-mono text-xs font-bold uppercase tracking-widest text-muted-foreground">The idea</p></div>
        <div className="flex flex-col gap-10 md:col-span-8 md:col-start-5">
          <p className="text-3xl font-medium leading-tight tracking-tight md:text-5xl">Typing is an everyday interaction. KeyTone makes the small details of that interaction easier to notice.</p>
          <div className="grid gap-8 text-muted-foreground sm:grid-cols-2">
            <p className="leading-relaxed">The project started as a simple way to compare mechanical switch sounds. It grew into a focused interface for listening, comparing, and deciding.</p>
            <p className="leading-relaxed">The visual system stays quiet so the audio can lead: strong typography, clear controls, and a deliberately restrained palette.</p>
          </div>
        </div>
      </section>

      <section className="border-t">
        <div className="mx-auto max-w-7xl px-5 py-20 md:px-8 md:py-28">
          <div className="mb-12 flex items-end justify-between"><h2 className="text-3xl font-semibold tracking-tight md:text-5xl">Listen</h2><Play aria-hidden="true" className="size-5" /></div>
          <div className="grid gap-4 md:grid-cols-3">
            {['Linear / soft press', 'Tactile / crisp bump', 'Clicky / bright release'].map((label) => (
              <button key={label} type="button" className="flex min-h-40 flex-col justify-between border p-5 text-left transition-colors hover:bg-muted">
                <span className="font-mono text-xs uppercase tracking-widest text-muted-foreground">Preview</span>
                <span className="flex items-center justify-between gap-4 font-medium">{label}<Play aria-hidden="true" className="size-4" /></span>
              </button>
            ))}
          </div>
          <p className="mt-5 text-sm text-muted-foreground">Audio previews will play here when the supplied KeyTone audio pack is added.</p>
        </div>
      </section>

      <section className="border-t bg-primary text-primary-foreground">
        <div className="mx-auto max-w-7xl px-5 py-20 md:px-8 md:py-28">
          <div className="mb-12 flex items-end justify-between"><h2 className="text-3xl font-semibold tracking-tight md:text-5xl">Download</h2><ArrowDown aria-hidden="true" className="size-5" /></div>
          <div className="grid gap-3 md:grid-cols-2">
            {downloads.map((item) => (
              <a key={item.name} href={item.href} download className="group flex items-center justify-between border border-primary-foreground/25 p-5 transition-colors hover:bg-primary-foreground hover:text-primary">
                <span className="flex flex-col gap-2"><span className="font-medium">{item.name}</span><span className="font-mono text-xs uppercase tracking-widest opacity-60">{item.detail}</span></span>
                <Download aria-hidden="true" className="size-5 transition-transform group-hover:translate-y-1" />
              </a>
            ))}
          </div>
          <p className="mt-5 text-sm text-primary-foreground/60">The download links are ready for the matching files from KeyTone-Web-Assets.zip.</p>
        </div>
      </section>

      <footer className="border-t"><div className="mx-auto flex max-w-7xl items-center justify-between px-5 py-8 md:px-8"><Link href="/" className="font-mono text-xs uppercase tracking-widest">Back to portfolio</Link><a href="#top" className="font-mono text-xs uppercase tracking-widest">Top <ArrowUpRight aria-hidden="true" className="inline size-3" /></a></div></footer>
    </main>
  )
}
