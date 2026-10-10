'use client'

const tools = [
  ['Canva', 'https://www.canva.com/', 'https://cdn.jsdelivr.net/gh/glincker/thesvg@main/public/icons/canva/default.svg'],
  ['Adobe Photoshop', 'https://www.adobe.com/products/photoshop.html', '/assets/icons/Icons/photoshop.svg'],
  ['Adobe Illustrator', 'https://www.adobe.com/products/illustrator.html', '/assets/icons/Icons/illustrator.svg'],
  ['CapCut', 'https://www.capcut.com/', 'https://cdn.jsdelivr.net/gh/glincker/thesvg@main/public/icons/capcut/default.svg'],
  ['Google Flow', 'https://labs.google/flow', '/assets/icons/google-flow.png'],
  ['Google AI Studio', 'https://aistudio.google.com/', '/assets/icons/google-ai-studio.png'],
  ['Kling AI', 'https://klingai.com/', '/assets/icons/Icons/kling.svg'],
  ['Higgsfield', 'https://higgsfield.ai/', '/assets/icons/Icons/higgsfield.png'],
  ['Claude', 'https://claude.ai/', 'https://cdn.jsdelivr.net/gh/glincker/thesvg@main/public/icons/claude/default.svg'],
  ['ChatGPT', 'https://chatgpt.com/', 'https://cdn.jsdelivr.net/gh/glincker/thesvg@main/public/icons/openai/default.svg'],
  ['ElevenLabs', 'https://elevenlabs.io/', '/assets/icons/elevenlabs.svg'],
  ['Pixabay', 'https://pixabay.com/', '/assets/icons/pixabay.svg'],
] as const

export function ToolsShowcase() {
  const marquee = [...tools, ...tools]
  return <section id="tools" className="border-t bg-muted"><div className="mx-auto max-w-7xl px-5 py-20 md:px-8 md:py-28"><div className="mb-12"><p className="mb-4 font-mono text-xs uppercase tracking-widest text-muted-foreground">Production stack</p><h2 className="text-3xl font-semibold tracking-tight md:text-5xl">Tools I Use</h2></div><div className="tools-marquee-wrap overflow-hidden border-y py-6"><div className="tools-marquee flex w-max items-center gap-4">{marquee.map(([name, href, icon], index) => <a key={`${name}-${index}`} href={href} target="_blank" rel="noreferrer" aria-label={`Open ${name} website`} className="flex size-16 shrink-0 items-center justify-center border bg-background p-3 transition-colors hover:bg-primary hover:invert sm:size-18 sm:p-3.5 md:size-20 md:p-4 lg:size-22 lg:p-4.5"><img src={icon} alt={`${name} icon`} className="size-8 object-contain sm:size-9 md:size-10 lg:size-11" loading="lazy" /></a>)}</div></div></div></section>
}
