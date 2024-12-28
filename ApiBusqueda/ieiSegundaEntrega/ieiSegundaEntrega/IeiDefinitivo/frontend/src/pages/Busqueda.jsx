import React, { useState, useEffect } from "react";
import ArrowLeft from "../assets/Arrow left.png";
import { useNavigate } from "react-router-dom";
import MapaView from "../components/MapaView";
function TablaResultado({ monumento }) {
  return (
    <tr>
      <td>{monumento.Nombre}</td>
      <td>{monumento.TipoMonumento}</td>
      <td>{monumento.Direccion}</td>
      <td>{monumento.Localidad}</td>
      <td>{monumento.CodigoPostal}</td>
      <td>{monumento.Provincia}</td>
      <td>{monumento.Descripcion}</td>
    </tr>
  );
}

export default function Busqueda() {
  const navigate = useNavigate();
  const [totalMonumentos, setTotalMonumentos] = useState([]);
  const [resultadoBusqueda, setResultadoBusqueda] = useState([]);

  const [formData, setFormData] = useState({
    Localidad: "",
    CodigoPostal: "",
    Provincia: "",
    TipoMonumento: "",
  });

  const [formTouched, setFormTouched] = useState(false); // Para detectar si el formulario cambió

  const handleChange = (e) => {
    const { name, value } = e.target;

    // Si seleccionan la opción "Selecciona un tipo", limpiar todo el formulario
    if (name === "TipoMonumento" && value === "Selecciona un tipo") {
      setFormData({
        Localidad: "",
        CodigoPostal: "",
        Provincia: "",
        TipoMonumento: "",
      });
      setFormTouched(true); // Marcar que el formulario ha cambiado
    } else {
      setFormData({
        ...formData,
        [name]: value,
      });
      setFormTouched(true); // Marcar que el formulario ha cambiado
    }
  };

  const handleBuscar = async (e) => {
    e.preventDefault();

    // Validación: Asegurar que al menos un campo tenga datos válidos
    if (
      formData.Localidad.trim() === "" &&
      formData.CodigoPostal.trim() === "" &&
      formData.Provincia.trim() === "" &&
      formData.TipoMonumento === ""
    ) {
      alert("Por favor, completa al menos un campo para realizar la búsqueda.");
      return;
    }

    // Validación: Asegurar que el código postal tenga exactamente 5 dígitos
    if (formData.CodigoPostal && formData.CodigoPostal.length !== 5) {
      alert("El código postal debe tener exactamente 5 dígitos.");
      return;
    }
    try {
      console.log("Datos del formulario:", formData);
      alert(`Datos enviados:\n${JSON.stringify(formData, null, 2)}`);
      //const result = await resultadoBusqueda(formData);
      //setResultadoBusqueda(result);
      setResultadoBusqueda([
        {
          Nombre: "Castillo",
          TipoMonumento: "Castillo",
          Direccion: "calle comando",
          Localidad: "localidad",
          CodigoPostal: "07180",
          Provincia: "Provincia",
          Descripcion: "Descripción",
        },
      ]);
    } catch (e) {
      console.error("Error al realizar la búsqueda:", e.message);
      alert("Error al realizar la búsqueda. Intente más tarde.");
      return;
    }
  };

  const handleCancel = () => {
    setFormData({
      Localidad: "",
      CodigoPostal: "",
      Provincia: "",
      TipoMonumento: "",
    });
    setResultadoBusqueda([])
    setFormTouched(false); // Restablecer el estado
  };

  //TODO: Llamada a la api
  useEffect(() => {
    const fetchMonuemtnos = async () => {
      try {
        console.log("FetchMonuemtnos");
        //const result = await obtenerMonumetnos();
        //setTotalMonumentos(result);
        setTotalMonumentos([
          {
            Nombre: "Castillo",
            TipoMonumento: "Castillo",
            Direccion: "calle comando",
            Localidad: "localidad",
            CodigoPostal: "07180",
            Provincia: "Provincia",
            Descripcion: "Descripción",
            Latitud: "43.09096533979687",
            Longitud: "-2.303607169311588",
          },
          {
            Nombre: "Torre",
            TipoMonumento: "Castillo",
            Direccion: "calle comando",
            Localidad: "localidad",
            CodigoPostal: "07180",
            Provincia: "Provincia",
            Descripcion: "Descripción",
            Latitud: "43.24845524092432",
            Longitud: "-2.393759662151337",
          },
        ]);
      } catch (e) {
        console.error("Error al cargar los monumentos:", e);
      }
    };

    fetchMonuemtnos();
  }, []);

  return (
    <div className="container">
      {/* Flechas arriba a la izquierda */}
      <div className="arrows">
        <img
          src={ArrowLeft}
          alt="arrow left"
          className="arrow"
          onClick={() => navigate("/")}
        />
      </div>

      {/* Título */}
      <h1>Buscador de monumentos de interés cultural</h1>

      {/* Contenedor principal */}
      <div className="main-content">
        {/* Inputs al lado del cuadro */}
        <form className="inputs" onSubmit={handleBuscar}>
          <label>
            Localidad:{" "}
            <input
              className="input-text"
              id="Localidad"
              name="Localidad"
              type="text"
              value={formData.Localidad}
              placeholder="Add text"
              onChange={handleChange}
            />
          </label>

          <label>
            Código postal:{" "}
            <input
              className="input-text"
              id="CodigoPostal"
              name="CodigoPostal"
              type="text"
              maxLength={5} // Máximo 5 caracteres
              value={formData.CodigoPostal}
              placeholder="Add text"
              onChange={(e) => {
                const value = e.target.value;
                if (/^\d*$/.test(value) && value.length <= 5) {
                  handleChange(e);
                }
              }}
            />
          </label>

          <label>
            Provincia:{" "}
            <input
              className="input-text"
              id="Provincia"
              name="Provincia"
              type="text"
              value={formData.Provincia}
              placeholder="Add text"
              onChange={handleChange}
            />
          </label>

          <label>Tipo: </label>
          <select
            className="input-select"
            id="TipoMonumento"
            name="TipoMonumento"
            value={formData.TipoMonumento}
            onChange={handleChange}
          >
            <option value="">Selecciona un tipo</option>
            <option>Yacimiento arqueológico</option>
            <option>Iglesia-Ermita</option>
            <option>Monasterio-Convento</option>
            <option>Castillo-Fortaleza-Torre</option>
            <option>Edificio singular</option>
            <option>Puente</option>
            <option>Otros</option>
          </select>

          <div className="button-container">
            <button className="reset" type="button" onClick={handleCancel}>
              Cancelar
            </button>
            <button
              type="submit"
              className="submit"
              disabled={!formTouched} // Botón deshabilitado hasta que cambien los datos
            >
              Buscar
            </button>
          </div>
        </form>

        {/* Cuadro gris */}
        <div className="box4">
          {" "}
          <MapaView monumentos={totalMonumentos} />
        </div>
      </div>

      {/* Resultados */}
      <div className="results">
        <h2 className="results-title">Resultados de la búsqueda:</h2>
        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Tipo</th>
                <th>Dirección</th>
                <th>Localidad</th>
                <th>Código postal</th>
                <th>Provincia</th>
                <th>Descripción</th>
              </tr>
            </thead>
            <tbody>
              {resultadoBusqueda.map((monumento, index) => (
                <TablaResultado key={index} monumento={monumento} />
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
