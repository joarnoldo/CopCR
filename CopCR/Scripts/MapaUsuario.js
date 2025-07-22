document.addEventListener("DOMContentLoaded", function () {
    var mapEl = document.getElementById("map");
    var currentUserId = parseInt(mapEl.dataset.userid, 10);

    var map = L.map("map").setView([9.7489, -83.7534], 8);
    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        maxZoom: 19,
        attribution: "© OpenStreetMap contributors"
    }).addTo(map);

    var incidentsLayer = L.layerGroup().addTo(map);
    var domicilioMarker, actualMarker, circle;
    var editarDomicilio = false;

    // Botón para activar edición de domicilio
    document.getElementById("btn-editar-domicilio")
        .addEventListener("click", function () {
            editarDomicilio = true;
            this.disabled = true;
            this.textContent = "Haz clic en el mapa…";
        });

    // Clic en el mapa para domicilio: solo si editarDomicilio == true
    map.on("click", function (e) {
        if (!editarDomicilio) return;
        var lat = e.latlng.lat, lng = e.latlng.lng;

        if (!confirm("¿Confirmas guardar esta ubicación como tu domicilio?")) {
            return;
        }

        // Actualizamos o creamos el marker
        if (!domicilioMarker) {
            domicilioMarker = L.marker([lat, lng]).addTo(map);
        } else {
            domicilioMarker.setLatLng([lat, lng]);
        }
        domicilioMarker.bindPopup("Domicilio Principal").openPopup();

        // Llamada al servidor
        saveDireccion(lat, lng, true);

        updateCircle();
        plotIncidents(window._mapaIncidents);

        // Desactivamos modo edición
        editarDomicilio = false;
        document.getElementById("btn-editar-domicilio").disabled = false;
        document.getElementById("btn-editar-domicilio").textContent = "Editar domicilio";
    });

    //Carga inicial de datos: domicilios, incidentes y categorías
    function loadData() {
        //Domicilio principal
        fetch('@Url.Action("GetDireccionPrincipal","IncidentesMapa")')
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    domicilioMarker = L.marker([data.lat, data.lng])
                        .addTo(map)
                        .bindPopup('Domicilio Principal')
                        .openPopup();
                    updateCircle();
                }
            });

        //Incidentes
        fetch('@Url.Action("GetIncidentes","IncidentesMapa")')
            .then(res => res.json())
            .then(incidents => {
                window._mapaIncidents = incidents;
                plotIncidents(incidents);
            });

        //Categorías
        fetch('@Url.Action("GetCategorias","IncidentesMapa")')
            .then(res => res.json())
            .then(cats => {
                var sel = document.getElementById('filter-category');
                cats.forEach(function (c) {
                    var opt = document.createElement('option');
                    opt.value = c.CategoriaIncidenteID;
                    opt.text = c.Nombre;
                    sel.appendChild(opt);
                });
            });
    }

    // 4) Helpers
    function updateCircle() {
        if (!domicilioMarker) return;
        var centerType = document.querySelector('input[name="radioCenter"]:checked').value;
        var center = (centerType === 'actual' && actualMarker)
            ? actualMarker.getLatLng()
            : domicilioMarker.getLatLng();
        var radius = parseInt(document.getElementById('filter-radius').value, 10) * 1000;
        if (circle) map.removeLayer(circle);
        circle = L.circle(center, { radius: radius, color: '#3388ff', weight: 1, fill: false })
            .addTo(map);
        map.setView(center);
    }

    function saveDireccion(lat, lng, isPrincipal) {
        fetch('@Url.Action("SaveDireccion","IncidentesMapa")', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Latitud: lat, Longitud: lng, IsDomicilioPrincipal: isPrincipal })
        })
            .then(res => res.json())
            .then(resp => {
                if (!resp.success) alert('Error al guardar la dirección');
            });
    }

    // 5) Eventos de UI
    document.getElementById('btn-actual').addEventListener('click', function () {
        if (!navigator.geolocation) {
            return alert('Tu navegador no soporta geolocalización');
        }
        navigator.geolocation.getCurrentPosition(function (pos) {
            var lat = pos.coords.latitude, lng = pos.coords.longitude;
            if (!actualMarker) {
                actualMarker = L.marker([lat, lng]).addTo(map).bindPopup('Ubicación Actual');
            } else {
                actualMarker.setLatLng([lat, lng]);
            }
            actualMarker.openPopup();
            saveDireccion(lat, lng, false);
            updateCircle();
            plotIncidents(window._mapaIncidents);
        }, function () {
            alert('No se pudo obtener tu ubicación');
        });
    });

    // Filtros (checkbox, select, range, radio)
    ['filter-own', 'filter-others', 'filter-category', 'filter-radius']
        .forEach(id => document.getElementById(id).addEventListener('input', function () {
            document.getElementById('radius-value').textContent =
                document.getElementById('filter-radius').value;
            updateCircle();
            plotIncidents(window._mapaIncidents);
        }));
    document.querySelectorAll('input[name="radioCenter"]')
        .forEach(el => el.addEventListener('change', function () {
            updateCircle();
            plotIncidents(window._mapaIncidents);
        }));

    //Click en mapa para domicilio
    map.on('click', function (e) {
        var lat = e.latlng.lat, lng = e.latlng.lng;
        if (!domicilioMarker) {
            domicilioMarker = L.marker([lat, lng]).addTo(map).bindPopup('Domicilio Principal');
        } else {
            domicilioMarker.setLatLng([lat, lng]);
        }
        domicilioMarker.openPopup();
        saveDireccion(lat, lng, true);
        updateCircle();
        plotIncidents(window._mapaIncidents);
    });

    loadData();
});
