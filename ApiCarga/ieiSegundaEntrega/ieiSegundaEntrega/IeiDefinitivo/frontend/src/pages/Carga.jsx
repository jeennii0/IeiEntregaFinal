import ArrowRight from "../assets/Arrow right.png";
import { useNavigate } from "react-router-dom";
import { useState } from "react";

export default function Carga() {
  const navigate = useNavigate();
  const [fuentesSeleccionadas, setFuentesSeleccionadas] = useState([]);
  const [resultadoCarga, setResultadoCarga] = useState([]);
  const fuentes = [
    { id: "cbox-cle", value: "CLE", label: "Castilla y León" },
    { id: "cbox-cv", value: "CV", label: "Comunitat Valenciana" },
    { id: "cbox-eus", value: "EUS", label: "Euskadi" },
  ];

  const handleCheckboxChange = (value) => {
    if (fuentesSeleccionadas.includes(value)) {
      setFuentesSeleccionadas(
        fuentesSeleccionadas.filter((fuente) => fuente !== value)
      );
    } else {
      setFuentesSeleccionadas([...fuentesSeleccionadas, value]);
    }
  };

  const handleSelectAll = () => {
    if (fuentesSeleccionadas.length === fuentes.length) {
      setFuentesSeleccionadas([]);
    } else {
      setFuentesSeleccionadas(fuentes.map((fuente) => fuente.value));
    }
  };

  const handleLoadData = async () => {
    try {
      console.log("Fuentes seleccionadas:", fuentesSeleccionadas);
      alert(
        `Cargando datos para las fuentes: ${fuentesSeleccionadas.join(", ")}`
      );
      // Simulación de carga de datos
      setResultadoCarga([
        {
          RegistrosCargados: "12",
          Duplicados: "{EUS, CLE, CV}",
          FuentesError: "{N/A}",
        },
      ]);
    } catch (e) {
      console.error("Error al cargar datos:", e);
      alert("Error al cargar datos");
    }
  };

  const handleCancel = () => {
    setFuentesSeleccionadas([]);
    setResultadoCarga([]);
  };

  const handleBorrarAlmacen = async () => {
    try {
      // Lógica de borrado de almacén
      setFuentesSeleccionadas([]);
      setResultadoCarga([]);
      alert("Almacén de datos borrado");
    } catch (error) {
      console.error("Error al borrar almacén de datos:", error);
      alert("Error al borrar almacén de datos");
    }
  };

  return (
    <div className="container">
      <div className="arrows">
        <img
          src={ArrowRight}
          alt="arrow right"
          className="arrow"
          onClick={() => navigate("/busqueda")}
        />
      </div>

      <h1>Carga del almacén de datos</h1>
      <div>
        <h4>Seleccione fuente:</h4>
        <div className="checkbox-group">
          <label>
            <input
              type="checkbox"
              checked={fuentesSeleccionadas.length === fuentes.length}
              onChange={handleSelectAll}
            />{" "}
            Seleccionar todas
          </label>
          {fuentes.map((fuente) => (
            <label key={fuente.id}>
              <input
                type="checkbox"
                id={fuente.id}
                value={fuente.value}
                checked={fuentesSeleccionadas.includes(fuente.value)}
                onChange={() => handleCheckboxChange(fuente.value)}
              />{" "}
              {fuente.label}
            </label>
          ))}
        </div>
      </div>
      <div className="button-group">
        <button className="reset" onClick={handleCancel}>
          Cancelar
        </button>
        <button
          className="submit"
          onClick={handleLoadData}
          disabled={fuentesSeleccionadas.length === 0}
        >
          Cargar
        </button>
        <button onClick={handleBorrarAlmacen}>Borrar almacén de datos</button>
      </div>

      <div className="results">
        <h2 className="results-title">Resultados de la carga:</h2>
        {resultadoCarga.length > 0 &&
          resultadoCarga.map((resultado, index) => (
            <div key={index}>
              <p>
                Número de registros cargados correctamente:{" "}
                <strong>{resultado.RegistrosCargados}</strong>
              </p>
              <p>
                Registros con errores y reparados:{" "}
                <strong>{resultado.Duplicados}</strong>
              </p>
              <p>
                Registros con errores y rechazados:{" "}
                <strong>{resultado.FuentesError}</strong>
              </p>
            </div>
          ))}
      </div>
    </div>
  );
}
