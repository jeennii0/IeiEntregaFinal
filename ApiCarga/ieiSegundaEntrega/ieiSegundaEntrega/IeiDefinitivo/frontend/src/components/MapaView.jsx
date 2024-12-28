import React from "react";
import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import "leaflet/dist/leaflet.css";

// Componente InfoMapPopup
function InfoMapPopup({ monumento }) {
  return (
    <div>
      <h4>{monumento.Nombre}</h4>
      <p>
        {monumento.TipoMonumento} <br />
        {monumento.Direccion} <br />
        {monumento.Localidad} <br />
        {monumento.CodigoPostal} <br />
        {monumento.Provincia} <br />
        {monumento.Descripcion} <br />
      </p>
    </div>
  );
}

const MapaView = ({ monumentos }) => {
  const handleMapClick = (e) => {
    e.stopPropagation(); // Detiene la propagación del clic
  };

  // Definir valores predeterminados para centro y zoom
  const defaultCenter = [40.4168, -3.7034]; // Coordenadas de ejemplo (Madrid)
  const defaultZoom = 13; // Nivel de zoom predeterminado

  return (
    <MapContainer
      className="map-container"
      center={defaultCenter} // Establecer centro predeterminado
      zoom={5} // Establecer nivel de zoom predeterminado
      style={{ width: "100%", height: "100%" }}
      onClick={handleMapClick}
    >
      <TileLayer
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
      />

      {/* Mapea los monumentos y coloca un marcador para cada uno */}
      {monumentos.map((monumento) => (
        <Marker
          key={monumento.id}
          position={[monumento.Latitud, monumento.Longitud]}
        >
          <Popup>
            <InfoMapPopup monumento={monumento} />
          </Popup>
        </Marker>
      ))}
    </MapContainer>
  );
};

export default MapaView;
