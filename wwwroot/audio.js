/* audio.js - generative background music and sound effects with the Web Audio API. */
(function () {
  'use strict';

  const midi = (m) => 440 * Math.pow(2, (m - 69) / 12);

  const PHASES = {
    academy:  { bpm: 100, drums: false, bars: [[60, 64, 67], [55, 59, 62], [57, 60, 64], [53, 57, 60]] },
    rising:   { bpm: 108, drums: true,  bars: [[57, 60, 64], [53, 57, 60], [60, 64, 67], [55, 59, 62]] },
    prime:    { bpm: 118, drums: true,  bars: [[62, 66, 69], [57, 61, 64], [59, 62, 66], [55, 59, 62]] },
    veteran:  { bpm: 92,  drums: true,  bars: [[62, 65, 69], [58, 62, 65], [53, 57, 60], [60, 64, 67]] },
    twilight: { bpm: 74,  drums: false, bars: [[53, 57, 60, 64], [52, 55, 59, 62], [50, 53, 57, 60], [48, 52, 55, 59]] }
  };

  let ctx = null, master = null, fx = null, noiseBuf = null;
  let timer = null, nextTime = 0, step = 0, phase = 'academy';
  let enabled = true;
  try { enabled = localStorage.getItem('seasons.music') !== 'off'; } catch (e) {}

  const LEVEL = 0.5;

  function ensure() {
    if (ctx) return;
    const AC = window.AudioContext || window.webkitAudioContext;
    if (!AC) return;
    ctx = new AC();

    master = ctx.createGain();
    master.gain.value = enabled ? LEVEL : 0;
    const soften = ctx.createBiquadFilter();
    soften.type = 'lowpass';
    soften.frequency.value = 2600;
    master.connect(soften);
    soften.connect(ctx.destination);

    fx = ctx.createGain();
    const delay = ctx.createDelay(1.0);
    delay.delayTime.value = 0.33;
    const feedback = ctx.createGain();
    feedback.gain.value = 0.35;
    fx.connect(delay);
    delay.connect(feedback);
    feedback.connect(delay);
    delay.connect(master);

    noiseBuf = ctx.createBuffer(1, Math.floor(ctx.sampleRate * 0.1), ctx.sampleRate);
    const d = noiseBuf.getChannelData(0);
    for (let i = 0; i < d.length; i++) d[i] = Math.random() * 2 - 1;
  }

  function tone(freq, t, dur, o) {
    o = o || {};
    const osc = ctx.createOscillator();
    const g = ctx.createGain();
    const gain = o.gain ?? 0.1, attack = o.attack ?? 0.02, release = o.release ?? 0.3;
    osc.type = o.type || 'sine';
    osc.frequency.setValueAtTime(freq, t);
    g.gain.setValueAtTime(0, t);
    g.gain.linearRampToValueAtTime(gain, t + attack);
    g.gain.setValueAtTime(gain, t + Math.max(attack, dur - release));
    g.gain.linearRampToValueAtTime(0.0001, t + dur);
    osc.connect(g);
    g.connect(master);
    if (o.send) { const s = ctx.createGain(); s.gain.value = o.send; g.connect(s); s.connect(fx); }
    osc.start(t);
    osc.stop(t + dur + 0.05);
  }

  function kick(t) {
    const osc = ctx.createOscillator();
    const g = ctx.createGain();
    osc.frequency.setValueAtTime(130, t);
    osc.frequency.exponentialRampToValueAtTime(42, t + 0.12);
    g.gain.setValueAtTime(0.28, t);
    g.gain.exponentialRampToValueAtTime(0.001, t + 0.22);
    osc.connect(g); g.connect(master);
    osc.start(t); osc.stop(t + 0.25);
  }

  function hat(t) {
    const src = ctx.createBufferSource();
    src.buffer = noiseBuf;
    const hp = ctx.createBiquadFilter();
    hp.type = 'highpass';
    hp.frequency.value = 7000;
    const g = ctx.createGain();
    g.gain.setValueAtTime(0.05, t);
    g.gain.exponentialRampToValueAtTime(0.001, t + 0.05);
    src.connect(hp); hp.connect(g); g.connect(master);
    src.start(t); src.stop(t + 0.1);
  }

  function playStep(n, t) {
    const P = PHASES[phase];
    const s = n % 16;
    const chord = P.bars[Math.floor(n / 16) % 4];
    const beat = 60 / P.bpm;

    if (s === 0) {
      chord.forEach((note) => tone(midi(note - 12), t, beat * 4 + 0.8, { type: 'triangle', gain: 0.035, attack: 0.5, release: 1.0 }));
    }
    if (s === 0 || s === 10) tone(midi(chord[0] - 24), t, beat * 0.9, { type: 'sine', gain: 0.17, attack: 0.01, release: 0.4 });

    if (s % 2 === 0 && Math.random() < 0.72) {
      const note = chord[Math.floor(Math.random() * chord.length)] + (Math.random() < 0.35 ? 24 : 12);
      tone(midi(note), t, 0.4, { type: 'triangle', gain: 0.05, attack: 0.005, release: 0.35, send: 0.5 });
    }

    if (P.drums) {
      if (s === 0 || s === 8) kick(t);
      if (s % 4 === 2) hat(t);
    }
  }

  function schedule() {
    const stepLen = 60 / PHASES[phase].bpm / 4;
    while (nextTime < ctx.currentTime + 0.25) {
      playStep(step, nextTime);
      nextTime += stepLen;
      step += 1;
    }
  }

  function start() {
    ensure();
    if (!ctx) return;
    if (ctx.state === 'suspended') ctx.resume();
    if (!timer) {
      nextTime = ctx.currentTime + 0.1;
      step = 0;
      timer = setInterval(schedule, 50);
    }
  }

  function setPhase(name) {
    if (PHASES[name]) phase = name;
  }

  function toggle() {
    enabled = !enabled;
    try { localStorage.setItem('seasons.music', enabled ? 'on' : 'off'); } catch (e) {}
    if (ctx) master.gain.setTargetAtTime(enabled ? LEVEL : 0, ctx.currentTime, 0.15);
    return enabled;
  }

  function sfx(name) {
    if (!ctx || !enabled) return;
    const t = ctx.currentTime + 0.02;
    if (name === 'whistle') {
      [[0, 0.16], [0.24, 0.5]].forEach(([offset, len]) => {
        const osc = ctx.createOscillator();
        const lfo = ctx.createOscillator();
        const depth = ctx.createGain();
        const g = ctx.createGain();
        osc.type = 'sine';
        osc.frequency.value = 2900;
        lfo.frequency.value = 38;
        depth.gain.value = 110;
        lfo.connect(depth); depth.connect(osc.frequency);
        g.gain.setValueAtTime(0, t + offset);
        g.gain.linearRampToValueAtTime(0.11, t + offset + 0.02);
        g.gain.setValueAtTime(0.11, t + offset + len - 0.04);
        g.gain.linearRampToValueAtTime(0, t + offset + len);
        osc.connect(g); g.connect(master);
        osc.start(t + offset); lfo.start(t + offset);
        osc.stop(t + offset + len + 0.05); lfo.stop(t + offset + len + 0.05);
      });
    } else if (name === 'ping' || name === 'goal') {
      tone(midi(84), t, 0.35, { type: 'sine', gain: 0.12, attack: 0.005, release: 0.3, send: 0.4 });
      tone(midi(91), t + 0.12, 0.5, { type: 'sine', gain: 0.12, attack: 0.005, release: 0.45, send: 0.4 });
    } else if (name === 'kick') {
      kick(t);
    }
  }

  document.addEventListener('visibilitychange', () => {
    if (!ctx) return;
    if (document.hidden) ctx.suspend();
    else ctx.resume();
  });

  window.Sound = { start, setPhase, toggle, sfx, isEnabled: () => enabled };
})();
