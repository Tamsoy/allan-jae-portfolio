import { Analytics } from '@vercel/analytics/next'
import type { Metadata, Viewport } from 'next'
import { Geist, Geist_Mono } from 'next/font/google'
import './globals.css'
import { SoundFeedbackProvider } from '@/components/sound-feedback'

const geist = Geist({ subsets: ['latin'], variable: '--font-geist' })
const geistMono = Geist_Mono({ subsets: ['latin'], variable: '--font-geist-mono' })

export const metadata: Metadata = {
  title: 'Allan Jae Eparwa — IT Professional & Web Developer',
  description: 'Portfolio of Allan Jae Eparwa, an IT professional experienced in web development, technical support, AI training, automation, and IT operations.',
  generator: 'v0.app',
}

export const viewport: Viewport = {
  colorScheme: 'light dark',
  themeColor: [
    { media: '(prefers-color-scheme: light)', color: '#ffffff' },
    { media: '(prefers-color-scheme: dark)', color: '#111111' },
  ],
}

const themeScript = `
  try {
    const saved = document.cookie.match(/(?:^|; )theme=([^;]*)/)?.[1]
    const dark = saved ? saved === 'dark' : window.matchMedia('(prefers-color-scheme: dark)').matches
    document.documentElement.classList.add(dark ? 'dark' : 'light')
  } catch (_) {}
`

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en" className="bg-background" suppressHydrationWarning>
      <head><script dangerouslySetInnerHTML={{ __html: themeScript }} /></head>
      <body className={`${geist.variable} ${geistMono.variable} font-sans antialiased`}>
        <SoundFeedbackProvider>
          {children}
        </SoundFeedbackProvider>
        {process.env.NODE_ENV === 'production' && <Analytics />}
      </body>
    </html>
  )
}
