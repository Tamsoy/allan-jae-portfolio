'use client'

import Link from 'next/link'
import { ArrowLeft, ArrowUpRight, Menu, X } from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import { ThemeToggle } from '@/components/theme-toggle'

const navItems = [
  ['Experience', '/experience'],
  ['Creations', '/creations'],
  ['Side Quests', '/side-quests'],
  ['Capabilities', '/capabilities'],
  ['About', '/about'],
  ['Contact', '/contact'],
] as const

const pageContent = {
  experience: { label: 'Experience', title: 'Experience', intro: 'Practical technical support, operations, and AI-assisted web development work focused on making technology more useful.', items: [{ title: 'Technical Support & IT Operations', meta: 'Remote support · Troubleshooting · Documentation', body: 'Helping people resolve technical problems clearly, efficiently, and with a useful record of what changed.' }, { title: 'AI-Assisted Web Development', meta: 'Web experiences · Automation · Workflow improvement', body: 'Building focused web projects with modern AI-assisted tools and a practical approach to usability.' }] },
  creations: { label: 'AI Video Creations', title: 'AI Video Creations', intro: 'Exploring AI-assisted production workflows to create engaging visual content across documentary, vlog, and explainer formats.', items: [{ title: 'Documentary & Explainer Formats', meta: '3D · VOX style · Skeleton · Stickman', body: 'A growing collection of short-form experiments made with different visual languages, pacing, and storytelling approaches.' }] },
  'side-quests': { label: 'Experiments & tools', title: 'Side Quests', intro: 'Small, practical software projects built to solve focused problems and explore useful interfaces.', items: [{ title: 'Six Windows productivity tools', meta: 'KeyTone · DeskCheck · SheetClean · ReplyShelf · ClientHub · FormFlow', body: 'Browse the project pages to see the purpose, interface, and technical details behind each experiment.', link: '/side-quests' }] },
  capabilities: { label: 'Capabilities', title: 'AI Video Creation · Web Development · Tech Support', intro: 'A practical mix of creative production, web building, technical support, and operations skills.', items: [{ title: 'Skills and Technical Expertise', meta: 'AI video · Web · Support · IT operations', body: 'AI Video Production, AI Video Editing, Codex, Claude Code, Cursor, Lovable, v0, Grok, Gemini, Technical Support, Hardware Troubleshooting, Software Troubleshooting, Remote Customer Support, Root-Cause Analysis, Incident Documentation, IT Operations.' }, { title: 'Creative Tools', meta: 'Production and development toolkit', body: 'Canva, Adobe Photoshop, Adobe Illustrator, CapCut, Google Flow, Google AI Studio, Kling AI, Higgsfield, WordPress, Wix, Elementor, ChatGPT, Claude, Cursor, Lovable, v0, Grok, Gemini, Codex, Claude Code.' }, { title: 'Productivity and Operations · Communication and Customer Support', meta: 'Everyday collaboration tools', body: 'Microsoft Office, Google Workspace, Odoo, Slack, Intercom, Zoom, Discord.' }] },
  about: { label: 'About', title: 'About Allan Jae', intro: 'I focus on AI video production and editing, AI-assisted web development, creative AI tools, and technical support.', items: [{ title: 'A practical creative workflow', meta: 'Visual content · Web projects · Technical problem solving', body: 'I enjoy creating engaging visual content, exploring AI-assisted creative workflows, building useful web projects, and solving technical problems with clarity and patience.' }] },
  contact: { label: 'Contact', title: "Let's work together.", intro: 'For AI video production, web development, technical support, and related opportunities, get in touch.', items: [{ title: 'eparwaallanjae@gmail.com', meta: 'Available for opportunities and collaboration', body: 'Send an email to start a conversation.', link: 'mailto:eparwaallanjae@gmail.com' }] },
} as const

type PageKey = keyof typeof pageContent

export function SectionPage({ page }: { page: PageKey }) {
  const content = pageContent[page]
  const [menuOpen, setMenuOpen] = useState(false)
  const menuButtonRef = useRef<HTMLButtonElement>(null)

  useEffect(() => {
    const closeOnDesktop = () => { if (window.innerWidth >= 768) setMenuOpen(false) }
    const closeOnEscape = (event: KeyboardEvent) => { if (event.key === 'Escape') { setMenuOpen(false); menuButtonRef.current?.focus() } }
    window.addEventListener('resize', closeOnDesktop)
    window.addEventListener('keydown', closeOnEscape)
    document.body.style.overflow = menuOpen ? 'hidden' : ''
    return () => { window.removeEventListener('resize', closeOnDesktop); window.removeEventListener('keydown', closeOnEscape); document.body.style.overflow = '' }
  }, [menuOpen])

  const closeMenu = () => { setMenuOpen(false); menuButtonRef.current?.focus() }

  return <main className="min-h-screen"><header className="sticky top-0 z-40 border-b bg-background/95 backdrop-blur-md"><div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-5 md:h-auto md:px-8 md:py-4"><Link href="/" className="flex items-center gap-2 font-mono text-sm font-semibold tracking-tight"><ArrowLeft aria-hidden="true" className="size-4" /> AJ AE</Link><nav className="hidden items-center gap-8 text-sm md:flex" aria-label="Primary navigation">{navItems.map(([label, href]) => <Link key={href} className="nav-link" href={href}>{label}</Link>)}</nav><div className="hidden md:block"><ThemeToggle /></div><button ref={menuButtonRef} type="button" className="flex size-11 items-center justify-center md:hidden" aria-label={menuOpen ? 'Close navigation menu' : 'Open navigation menu'} aria-expanded={menuOpen} aria-controls="section-mobile-navigation" onClick={() => setMenuOpen((open) => !open)}>{menuOpen ? <X className="size-5" aria-hidden="true" /> : <Menu className="size-5" aria-hidden="true" />}</button></div><div id="section-mobile-navigation" aria-hidden={!menuOpen} className={`mobile-navigation md:hidden ${menuOpen ? 'mobile-navigation-open' : ''}`}><nav className="flex flex-col" aria-label="Mobile navigation">{navItems.map(([label, href]) => <Link key={href} href={href} onClick={closeMenu} className="mobile-nav-link">{label}</Link>)}</nav><div className="mobile-menu-group"><p className="menu-label">Appearance</p><ThemeToggle /></div></div></header><section className="mx-auto max-w-7xl px-5 py-20 md:px-8 md:py-32"><p className="mb-5 font-mono text-xs font-bold uppercase tracking-widest text-muted-foreground">{content.label}</p><h1 className="max-w-5xl text-[clamp(3rem,8vw,7rem)] font-semibold leading-[0.9] tracking-[-0.07em] text-balance">{content.title}</h1><p className="mt-10 max-w-2xl text-xl leading-relaxed text-muted-foreground">{content.intro}</p><div className="mt-20 border-t">{content.items.map((item) => <article key={item.title} className="grid gap-6 border-b py-10 md:grid-cols-12 md:gap-8"><div className="md:col-span-4"><p className="font-mono text-xs uppercase tracking-widest text-muted-foreground">{item.meta}</p></div><div className="md:col-span-7 md:col-start-6"><h2 className="text-2xl font-semibold tracking-tight">{item.title}</h2><p className="mt-4 leading-relaxed text-muted-foreground">{item.body}</p>{item.link && <Link href={item.link} className="mt-6 flex w-fit items-center gap-2 text-sm font-medium underline underline-offset-4">Open page <ArrowUpRight className="size-4" /></Link>}</div></article>)}</div></section></main>
}
