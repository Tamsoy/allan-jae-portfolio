'use client'

import Image from 'next/image'
import { ArrowUpRight, Menu, X } from 'lucide-react'
import { FaFacebookF, FaInstagram, FaLinkedinIn, FaWhatsapp } from 'react-icons/fa'
import { ThemeToggle } from '@/components/theme-toggle'
import { AiVideoCreations } from '@/components/ai-video-creations'
import { ToolsShowcase } from '@/components/tools-showcase'
import { useEffect, useRef, useState } from 'react'

const experience = [
  {
    period: 'May 2026 — September 2026',
    role: 'Programmer & Community Coordinator',
    company: 'LFT Curated Workspaces',
    location: 'Dumaguete City, Negros Oriental',
    description: 'Developed and maintained web features for internal systems and the company website. Coordinated daily coworking operations and supported members and visitors. Implemented automation and AI-powered tools to reduce manual administrative work.',
    href: 'https://lftdumaguete.com',
  },
  {
    period: 'October 2025 — March 2026',
    role: 'Technical Support Specialist',
    company: 'Zimplistic Pte Ltd (Rotimatic.com)',
    location: 'Dumaguete City, Negros Oriental',
    description: 'Supported 50+ customers daily with machine setup, operation, and performance issues. Diagnosed hardware and software faults through root-cause analysis and step-by-step troubleshooting. Provided clear technical guidance to improve customer experience and reduce recurring support requests.',
    href: 'https://rotimatic.com',
  },
  {
    period: 'May 2025',
    role: 'Technical Support (CCS Operator)',
    company: 'Miru Systems – 2025 Philippine National Election',
    location: 'Siaton, Negros Oriental',
    description: 'Supported election IT operations serving over 57,737 voters. Troubleshot system issues to minimize operational downtime. Logged incidents and resolutions with 100% compliance with audit and data-integrity requirements.',
  },
  {
    period: 'Aug 2024 — Oct 2024',
    role: 'AI Training Task Contributor',
    company: 'Remotasks & Outlier',
    location: 'Dumaguete City, Negros Oriental',
    description: 'Annotated over 2,000 images and videos for computer-vision and autonomous-driving datasets. Transcribed and categorized language data. Evaluated chatbot responses and provided structured feedback.',
  },
]

const skillGroups = [
  { title: 'AI Video Creation · Web Development · Tech Support', items: ['AI Video Production', 'AI Video Editing', 'AI-Assisted Web Development', 'Technical Support', 'Hardware Troubleshooting', 'Software Troubleshooting', 'Remote Customer Support', 'Root-Cause Analysis', 'Incident Documentation', 'IT Operations'] },
  { title: 'Productivity and Operations · Communication and Customer Support', items: ['Microsoft Office', 'Google Workspace', 'Odoo', 'Slack', 'Intercom', 'Zoom', 'Discord'] },
]

const navItems = [['Experience', '/experience'], ['Creations', '/creations'], ['Side Quests', '/side-quests'], ['Capabilities', '/capabilities'], ['About', '/about'], ['Contact', '/contact']] as const

export function Portfolio() {
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

  return (
    <main>
      <header className="sticky top-0 z-40 border-b bg-background/95 backdrop-blur-md">
        <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-5 md:h-auto md:px-8 md:py-4">
          <a href="#top" onClick={() => setMenuOpen(false)} className="font-mono text-sm font-semibold tracking-tight" aria-label="Allan Jae Eparwa, home">AJ AE</a>
          <nav className="hidden items-center gap-8 text-sm md:flex" aria-label="Primary navigation">
            {navItems.map(([label, href]) => <a key={href} className="nav-link" href={href}>{label}</a>)}
          </nav>
          <div className="hidden md:block"><ThemeToggle /></div>
          <button ref={menuButtonRef} type="button" className="flex size-11 items-center justify-center md:hidden" aria-label={menuOpen ? 'Close navigation menu' : 'Open navigation menu'} aria-expanded={menuOpen} aria-controls="mobile-navigation" onClick={() => setMenuOpen((open) => !open)}>
            {menuOpen ? <X className="size-5" aria-hidden="true" /> : <Menu className="size-5" aria-hidden="true" />}
          </button>
        </div>
        <div id="mobile-navigation" aria-hidden={!menuOpen} className={`mobile-navigation md:hidden ${menuOpen ? 'mobile-navigation-open' : ''}`}>
          <nav className="flex flex-col" aria-label="Mobile navigation">
            {navItems.map(([label, href]) => <a key={href} href={href} onClick={closeMenu} className="mobile-nav-link">{label}</a>)}
          </nav>
          <div className="mobile-menu-group"><p className="menu-label">Appearance &amp; sound</p><ThemeToggle /></div>
          <div className="mobile-menu-group flex flex-col items-start gap-4"><a href="/downloads/Allan-Jae-Eparwa-Resume-CV.pdf" download onClick={closeMenu} className="underline underline-offset-4">Download résumé</a><p className="text-sm text-muted-foreground">For opportunities, reach me at:</p><a href="mailto:eparwaallanjae@gmail.com" onClick={closeMenu} className="break-all underline underline-offset-4">eparwaallanjae@gmail.com</a></div>
        </div>
      </header>

      <section id="top" className="mx-auto max-w-7xl px-5 py-12 md:px-8 md:py-20">
        <div className="flex items-start justify-between gap-6 font-mono text-xs uppercase tracking-widest text-muted-foreground">
          <div className="hero-intro-group flex max-w-xl flex-col gap-2"><p className="font-bold"><span className="hidden md:inline">AI Video Specialist · AI-Assisted Web Developer · VA · Technical Support · IT Professional</span><span className="hero-title-mobile md:hidden">AI VIDEO SPECIALIST ·<br />AI-ASSISTED WEB DEVELOPER ·<br />VA · TECHNICAL SUPPORT ·<br />IT PROFESSIONAL</span></p></div>
          <p className="hidden shrink-0 font-bold sm:block">Dumaguete City · Philippines</p>
        </div>
        <div className="grid items-start gap-8 pt-10 md:grid-cols-12 md:items-center md:gap-8 md:pt-24">
          <div className="order-2 flex flex-col gap-7 md:order-1 md:col-span-8">
            <h1 className="font-sans text-[clamp(4rem,10vw,9rem)] font-semibold leading-[0.82] tracking-[-0.075em] text-balance">Allan Jae</h1>
            <p className="max-w-2xl text-lg leading-relaxed text-muted-foreground md:text-xl">I build useful web experiences with AI-assisted tools, solve technical problems, and help teams work more efficiently.</p>
            <div className="flex flex-col gap-3 min-[390px]:flex-row"><a href="#side-quests" className="flex min-h-12 items-center justify-center border bg-primary px-5 py-3 text-sm font-medium text-primary-foreground">View my projects</a><a href="/downloads/Allan-Jae-Eparwa-Resume-CV.pdf" download aria-label="Download Allan Jae Eparwa’s résumé as a PDF" className="flex min-h-12 items-center justify-center border px-5 py-3 text-sm font-medium">Download résumé</a></div>
            <nav className="hero-socials flex items-center gap-2 pt-2" aria-label="Social media profiles">{[['Facebook', FaFacebookF, 'https://facebook.com/allanjae.eparwa'], ['Instagram', FaInstagram, 'https://instagram.com/aint.ajae'], ['LinkedIn', FaLinkedinIn, 'https://linkedin.com/in/allan-jae-eparwa-3b635b427'], ['WhatsApp', FaWhatsapp, 'https://wa.me/639383169656']].map(([label, Icon, href]) => <a key={label as string} href={href as string} target="_blank" rel="noreferrer" className="flex size-12 shrink-0 items-center justify-center border transition-colors hover:bg-primary hover:text-primary-foreground md:size-10" aria-label={`${label as string} profile`}><Icon aria-hidden="true" className="size-4" /></a>)}</nav>
          </div>
          <div className="order-1 flex flex-col gap-4 md:order-2 md:col-span-4">
            <div className="overflow-hidden bg-muted">
              <Image
                src="https://hebbkx1anhila5yf.public.blob.vercel-storage.com/ChatGPT%20Image%20Aug%2011%2C%202026%2C%2002_14_24%20PM-3bx7F1fzad1cLygsdrdtWHfgS8XFjJ.png"
                alt="Portrait of Allan Jae Eparwa wearing a black polo shirt"
                width={1254}
                height={1254}
                priority
                className="aspect-[4/5] w-full object-cover object-top grayscale-0 dark:grayscale"
              />
            </div>
          </div>
        </div>
      </section>

      <AiVideoCreations />

      <ToolsShowcase />

      <section id="experience" className="border-t">
        <div className="mx-auto max-w-7xl px-5 py-20 md:px-8 md:py-28">
          <div className="experience-heading mb-12 flex w-full items-center justify-between pb-6 md:mb-16">
            <h2 className="text-3xl font-semibold tracking-tight md:text-5xl">Experience</h2>
            <span className="font-mono text-xs text-muted-foreground">2024 — 2026</span>
          </div>
          <div className="flex flex-col">
            {experience.map((item) => (
              <article key={`${item.company}-${item.period}`} className="group grid gap-5 border-t py-8 md:grid-cols-12 md:gap-8 md:py-10">
                <p className="font-mono text-xs font-bold uppercase tracking-widest text-muted-foreground md:col-span-3">{item.period}</p>
                <div className="flex flex-col gap-2 md:col-span-4">
                  <h3 className="text-2xl font-semibold tracking-tight">{item.role}</h3>
                  <p>{item.company} · {item.location}</p>
                </div>
                <div className="flex flex-col gap-5 md:col-span-5">
                  <p className="max-w-xl leading-relaxed text-muted-foreground">{item.description}</p>
                  {item.href && <a href={item.href} target="_blank" rel="noreferrer" className="flex w-fit items-center gap-2 text-sm font-medium">Visit company <ArrowUpRight aria-hidden="true" className="size-4 transition-transform group-hover:translate-x-1 group-hover:-translate-y-1" /></a>}
                </div>
              </article>
            ))}
          </div>
        </div>
      </section>

      <section id="side-quests" className="border-t">
        <div className="mx-auto max-w-7xl px-5 py-20 md:px-8 md:py-28">
          <div className="mb-12 flex items-end justify-between md:mb-16">
            <div>
              <p className="mb-4 font-mono text-xs uppercase tracking-widest text-muted-foreground">Experiments & tools</p>
              <h2 className="text-3xl font-semibold tracking-tight md:text-5xl">Side Quests</h2>
            </div>
            <span className="font-mono text-xs text-muted-foreground">06 personal projects</span>
          </div>
          <div className="grid gap-8 border-t pt-8 md:grid-cols-2 md:gap-x-8 md:gap-y-14 md:pt-10">{[
            ['keytone', 'KeyTone', 'A Windows utility for personal keyboard sounds and per-key tuning.', '/projects/keytone/keytone-keyboard.png', 'Windows utility · C# · WPF'],
            ['deskcheck', 'DeskCheck', 'A Windows diagnostic utility for clear, shareable support reports.', '/projects/deskcheck/deskcheck-ready.png', 'C# · WPF · IT support'],
            ['sheetclean', 'SheetClean', 'A local CSV cleaner with visible changes and an audit trail.', '/projects/sheetclean/sheetclean-ready.png', 'C# · WPF · CSV processing'],
            ['replyshelf', 'ReplyShelf', 'An offline response library with searchable templates and favorites.', '/projects/replyshelf/replyshelf-ready.png', 'C# · WPF · Local JSON'],
            ['clienthub', 'ClientHub', 'An offline workspace for contacts, follow-ups, status and notes.', '/projects/clienthub/clienthub-ready.png', 'C# · WPF · Client operations'],
            ['formflow', 'FormFlow', 'A local workspace for planning forms and tracking review status.', '/projects/formflow/formflow-ready.png', 'C# · WPF · Form planning'],
          ].map(([slug, name, description, image, tags], index) => <a key={slug} href={`/side-quests/${slug}`} className="group flex flex-col gap-5">
            <div className="overflow-hidden bg-muted"><Image src={image} alt={`${name} desktop app preview`} width={1600} height={1000} className="aspect-video w-full object-cover grayscale-0 transition duration-500 group-hover:scale-[1.02] motion-reduce:transition-none" /></div>
            <div className="flex flex-col gap-3"><div className="flex items-center justify-between gap-4"><p className="font-mono text-xs uppercase tracking-widest text-muted-foreground">{String(index + 1).padStart(2, '0')} / 06</p><ArrowUpRight aria-hidden="true" className="size-5 transition-transform group-hover:translate-x-1 group-hover:-translate-y-1 motion-reduce:transition-none" /></div><h3 className="text-2xl font-semibold tracking-tight">{name}</h3><p className="leading-relaxed text-muted-foreground">{description}</p><p className="font-mono text-xs uppercase tracking-widest text-muted-foreground">{tags}</p></div>
          </a>)}</div>
        </div>
      </section>

      <section id="skills" className="border-t bg-muted">
        <div className="mx-auto max-w-7xl px-5 py-20 md:px-8 md:py-28">
          <p className="mb-4 font-mono text-xs uppercase tracking-widest text-muted-foreground">Capabilities</p>
          <div className="grid gap-10 sm:grid-cols-2 md:grid-cols-2 md:gap-x-8 md:gap-y-14">
            {skillGroups.map((group) => (
              <div key={group.title} className="flex flex-col gap-6">
                <h2 className="text-2xl font-semibold tracking-tight">{group.title}</h2>
                <ul className="flex flex-col gap-3 text-muted-foreground">
                  {group.items.map((item) => <li key={item} className="border-b pb-3">{item}</li>)}
                </ul>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section id="about" className="border-t">
        <div className="mx-auto grid max-w-7xl gap-12 px-5 py-20 md:grid-cols-12 md:px-8 md:py-28">
          <p className="font-mono text-xs font-bold uppercase tracking-widest text-muted-foreground md:col-span-3">About</p>
          <div className="flex flex-col gap-10 md:col-span-8 md:col-start-5">
<p className="text-3xl font-medium leading-tight tracking-tight text-balance md:text-5xl">I focus on AI video production and editing, AI-assisted web development, creative AI tools, and practical technical support.</p>
          <div className="grid gap-8 text-muted-foreground sm:grid-cols-2">
            <p className="leading-relaxed">I create engaging visual content while exploring AI-assisted creative workflows that make production more accessible, intentional, and useful.</p>
            <p className="leading-relaxed">I also build web projects and solve technical problems through clear troubleshooting, documentation, and thoughtful support.</p>
            </div>
            <div className="border-t pt-6"><p className="mb-2 font-mono text-xs uppercase tracking-widest text-muted-foreground">Education</p><p className="font-medium">Bachelor of Science in Information Technology</p><p className="text-muted-foreground">Negros Oriental State University · 2025 · Dumaguete City, Negros Oriental</p></div>
          </div>
        </div>
      </section>

      <footer id="contact" className="border-t bg-primary text-primary-foreground">
        <div className="mx-auto flex max-w-7xl flex-col gap-16 px-5 py-16 md:px-8 md:py-24">
          <div className="flex flex-col gap-6">
            <p className="font-mono text-xs font-bold uppercase tracking-widest text-primary-foreground/60">I’m open to IT support, technical support, web development, AI-assisted development, virtual assistant, and technical operations opportunities.</p>
            <a href="mailto:eparwaallanjae@gmail.com" className="text-[clamp(2.8rem,8vw,7rem)] font-semibold leading-none tracking-[-0.06em] text-balance underline decoration-1 underline-offset-8">Let&apos;s work together.</a>
            <a href="mailto:eparwaallanjae@gmail.com" className="w-fit break-all text-lg underline underline-offset-4 sm:text-xl">eparwaallanjae@gmail.com</a>
            <div className="flex flex-wrap gap-3 pt-2"><a href="mailto:eparwaallanjae@gmail.com" className="border border-primary-foreground/30 px-4 py-2 text-sm">Send an email</a><a href="/downloads/Allan-Jae-Eparwa-Resume-CV.pdf" download className="border border-primary-foreground/30 px-4 py-2 text-sm">Download résumé</a><a href="#side-quests" className="border border-primary-foreground/30 px-4 py-2 text-sm">View projects</a></div>
          </div>
          <div className="flex flex-col gap-6 border-t border-primary-foreground/20 pt-6 text-sm sm:flex-row sm:items-center sm:justify-between">
            <p>© 2026 Allan Jae Eparwa</p>
            <nav className="flex items-center gap-2" aria-label="Footer social media profiles">
              <a href="https://facebook.com/allanjae.eparwa" target="_blank" rel="noreferrer" className="flex size-10 items-center justify-center border border-primary-foreground/30 transition-colors hover:bg-primary-foreground hover:text-primary" aria-label="Facebook profile">
                <FaFacebookF aria-hidden="true" className="size-4" />
              </a>
              <a href="https://instagram.com/aint.ajae" target="_blank" rel="noreferrer" className="flex size-10 items-center justify-center border border-primary-foreground/30 transition-colors hover:bg-primary-foreground hover:text-primary" aria-label="Instagram profile">
                <FaInstagram aria-hidden="true" className="size-4" />
              </a>
              <a href="https://linkedin.com/in/allan-jae-eparwa-3b635b427" target="_blank" rel="noreferrer" className="flex size-10 items-center justify-center border border-primary-foreground/30 transition-colors hover:bg-primary-foreground hover:text-primary" aria-label="LinkedIn profile">
                <FaLinkedinIn aria-hidden="true" className="size-4" />
              </a>
              <a href="https://wa.me/639383169656" target="_blank" rel="noreferrer" className="flex size-10 items-center justify-center border border-primary-foreground/30 transition-colors hover:bg-primary-foreground hover:text-primary" aria-label="WhatsApp profile">
                <FaWhatsapp aria-hidden="true" className="size-4" />
              </a>
            </nav>
          </div>
        </div>
      </footer>
    </main>
  )
}
