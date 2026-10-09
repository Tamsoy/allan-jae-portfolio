'use client'

const groups = [
  { title: 'Creative Tools', tools: [['Canva', 'canva'], ['Adobe Photoshop', 'adobe-photoshop'], ['Adobe Illustrator', 'adobe-illustrator'], ['CapCut', 'capcut'], ['Google Flow', 'google'], ['Google AI Studio', 'google'], ['Kling AI', 'klingai'], ['Higgsfield', 'higgsfield'], ['WordPress', 'wordpress'], ['Wix', 'wix'], ['Elementor', 'elementor'], ['ChatGPT', 'openai'], ['Claude', 'claude'], ['Cursor', 'cursor'], ['Lovable', 'lovable'], ['v0', 'vercel'], ['Grok', 'xai'], ['Gemini', 'gemini'], ['Codex', 'openai'], ['Claude Code', 'claude']] },
  { title: 'Productivity and Operations · Communication and Customer Support', tools: [['Microsoft Office', 'microsoft-office'], ['Google Workspace', 'google'], ['Odoo', 'odoo'], ['Slack', 'slack'], ['Intercom', 'intercom'], ['Zoom', 'zoom'], ['Discord', 'discord']] },
] as const

export function ToolsShowcase() {
  return <section id="tools" className="border-t bg-muted"><div className="mx-auto max-w-7xl px-5 py-20 md:px-8 md:py-28"><div className="mb-12"><p className="mb-4 font-mono text-xs uppercase tracking-widest text-muted-foreground">Production stack</p><h2 className="text-3xl font-semibold tracking-tight md:text-5xl">Tools I Use</h2></div><div className="flex flex-col gap-14">{groups.map((group) => <div key={group.title}><h3 className="mb-6 text-2xl font-semibold tracking-tight">{group.title}</h3><div className="grid grid-cols-2 border-l border-t sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6">{group.tools.map(([name, slug]) => <div key={name} className="flex min-h-32 flex-col items-center justify-center gap-4 border-b border-r p-5 text-center"><img src={`https://cdn.jsdelivr.net/gh/glincker/thesvg@main/public/icons/${slug}/default.svg`} alt={`${name} logo`} className="h-9 w-24 object-contain" loading="lazy" /><span className="font-mono text-[0.65rem] uppercase tracking-widest text-muted-foreground">{name}</span></div>)}</div></div>)}</div></div></section>
}
