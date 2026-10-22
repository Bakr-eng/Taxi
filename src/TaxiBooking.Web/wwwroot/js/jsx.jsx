(function () {
    // --- Grundinställningar --------------------------------------------------
    // PRISMODELL — måste alltid matcha FareCalculator.cs i Application (samma tre värden).
    const BASE_FARE = 50;        // Fast grundavgift, kr
    const PRICE_PER_KM = 15;     // Kr per km
    const PRICE_PER_MINUTE = 5;  // Kr per minuts körtid

    // Startvy: Nyköping (kan justeras om företaget flyttar)
    const map = L.map('bookingMap').setView([58.7526, 17.0089], 11);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>-bidragsgivare'
    }).addTo(map);

    let pickupMarker = null;
    let dropoffMarker = null;
    let routeLine = null;

    const pickupInput = document.getElementById('pickupInput');
    const dropoffInput = document.getElementById('dropoffInput');
    const pickupSuggestions = document.getElementById('pickupSuggestions');
    const dropoffSuggestions = document.getElementById('dropoffSuggestions');
    const pickupLat = document.getElementById('pickupLat');
    const pickupLng = document.getElementById('pickupLng');
    const dropoffLat = document.getElementById('dropoffLat');
    const dropoffLng = document.getElementById('dropoffLng');
    const distanceKmField = document.getElementById('distanceKm');
    const durationMinutesField = document.getElementById('durationMinutes');
    const priceBox = document.getElementById('priceEstimateBox');
    const distanceText = document.getElementById('distanceText');
    const priceBase = document.getElementById('priceBase');
    const priceKmPart = document.getElementById('priceKmPart');
    const priceMinPart = document.getElementById('priceMinPart');
    const priceText = document.getElementById('priceText');

    function formatKr(v) {
        return Math.round(v) + ' kr';
    }

    function debounce(fn, delay) {
        let timer;
        return function (...args) {
            clearTimeout(timer);
            timer = setTimeout(() => fn.apply(this, args), delay);
        };
    }

    // --- Adressökning (gratis, via OpenStreetMap Nominatim) ------------------
    async function searchAddress(query) {
        if (!query || query.trim().length < 3) return [];
        const url = 'https://nominatim.openstreetmap.org/search?format=jsonv2&limit=5&countrycodes=se&q=' + encodeURIComponent(query);
        try {
            const res = await fetch(url, { headers: { 'Accept-Language': 'sv' } });
            if (!res.ok) return [];
            return await res.json();
        } catch (e) {
            return [];
        }
    }

    function showSuggestions(container, results, onPick) {
        container.innerHTML = '';
        if (!results.length) {
            container.classList.add('d-none');
            return;
        }
        results.forEach(r => {
            const item = document.createElement('div');
            item.className = 'autocomplete-item';
            item.textContent = r.display_name;
            item.addEventListener('click', () => onPick(r));
            container.appendChild(item);
        });
        container.classList.remove('d-none');
    }

    function hideSuggestions(container) {
        container.classList.add('d-none');
        container.innerHTML = '';
    }

    function wireAutocomplete(input, container, latField, lngField, isPickup) {
        const doSearch = debounce(async () => {
            const results = await searchAddress(input.value);
            showSuggestions(container, results, (r) => {
                input.value = r.display_name;
                latField.value = r.lat;
                lngField.value = r.lon;
                hideSuggestions(container);
                placeMarker(isPickup, parseFloat(r.lat), parseFloat(r.lon));
                updateRoute();
            });
        }, 600);

        input.addEventListener('input', () => {
            // Adressen redigeras för hand igen -> tidigare koordinater/rutt/pris gäller inte längre
            latField.value = '';
            lngField.value = '';
            distanceKmField.value = '';
            durationMinutesField.value = '';
            priceBox.classList.add('d-none');
            doSearch();
        });
        input.addEventListener('blur', () => {
            setTimeout(() => hideSuggestions(container), 200);
        });
    }

    function placeMarker(isPickup, lat, lon) {
        const latlng = [lat, lon];
        if (isPickup) {
            if (pickupMarker) { pickupMarker.setLatLng(latlng); }
            else { pickupMarker = L.marker(latlng).addTo(map).bindPopup('Upphämtning'); }
        } else {
            if (dropoffMarker) { dropoffMarker.setLatLng(latlng); }
            else { dropoffMarker = L.marker(latlng).addTo(map).bindPopup('Avlämning'); }
        }
        map.setView(latlng, Math.max(map.getZoom(), 12));
    }

    // --- Rutt och avstånd (gratis, via OSRM:s publika demo-server) -----------
    async function updateRoute() {
        if (!pickupLat.value || !pickupLng.value || !dropoffLat.value || !dropoffLng.value) {
            return;
        }
        const url = `https://router.project-osrm.org/route/v1/driving/${pickupLng.value},${pickupLat.value};${dropoffLng.value},${dropoffLat.value}?overview=full&geometries=geojson`;
        try {
            const res = await fetch(url);
            if (!res.ok) return;
            const data = await res.json();
            if (!data.routes || !data.routes.length) return;

            const route = data.routes[0];
            const km = route.distance / 1000;
            const minutes = route.duration / 60;
            distanceKmField.value = km.toFixed(2);
            durationMinutesField.value = minutes.toFixed(1);

            const coords = route.geometry.coordinates.map(c => [c[1], c[0]]);
            if (routeLine) { map.removeLayer(routeLine); }
            routeLine = L.polyline(coords, { color: '#1a1a2e', weight: 4 }).addTo(map);
            map.fitBounds(routeLine.getBounds(), { padding: [30, 30] });

            const kmCost = PRICE_PER_KM * km;
            const minCost = PRICE_PER_MINUTE * minutes;
            const total = BASE_FARE + kmCost + minCost;

            const minutesRounded = Math.round(minutes);
            distanceText.textContent = km.toFixed(1).replace('.', ',') + ' km, ca ' + minutesRounded + ' min';
            priceBase.textContent = formatKr(BASE_FARE);
            priceKmPart.textContent = km.toFixed(1).replace('.', ',') + ' km × ' + PRICE_PER_KM + ' kr = ' + formatKr(kmCost);
            priceMinPart.textContent = minutesRounded + ' min × ' + PRICE_PER_MINUTE + ' kr = ' + formatKr(minCost);
            priceText.textContent = formatKr(total);
            priceBox.classList.remove('d-none');
        } catch (e) {
            // Om ruttjänsten inte svarar visas inget live-pris - servern räknar då
            // ett grovt uppskattat pris istället när bokningen skickas in.
        }
    }

    wireAutocomplete(pickupInput, pickupSuggestions, pickupLat, pickupLng, true);
    wireAutocomplete(dropoffInput, dropoffSuggestions, dropoffLat, dropoffLng, false);

    document.addEventListener('click', (e) => {
        if (!pickupInput.contains(e.target) && !pickupSuggestions.contains(e.target)) hideSuggestions(pickupSuggestions);
        if (!dropoffInput.contains(e.target) && !dropoffSuggestions.contains(e.target)) hideSuggestions(dropoffSuggestions);
    });
})();