"""Convert pinned Mechvibes recordings to embedded 48 kHz mono PCM packs.
Requires numpy and soundfile only at asset-build time; never at app runtime.
"""
import json, pathlib, struct, hashlib, sys
sys.path.insert(0, str(pathlib.Path(__file__).resolve().parents[1] / 'tooldeps'))
import numpy as np
import soundfile as sf

ROOT = pathlib.Path(__file__).resolve().parent
mapping = {'Escape':1, 'Backquote':41, 'Minus':12, 'Equal':13, 'Backspace':14,
 'Tab':15, 'BracketLeft':26, 'BracketRight':27, 'Backslash':43, 'CapsLock':58,
 'Semicolon':39,'Quote':40,'Enter':28,'ShiftLeft':42,'ShiftRight':54,'Comma':51,
 'Period':52,'Slash':53,'ControlLeft':29,'ControlRight':285,'AltLeft':56,'AltRight':312,
 'MetaLeft':347,'MetaRight':348,'ContextMenu':349,'Space':57,'ArrowLeft':331,
 'ArrowRight':333,'ArrowUp':328,'ArrowDown':336,'Insert':338,'Delete':339,'Home':327,
 'End':335,'PageUp':329,'PageDown':337,'PrintScreen':311,'ScrollLock':70,'Pause':581,
 'NumLock':69,'NumpadDivide':309,'NumpadMultiply':55,'NumpadSubtract':74,
 'NumpadAdd':78,'NumpadEnter':284,'NumpadDecimal':83}
for row, start in [('QWERTYUIOP',16),('ASDFGHJKL',30),('ZXCVBNM',44)]:
    mapping.update({'Key'+char:start+i for i,char in enumerate(row)})
mapping.update({'Digit'+char:i+2 for i,char in enumerate('1234567890')})
mapping.update({'F'+str(i):58+i if i<=10 else 76+i for i in range(1,13)})
mapping.update({'Numpad'+str(i):scan for i,scan in enumerate([82,79,80,81,75,76,77,71,72,73])})
audit = {}
for kind in ['blue','red','brown']:
    config = json.loads((ROOT/'upstream'/f'{kind}-config.json').read_text())
    pcm,rate = sf.read(ROOT/'upstream'/f'{kind}-sound.ogg',always_2d=True)
    pcm = pcm.mean(axis=1)
    slices = {}
    for name,definition in config['definitions'].items():
        if name not in mapping: continue
        events=[]
        for start,end in definition['timing'][:2]:
            segment=pcm[int(round(start*rate/1000)):int(round(end*rate/1000))].copy()
            if len(segment)==0: raise ValueError(f'Empty sample: {kind} {name}')
            # Remove DC, trim leading/trailing silence independently so key-up never waits
            # for the pause between down and up in the original recording.
            segment-=segment.mean()
            peak=np.max(np.abs(segment)); active=np.flatnonzero(np.abs(segment)>max(.0003,peak*.018))
            if len(active)==0: raise ValueError(f'Silent sample: {kind} {name}')
            segment=segment[max(0,active[0]-int(rate*.0007)):min(len(segment),active[-1]+int(rate*.006)+1)]
            target=np.arange(max(1,round(len(segment)*48000/rate)))*rate/48000
            segment=np.interp(target,np.arange(len(segment)),segment)
            fade=min(96,len(segment)//4)
            segment[:min(16,fade)]*=np.linspace(0,1,min(16,fade))
            segment[-fade:]*=np.linspace(1,0,fade)
            events.append(segment)
        if len(events)!=2: raise ValueError('Missing press/release')
        slices[mapping[name]]=events
    # Right modifiers and Windows/menu keys reuse a corresponding recorded modifier.
    for key,fallback in [(285,29),(312,56),(347,29),(348,29),(349,29),(86,43)]:
        if key not in slices: slices[key]=slices[fallback]
    # One pack-wide gain retains genuine key-to-key and down/up loudness differences.
    peak=max(float(np.max(np.abs(a))) for pair in slices.values() for a in pair)
    gain=.82/peak
    destination=ROOT/'switches'/f'{kind}.bin'
    with destination.open('wb') as f:
        f.write(struct.pack('<ii',0x3153544b,len(slices)))
        for code,events in sorted(slices.items()):
            f.write(struct.pack('<i',code))
            for segment in events:
                f.write(struct.pack('<i',len(segment)))
                f.write(np.rint(np.clip(segment*gain,-.98,.98)*32767).astype('<i2').tobytes())
    sf.write(ROOT/'switches'/f'{kind}-preview.wav', np.concatenate([np.concatenate([slices[c][0]*gain,np.zeros(2500),slices[c][1]*gain,np.zeros(3200)]) for c in [30,31,32,33,57,28]]),48000,subtype='PCM_16')
    audit[kind]={'keys':len(slices),'rate':48000,'gain':gain,'sha256':hashlib.sha256(destination.read_bytes()).hexdigest(), 'source_sha256':hashlib.sha256((ROOT/'upstream'/f'{kind}-sound.ogg').read_bytes()).hexdigest(),'min_event_peak':min(float(np.max(np.abs(a)))*gain for pair in slices.values() for a in pair)}
(ROOT/'switches'/'asset-audit.json').write_text(json.dumps(audit,indent=2))
print(json.dumps(audit,indent=2))
