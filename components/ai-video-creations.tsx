'use client'

import { useEffect, useState } from 'react'
import { Play, X } from 'lucide-react'

export const aiVideos = [
  { title: '3D Style Documentary', category: '3D Style Documentary', id: '1iDy3WgoaPsMrkLXmLJ8YSXQ8bcca2OKZ', thumbnail: '/assets/thumbnails/Thumbnail/3D Style Documentary.jpg', aspect: 'aspect-video' },
  { title: 'Realistic Vlog', category: 'Realistic Vlog', id: '1G16baZM47RyV_MNYAhqRnyPFUK1PVqRP', thumbnail: '/assets/thumbnails/Thumbnail/Realistic Vlog.jpg', aspect: 'aspect-[9/16]' },
  { title: 'Skeleton Explainer', category: 'Skeleton Explainer', id: '1mzj_YILdD1ijVYfE3lhN0imQg_75oY6A', thumbnail: '/assets/thumbnails/Thumbnail/Skeleton Explainer.jpg', aspect: 'aspect-[9/16]' },
  { title: 'Stickman Explainer', category: 'Stickman Explainer', id: '1dd0GAuEWxyn9HNBiM-7MybMHK6QCryt2', thumbnail: '/assets/thumbnails/Thumbnail/Stickman Explainer.jpg', aspect: 'aspect-[9/16]' },
  { title: 'VOX Style Documentary', category: 'VOX Style Documentary', id: '1rZYGC-6W09mrPsO0ehG4niNlsPpwQ7Ks', thumbnail: '/assets/thumbnails/Thumbnail/VOX Style Documentary.jpg', aspect: 'aspect-video' },
] as const


export function AiVideoCreations() {
  const [activeVideo, setActiveVideo] = useState<(typeof aiVideos)[number] | null>(null)

  useEffect(() => {
    if (!activeVideo) return
    const onKeyDown = (event: KeyboardEvent) => { if (event.key === 'Escape') setActiveVideo(null) }
    document.body.style.overflow = 'hidden'
    window.addEventListener('keydown', onKeyDown)
    return () => { document.body.style.overflow = ''; window.removeEventListener('keydown', onKeyDown) }
  }, [activeVideo])

  return <>
    <section id="creations" className="border-t">
      <div className="mx-auto max-w-7xl px-5 py-20 md:px-8 md:py-28">
        <div className="mb-12 flex flex-col gap-4 md:mb-16 md:flex-row md:items-end md:justify-between">
          <div><p className="mb-4 font-mono text-xs uppercase tracking-widest text-muted-foreground">Selected work</p><h2 className="text-3xl font-semibold tracking-tight md:text-5xl">AI Video Creations</h2></div>
          <p className="max-w-sm text-muted-foreground">Long & short form documentaries, explainers, and visual stories built with AI-assisted production workflows.</p>
        </div>
        <div className="grid gap-8 sm:grid-cols-2 md:grid-cols-3 md:gap-x-8 md:gap-y-14">
          {aiVideos.map((video, index) => <button key={video.id} type="button" onClick={() => setActiveVideo(video)} className="group min-w-0 text-left">
            <div className={`relative ${video.aspect} overflow-hidden bg-muted`}>
              <img className="block h-full w-full object-cover transition duration-500 group-hover:scale-[1.02]" src={video.thumbnail} alt={`${video.title} thumbnail`} loading="lazy" />
              <span className="absolute inset-0 flex items-center justify-center bg-black/0 transition group-hover:bg-black/30"><span className="flex size-12 items-center justify-center rounded-full bg-white text-black opacity-0 transition group-hover:opacity-100"><Play className="ml-0.5 size-5 fill-current" aria-hidden="true" /></span></span>
            </div>
            <div className="mt-5 flex items-start justify-between gap-4"><div><p className="font-mono text-xs uppercase tracking-widest text-muted-foreground">{String(index + 1).padStart(2, '0')} / 05</p><h3 className="mt-2 text-xl font-semibold tracking-tight">{video.title}</h3></div><span className="mt-1 text-xs text-muted-foreground">Play</span></div>
          </button>)}
        </div>
      </div>
    </section>
    {activeVideo && <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/90 p-4 md:p-10" role="dialog" aria-modal="true" aria-label={`${activeVideo.title} video player`} onMouseDown={(event) => { if (event.target === event.currentTarget) setActiveVideo(null) }}>
      <div className="w-full max-w-5xl"><div className="mb-4 flex items-center justify-between text-white"><div><p className="font-mono text-xs uppercase tracking-widest text-white/60">{activeVideo.category}</p><h2 className="mt-1 text-xl font-semibold">{activeVideo.title}</h2></div><button type="button" onClick={() => setActiveVideo(null)} className="flex size-11 items-center justify-center border border-white/30" aria-label="Close video"><X className="size-5" /></button></div><iframe className="aspect-video w-full border-0 bg-black" src={`https://drive.google.com/file/d/${activeVideo.id}/preview`} title={`${activeVideo.title} video player`} allow="autoplay; fullscreen" allowFullScreen /></div>
    </div>}
  </>
}
