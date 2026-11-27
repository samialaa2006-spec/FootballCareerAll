/* app.js - Player Customizer, Real Clubs & Leagues, Live Interactive Match Pitch, and Career Hub */
(function () {
  'use strict';

  const $ = (sel) => document.querySelector(sel);
  const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

  const POSITIONS = ['GK', 'CB', 'LB', 'RB', 'CDM', 'CM', 'CAM', 'LM', 'RM', 'LW', 'RW', 'CF', 'ST'];
  const POSITION_NAMES = {
    GK: 'Goalkeeper', CB: 'Centre-back', LB: 'Left-back', RB: 'Right-back',
    CDM: 'Defensive midfielder', CM: 'Central midfielder', CAM: 'Attacking midfielder',
    LM: 'Left midfielder', RM: 'Right midfielder', LW: 'Left winger', RW: 'Right winger',
    CF: 'Centre-forward', ST: 'Striker'
  };
  const ATTR_SHORT = ['PAC', 'SHO', 'PAS', 'DRI', 'DEF', 'PHY', 'GK'];
  const PHASE_LABEL = { academy: 'Academy', rising: 'Rising star', prime: 'Prime', veteran: 'Veteran', twilight: 'Twilight' };
  const MAX_AGE = 38;

  const SKIN_TONES = ['#f9d5b8', '#eab892', '#d2946b', '#a86b43', '#734427', '#452614'];
  const HAIR_COLORS = ['#1c1613', '#4a3222', '#8c5828', '#d9b36c', '#b83b27', '#e6e6e6'];
  const BOOT_COLORS = ['#4db8ff', '#ff4d6d', '#ffc233', '#38e54d', '#ffffff', '#111111'];

  let catalog = { leagues: [], clubs: [], nationalities: [] };
  let look = {
    skinTone: SKIN_TONES[1],
    hairStyle: 'short',
    hairColor: HAIR_COLORS[0],
    facialHair: 'none',
    bootColor: BOOT_COLORS[0],
    shirtNumber: 9
  };

  let career = null;
  let services = { native: false, java: false, php: false };
  let busy = false;

  // ---------- server calls ----------

  async function api(path, options) {
    const res = await fetch(path, Object.assign({ headers: { 'Content-Type': 'application/json' } }, options));
    let data = null;
    try { data = await res.json(); } catch (e) {}
    if (!res.ok) throw new Error((data && data.error) || 'Request failed (' + res.status + ')');
    return data;
  }

  async function refreshServices() {
    try { services = await api('/api/info'); }
    catch (e) { services = { native: false, java: false, php: false }; }
    const on = (v) => (v ? 'on' : 'off');
    $('#engineInfo').textContent =
      'C++ market values: ' + on(services.native) + '. Java news: ' + on(services.java) + '. PHP saves: ' + on(services.php) + '.';
    $('#saveBtn').disabled = !services.php;
  }

  // ---------- SVG Avatar Renderer ----------

  function renderAvatarSVG(lk, primaryColor, secondaryColor) {
    const skin = lk.skinTone || '#eab892';
    const hair = lk.hairColor || '#1c1613';
    const kit1 = primaryColor || '#EF0107';
    const kit2 = secondaryColor || '#FFFFFF';
    const boots = lk.bootColor || '#4db8ff';

    let hairSvg = '';
    switch (lk.hairStyle) {
      case 'buzz':
        hairSvg = `<path d="M36 33 C36 18, 64 18, 64 33 C62 23, 38 23, 36 33 Z" fill="${hair}" opacity="0.75"/>`;
        break;
      case 'fade':
        hairSvg = `<path d="M36 32 C36 15, 64 15, 64 32 L62 24 L38 24 Z" fill="${hair}"/>`;
        break;
      case 'curly':
        hairSvg = `<circle cx="38" cy="24" r="6" fill="${hair}"/><circle cx="45" cy="19" r="7" fill="${hair}"/>
                   <circle cx="55" cy="19" r="7" fill="${hair}"/><circle cx="62" cy="24" r="6" fill="${hair}"/>`;
        break;
      case 'afro':
        hairSvg = `<circle cx="50" cy="24" r="18" fill="${hair}"/>`;
        break;
      case 'long':
        hairSvg = `<path d="M33 28 C33 13, 67 13, 67 28 L69 52 L63 52 L63 32 L37 32 L37 52 L31 52 Z" fill="${hair}"/>`;
        break;
      case 'ponytail':
        hairSvg = `<circle cx="50" cy="14" r="6" fill="${hair}"/><path d="M36 31 C36 16, 64 16, 64 31 C60 22, 40 22, 36 31 Z" fill="${hair}"/>`;
        break;
      default: // short
        hairSvg = `<path d="M35 32 C35 15, 65 15, 65 32 C62 21, 38 21, 35 32 Z" fill="${hair}"/>`;
    }

    let facialSvg = '';
    if (lk.facialHair === 'stubble') {
      facialSvg = `<path d="M39 38 Q50 49 61 38 Q59 46 50 47 Q41 46 39 38 Z" fill="${hair}" opacity="0.32"/>`;
    } else if (lk.facialHair === 'beard') {
      facialSvg = `<path d="M38 37 Q50 52 62 37 L60 44 Q50 53 40 44 Z" fill="${hair}"/>`;
    } else if (lk.facialHair === 'mustache') {
      facialSvg = `<path d="M43 40 Q50 37 57 40 Q50 42 43 40 Z" fill="${hair}"/>`;
    }

    return `<svg viewBox="0 0 100 115" width="100%" height="100%" aria-hidden="true">
      <!-- Legs & Boots -->
      <rect x="40" y="82" width="7" height="20" rx="3" fill="${skin}"/>
      <rect x="53" y="82" width="7" height="20" rx="3" fill="${skin}"/>
      <rect x="38" y="100" width="10" height="6" rx="3" fill="${boots}"/>
      <rect x="52" y="100" width="10" height="6" rx="3" fill="${boots}"/>
      <!-- Shorts -->
      <path d="M36 72 L64 72 L62 86 L51 86 L50 80 L49 86 L38 86 Z" fill="${kit2}"/>
      <!-- Jersey -->
      <path d="M30 48 Q50 44 70 48 L74 66 L65 68 L63 56 L63 73 L37 73 L37 56 L35 68 L26 66 Z" fill="${kit1}"/>
      <text x="50" y="66" text-anchor="middle" font-family="sans-serif" font-weight="900" font-size="11" fill="${kit2}">${lk.shirtNumber || 9}</text>
      <!-- Arms -->
      <rect x="27" y="64" width="6" height="14" rx="3" fill="${skin}"/>
      <rect x="67" y="64" width="6" height="14" rx="3" fill="${skin}"/>
      <!-- Head -->
      ${lk.hairStyle === 'afro' || lk.hairStyle === 'long' ? hairSvg : ''}
      <rect x="46" y="42" width="8" height="7" fill="${skin}"/>
      <ellipse cx="50" cy="32" rx="13" ry="15" fill="${skin}"/>
      <!-- Eyes -->
      <circle cx="45" cy="31" r="1.5" fill="#111"/>
      <circle cx="55" cy="31" r="1.5" fill="#111"/>
      ${facialSvg}
      ${lk.hairStyle !== 'afro' && lk.hairStyle !== 'long' ? hairSvg : ''}
    </svg>`;
  }

  // ---------- Helpers ----------

  function phaseOf(age) {
    if (age <= 19) return 'academy';
    if (age <= 23) return 'rising';
    if (age <= 29) return 'prime';
    if (age <= 33) return 'veteran';
    return 'twilight';
  }

  function money(v) {
    return v >= 1e6 ? '€' + (v / 1e6).toFixed(1) + 'M' : '€' + Math.round(v / 1e3) + 'K';
  }

  function showScreen(name) {
    ['start', 'hub', 'retired'].forEach((n) => { $('#screen-' + n).hidden = n !== name; });
    window.scrollTo(0, 0);
  }

  function applyPhase(age) {
    const phase = phaseOf(age);
    document.body.dataset.phase = phase;
    Sound.setPhase(phase);
    $('#phaseTag').textContent = PHASE_LABEL[phase];
  }

  function countTo(el, to, format, ms) {
    const from = Number(el.dataset.v || 0);
    el.dataset.v = to;
    if (reduceMotion || from === to) { el.textContent = format(to); return; }
    const t0 = performance.now();
    (function tick(t) {
      const k = Math.min(1, (t - t0) / (ms || 750));
      const eased = 1 - Math.pow(1 - k, 3);
      el.textContent = format(from + (to - from) * eased);
      if (k < 1) requestAnimationFrame(tick);
    })(t0);
  }

  function setPitchSpeed(rate) {
    if (reduceMotion) return;
    document.querySelector('.turf').getAnimations().forEach((a) => { if (a.updatePlaybackRate) a.updatePlaybackRate(rate); });
    document.body.classList.toggle('playing', rate > 1);
  }

  async function kickoff() {
    if (reduceMotion) return;
    setPitchSpeed(5);
    await sleep(750);
    setPitchSpeed(1);
  }

  // ---------- Start Screen & Customizer Setup ----------

  function initSwatches(containerId, colors, current, onPick) {
    const box = $(containerId);
    box.replaceChildren();
    colors.forEach((hex) => {
      const btn = document.createElement('button');
      btn.type = 'button';
      btn.className = 'swatch' + (hex === current ? ' is-on' : '');
      btn.style.background = hex;
      btn.addEventListener('click', () => {
        box.querySelectorAll('.swatch').forEach((s) => s.classList.remove('is-on'));
        btn.classList.add('is-on');
        onPick(hex);
        updateCreatorPreview();
      });
      box.appendChild(btn);
    });
  }

  function populateClubsForLeague(leagueId, clubSelectEl) {
    const clubs = catalog.clubs.filter((c) => c.leagueId === leagueId);
    clubSelectEl.replaceChildren();
    clubs.forEach((c) => {
      const opt = document.createElement('option');
      opt.value = c.id;
      opt.textContent = `${c.name} (${c.strength} OVR)`;
      clubSelectEl.appendChild(opt);
    });
  }

  function updateCreatorPreview() {
    const clubId = $('#clubSelect').value;
    const club = catalog.clubs.find((c) => c.id === clubId) || { name: 'Arsenal', leagueId: 'epl', primaryColor: '#EF0107', secondaryColor: '#FFFFFF' };
    const league = catalog.leagues.find((l) => l.id === club.leagueId) || { name: 'Premier League' };
    look.hairStyle = $('#hairStyleSelect').value;
    look.facialHair = $('#facialSelect').value;
    look.shirtNumber = Math.max(1, Math.min(99, Number($('#numberInput').value) || 9));

    $('#previewAvatar').innerHTML = renderAvatarSVG(look, club.primaryColor, club.secondaryColor);
    $('#previewClubBadge').textContent = `${club.name} • ${league.name}`;
    $('#previewClubBadge').style.background = club.primaryColor;
    $('#previewClubBadge').style.color = isLightColor(club.primaryColor) ? '#111111' : '#ffffff';

    const nat = $('#natSelect').value || 'England';
    const foot = $('#footSelect').value || 'Right';
    const height = $('#heightInput').value || 180;
    $('#previewBio').textContent = `${nat} • ${foot} Foot • ${height} cm • #${look.shirtNumber}`;
  }

  function isLightColor(hex) {
    const c = hex.replace('#', '');
    const r = parseInt(c.substr(0, 2), 16), g = parseInt(c.substr(2, 2), 16), b = parseInt(c.substr(4, 2), 16);
    return (r * 299 + g * 587 + b * 114) / 1000 > 155;
  }

  async function initCatalog() {
    try {
      catalog = await api('/api/catalog');
    } catch (e) {
      catalog = {
        leagues: [{ id: 'epl', name: 'Premier League' }],
        clubs: [{ id: 'ars', name: 'Arsenal', leagueId: 'epl', strength: 89, primaryColor: '#EF0107', secondaryColor: '#FFFFFF' }],
        nationalities: ['England', 'Spain', 'France', 'Germany', 'Italy', 'Brazil', 'Argentina', 'Hungary']
      };
    }

    const natSel = $('#natSelect');
    catalog.nationalities.forEach((n) => {
      const opt = document.createElement('option');
      opt.value = n; opt.textContent = n;
      natSel.appendChild(opt);
    });

    const lSel = $('#leagueSelect');
    const tlSel = $('#transferLeagueSelect');
    catalog.leagues.forEach((l) => {
      const opt = document.createElement('option');
      opt.value = l.id; opt.textContent = `${l.name} (${l.country})`;
      lSel.appendChild(opt);
      tlSel.appendChild(opt.cloneNode(true));
    });

    populateClubsForLeague(lSel.value, $('#clubSelect'));
    populateClubsForLeague(tlSel.value, $('#transferClubSelect'));

    lSel.addEventListener('change', () => { populateClubsForLeague(lSel.value, $('#clubSelect')); updateCreatorPreview(); });
    tlSel.addEventListener('change', () => populateClubsForLeague(tlSel.value, $('#transferClubSelect')));
    ['#clubSelect', '#natSelect', '#footSelect', '#heightInput', '#numberInput', '#hairStyleSelect', '#facialSelect'].forEach((id) => {
      $(id).addEventListener('change', updateCreatorPreview);
      $(id).addEventListener('input', updateCreatorPreview);
    });

    initSwatches('#skinSwatches', SKIN_TONES, look.skinTone, (c) => { look.skinTone = c; });
    initSwatches('#hairColorSwatches', HAIR_COLORS, look.hairColor, (c) => { look.hairColor = c; });
    initSwatches('#bootSwatches', BOOT_COLORS, look.bootColor, (c) => { look.bootColor = c; });
    updateCreatorPreview();
  }

  const posGrid = $('#posGrid');
  POSITIONS.forEach((pos) => {
    posGrid.insertAdjacentHTML('beforeend',
      '<input type="radio" name="pos" id="pos-' + pos + '" value="' + pos + '"' + (pos === 'ST' ? ' checked' : '') + '>' +
      '<label for="pos-' + pos + '">' + pos + '</label>');
  });
  function updatePosHelp() {
    const pos = document.querySelector('input[name="pos"]:checked').value;
    $('#posHelp').textContent = pos + ': ' + POSITION_NAMES[pos];
  }
  posGrid.addEventListener('change', updatePosHelp);
  updatePosHelp();

  async function loadSavesList() {
    const box = $('#savesBox');
    const list = $('#savesList');
    list.replaceChildren();
    if (!services.php) { box.hidden = true; return; }
    try {
      const data = await api('/api/saves');
      const saves = data.saves || [];
      box.hidden = saves.length === 0;
      saves.slice(0, 8).forEach((s) => {
        const li = document.createElement('li');
        const b = document.createElement('button');
        b.type = 'button';
        b.className = 'save-btn';
        b.textContent = `${s.name} (${s.club || 'Club'}) • ${s.position}, age ${s.age}`;
        b.addEventListener('click', () => loadCareer(s.slot));
        li.appendChild(b);
        list.appendChild(li);
      });
    } catch (e) { box.hidden = true; }
  }

  function enterHub() {
    if (career.player.age >= MAX_AGE) { showRetired(); return; }
    buildAttrRows();
    showScreen('hub');
    applyPhase(career.player.age);
    renderHub();
    refreshNews();
  }

  $('#startBtn').addEventListener('click', async () => {
    Sound.start();
    const btn = $('#startBtn');
    $('#startError').textContent = '';
    btn.disabled = true;
    try {
      career = await api('/api/career', {
        method: 'POST',
        body: JSON.stringify({
          name: $('#nameInput').value.trim(),
          position: document.querySelector('input[name="pos"]:checked').value,
          nationality: $('#natSelect').value,
          clubId: $('#clubSelect').value,
          preferredFoot: $('#footSelect').value,
          heightCm: Number($('#heightInput').value) || 180,
          look: look
        })
      });
      enterHub();
      kickoff();
    } catch (e) {
      $('#startError').textContent = 'Could not start: ' + e.message;
    } finally {
      btn.disabled = false;
    }
  });

  async function loadCareer(slot) {
    Sound.start();
    $('#startError').textContent = '';
    try {
      career = await api('/api/load/' + encodeURIComponent(slot), { method: 'POST' });
      enterHub();
      kickoff();
    } catch (e) {
      $('#startError').textContent = 'Could not load: ' + e.message;
    }
  }

  // ---------- Career Hub & League Rendering ----------

  function buildAttrRows() {
    const isGK = career.player.position === 'GK';
    const rows = [];
    for (let i = 0; i < 7; i++) {
      if (i === 6 && !isGK) continue;
      rows.push(
        '<div class="attr" data-i="' + i + '"><span class="attr-k">' + ATTR_SHORT[i] + '</span>' +
        '<div class="bar" role="presentation"><i style="--v:0"></i></div><span class="attr-v">0</span></div>');
    }
    $('#attrs').innerHTML = rows.join('');
    $('#ovr').dataset.v = 0; $('#pot').dataset.v = 0; $('#value').dataset.v = 0;
  }

  function renderHub() {
    const p = career.player;
    const club = career.club;
    const lg = career.league;
    const values = career.values;

    $('#hubAvatar').innerHTML = renderAvatarSVG(p.look || look, club.primaryColor, club.secondaryColor);
    $('#playerName').textContent = p.name;
    $('#playerMeta').textContent = `${p.position} (${POSITION_NAMES[p.position]}) • Age ${p.age} • ${p.nationality}`;
    $('#playerClubMeta').textContent = `${club.name} (#${(p.look && p.look.shirtNumber) || 9}) • ${p.preferredFoot} Foot • ${p.heightCm}cm`;

    countTo($('#ovr'), career.overall, (n) => String(Math.round(n)));
    countTo($('#pot'), p.potential, (n) => String(Math.round(n)));
    countTo($('#value'), career.marketValue, (n) => money(n), 900);
    $('#seasonTally').textContent = `${lg.seasonGoals}G • ${lg.seasonAssists}A`;

    requestAnimationFrame(() => {
      document.querySelectorAll('.attr').forEach((row) => {
        const v = p.attr[Number(row.dataset.i)];
        row.querySelector('.bar i').style.setProperty('--v', v);
        countTo(row.querySelector('.attr-v'), v, (n) => String(Math.round(n)), 600);
      });
    });

    // League & Next Fixture UI
    const userRank = lg.table.findIndex((r) => r.clubId === p.clubId) + 1;
    $('#leaguePosBadge').textContent = `League Pos: #${userRank || 1}`;
    $('#transferToggleBtn').hidden = lg.currentMatchday > 1;
    if (lg.currentMatchday > 1) $('#transferBox').hidden = true;

    const fixBox = $('#nextFixtureBox');
    if (lg.nextFixture) {
      const nf = lg.nextFixture;
      $('#leagueTitle').textContent = `${lg.leagueName} • Matchday ${lg.currentMatchday} / ${lg.totalMatchdays}`;
      const homeName = nf.isHome ? club.name : nf.clubName;
      const homeCol = nf.isHome ? club.primaryColor : nf.primaryColor;
      const awayName = nf.isHome ? nf.clubName : club.name;
      const awayCol = nf.isHome ? nf.primaryColor : club.primaryColor;
      fixBox.innerHTML = `
        <div class="team-chip"><span class="kit-dot" style="background:${homeCol}"></span>${homeName}</div>
        <span class="vs-tag">VS</span>
        <div class="team-chip">${awayName}<span class="kit-dot" style="background:${awayCol}"></span></div>
      `;
      $('#playInteractiveBtn').disabled = false;
      $('#playInteractiveBtn').textContent = `Play Matchday ${lg.currentMatchday} (Live Pitch)`;
      $('#simMatchBtn').disabled = false;
      $('#playBtn').textContent = 'Sim Rest of Season';
    } else {
      $('#leagueTitle').textContent = `${lg.leagueName} • Season Complete!`;
      fixBox.innerHTML = `<div style="font-weight:700;color:var(--accent)">All ${lg.totalMatchdays} matchdays played! Finish the season to advance your career.</div>`;
      $('#playInteractiveBtn').disabled = true;
      $('#playInteractiveBtn').textContent = 'Season Matches Complete';
      $('#simMatchBtn').disabled = true;
      $('#playBtn').textContent = `Complete Season ${p.seasons + 1} & Advance`;
    }

    // Render League Standings Table
    $('#leagueTableBody').innerHTML = lg.table.map((r, idx) => {
      const isUser = r.clubId === p.clubId;
      const cls = [isUser ? 'is-user' : '', idx < 4 ? 'ucl' : ''].filter(Boolean).join(' ');
      return `<tr class="${cls}">
        <td>${idx + 1}</td>
        <td style="text-align:left"><span class="kit-dot" style="display:inline-block;width:9px;height:9px;margin-right:6px;background:${r.primaryColor}"></span>${r.clubName}</td>
        <td>${r.played}</td>
        <td>${r.won}</td>
        <td>${r.drawn}</td>
        <td>${r.lost}</td>
        <td>${r.gd >= 0 ? '+' + r.gd : r.gd}</td>
        <td><strong>${r.points}</strong></td>
      </tr>`;
    }).join('');

    // Market value delta & sparkline
    const delta = $('#delta');
    if (values.length < 2) {
      delta.textContent = '';
      delta.className = 'delta';
    } else {
      const diff = values[values.length - 1] - values[values.length - 2];
      delta.textContent = (diff >= 0 ? '+' : '-') + money(Math.abs(diff));
      delta.className = 'delta ' + (diff > 0 ? 'up' : diff < 0 ? 'down' : '');
    }

    const hasHistory = values.length > 1;
    $('#spark').toggleAttribute('hidden', !hasHistory);
    $('#sparkHint').hidden = hasHistory;
    if (hasHistory) drawSpark(values);

    renderLog();
    $('#playBtn').disabled = false;
  }

  function renderLog() {
    const rows = career.history;
    $('#logEmpty').hidden = rows.length > 0;
    $('#logBody').innerHTML = rows.slice().reverse().map((r) =>
      `<tr><td>${r.age}</td><td>${r.clubName || career.player.clubName}</td><td>#${r.leaguePos || '-'}</td><td>${r.ovr}</td><td>${r.goals}G/${r.assists}A</td><td>${money(r.value)}</td></tr>`
    ).join('');
  }

  function drawSpark(values) {
    const W = 300, H = 84, pad = 6;
    const max = Math.max.apply(null, values) || 1;
    const x = (i) => pad + i * ((W - 2 * pad) / Math.max(1, values.length - 1));
    const y = (v) => H - pad - (v / max) * (H - 2 * pad);
    const pts = values.map((v, i) => [x(i), y(v)]);
    const peakIdx = values.indexOf(max);
    const line = pts.map((p, i) => (i ? 'L' : 'M') + p[0].toFixed(1) + ' ' + p[1].toFixed(1)).join(' ');
    const area = line + ' L' + pts[pts.length - 1][0].toFixed(1) + ' ' + (H - pad) + ' L' + pts[0][0].toFixed(1) + ' ' + (H - pad) + ' Z';
    $('#spark').innerHTML =
      '<defs><linearGradient id="sg" x1="0" y1="0" x2="0" y2="1">' +
      '<stop offset="0" stop-color="currentColor" stop-opacity="0.4"/><stop offset="1" stop-color="currentColor" stop-opacity="0"/></linearGradient></defs>' +
      '<path d="' + area + '" fill="url(#sg)"/><path class="spark-line" d="' + line + '"/>' +
      '<circle class="spark-dot" cx="' + pts[peakIdx][0].toFixed(1) + '" cy="' + pts[peakIdx][1].toFixed(1) + '" r="4"/>';
  }

  async function refreshNews() {
    const panel = $('#newsPanel');
    if (!services.java || !career) { panel.hidden = true; return; }
    try {
      const data = await api('/api/career/' + career.id + '/news');
      const list = $('#newsList');
      list.replaceChildren();
      (data.headlines || []).forEach((h) => {
        const li = document.createElement('li');
        li.textContent = h;
        list.appendChild(li);
      });
      panel.hidden = list.children.length === 0;
    } catch (e) { panel.hidden = true; }
  }

  // ---------- Transfers ----------

  $('#transferToggleBtn').addEventListener('click', () => {
    $('#transferBox').hidden = !$('#transferBox').hidden;
  });

  $('#confirmTransferBtn').addEventListener('click', async () => {
    if (busy || !career) return;
    busy = true;
    try {
      career = await api('/api/career/' + career.id + '/transfer', {
        method: 'POST',
        body: JSON.stringify({ clubId: $('#transferClubSelect').value })
      });
      $('#transferBox').hidden = true;
      $('#status').textContent = `Transferred to ${career.player.clubName}!`;
      renderHub();
      refreshNews();
    } catch (e) {
      $('#status').textContent = e.message;
    } finally { busy = false; }
  });

  // ---------- Sim Single Matchday ----------

  $('#simMatchBtn').addEventListener('click', async () => {
    if (busy || !career || !career.league.nextFixture) return;
    busy = true;
    setPitchSpeed(5);
    Sound.sfx('whistle');
    try {
      const [res] = await Promise.all([
        api('/api/career/' + career.id + '/matchday', { method: 'POST', body: JSON.stringify({ interactive: false }) }),
        sleep(reduceMotion ? 100 : 500)
      ]);
      career = res.career;
      if (res.match) $('#status').textContent = res.match.summary;
      renderHub();
    } catch (e) {
      $('#status').textContent = e.message;
    } finally {
      setPitchSpeed(1);
      busy = false;
    }
  });

  // ---------- Interactive Live Pitch Mini-Game ----------

  let matchEngine = null;

  $('#playInteractiveBtn').addEventListener('click', () => {
    if (busy || !career || !career.league.nextFixture) return;
    startInteractiveMatch();
  });

  function startInteractiveMatch() {
    busy = true;
    const nf = career.league.nextFixture;
    const club = career.club;
    const p = career.player;
    const modal = $('#pitchModal');
    modal.hidden = false;
    modal.scrollIntoView({ behavior: 'smooth', block: 'center' });

    const homeName = nf.isHome ? club.shortName : nf.shortName;
    const awayName = nf.isHome ? nf.shortName : club.shortName;
    $('#pitchMatchTitle').textContent = `MD ${nf.matchday}: ${homeName} vs ${awayName}`;
    Sound.sfx('whistle');
    setPitchSpeed(6);

    const canvas = $('#pitchCanvas');
    const ctx = canvas.getContext('2d');
    const W = canvas.width, H = canvas.height;

    // Player stats scale speed & shot power
    const paceFactor = 1.6 + (p.attr[0] / 100) * 1.4;
    const shotFactor = 4.5 + (p.attr[1] / 100) * 3.5;

    const state = {
      running: true,
      elapsed: 0,
      duration: 24, // 24 real seconds = 90 match minutes
      userGoals: 0,
      userAssists: 0,
      extraTeamGoals: 0,
      oppGoals: 0,
      rating: 6.6,
      user: { x: 110, y: H / 2, vx: 0, vy: 0, r: 9 },
      mate: { x: 180, y: H * 0.3, vy: 1.1, r: 8 },
      def1: { x: 255, y: H * 0.35, vy: 1.3, r: 8 },
      def2: { x: 315, y: H * 0.65, vy: -1.4, r: 8 },
      gk:   { x: W - 18, y: H / 2, vy: 1.5, r: 8 },
      ball: { x: 124, y: H / 2, vx: 0, vy: 0, owner: 'user' },
      keys: {},
      pointerTarget: null,
      flashMsg: ''
    };

    function resetKickoff(userHasBall) {
      state.user.x = 120; state.user.y = H / 2;
      state.ball.x = userHasBall ? 132 : 260;
      state.ball.y = H / 2;
      state.ball.vx = 0; state.ball.vy = 0;
      state.ball.owner = userHasBall ? 'user' : 'def1';
    }

    function kickOrTackle() {
      if (!state.running) return;
      const b = state.ball, u = state.user;
      const dist = Math.hypot(b.x - u.x, b.y - u.y);

      if (b.owner === 'user' || dist < 22) {
        // Shoot toward right goal (or pass to teammate if teammate is open in box)
        Sound.sfx('kick');
        b.owner = null;
        const goalY = H / 2 + (Math.random() - 0.5) * 44;
        const angle = Math.atan2(goalY - b.y, (W - 10) - b.x);
        b.vx = Math.cos(angle) * shotFactor;
        b.vy = Math.sin(angle) * shotFactor;
        state.rating = Math.min(10, state.rating + 0.15);
      } else if (dist < 44) {
        // Slide tackle / interception!
        Sound.sfx('kick');
        b.owner = 'user';
        b.vx = 0; b.vy = 0;
        state.rating = Math.min(10, state.rating + 0.35);
        state.flashMsg = 'Great tackle!';
      }
    }

    const onKeyDown = (e) => {
      state.keys[e.key.toLowerCase()] = true;
      if (e.code === 'Space') { e.preventDefault(); kickOrTackle(); }
    };
    const onKeyUp = (e) => { state.keys[e.key.toLowerCase()] = false; };
    const onPointer = (e) => {
      const rect = canvas.getBoundingClientRect();
      state.pointerTarget = {
        x: ((e.clientX - rect.left) / rect.width) * W,
        y: ((e.clientY - rect.top) / rect.height) * H
      };
    };
    const onCanvasClick = () => kickOrTackle();

    window.addEventListener('keydown', onKeyDown);
    window.addEventListener('keyup', onKeyUp);
    canvas.addEventListener('pointermove', onPointer);
    canvas.addEventListener('pointerdown', (e) => { onPointer(e); kickOrTackle(); });
    $('#pitchActionBtn').onclick = kickOrTackle;
    $('#pitchSkipBtn').onclick = () => { state.elapsed = state.duration; };

    let lastT = performance.now();

    function frame(now) {
      if (!state.running) return;
      const dt = Math.min(0.05, (now - lastT) / 1000);
      lastT = now;
      state.elapsed += dt;

      const minute = Math.min(90, Math.floor((state.elapsed / state.duration) * 90));
      $('#pitchClock').textContent = minute + "'";

      // User movement
      let dx = 0, dy = 0;
      if (state.keys['arrowup'] || state.keys['w']) dy -= 1;
      if (state.keys['arrowdown'] || state.keys['s']) dy += 1;
      if (state.keys['arrowleft'] || state.keys['a']) dx -= 1;
      if (state.keys['arrowright'] || state.keys['d']) dx += 1;

      if (dx !== 0 || dy !== 0) {
        state.pointerTarget = null;
        const len = Math.hypot(dx, dy);
        state.user.x += (dx / len) * paceFactor;
        state.user.y += (dy / len) * paceFactor;
      } else if (state.pointerTarget) {
        const pdx = state.pointerTarget.x - state.user.x;
        const pdy = state.pointerTarget.y - state.user.y;
        const pdist = Math.hypot(pdx, pdy);
        if (pdist > 4) {
          state.user.x += (pdx / pdist) * paceFactor;
          state.user.y += (pdy / pdist) * paceFactor;
        }
      }

      state.user.x = Math.max(14, Math.min(W - 14, state.user.x));
      state.user.y = Math.max(14, Math.min(H - 14, state.user.y));

      // AI defenders & teammate patrol
      [state.mate, state.def1, state.def2].forEach((bot) => {
        bot.y += bot.vy;
        if (bot.y < 28 || bot.y > H - 28) bot.vy *= -1;
      });
      // GK tracks ball Y inside goal mouth
      state.gk.y += Math.sign(state.ball.y - state.gk.y) * 1.15;
      state.gk.y = Math.max(H / 2 - 34, Math.min(H / 2 + 34, state.gk.y));

      // Ball physics
      const b = state.ball;
      if (b.owner === 'user') {
        b.x = state.user.x + 11;
        b.y = state.user.y;
        // Defender steals if colliding closely
        for (const d of [state.def1, state.def2]) {
          if (Math.hypot(b.x - d.x, b.y - d.y) < 14) {
            b.owner = 'def';
            b.vx = -3.4;
            b.vy = (Math.random() - 0.5) * 2.5;
            state.flashMsg = 'Tackled!';
          }
        }
      } else {
        b.x += b.vx;
        b.y += b.vy;
        b.vx *= 0.985;
        b.vy *= 0.985;

        // User picks up loose ball
        if (Math.hypot(b.x - state.user.x, b.y - state.user.y) < 15 && Math.hypot(b.vx, b.vy) < 3.2) {
          b.owner = 'user';
        }

        // Teammate deflects shot into goal -> Assist!
        if (b.vx > 1.5 && Math.hypot(b.x - state.mate.x, b.y - state.mate.y) < 14) {
          b.vx = 5.2;
          b.vy = ((H / 2) - b.y) * 0.05;
          b.assisted = true;
        }

        // Defender blocks shot
        for (const d of [state.def1, state.def2]) {
          if (b.vx > 0 && Math.hypot(b.x - d.x, b.y - d.y) < 13) {
            b.vx = -2.8;
            b.vy = (Math.random() - 0.5) * 3;
          }
        }

        // GK save check
        if (b.vx > 0 && Math.hypot(b.x - state.gk.x, b.y - state.gk.y) < 13) {
          b.vx = -3.2;
          b.vy = (Math.random() - 0.5) * 4;
          state.flashMsg = 'Saved by keeper!';
        }

        // Goal scored on right (User's team)!
        if (b.x >= W - 8 && Math.abs(b.y - H / 2) < 38) {
          Sound.sfx('goal');
          if (b.assisted) {
            state.userAssists++;
            state.extraTeamGoals++;
            state.rating = Math.min(10, state.rating + 0.8);
            state.flashMsg = 'GOAL! Assisted by ' + p.name + '!';
          } else {
            state.userGoals++;
            state.rating = Math.min(10, state.rating + 1.1);
            state.flashMsg = 'GOOOAL! ' + p.name + ' scores!';
          }
          b.assisted = false;
          resetKickoff(true);
        } else if (b.x <= 8 && Math.abs(b.y - H / 2) < 38) {
          // Opponent counter goal
          state.oppGoals++;
          state.flashMsg = nf.clubName + ' score on the counter!';
          resetKickoff(true);
        } else {
          // Bounce off touchlines
          if (b.y < 10 || b.y > H - 10) b.vy *= -1;
          if (b.x < 10 || b.x > W - 10) {
            b.vx *= -0.7;
            b.x = Math.max(12, Math.min(W - 12, b.x));
          }
        }
      }

      // Update HUD
      const teamScore = state.userGoals + state.extraTeamGoals;
      const homeS = nf.isHome ? teamScore : state.oppGoals;
      const awayS = nf.isHome ? state.oppGoals : teamScore;
      $('#pitchScoreboard').textContent = `${homeS} - ${awayS}`;
      $('#pitchPlayerStats').textContent = `You: ${state.userGoals}G • ${state.userAssists}A • Rtg ${state.rating.toFixed(1)}`;

      // Render Pitch
      ctx.fillStyle = '#176b3a';
      ctx.fillRect(0, 0, W, H);
      // Stripes
      ctx.fillStyle = '#1d7a43';
      for (let sx = 0; sx < W; sx += 50) ctx.fillRect(sx, 0, 25, H);

      // Pitch lines
      ctx.strokeStyle = 'rgba(255,255,255,0.75)';
      ctx.lineWidth = 2;
      ctx.strokeRect(8, 8, W - 16, H - 16);
      ctx.beginPath(); ctx.moveTo(W / 2, 8); ctx.lineTo(W / 2, H - 8); ctx.stroke();
      ctx.beginPath(); ctx.arc(W / 2, H / 2, 32, 0, Math.PI * 2); ctx.stroke();
      // Penalty boxes & Goals
      ctx.strokeRect(8, H / 2 - 58, 48, 116);
      ctx.strokeRect(W - 56, H / 2 - 58, 48, 116);
      ctx.fillStyle = '#ffffff';
      ctx.fillRect(2, H / 2 - 36, 6, 72);
      ctx.fillRect(W - 8, H / 2 - 36, 6, 72);

      // Draw Players
      function drawDot(x, y, r, fill, stroke, label) {
        ctx.beginPath(); ctx.arc(x, y, r, 0, Math.PI * 2);
        ctx.fillStyle = fill; ctx.fill();
        ctx.lineWidth = 2; ctx.strokeStyle = stroke; ctx.stroke();
        if (label) {
          ctx.fillStyle = '#fff'; ctx.font = 'bold 9px sans-serif'; ctx.textAlign = 'center';
          ctx.fillText(label, x, y - r - 3);
        }
      }

      drawDot(state.mate.x, state.mate.y, state.mate.r, club.primaryColor, club.secondaryColor, 'TEAM');
      drawDot(state.def1.x, state.def1.y, state.def1.r, nf.primaryColor, nf.secondaryColor, 'DEF');
      drawDot(state.def2.x, state.def2.y, state.def2.r, nf.primaryColor, nf.secondaryColor, 'DEF');
      drawDot(state.gk.x, state.gk.y, state.gk.r, '#ffd166', '#111', 'GK');
      drawDot(state.user.x, state.user.y, state.user.r, club.primaryColor, '#4db8ff', 'YOU');

      // Ball
      ctx.beginPath(); ctx.arc(b.x, b.y, 4.5, 0, Math.PI * 2);
      ctx.fillStyle = '#ffffff'; ctx.fill();
      ctx.strokeStyle = '#111'; ctx.lineWidth = 1.5; ctx.stroke();

      if (state.flashMsg) {
        ctx.fillStyle = 'rgba(0,0,0,0.6)';
        ctx.fillRect(W / 2 - 110, 14, 220, 22);
        ctx.fillStyle = '#ffc233'; ctx.font = 'bold 12px sans-serif'; ctx.textAlign = 'center';
        ctx.fillText(state.flashMsg, W / 2, 29);
      }

      if (state.elapsed >= state.duration) {
        finishInteractiveMatch();
      } else {
        requestAnimationFrame(frame);
      }
    }

    async function finishInteractiveMatch() {
      state.running = false;
      window.removeEventListener('keydown', onKeyDown);
      window.removeEventListener('keyup', onKeyUp);
      Sound.sfx('whistle');
      setPitchSpeed(1);
      modal.hidden = true;

      try {
        const res = await api('/api/career/' + career.id + '/matchday', {
          method: 'POST',
          body: JSON.stringify({
            interactive: true,
            userGoals: state.userGoals,
            userAssists: state.userAssists,
            userRating: Number(state.rating.toFixed(1)),
            extraTeamGoals: state.extraTeamGoals
          })
        });
        career = res.career;
        if (res.match) $('#status').textContent = res.match.summary;
        renderHub();
      } catch (e) {
        $('#status').textContent = e.message;
      } finally {
        busy = false;
      }
    }

    requestAnimationFrame(frame);
  }

  // ---------- Complete Full Season ----------

  $('#playBtn').addEventListener('click', async () => {
    if (busy || !career) return;
    busy = true;
    const btn = $('#playBtn');
    btn.disabled = true;
    btn.textContent = 'Simulating season ' + (career.player.seasons + 1) + '...';
    $('#status').textContent = '';

    Sound.sfx('whistle');
    setPitchSpeed(6);
    try {
      const prevPeak = career.peak.value;
      const [res] = await Promise.all([
        api('/api/career/' + career.id + '/season', { method: 'POST' }),
        sleep(reduceMotion ? 150 : 1200)
      ]);
      career = res.career;
      const row = res.row;

      setPitchSpeed(1);
      Sound.sfx('whistle');
      applyPhase(career.player.age);
      $('#status').textContent = `Season ${row.season} finished (#${row.leaguePos} in league): ${row.goals} goals, ${row.assists} assists.`;
      if (row.value > prevPeak) setTimeout(() => Sound.sfx('ping'), 400);

      busy = false;
      if (career.player.age >= MAX_AGE) { await sleep(reduceMotion ? 0 : 1000); showRetired(); }
      else { renderHub(); refreshNews(); }
    } catch (e) {
      setPitchSpeed(1);
      busy = false;
      btn.disabled = false;
      renderHub();
      $('#status').textContent = e.message;
    }
  });

  $('#saveBtn').addEventListener('click', async () => {
    if (!career) return;
    const btn = $('#saveBtn');
    btn.disabled = true;
    try {
      await api('/api/career/' + career.id + '/save', { method: 'POST' });
      $('#status').textContent = 'Career saved.';
    } catch (e) {
      $('#status').textContent = 'Could not save: ' + e.message;
    } finally {
      btn.disabled = !services.php;
    }
  });

  $('#retireBtn').addEventListener('click', () => { if (!busy && career) showRetired(); });

  // ---------- Retired ----------

  function showRetired() {
    const p = career.player, t = career.totals;
    $('#retiredLead').textContent = `${p.name} (${p.nationality}) retired at ${p.age} after ${p.seasons} ${p.seasons === 1 ? 'season' : 'seasons'}.`;
    const stat = (label, value) => '<div><dt>' + label + '</dt><dd>' + value + '</dd></div>';
    $('#retiredStats').innerHTML =
      stat('Peak value', money(career.peak.value)) +
      stat('Peak at age', career.peak.age) +
      stat('Goals', t.goals) +
      stat('Assists', t.assists) +
      stat('Games', t.apps) +
      stat('Final overall', career.overall);
    const isGK = p.position === 'GK';
    const parts = [];
    for (let i = 0; i < 7; i++) { if (i === 6 && !isGK) continue; parts.push(ATTR_SHORT[i] + ' ' + p.attr[i]); }
    $('#retiredCard').textContent = 'Final attributes: ' + parts.join(', ');
    showScreen('retired');
  }

  $('#againBtn').addEventListener('click', async () => {
    career = null;
    document.body.dataset.phase = 'academy';
    Sound.setPhase('academy');
    showScreen('start');
    await refreshServices();
    loadSavesList();
  });

  // ---------- Music Button ----------

  const muteBtn = $('#muteBtn');
  function renderMute() {
    const on = Sound.isEnabled();
    muteBtn.setAttribute('aria-pressed', String(on));
    $('#muteLabel').textContent = on ? 'Music on' : 'Music off';
  }
  muteBtn.addEventListener('click', () => { Sound.toggle(); renderMute(); });
  renderMute();

  // ---------- Boot ----------

  Promise.all([initCatalog(), refreshServices()]).then(loadSavesList);
})();
