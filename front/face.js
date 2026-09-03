// ================================================================
// CONFIG
// Point this at the real backend once Person 3/4's API is running.
// Every endpoint below matches the documented API:
//   POST   /auth/register
//   POST   /auth/login
//   GET    /hotels?city=...
//   GET    /hotels/{id}                (expected to include roomTypes[])
//   GET    /room-types/{id}/availability?from=...&to=...
//   POST   /bookings
//   GET    /me/bookings
//   PATCH  /bookings/{id}/cancel
// ================================================================
const API_BASE_URL = 'https://localhost:5001/api'; // TODO: replace with your team's real backend URL

// --- STATE (all populated from the backend, nothing hardcoded) ---
let currentUser = null;   // { id, name, email } from /auth/login
let authToken = null;     // JWT returned by /auth/login, sent as Authorization: Bearer <token>
let currentSearch = { city: '', checkIn: '', checkOut: '', guests: 2, rooms: 1 };
let myBookings = [];      // filled by loadMyBookings() -> GET /me/bookings
let allHotelsCache = [];  // filled by loadAllHotels() -> GET /hotels, used for Home/city list/deals

// ================================================================
// API HELPER
// ================================================================
async function apiFetch(path, options = {}) {
    const headers = { 'Content-Type': 'application/json', ...(options.headers || {}) };
    if (authToken) headers['Authorization'] = `Bearer ${authToken}`;

    let res;
    try {
        res = await fetch(`${API_BASE_URL}${path}`, { ...options, headers });
    } catch (networkErr) {
        throw new Error('Could not reach the server. Check the backend is running and API_BASE_URL is correct.');
    }

    if (!res.ok) {
        let message = `Request failed (${res.status})`;
        try {
            const errBody = await res.json();
            if (errBody && (errBody.message || errBody.title)) message = errBody.message || errBody.title;
        } catch (_) { /* response wasn't JSON, keep default message */ }
        throw new Error(message);
    }
    if (res.status === 204) return null;
    try {
        return await res.json();
    } catch (_) {
        return null;
    }
}

// Small helper: pull a field that the backend might return in either
// camelCase or snake_case, without guessing at content.
function pick(obj, ...keys) {
    for (const k of keys) {
        if (obj && obj[k] !== undefined && obj[k] !== null) return obj[k];
    }
    return undefined;
}

// ================================================================
// DATE PICKERS (dd/mm/yyyy, self-contained — no external library required)
// ================================================================
let checkinPicker, checkoutPicker;

function startOfDay(date) {
    const d = new Date(date);
    d.setHours(0, 0, 0, 0);
    return d;
}

function isSameDay(a, b) {
    return a && b && a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
}

function formatDisplay(date) {
    const d = String(date.getDate()).padStart(2, '0');
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const y = date.getFullYear();
    return `${d}/${m}/${y}`;
}

function createDatePicker(inputId, options) {
    const input = document.getElementById(inputId);
    let selected = null;
    let viewDate = new Date();
    let popover = null;

    function open() {
        closeAllDatePickers();
        viewDate = selected ? new Date(selected) : new Date();
        popover = document.createElement('div');
        popover.className = 'date-popover';
        document.body.appendChild(popover);
        render();
        position();
        document.addEventListener('mousedown', onOutsideClick, true);
        window.addEventListener('resize', position);
        window.addEventListener('scroll', position, true);
    }

    function position() {
        if (!popover) return;
        const rect = input.getBoundingClientRect();
        popover.style.top = (window.scrollY + rect.bottom + 6) + 'px';
        popover.style.left = (window.scrollX + rect.left) + 'px';
    }

    function close() {
        if (popover) {
            popover.remove();
            popover = null;
            document.removeEventListener('mousedown', onOutsideClick, true);
            window.removeEventListener('resize', position);
            window.removeEventListener('scroll', position, true);
        }
    }

    function onOutsideClick(e) {
        if (popover && !popover.contains(e.target) && e.target !== input) close();
    }

    function render() {
        const year = viewDate.getFullYear();
        const month = viewDate.getMonth();
        const monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        const firstDay = new Date(year, month, 1);
        const startWeekday = firstDay.getDay();
        const daysInMonth = new Date(year, month + 1, 0).getDate();
        const minDate = options.getMinDate ? startOfDay(options.getMinDate()) : startOfDay(new Date());
        const today = startOfDay(new Date());
        const thisYear = today.getFullYear();
        const yearOptions = [];
        for (let y = thisYear; y <= thisYear + 5; y++) yearOptions.push(y);

        let html = `
            <div class="dp-header">
                <button type="button" class="dp-nav" data-dir="-1">‹</button>
                <span class="dp-title-group">
                    <span class="dp-title">${monthNames[month]}</span>
                    <select class="dp-year-select">
                        ${yearOptions.map(y => `<option value="${y}" ${y === year ? 'selected' : ''}>${y}</option>`).join('')}
                    </select>
                </span>
                <button type="button" class="dp-nav" data-dir="1">›</button>
            </div>
            <div class="dp-weekdays">${['S', 'M', 'T', 'W', 'T', 'F', 'S'].map(d => `<span>${d}</span>`).join('')}</div>
            <div class="dp-days">`;

        for (let i = 0; i < startWeekday; i++) html += `<span class="dp-day dp-empty"></span>`;

        for (let d = 1; d <= daysInMonth; d++) {
            const dateObj = new Date(year, month, d);
            const disabled = dateObj < minDate;
            const cls = ['dp-day'];
            if (disabled) cls.push('dp-disabled');
            if (isSameDay(dateObj, selected)) cls.push('dp-selected');
            if (isSameDay(dateObj, today)) cls.push('dp-today');
            html += `<span class="${cls.join(' ')}" data-day="${d}">${d}</span>`;
        }
        html += `</div>`;
        popover.innerHTML = html;

        popover.querySelectorAll('.dp-nav').forEach(btn => {
            btn.addEventListener('click', () => {
                viewDate.setMonth(viewDate.getMonth() + parseInt(btn.dataset.dir, 10));
                render();
            });
        });
        popover.querySelector('.dp-year-select').addEventListener('change', (e) => {
            viewDate.setFullYear(parseInt(e.target.value, 10));
            render();
        });
        popover.querySelectorAll('.dp-day:not(.dp-empty):not(.dp-disabled)').forEach(el => {
            el.addEventListener('click', () => {
                selected = new Date(year, month, parseInt(el.dataset.day, 10));
                input.value = formatDisplay(selected);
                close();
                if (options.onSelect) options.onSelect(selected);
            });
        });
    }

    input.addEventListener('click', open);

    const controller = {
        getDate: () => selected,
        clear: () => { selected = null; input.value = ''; },
        _close: close
    };
    openDatePickers.push(controller);
    return controller;
}

const openDatePickers = [];
function closeAllDatePickers() {
    openDatePickers.forEach(p => p._close());
}

function initDatePickers() {
    checkinPicker = createDatePicker('search-checkin', {
        getMinDate: () => new Date(),
        onSelect: (date) => {
            const co = checkoutPicker.getDate();
            if (co && co <= date) checkoutPicker.clear();
        }
    });

    checkoutPicker = createDatePicker('search-checkout', {
        getMinDate: () => {
            const ci = checkinPicker.getDate();
            if (ci) {
                const next = new Date(ci);
                next.setDate(next.getDate() + 1);
                return next;
            }
            return new Date();
        }
    });
}

function toISODate(date) {
    if (!date) return '';
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
}

function toDisplayDate(isoStr) {
    if (!isoStr) return '';
    const [y, m, d] = isoStr.split('T')[0].split('-');
    return `${d}/${m}/${y}`;
}

// ================================================================
// HOME PAGE / HOTEL LIST (all from GET /hotels, cached client-side)
// ================================================================
function roomTypeLabel(capacity) {
    if (capacity <= 1) return 'Single';
    if (capacity === 2) return 'Double';
    if (capacity === 3) return 'Triple';
    return 'Quad';
}

function bedsLabel(capacity) {
    if (capacity <= 1) return '1 Single Bed';
    if (capacity === 2) return '1 Double Bed';
    if (capacity === 3) return '1 Double Bed + 1 Single Bed';
    return '2 Double Beds';
}

function hotelImage(hotel) {
    // Real backend-provided image only — no random/generic stock photo fallback.
    return pick(hotel, 'thumbnailUrl', 'thumbnail_url') || '';
}

function starsLabel(hotel) {
    const stars = pick(hotel, 'stars');
    return stars ? '★'.repeat(stars) + '☆'.repeat(Math.max(0, 5 - stars)) : '';
}

function buildHotelCard(hotel) {
    const img = hotelImage(hotel);
    const imgTag = img
        ? `<img src="${img}" alt="${hotel.name}" loading="lazy">`
        : `<div class="image-placeholder">No photo yet</div>`;
    const price = pick(hotel, 'startingPrice', 'starting_price');
    return `
        <div class="hotel-item">
            <div class="hotel-item-image">${imgTag}</div>
            <div class="hotel-item-body">
                <span class="hotel-city-tag">${hotel.city}</span>
                <h3 class="guest m-0">${hotel.name}</h3>
                ${starsLabel(hotel) ? `<div class="hotel-rating">${starsLabel(hotel)}</div>` : ''}
                <p style="font-size: 14px;">${hotel.description || ''}</p>
                ${price ? `<div class="hotel-price">From $${price} <span class="muted" style="font-size:12.5px; font-weight:400;">/ night</span></div>` : ''}
                <button class="btn-custom btn-ghost w-100 mt-2" onclick="viewDetails(${hotel.id})">View Rooms</button>
            </div>
        </div>
    `;
}

async function loadAllHotels() {
    document.getElementById('trending-list').innerHTML = `<p class="muted">Loading hotels…</p>`;
    document.getElementById('random-list').innerHTML = '';
    try {
        allHotelsCache = await apiFetch('/hotels');
        populateCityOptions();
        loadTrendingHotels();
        loadRandomHotels();
        loadDeals();
    } catch (err) {
        allHotelsCache = [];
        document.getElementById('trending-list').innerHTML = `<p class="muted">${err.message}</p>`;
        document.getElementById('random-list').innerHTML = '';
        document.getElementById('deals-list').innerHTML = `<p class="muted">${err.message}</p>`;
    }
}

function populateCityOptions() {
    const select = document.getElementById('search-city');
    select.innerHTML = '<option value="">Select a city</option>';
    const cities = [...new Set(allHotelsCache.map(h => h.city))].sort();
    cities.forEach(city => {
        const opt = document.createElement('option');
        opt.value = city;
        opt.textContent = city;
        select.appendChild(opt);
    });
}

function loadTrendingHotels() {
    // "Trending" = highest hotel star rating in the real data — not a fabricated flag.
    const trending = [...allHotelsCache]
        .filter(h => pick(h, 'stars'))
        .sort((a, b) => b.stars - a.stars)
        .slice(0, 4);
    document.getElementById('trending-list').innerHTML = trending.length
        ? trending.map(buildHotelCard).join('')
        : `<p class="muted">No hotels yet.</p>`;
}

function loadRandomHotels() {
    const shuffled = [...allHotelsCache].sort(() => Math.random() - 0.5);
    const picks = shuffled.slice(0, 4);
    document.getElementById('random-list').innerHTML = picks.length
        ? picks.map(buildHotelCard).join('')
        : '';
}

document.addEventListener('DOMContentLoaded', () => {
    const shuffleBtn = document.getElementById('shuffle-btn');
    if (shuffleBtn) shuffleBtn.addEventListener('click', loadRandomHotels);
});

// ================================================================
// DEALS & OFFERS — same card layout as Trending Hotels, built from
// real hotels in allHotelsCache (not static placeholder text).
// ================================================================
let selectedDeal = null;

function loadDeals() {
    const container = document.getElementById('deals-list');
    if (!allHotelsCache.length) {
        container.innerHTML = `<p class="muted">No deals available right now.</p>`;
        return;
    }

    const dealDefs = [
        { id: 'nile-season', title: 'Early Nile Season', blurb: 'Save on stays before the season starts.', filter: h => h.city === 'Cairo' || h.city === 'Luxor' },
        { id: 'red-sea', title: 'Red Sea Getaway', blurb: 'Discounted sea-view stays.', filter: h => h.city === 'Hurghada' || h.city === 'Sharm El Sheikh' },
        { id: 'extended-stay', title: 'Extended Stay', blurb: 'Reduced nightly rate for 5+ night stays.', filter: () => true }
    ];

    const cards = dealDefs.map(def => {
        const matches = allHotelsCache.filter(def.filter);
        if (!matches.length) return '';
        const hotel = matches[Math.floor(Math.random() * matches.length)];
        return buildDealCard(def, hotel);
    }).filter(Boolean);

    container.innerHTML = cards.length ? cards.join('') : `<p class="muted">No deals available right now.</p>`;
}

function buildDealCard(def, hotel) {
    const img = hotelImage(hotel);
    const imgTag = img
        ? `<img src="${img}" alt="${hotel.name}" loading="lazy">`
        : `<div class="image-placeholder">No photo yet</div>`;
    return `
        <div class="hotel-item deal-card" data-deal-id="${def.id}">
            <div class="hotel-item-image">${imgTag}</div>
            <div class="hotel-item-body">
                <span class="hotel-city-tag">${def.title}</span>
                <h3 class="guest m-0">${hotel.name}</h3>
                <p style="font-size: 14px;">${def.blurb}</p>
                <p class="muted" style="font-size:12.5px;">${hotel.city}${starsLabel(hotel) ? ' · ' + starsLabel(hotel) : ''}</p>
                <div class="deal-card-actions">
                    <button type="button" class="btn-custom btn-ghost btn-sm-custom" onclick="viewDetails(${hotel.id})">View Hotel</button>
                    <button type="button" class="btn-custom btn-brass btn-sm-custom" onclick="selectDeal('${def.id}', '${def.title}')">Select this deal</button>
                </div>
            </div>
        </div>
    `;
}

function selectDeal(dealId, dealName) {
    document.querySelectorAll('.deal-card').forEach(c => c.classList.remove('selected'));

    if (selectedDeal === dealId) {
        selectedDeal = null;
        showToast('Deal removed');
        return;
    }

    selectedDeal = dealId;
    const card = document.querySelector(`.deal-card[data-deal-id="${dealId}"]`);
    if (card) card.classList.add('selected');
    showToast(`Selected: ${dealName}`);
}

// ================================================================
// NAVIGATION
// ================================================================
function navigate(viewName) {
    document.querySelectorAll('.section').forEach(sec => sec.classList.remove('active'));
    document.querySelectorAll('.nav a').forEach(nav => nav.classList.remove('active'));

    document.getElementById(`view-${viewName}`).classList.add('active');
    const navLink = document.getElementById(`nav-${viewName}`);
    if (navLink) navLink.classList.add('active');

    if (viewName === 'bookings') loadMyBookings();
    if (viewName === 'home') { loadTrendingHotels(); loadRandomHotels(); }
}

// ================================================================
// AUTHENTICATION — POST /auth/login (real request, real JWT)
// ================================================================
document.getElementById('login-form').addEventListener('submit', async function(e) {
    e.preventDefault();
    const email = document.getElementById('login-email').value;
    const password = document.getElementById('login-password').value;
    const submitBtn = e.target.querySelector('button[type="submit"]');
    const originalLabel = submitBtn.textContent;
    submitBtn.disabled = true;
    submitBtn.textContent = 'Logging in…';

    try {
        const data = await apiFetch('/auth/login', {
            method: 'POST',
            body: JSON.stringify({ email, password })
        });
        authToken = pick(data, 'token', 'accessToken');
        currentUser = pick(data, 'user') || { email };

        document.getElementById('nav-login').style.display = 'none';
        document.getElementById('nav-logout').style.display = 'flex';

        const displayName = pick(currentUser, 'name', 'email') || email;
        showToast(`Welcome back, ${displayName.split('@')[0]}`);
        navigate('search');
    } catch (err) {
        showToast(err.message);
    } finally {
        submitBtn.disabled = false;
        submitBtn.textContent = originalLabel;
    }
});

function logout() {
    currentUser = null;
    authToken = null;
    myBookings = [];
    document.getElementById('nav-login').style.display = 'flex';
    document.getElementById('nav-logout').style.display = 'none';
    showToast('Logged out successfully');
    navigate('home');
}

// ================================================================
// SEARCH & HOTELS — GET /hotels?city=...
// ================================================================
document.getElementById('search-btn').addEventListener('click', async function() {
    const city = document.getElementById('search-city').value;
    const guests = parseInt(document.getElementById('search-guests').value, 10);
    const roomsCount = parseInt(document.getElementById('search-rooms').value, 10);
    const checkInDate = checkinPicker.getDate();
    const checkOutDate = checkoutPicker.getDate();

    if (!city) { showToast('Please choose a destination'); return; }
    if (!checkInDate || !checkOutDate) { showToast('Please choose check-in and check-out dates'); return; }
    if (checkOutDate <= checkInDate) { showToast('Check-out must be after check-in'); return; }

    currentSearch.city = city;
    currentSearch.guests = guests;
    currentSearch.rooms = roomsCount;
    currentSearch.checkIn = toISODate(checkInDate);
    currentSearch.checkOut = toISODate(checkOutDate);

    const btn = this;
    const originalHtml = btn.innerHTML;
    btn.disabled = true;
    btn.textContent = 'Searching…';

    const container = document.getElementById('search-results-container');
    const list = document.getElementById('hotel-list');
    container.style.display = 'block';
    list.innerHTML = `<p class="muted">Loading hotels…</p>`;

    try {
        const results = await apiFetch(`/hotels?city=${encodeURIComponent(city)}&checkIn=${currentSearch.checkIn}&checkOut=${currentSearch.checkOut}&guests=${guests}&rooms=${roomsCount}`);

        document.getElementById('results-title').textContent = `Hotels in ${currentSearch.city}`;
        document.getElementById('results-count').textContent = `${results.length} propert${results.length === 1 ? 'y' : 'ies'} found`;

        list.innerHTML = results.length
            ? results.map(buildHotelCard).join('')
            : `<p class="muted">No hotels found in ${currentSearch.city} yet.</p>`;
    } catch (err) {
        list.innerHTML = `<p class="muted">${err.message}</p>`;
    } finally {
        btn.disabled = false;
        btn.innerHTML = originalHtml;
    }
});

// ================================================================
// HOTEL DETAILS — GET /hotels/{id} (expected to include roomTypes[])
// ================================================================
async function viewDetails(hotelId) {
    navigate('details');
    document.getElementById('detail-title').textContent = 'Loading…';
    document.getElementById('detail-location').textContent = '';
    document.getElementById('detail-desc').textContent = '';
    document.getElementById('detail-hero').src = '';
    document.getElementById('room-list').innerHTML = `<p class="muted">Loading rooms…</p>`;

    try {
        const hotel = await apiFetch(`/hotels/${hotelId}`);

        document.getElementById('detail-title').textContent = hotel.name;
        document.getElementById('detail-location').innerHTML =
            `${hotel.city}${starsLabel(hotel) ? ' &nbsp;·&nbsp; ' + starsLabel(hotel) : ''}`;
        document.getElementById('detail-desc').textContent = hotel.description || '';
        const heroImg = hotelImage(hotel);
        document.getElementById('detail-hero').src = heroImg;
        document.getElementById('detail-hero').alt = hotel.name;

        const roomTypes = pick(hotel, 'roomTypes', 'room_types') || [];
        const grid = document.getElementById('room-list');
        grid.innerHTML = '';

        if (!roomTypes.length) {
            grid.innerHTML = `<p class="muted">No room types published for this hotel yet.</p>`;
            return;
        }

        roomTypes.forEach(room => {
            const capacity = pick(room, 'capacity') || 2;
            const type = pick(room, 'bedType', 'bed_type') || roomTypeLabel(capacity);
            const beds = bedsLabel(capacity);
            const price = pick(room, 'basePrice', 'base_price');
            const roomImg = pick(room, 'imageUrl', 'image_url') || heroImg; // real photo if backend provides one, else the hotel's own photo
            const roomImgTag = roomImg
                ? `<img src="${roomImg}" alt="${room.name}" loading="lazy">`
                : `<div class="image-placeholder">No photo yet</div>`;
            const fitsGuests = capacity >= currentSearch.guests;
            const fitNote = fitsGuests ? '' : `<div class="room-fit-warning">Fits fewer guests than selected</div>`;

            grid.innerHTML += `
                <div class="room-card">
                    <div class="room-card-image">${roomImgTag}</div>
                    <div class="room-card-body">
                        <div class="room-card-top">
                            <h3 class="guest m-0">${room.name}</h3>
                            <span class="badge-room-type">${roomTypeLabel(capacity)}</span>
                        </div>
                        <p class="room-desc">${room.description || ''}</p>
                        <div class="room-meta">
                            <span>🛏️ ${beds}</span>
                            <span>👤 Up to ${capacity} guest${capacity === 1 ? '' : 's'}</span>
                        </div>
                        ${fitNote}
                        <div class="room-card-footer">
                            <div class="room-price">${price != null ? `$${price}` : '—'} <span class="muted">/ night</span></div>
                            <button class="btn-custom btn-brass btn-sm-custom" onclick="bookRoom(${hotel.id}, ${room.id}, '${hotel.name.replace(/'/g, "\\'")}', '${room.name.replace(/'/g, "\\'")}', ${price})">Book Now</button>
                        </div>
                    </div>
                </div>
            `;
        });
    } catch (err) {
        document.getElementById('detail-title').textContent = 'Could not load hotel';
        document.getElementById('room-list').innerHTML = `<p class="muted">${err.message}</p>`;
    }
}

// ================================================================
// BOOKING — POST /bookings (server computes nights + total price)
// ================================================================
async function bookRoom(hotelId, roomId, hotelName, roomName, price) {
    if (!currentUser) {
        showToast("Please login first");
        navigate('login');
        return;
    }
    if (!currentSearch.checkIn || !currentSearch.checkOut) {
        showToast("Please choose your dates first");
        navigate('search');
        return;
    }

    try {
        await apiFetch('/bookings', {
            method: 'POST',
            body: JSON.stringify({
                hotelId: hotelId,
                roomTypeId: roomId,
                checkIn: currentSearch.checkIn,
                checkOut: currentSearch.checkOut,
                guests: currentSearch.guests,
                rooms: currentSearch.rooms
            })
        });
        showToast('Reservation request submitted');
        navigate('bookings'); // loadMyBookings() re-fetches from the server, so the new PENDING booking shows up
    } catch (err) {
        showToast(err.message);
    }
}

// ================================================================
// MY BOOKINGS — GET /me/bookings, PATCH /bookings/{id}/cancel
// ================================================================
async function loadMyBookings() {
    const tbody = document.getElementById('bookings-body');

    if (!currentUser) {
        tbody.innerHTML = `<tr><td colspan="5" style="text-align:center;" class="muted">Please login to view bookings.</td></tr>`;
        return;
    }

    tbody.innerHTML = `<tr><td colspan="5" style="text-align:center;" class="muted">Loading your bookings…</td></tr>`;

    try {
        myBookings = await apiFetch('/me/bookings');
        renderBookings();
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="5" style="text-align:center;" class="muted">${err.message}</td></tr>`;
    }
}

function renderBookings() {
    const tbody = document.getElementById('bookings-body');
    tbody.innerHTML = '';

    if (!myBookings.length) {
        tbody.innerHTML = `<tr><td colspan="5" style="text-align:center;" class="muted">No bookings yet.</td></tr>`;
        return;
    }

    myBookings.forEach(b => {
        const status = pick(b, 'status') || 'PENDING';
        let badgeClass = 'badge-cancelled';
        if (status === 'PENDING') badgeClass = 'badge-pending';
        if (status === 'CONFIRMED') badgeClass = 'badge-confirmed';
        if (status === 'REJECTED') badgeClass = 'badge-rejected';

        const cancelBtn = (status === 'CONFIRMED')
            ? `<button class="btn-custom btn-ghost btn-sm-custom" onclick="cancelBooking(${b.id})">Cancel</button>`
            : '<span class="muted">—</span>';

        const checkIn = pick(b, 'checkIn', 'check_in');
        const checkOut = pick(b, 'checkOut', 'check_out');
        const dateStr = `${toDisplayDate(checkIn)} → ${toDisplayDate(checkOut)}`;

        const hotelName = pick(b, 'hotelName') || pick(b.hotel || {}, 'name') || 'Hotel';
        const roomName = pick(b, 'roomName') || pick(b.roomType || {}, 'name') || 'Room';
        const guests = pick(b, 'guests');
        const rooms = pick(b, 'rooms');
        const detailBits = [guests ? `${guests} guest${guests === 1 ? '' : 's'}` : '', rooms ? `${rooms} room${rooms === 1 ? '' : 's'}` : ''].filter(Boolean);
        const detailLine = detailBits.length ? `<div class="muted">${detailBits.join(' · ')}</div>` : '';

        const total = pick(b, 'totalPrice', 'total_price');

        tbody.innerHTML += `
            <tr>
                <td><div class="guest">${hotelName}</div><div class="muted">${roomName}</div>${detailLine}</td>
                <td>${dateStr}</td>
                <td>${total != null ? '$' + total : '—'}</td>
                <td><span class="badge-custom ${badgeClass}">${status}</span></td>
                <td>${cancelBtn}</td>
            </tr>
        `;
    });
}

async function cancelBooking(id) {
    if (!confirm("Cancel this confirmed booking?")) return;
    try {
        await apiFetch(`/bookings/${id}/cancel`, { method: 'PATCH' });
        showToast("Booking cancelled");
        loadMyBookings();
    } catch (err) {
        showToast(err.message);
    }
}

// ================================================================
// TOAST NOTIFICATIONS
// ================================================================
let toastTimer;
function showToast(msg) {
    const t = document.getElementById('toast');
    t.textContent = msg;
    t.classList.add('show');
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => t.classList.remove('show'), 2500);
}

// ================================================================
// INIT
// ================================================================
initDatePickers();
loadAllHotels();